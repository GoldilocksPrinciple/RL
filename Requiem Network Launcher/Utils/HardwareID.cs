using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace Requiem_Network_Launcher
{
    class HardwareID
    {
        private static string _hwid;

        public static string Hwid { get => _hwid; set => _hwid = value; }

        public static void GetHardwareID()
        {
            StringBuilder sb = new StringBuilder();

            ManagementObjectSearcher cpu = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            ManagementObjectCollection cpu_Collection = cpu.Get();
            foreach (ManagementObject obj in cpu_Collection)
            {
                sb.Append(obj.Properties["ProcessorId"].Value.ToString());
                break;
            }

            ManagementObjectSearcher bios = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
            ManagementObjectCollection bios_Collection = bios.Get();
            foreach (ManagementObject obj in bios_Collection)
            {
                sb.Append(obj.Properties["SerialNumber"].Value.ToString());
                break;
            }

            ManagementObjectSearcher motherboard = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            ManagementObjectCollection motherboard_Collection = motherboard.Get();
            foreach (ManagementObject obj in motherboard_Collection)
            {
                sb.Append(obj.Properties["SerialNumber"].Value.ToString());
                break;
            }

            ManagementObject dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""c:""");
            dsk.Get();
            sb.Append(dsk["VolumeSerialNumber"].ToString());
            
            Hwid = sb.ToString().PadLeft(32, '0').Substring(0, 32);
        }
    }
}
