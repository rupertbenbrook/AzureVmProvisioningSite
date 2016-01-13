using System;
using System.Collections;
using System.Collections.Generic;

namespace AzureVmProvisioningSite.Models
{
    public class Snapshot
    {
        public Snapshot()
        {
            DataDisks = new List<string>();
        }
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }
        public string OsDisk { get; set; }
        public List<string> DataDisks { get; set; }
    }
}