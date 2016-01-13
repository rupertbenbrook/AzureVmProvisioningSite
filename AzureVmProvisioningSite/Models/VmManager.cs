using System;
using Microsoft.Azure;
using Microsoft.Azure.Management.Compute;
using Microsoft.Azure.Management.Compute.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using WebGrease.Css.Extensions;

namespace AzureVmProvisioningSite.Models
{
    public class VmManager : IVmManager
    {
        private readonly IServicePrincipalAuthentication _servicePrincipalAuthentication;
        private IComputeManagementClient _compute;
        private IStorageManagementClient _storage;

        public VmManager(IServicePrincipalAuthentication servicePrincipalAuthentication)
        {
            _servicePrincipalAuthentication = servicePrincipalAuthentication;
        }

        public VmManager()
            : this(new ServicePrincipalAuthentication())
        {
        }

        public async Task<IList<VmState>> GetVmsAsync()
        {
            // TODO: Add filtering VMs by tag based on logged in user
            // Have tag that can have one or more user IDs and filter list based on this
            // Also add tag when creating VM
            var compute = await GetComputeManagementAsync();
            var vmList = new List<Task<VmState>>();
            var result = await compute.VirtualMachines.ListAsync(Configuration.DeploymentResourceGroup);
            do
            {
                vmList.AddRange(result.VirtualMachines
                    .Select(v => compute.VirtualMachines.GetWithInstanceViewAsync(Configuration.DeploymentResourceGroup, v.Name)
                        .ContinueWith(t => GetVmStateFromInstance(t.Result.VirtualMachine))
                        .Unwrap()));
                if (!string.IsNullOrEmpty(result.NextLink))
                {
                    result = await compute.VirtualMachines.ListNextAsync(result.NextLink);
                    continue;
                }
                break;
            } while (!string.IsNullOrEmpty(result.NextLink));
            return (await Task.WhenAll(vmList)).OrderBy(v => v.Name).ToList().AsReadOnly();
        }

        public Task CreateAsync(string name)
        {
            throw new NotImplementedException();
        }

        private async Task<VmState> GetVmStateFromInstance(VirtualMachine vm)
        {
            var powerState = vm.InstanceView?.Statuses.FirstOrDefault(s => s.Code.StartsWith("PowerState/"));
            var state = new VmState
            {
                Id = vm.Id,
                Name = vm.Name,
                State = powerState == null ? "Unknown" : powerState.DisplayStatus,
            };
            var snapshotState = await GetSnapshotStateForVm(vm);
            snapshotState.Snapshots.ForEach(s => state.Snapshots.Add(s));
            return state;
        }

        public async Task StartAsync(string name)
        {
            var compute = await GetComputeManagementAsync();
            await compute.VirtualMachines.StartAsync(Configuration.DeploymentResourceGroup, name);
        }

        public async Task StopAsync(string name)
        {
            var compute = await GetComputeManagementAsync();
            await compute.VirtualMachines.DeallocateAsync(Configuration.DeploymentResourceGroup, name);
        }

        public async Task DeleteAsync(string name)
        {
            var compute = await GetComputeManagementAsync();
            await compute.VirtualMachines.DeleteAsync(Configuration.DeploymentResourceGroup, name);
        }

        public async Task RestartAsync(string name)
        {
            var compute = await GetComputeManagementAsync();
            await compute.VirtualMachines.RestartAsync(Configuration.DeploymentResourceGroup, name);
        }

        public async Task SnapshotAsync(string name, string snapshotName)
        {
            var compute = await GetComputeManagementAsync();

            // Stop if running
            var vm = await compute.VirtualMachines.GetWithInstanceViewAsync(Configuration.DeploymentResourceGroup, name);
            var running = IsVmRunning(vm.VirtualMachine);
            if (running)
            {
                await compute.VirtualMachines.PowerOffAsync(Configuration.DeploymentResourceGroup, name);
            }

            // Grab snapshot state json from the VM tags
            var snapshotState = await GetSnapshotStateForVm(vm.VirtualMachine);

            // Snapshot disks
            // https://azure.microsoft.com/en-us/documentation/articles/storage-blob-snapshots/
            var newSnapshot = new Snapshot
            {
                Name = snapshotName,
                Timestamp = DateTime.UtcNow,
                OsDisk = await TakeDiskSnapshot(vm.VirtualMachine.StorageProfile.OSDisk.VirtualHardDisk.Uri)
            };
            newSnapshot.DataDisks.AddRange(await Task.WhenAll(
                vm.VirtualMachine.StorageProfile.DataDisks
                .Select(async d => await TakeDiskSnapshot(d.VirtualHardDisk.Uri))));
            snapshotState.Snapshots.Add(newSnapshot);

            // Update snapshot state
            await SaveSnapshotStateForVm(snapshotState, vm.VirtualMachine);

            // Restart if originally running
            if (running)
            {
                await compute.VirtualMachines.StartAsync(Configuration.DeploymentResourceGroup, name);
            }
        }

