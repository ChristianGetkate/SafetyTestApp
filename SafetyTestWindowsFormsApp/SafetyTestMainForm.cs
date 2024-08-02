
using System;
using System.Windows.Forms;
using TwinCAT.Ads;

namespace SafetyTestWindowsFormsApp
{
    public partial class SafetyTestMainForm : Form
    {
        private LocalPlcManager plcManager;
        private TwinCATConnector remoteConnector = new TwinCATConnector();
        private Dictionary<uint, MotorDriveData> motordriveDataValues = new Dictionary<uint, MotorDriveData>();
        private Dictionary<uint, DataGridViewRow> rowMapping = new Dictionary<uint, DataGridViewRow>();

        public SafetyTestMainForm(string amsNetId, int port)
        {
            InitializeComponent();
            plcManager = new LocalPlcManager();
            plcManager.InitializeLocalConnector(AmsNetId.Local.ToString(), 851);
            plcManager.AddControlsToTabControl(this.tabControl1);
            plcManager.StartCyclicReadLedStates();

            InitializeRemoteConnector(amsNetId, port);
        }

        private void InitializeRemoteConnector(string amsNetId, int port)
        {
            remoteConnector.Connect(AmsNetId.Local.ToString(), port);
            InitializeDataGridView();
        }

        private void InitializeDataGridView()
        {
            dataGridView1.Columns.Add("Object Name", "Object Name");
            dataGridView1.Columns.Add("Motordrive Name", "Motordrive Name");
            dataGridView1.Columns.Add("Motordrive In STO", "Motordrive In STO");
            dataGridView1.Columns.Add("STO Release Status", "STO Release Status");
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
        }

        private async void ReadCycle()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);
                    motordriveDataValues = remoteConnector.ReadMotorDriveDataValues();
                    Invoke(new Action(() => UpdateRows()));
                }
            });
        }

        private void UpdateRows()
        {
            dataGridView1.SuspendLayout();
            foreach (var entry in motordriveDataValues)
            {
                uint index = entry.Key;
                MotorDriveData motorDriveData = entry.Value;

                bool motorDriveInSTO = MotorDriveInSTO(motorDriveData);

                if (rowMapping.ContainsKey(index))
                {
                    UpdateExistingRow(index, motorDriveData, motorDriveInSTO);
                }
                else
                {
                    AddNewRow(index, motorDriveData, motorDriveInSTO);
                }
            }
            dataGridView1.ResumeLayout();
        }

        private bool MotorDriveInSTO(MotorDriveData motorDriveData)
        {
            bool bit03SignalStatusWord = Convert.ToBoolean((motorDriveData.SignalStatusWord >> 3) & 1);
            E_MotorDriveType motorDriveType = (E_MotorDriveType)motorDriveData.Type;
            return ((motorDriveType == E_MotorDriveType.MD_Type_Emerson_M300 && motorDriveData.DriveStatusWord != 563) ||
                    (motorDriveType == E_MotorDriveType.MD_Type_BR_EtherCAT && bit03SignalStatusWord));
        }

        private void UpdateExistingRow(uint index, MotorDriveData motorDriveData, bool motorDriveInSTO)
        {
            var row = rowMapping[index];
            row.Cells["Object Name"].Value = motorDriveData.ObjectName;
            row.Cells["Motordrive Name"].Value = motorDriveData.DriveName;
            row.Cells["Motordrive In STO"].Value = motorDriveInSTO.ToString().ToUpper();
            row.Cells["STO Release Status"].Value = motorDriveData.STO_Release.ToString().ToUpper();
            row.Visible = !motorDriveData.STO_Release && !string.IsNullOrEmpty(motorDriveData.ObjectName);
        }

        private void AddNewRow(uint index, MotorDriveData motorDriveData, bool motorDriveInSTO)
        {
            var row = new DataGridViewRow();
            row.CreateCells(dataGridView1,
                            motorDriveData.ObjectName,
                            motorDriveData.DriveName,
                            motorDriveInSTO.ToString().ToUpper(),
                            motorDriveData.STO_Release.ToString().ToUpper());
            dataGridView1.Rows.Add(row);
            rowMapping[index] = row;
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Motordrive In STO" ||
                dataGridView1.Columns[e.ColumnIndex].Name == "STO Release Status")
            {
                if (e.Value != null)
                {
                    e.CellStyle.BackColor = e.Value.ToString().ToUpper() == "TRUE" ? Color.LightGreen : Color.FromArgb(255, 182, 193);
                    e.CellStyle.ForeColor = Color.Black;
                }
            }
        }
    }

    public enum E_MotorDriveType
    {
        MD_Type_None = 0,
        MD_Type_DigitalDir = 1,
        MD_Type_Spare1 = 2,
        MD_Type_BR_Sercos = 3,
        MD_Type_Emerson_M300 = 4,
        MD_Type_SuctionDrive = 5,
        MD_Type_Spare2 = 6,
        MD_Type_Spare3 = 7,
        MD_Type_Beckhoff = 8,
        MD_Type_BR_EtherCAT = 9,
        MD_Type_Emerson_Digitax = 10,
        MD_Type_Emerson_M700 = 11,
        MD_Type_MotorFan = 12,
        MD_Type_BeckhoffBrakeResistor = 13,
        MD_Type_DigitalDirEStop = 14,
        MD_Type_Beckhoff_AX8xxx = 15
    }
}
