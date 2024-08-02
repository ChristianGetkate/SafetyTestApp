using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafetyTestWindowsFormsApp
{
    public class MotorDriveData
    {
        public string ObjectName { get; set; }
        public string DriveName { get; set; }
        public uint DriveStatusWord { get; set; }
        public uint SignalStatusWord { get; set; }
        public uint Type { get; set; }
        public bool STO_Release { get; set; }
    }
}