        public async Task RestoreSnapshotAsync(string name, int snapshotIndex)
        {
            var compute = await GetComputeManagementAsync();

            // Stop if running
            var vm = await compute.VirtualMachines.GetWithInstanceViewAsync(Configuration.DeploymentResourceGroup, name);
            var running = IsVmRunning(vm.VirtualMachine);
            if (running)
            {
                await compute.VirtualMachines.PowerOffAsync(Configuration.DeploymentResourceGroup, name);
            }

            // Grab snapshot state json from the VM tags
            var snapshotState = await GetSnapshotStateForVm(vm.VirtualMachine);

            // Get the snapshot
            var snapshot = snapshotState.Snapshots[snapshotIndex];

            // Delete the vm
            await compute.VirtualMachines.DeleteAsync(Configuration.DeploymentResourceGroup, name);

            // Copy snapshot blobs over existing blobs
            // TODO: This doesn't work when VM created as leases exist on the blobs
            // Only way around seems to be to delete VM and recreate, but recreate cannot round trip at the moment
            // ERROR: Could not find member 'resources' on object of type 'ResourceDefinition'. Path 'resources'
            // ARM seems to have no disk management functions, only ASM - perhaps use that?

            await RestoreDiskSnapshot(vm.VirtualMachine.StorageProfile.OSDisk.VirtualHardDisk.Uri, snapshot.OsDisk);
            for (var pos = 0; pos < vm.VirtualMachine.StorageProfile.DataDisks.Count; pos++)
            {
                await RestoreDiskSnapshot(vm.VirtualMachine.StorageProfile.DataDisks[pos].VirtualHardDisk.Uri, snapshot.DataDisks[pos]);
            }

            // Recreate the vm
            // NOT WORKING!
            await compute.VirtualMachines.CreateOrUpdateAsync(Configuration.DeploymentResourceGroup, vm.VirtualMachine);

            // Restart if originally running
            if (running)
            {
                await compute.VirtualMachines.StartAsync(Configuration.DeploymentResourceGroup, name);
            }
        }

        public Task ConnectAsync(string name)
        {
            // Download an RDP file for connecting to public RDP endpoint of VM
            throw new NotImplementedException();
        }

        private static bool IsVmRunning(VirtualMachine vm)
        {
            var powerState = vm.InstanceView?.Statuses.FirstOrDefault(s => s.Code.StartsWith("PowerState/"));
            return (powerState == null ? "Unknown" : powerState.DisplayStatus) == "VM running";
        }

        private async Task<SnapshotState> GetSnapshotStateForVm(VirtualMachine vm)
        {
            var snapshotStateUri = vm.StorageProfile.OSDisk.VirtualHardDisk.Uri + ".snapshotstate";
            var storageCred = await GetStorageCredentialsForUri(snapshotStateUri);
            var blob = new CloudBlockBlob(new Uri(snapshotStateUri), storageCred);
            var snapshotState = new SnapshotState();
            if (await blob.ExistsAsync())
            {
                snapshotState = JsonConvert.DeserializeObject<SnapshotState>(await blob.DownloadTextAsync());
            }
            return snapshotState;
        }

        private async Task SaveSnapshotStateForVm(SnapshotState snapshotState, VirtualMachine vm)
        {
            var snapshotStateUri = vm.StorageProfile.OSDisk.VirtualHardDisk.Uri + ".snapshotstate";
            var storageCred = await GetStorageCredentialsForUri(snapshotStateUri);
            var blob = new CloudBlockBlob(new Uri(snapshotStateUri), storageCred);
            await blob.UploadTextAsync(JsonConvert.SerializeObject(snapshotState));
        }

        private async Task<IComputeManagementClient> GetComputeManagementAsync()
        {
            if (_compute != null)
            {
                return _compute;
            }
            var token = await _servicePrincipalAuthentication.AquireTokenAsync();
            _compute = new ComputeManagementClient(new TokenCloudCredentials(Configuration.SubscriptionId, token));
            return _compute;
        }

        private async Task<IStorageManagementClient> GetStorageManagementAsync()
        {
            if (_storage != null)
            {
                return _storage;
            }
            var token = await _servicePrincipalAuthentication.AquireTokenAsync();
            _storage = new StorageManagementClient(new TokenCloudCredentials(Configuration.SubscriptionId, token));
            return _storage;
        }

        private async Task<string> TakeDiskSnapshot(string diskUri)
        {
            var storageCred = await GetStorageCredentialsForUri(diskUri);
            var blob = new CloudPageBlob(new Uri(diskUri), storageCred);
            var snapshot = await blob.CreateSnapshotAsync();
            return snapshot.SnapshotQualifiedUri.ToString();
        }

        private async Task RestoreDiskSnapshot(string diskUri, string snapshotUri)
        {
            var storageCred = await GetStorageCredentialsForUri(diskUri);
            var diskBlob = new CloudPageBlob(new Uri(diskUri), storageCred);
            await diskBlob.StartCopyAsync(new Uri(snapshotUri));
            while (diskBlob.CopyState.Status == CopyStatus.Pending)
            {
                await Task.Delay(500);
            }
        }

        private async Task<StorageCredentials> GetStorageCredentialsForUri(string uri)
        {
            var storage = await GetStorageManagementAsync();
            var disk = new Uri(uri);
            var storageAccount = disk.Host.Split('.')[0];
            var keys = await storage.StorageAccounts.ListKeysAsync(Configuration.DeploymentResourceGroup, storageAccount);
            return new StorageCredentials(storageAccount, keys.StorageAccountKeys.Key1);
        }
    }
}