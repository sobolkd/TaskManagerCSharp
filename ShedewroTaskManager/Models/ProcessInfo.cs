using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShedewroTaskManager.Models
{
    class ProcessInfo
    {
        public string Name { get; }
        public long MemoryUsage { get; }
        public double CpuUsage { get; }

        public ProcessInfo(string name, long memoryUsage, double cpuUsage)
        {
            Name = name;
            MemoryUsage = memoryUsage;
            CpuUsage = cpuUsage;
        }
    }


}
