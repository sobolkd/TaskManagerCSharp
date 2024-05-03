using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace ShedewroTaskManager.Models
{
    public class RamInfo
    {
        public ulong Capacity { get; private set; }
        public string Manufacturer { get; private set; }
        public string PartNumber { get; private set; }
        public string Speed { get; private set; }
        public string FormFactor { get; private set; }

        public RamInfo(ManagementObject memoryObj)
        {
            Capacity = Convert.ToUInt64(memoryObj["Capacity"]);
            Manufacturer = memoryObj["Manufacturer"].ToString();
            PartNumber = memoryObj["PartNumber"].ToString();
            Speed = memoryObj["Speed"].ToString();
            FormFactor = memoryObj["FormFactor"].ToString();
        }

        public string GetRamInfoString()
        {
            return $"RAM Capacity: {Capacity} bytes\nManufacturer: {Manufacturer}\nPart Number: {PartNumber}\nSpeed: {Speed} MHz\nForm Factor: {FormFactor}";
        }
    }

}
