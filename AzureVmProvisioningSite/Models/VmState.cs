using System.Collections.Generic;

namespace AzureVmProvisioningSite.Models
{
    public class VmState
    {
        public VmState()
        {
            Snapshots = new List<Snapshot>();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public IList<Snapshot> Snapshots { get; }
    }
}