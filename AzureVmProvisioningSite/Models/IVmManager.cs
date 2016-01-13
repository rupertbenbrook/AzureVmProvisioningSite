using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureVmProvisioningSite.Models
{
    public interface IVmManager
    {
        Task<IList<VmState>> GetVmsAsync();
        Task CreateAsync(string name);
        Task StartAsync(string name);
        Task StopAsync(string name);
        Task DeleteAsync(string name);
        Task RestartAsync(string name);
        Task SnapshotAsync(string name, string snapshotName);
        Task RestoreSnapshotAsync(string name, int snapshotIndex);
        Task ConnectAsync(string name);
    }
}