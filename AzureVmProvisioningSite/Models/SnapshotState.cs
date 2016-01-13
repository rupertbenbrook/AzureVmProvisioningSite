using System.Collections.Generic;

namespace AzureVmProvisioningSite.Models
{
    public class SnapshotState
    {
        public SnapshotState()
        {
            Snapshots = new List<Snapshot>();
        }

        public IList<Snapshot> Snapshots { get; }
    }
}