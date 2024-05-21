using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace ShedewroTaskManager.Models
{
    public class ProcessorInfo
    {
        public string ProcessorId { get; private set; }
        public string ProcessorName { get; private set; }
        public string Architecture { get; private set; }
        public int NumberOfCores { get; private set; }
        public int MaxClockSpeed { get; private set; }

        public ProcessorInfo(ManagementObject processorObj)
        {
            ProcessorId = processorObj["ProcessorId"].ToString();
            ProcessorName = processorObj["Name"].ToString();
            Architecture = processorObj["Architecture"].ToString();
            NumberOfCores = Convert.ToInt32(processorObj["NumberOfCores"]);
            MaxClockSpeed = Convert.ToInt32(processorObj["MaxClockSpeed"]);
        }

        public string GetProcessorInfoString()
        {
            return $"Processor ID: {ProcessorId}\nProcessor Name: {ProcessorName}\nArchitecture: {Architecture}\nNumber of Cores: {NumberOfCores}\nMax Clock Speed: {MaxClockSpeed}";
        }
    }


}