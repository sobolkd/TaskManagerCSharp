using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace ShedewroTaskManager.Models
{
    public class GraphicsCardInfo
    {
        public string VideoName { get; private set; }
        public string DriverVersion { get; private set; }
        public long VideoMemory { get; private set; }

        public GraphicsCardInfo(ManagementObject videoObj)
        {
            VideoName = videoObj["Name"].ToString();
            DriverVersion = videoObj["DriverVersion"].ToString();
            VideoMemory = Convert.ToInt64(videoObj["AdapterRAM"]);
        }

        public string GetGraphicsCardInfoString()
        {
            return $"Video Card Name: {VideoName}\nDriver Version: {DriverVersion}\nVideo Memory: {VideoMemory} bytes";
        }
    }

}
