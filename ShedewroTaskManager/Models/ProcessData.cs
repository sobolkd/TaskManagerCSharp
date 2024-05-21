using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShedewroTaskManager.Models
{
    public class ProcessData
    {
        public string Name { get; set; }
        public string CPUUsage { get; set; }
        public string MemoryUsage { get; set; }
        public string NetworkUsage { get; set; }
        public string DiskUsage { get; set; }
    }
}

