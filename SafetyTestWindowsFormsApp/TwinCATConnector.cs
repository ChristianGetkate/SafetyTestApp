using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace SafetyTestWindowsFormsApp
{
    public class TwinCATConnector
    {
        private AdsClient adsClient = new AdsClient();
        private Dictionary<uint, MotorDriveData> motorDriveDataValues = new Dictionary<uint, MotorDriveData>();

        public void Connect(string AmsNetId, int port)
        {
            adsClient.Connect(AmsNetId, port);
        }

        public uint CreateVariableHandle(string variableName)
        {
            try
            {
                return adsClient.CreateVariableHandle(variableName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating variable handle: {ex.Message}");
                throw;
            }
        }

        public bool ReadBool(uint handle)
        {
            try
            {
                return (bool)adsClient.ReadAny(handle, typeof(bool));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading variable: {ex.Message}");
                return false;
            }
        }

        public void WriteBool(uint handle, bool value)
        {
            try
            {
                adsClient.WriteAny(handle, value);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing variable: {ex.Message}");
            }
        }
        public void Disconnect()
        {
            if (adsClient != null)
            {
                adsClient.Dispose();
            }
        }

        public void AddReadMotorDriveDataValue(uint index)
        {
            MotorDriveData motorDriveData = ReadMotorDriveDataValue(index);
            motorDriveDataValues[index] = motorDriveData;
        }

        private MotorDriveData ReadMotorDriveDataValue(uint index)
        {
            uint objectNameToReadHandle = (uint)adsClient.CreateVariableHandle($"MachineObjectsArray.MotorDrive[{index}].Object.sName");
            uint driveNameToReadHandle = (uint)adsClient.CreateVariableHandle($"MachineObjectsArray.MotorDrive[{index}].sDriveName");
            uint driveStatusWordToReadHandle = (uint)adsClient.CreateVariableHandle($"MachineObjectsArray.MotorDrive[{index}].Communication.DriveStatusWord");
            uint signalStatusWordToReadHandle = (uint)adsClient.CreateVariableHandle($"MachineObjectsArray.MotorDrive[{index}].Communication.SignalStatusWord");
            uint typeToReadHandle = (uint)adsClient.CreateVariableHandle($"MachineObjectsArray.MotorDrive[{index}].eType");
            uint stoReleaseToReadHandle = (uint)adsClient.CreateVariableHandle($"MachineObjectsArray.MotorDrive[{index}].Safety.TS_STO_Release");

            string objectNameToRead = adsClient.ReadAny(objectNameToReadHandle, typeof(string), new int[] { 50 }) as string;
            string driveNameToRead = adsClient.ReadAny(driveNameToReadHandle, typeof(string), new int[] { 20 }) as string;
            uint driveStatusWordToRead = (uint)adsClient.ReadAny(driveStatusWordToReadHandle, typeof(uint));
            uint signalStatusWordToRead = (uint)adsClient.ReadAny(signalStatusWordToReadHandle, typeof(uint));
            uint typeToRead = (uint)adsClient.ReadAny(typeToReadHandle, typeof(uint));
            bool stoReleaseToRead = (bool)adsClient.ReadAny(stoReleaseToReadHandle, typeof(bool));

            return new MotorDriveData
            {
                ObjectName = objectNameToRead,
                DriveName = driveNameToRead,
                DriveStatusWord = driveStatusWordToRead,
                SignalStatusWord = signalStatusWordToRead,
                Type = typeToRead,
                STO_Release = stoReleaseToRead
            };
        }

        public Dictionary<uint, MotorDriveData> ReadMotorDriveDataValues()
        {
            Dictionary<uint, MotorDriveData> result = new Dictionary<uint, MotorDriveData>();
            foreach (uint index in motorDriveDataValues.Keys)
            {
                MotorDriveData motorDriveData = ReadMotorDriveDataValue(index);
                result.Add(index, motorDriveData);
            }
            return result;
        }
    }
}
