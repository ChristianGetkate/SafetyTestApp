/*
using TwinCAT.Ads;
using LedSingleLibrary;
using System.Threading;

namespace SafetyTestWindowsFormsApp
{
    public partial class SafetyTestMainForm : Form
    {
        private TwinCATConnector localConnector = new TwinCATConnector();
        private Dictionary<string, uint> variableHandleData = new Dictionary<string, uint>();
        private List<LedSingleControl> leds = new List<LedSingleControl>();
        private Dictionary<string, bool> ledStates = new Dictionary<string, bool>();
        private SynchronizationContext context;
        private TwinCATConnector remoteConnector = new TwinCATConnector();
        private Dictionary<uint, MotorDriveData> motordriveDataValues = new Dictionary<uint, MotorDriveData>();
        private Dictionary<uint, DataGridViewRow> rowMapping = new Dictionary<uint, DataGridViewRow>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(); // Cancellation token source

        public SafetyTestMainForm(string amsNetId, int port)
        {
            InitializeComponent();
            InitializeLocalConnector();
            InitializeRemoteConnector(amsNetId, port);
            InitializeUIComponents();
            StartCyclicReadLedStates();
        }
        private void InitializeLocalConnector()
        {
            localConnector.Connect(AmsNetId.Local.ToString(), 851);
            context = SynchronizationContext.Current;
        }

        private void InitializeRemoteConnector(string amsNetId, int port)
        {
            remoteConnector.Connect(AmsNetId.Local.ToString(), port);
            InitializeDataGridView();
        }

        private void InitializeUIComponents()
        {
            tabControl1.TabPages[0].Controls.Add(new Button());



            //AddButtonsAndTextBoxes("ESB", "EmergencyStopButtons_TS", 20, 20, 8, Color.LightSalmon);
            //AddButtonsAndTextBoxes("LC", "Lightcurtains_TS", 180, 20, 8, Color.LightYellow);
            //AddButtonsAndTextBoxes("IG", "InterlockingGuards_TS", 340, 20, 4, Color.LightGray);
            //AddSingleButtonAndTextBox("IG", "bInterlockingGuard1_MZ", 340, 20 + 4 * 40, "IG1 (MZ)", Color.LightGray);
            //AddButtonsAndTextBoxes("RB", "ResetButtons_TS", 500, 20, 8, Color.LightBlue);
            //AddLeds("LampRB", "LampResetButtons_TS", 580, 15, 8);
            //AddAdditionalButtons();
        }

        private void AddAdditionalButtons()
        {
            int additionalButtonY = 20 + 8 * 40;
            AddSingleButtonAndTextBox("ESB", "bEmergencyStopButton1_MZ", 20, additionalButtonY, "ESB1 (MZ)", Color.LightSalmon);
            AddSingleButtonAndTextBox("LC", "bLightcurtain1_MZ", 180, additionalButtonY, "LC1 (MZ)", Color.LightYellow);
            AddSingleButtonAndTextBox("RB", "bResetButton1_MZ", 500, additionalButtonY, "RB1 (MZ)", Color.LightBlue);
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

        private void AddButtonsAndTextBoxes(string prefix, string variableArrayName, int buttonX, int startY, int count, Color buttonColor)
        {
            for (int i = 0; i < count; i++)
            {
                AddSingleButtonAndTextBox(prefix, $"arr{variableArrayName}[{i}]", buttonX, startY + i * 40, $"{prefix}{i + 1} (TS)", buttonColor);
            }
        }

        private void AddSingleButtonAndTextBox(string prefix, string variableName, int buttonX, int yPosition, string buttonText, Color buttonColor)
        {
            var button = CreateButton(buttonText, buttonX, yPosition, buttonColor);
            var textBox = CreateTextBox(buttonX + 80, yPosition);

            AddVariableHandle(buttonText, variableName);
            ConfigureButtonEvents(button, buttonColor);

            this.Controls.Add(button);
            this.Controls.Add(textBox);

            this.tabControl1.TabPages[0].Controls.Add(button);
        }

        private Button CreateButton(string buttonText, int x, int y, Color color)
        {
            Button button = new Button
            {
                Text = buttonText,
                Location = new Point(x, y),
                BackColor = color,
                AutoSize = false,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            button.Font = GetAdjustedFont(button, buttonText, button.ClientRectangle.Size, 7, button.Font, true);
            return button;
        }

        private TextBox CreateTextBox(int x, int y)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Width = 70,
                Font = GetAdjustedFont(new TextBox(), "", new Size(70, 20), 7, SystemFonts.DefaultFont, true)
            };
        }

        private void AddVariableHandle(string buttonText, string variableName)
        {
            try
            {
                uint handle = localConnector.CreateVariableHandle($"MachineObjectsArray.SafetyTestTrolley[0].{variableName}");
                if (!variableHandleData.ContainsKey(buttonText))
                {
                    variableHandleData.Add(buttonText, handle);
                }
                else
                {
                    ShowError($"Duplicate key detected for {buttonText}. Please check your configuration.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error creating variable handle for {buttonText}: {ex.Message}");
            }
        }

        private void ConfigureButtonEvents(Button button, Color originalColor)
        {
            button.MouseDown += (sender, e) => Button_MouseDown(sender, e, originalColor);
            button.MouseUp += (sender, e) => Button_MouseUp(sender, e, originalColor);
        }

        private Font GetAdjustedFont(Control control, string text, Size containerSize, int maxFontSize, Font originalFont, bool smallestOnFail)
        {
            Font adjustedFont = null;
            using (Graphics g = control.CreateGraphics())
            {
                SizeF extent = g.MeasureString(text, originalFont);
                float hRatio = containerSize.Height / extent.Height;
                float wRatio = containerSize.Width / extent.Width;
                float ratio = Math.Min(hRatio, wRatio);
                float newSize = originalFont.Size * ratio;

                adjustedFont = new Font(originalFont.FontFamily, Math.Min(newSize, maxFontSize));
            }
            return adjustedFont;
        }

        private void Button_MouseDown(object sender, MouseEventArgs e, Color originalColor)
        {
            Button button = (Button)sender;
            uint variableHandle = variableHandleData[button.Text];
            localConnector.WriteBool(variableHandle, true);
            button.BackColor = GetPressedColor(originalColor);
        }

        private Color GetPressedColor(Color originalColor)
        {
            if (originalColor == Color.LightSalmon)
            {
                return Color.Red;
            }
            else if (originalColor == Color.LightYellow)
            {
                return Color.Yellow;
            }
            else if (originalColor == Color.LightGray)
            {
                return Color.Gray;
            }
            else if (originalColor == Color.LightBlue)
            {
                return Color.Blue;
            }
            else
            {
                return originalColor;
            }
        }

        private void Button_MouseUp(object sender, MouseEventArgs e, Color originalColor)
        {
            Button button = (Button)sender;
            uint variableHandle = variableHandleData[button.Text];
            localConnector.WriteBool(variableHandle, false);
            button.BackColor = originalColor;
        }

        private void AddLeds(string prefix, string variableArrayName, int ledX, int startY, int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddSingleLed(prefix, $"arr{variableArrayName}[{i}]", ledX, startY + i * 40, $"{prefix}{i + 1} (TS)");
            }
            AddSingleLed(prefix, "bLampResetButton1_MZ", ledX, startY + count * 40, $"{prefix}{count + 1} (MZ)");
        }

        private void AddSingleLed(string prefix, string variableName, int ledX, int yPosition, string ledText)
        {
            LedSingleControl led = new LedSingleControl
            {
                Width = 35,
                Height = 35,
                Text = ledText,
                Value = false,
                Location = new Point(ledX + 80, yPosition),
                OnColor = Color.Blue
            };

            AddVariableHandle(ledText, variableName);
            leds.Add(led);
            ledStates[ledText] = false;

            this.Controls.Add(led);
        }
        
        private void StartCyclicReadLedStates()
        {
            Task.Run(async() =>
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(500, cancellationTokenSource.Token);
                    ReadLedStates();
                }
            });
        }
        
        private void ReadLedStates()
        {
            foreach (var led in leds)
            {
                try
                {
                    uint handle = variableHandleData[led.Text];
                    bool currentValue = localConnector.ReadBool(handle);
                    if (ledStates[led.Text] != currentValue)
                    {
                        ledStates[led.Text] = currentValue;
                        UpdateLedControl(led, currentValue);
                    }
                }
                catch (Exception ex)
                {
                    // Optionally log the exception
                }
            }
        }

        private void UpdateLedControl(LedSingleControl led, bool state)
        {
            context.Post(new SendOrPostCallback(o => led.Value = state), null);
        }

        private void UpdateRows()
        {
            dataGridView1.SuspendLayout();
            foreach (var entry in motordriveDataValues)
            {
                uint index = entry.Key;
                MotorDriveData motorDriveData = entry.Value;

                bool motorDriveInSTO = ShouldShowMotorDriveInSTO(motorDriveData);

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

        private bool ShouldShowMotorDriveInSTO(MotorDriveData motorDriveData)
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

        private void ShowError(string message)
        {
            MessageBox.Show(message);
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
*/
using TwinCAT.Ads;
using LedSingleLibrary;
using System.Threading;

namespace SafetyTestWindowsFormsApp
{
    public partial class SafetyTestMainForm : Form
    {
        private TwinCATConnector localConnector = new TwinCATConnector();
        
        private Dictionary<string, uint> variableHandleData = new Dictionary<string, uint>();
        private List<LedSingleControl> leds = new List<LedSingleControl>();
        private Dictionary<string, bool> ledStates = new Dictionary<string, bool>();
        private SynchronizationContext context;
        private TwinCATConnector remoteConnector = new TwinCATConnector();
        private Dictionary<uint, MotorDriveData> motordriveDataValues = new Dictionary<uint, MotorDriveData>();
        private Dictionary<uint, DataGridViewRow> rowMapping = new Dictionary<uint, DataGridViewRow>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(); // Cancellation token source

        public SafetyTestMainForm(string amsNetId, int port)
        {
            InitializeComponent();
            InitializeLocalConnector();
            InitializeRemoteConnector(amsNetId, port);
            InitializeUIComponents();
            StartCyclicReadLedStates();
        }
        private void InitializeLocalConnector()
        {
            localConnector.Connect(AmsNetId.Local.ToString(), 851);
            context = SynchronizationContext.Current;
        }

        private void InitializeRemoteConnector(string amsNetId, int port)
        {
            remoteConnector.Connect(AmsNetId.Local.ToString(), port);
            InitializeDataGridView();
        }

        private void InitializeUIComponents()
        {
            tabControl1.TabPages[0].Controls.Add(new Button());



            AddButtonsAndTextBoxes("ESB", "EmergencyStopButtons_TS", 20, 20, 8, Color.LightSalmon);
            AddButtonsAndTextBoxes("LC", "Lightcurtains_TS", 180, 20, 8, Color.LightYellow);
            AddButtonsAndTextBoxes("IG", "InterlockingGuards_TS", 340, 20, 4, Color.LightGray);
            AddSingleButtonAndTextBox("IG", "bInterlockingGuard1_MZ", 340, 20 + 4 * 40, "IG1 (MZ)", Color.LightGray);
            AddButtonsAndTextBoxes("RB", "ResetButtons_TS", 500, 20, 8, Color.LightBlue);
            AddLeds("LampRB", "LampResetButtons_TS", 580, 15, 8);
            AddAdditionalButtons();
        }

        private void AddAdditionalButtons()
        {
            int additionalButtonY = 20 + 8 * 40;
            AddSingleButtonAndTextBox("ESB", "bEmergencyStopButton1_MZ", 20, additionalButtonY, "ESB1 (MZ)", Color.LightSalmon);
            AddSingleButtonAndTextBox("LC", "bLightcurtain1_MZ", 180, additionalButtonY, "LC1 (MZ)", Color.LightYellow);
            AddSingleButtonAndTextBox("RB", "bResetButton1_MZ", 500, additionalButtonY, "RB1 (MZ)", Color.LightBlue);
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

        private void AddButtonsAndTextBoxes(string prefix, string variableArrayName, int buttonX, int startY, int count, Color buttonColor)
        {
            for (int i = 0; i < count; i++)
            {
                AddSingleButtonAndTextBox(prefix, $"arr{variableArrayName}[{i}]", buttonX, startY + i * 40, 0, $"{prefix}{i + 1} (TS)", buttonColor);
            }
        }

        private void AddSingleButtonAndTextBox(string prefix, string variableName, int buttonX, int yPosition, 
            int trolleyNumber, string buttonText, Color buttonColor)
        {
            string buttonTag = $"{trolleyNumber}:{buttonText}";
            var button = CreateButton(trolleyNumber, buttonText, buttonX, yPosition, buttonColor);
            var textBox = CreateTextBox(buttonX + 80, yPosition);

            AddVariableHandle(trolleyNumber, buttonText, variableName);
            ConfigureButtonEvents(button, buttonColor);

            //this.Controls.Add(button);
            //this.Controls.Add(textBox);

            this.tabControl1.TabPages[trolleyNumber].Controls.Add(button);
        }

        private Button CreateButton(int trolleyNumber, string buttonText, int x, int y, Color color)
        {
            Button button = new Button
            {
                Text = buttonText,
                Tag = $"{trolleyNumber}:" + buttonText,
                Location = new Point(x, y),
                BackColor = color,
                AutoSize = false,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            button.Font = GetAdjustedFont(button, buttonText, button.ClientRectangle.Size, 7, button.Font, true);
            return button;
        }

        private TextBox CreateTextBox(int x, int y)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Width = 70,
                Font = GetAdjustedFont(new TextBox(), "", new Size(70, 20), 7, SystemFonts.DefaultFont, true)
            };
        }

        private void AddVariableHandle(int trolleyNumber, string buttonText, string variableName)
        {
            try
            {
                string buttonTag = $"{trolleyNumber}:" + buttonText;
                uint handle = localConnector.CreateVariableHandle($"MachineObjectsArray.SafetyTestTrolley[{trolleyNumber}].{variableName}");
                if (!variableHandleData.ContainsKey(buttonTag))
                {
                    variableHandleData.Add(buttonTag, handle);
                }
                else
                {
                    ShowError($"Duplicate key detected for {buttonTag}. Please check your configuration.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error creating variable handle for {buttonText}: {ex.Message}");
            }
        }

        private void ConfigureButtonEvents(Button button, Color originalColor)
        {
            button.MouseDown += (sender, e) => Button_MouseDown(sender, e, originalColor);
            button.MouseUp += (sender, e) => Button_MouseUp(sender, e, originalColor);
        }

        private Font GetAdjustedFont(Control control, string text, Size containerSize, int maxFontSize, Font originalFont, bool smallestOnFail)
        {
            Font adjustedFont = null;
            using (Graphics g = control.CreateGraphics())
            {
                SizeF extent = g.MeasureString(text, originalFont);
                float hRatio = containerSize.Height / extent.Height;
                float wRatio = containerSize.Width / extent.Width;
                float ratio = Math.Min(hRatio, wRatio);
                float newSize = originalFont.Size * ratio;

                adjustedFont = new Font(originalFont.FontFamily, Math.Min(newSize, maxFontSize));
            }
            return adjustedFont;
        }

        private void Button_MouseDown(object sender, MouseEventArgs e, Color originalColor)
        {
            Button button = (Button)sender;
            uint variableHandle = variableHandleData[button.Text];
            localConnector.WriteBool(variableHandle, true);
            button.BackColor = GetPressedColor(originalColor);
        }

        private Color GetPressedColor(Color originalColor)
        {
            if (originalColor == Color.LightSalmon)
            {
                return Color.Red;
            }
            else if (originalColor == Color.LightYellow)
            {
                return Color.Yellow;
            }
            else if (originalColor == Color.LightGray)
            {
                return Color.Gray;
            }
            else if (originalColor == Color.LightBlue)
            {
                return Color.Blue;
            }
            else
            {
                return originalColor;
            }
        }

        private void Button_MouseUp(object sender, MouseEventArgs e, Color originalColor)
        {
            Button button = (Button)sender;
            uint variableHandle = variableHandleData[button.Tag.ToString()];
            localConnector.WriteBool(variableHandle, false);
            button.BackColor = originalColor;
        }

        private void AddLeds(string prefix, string variableArrayName, int ledX, int startY, int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddSingleLed(prefix, $"arr{variableArrayName}[{i}]", ledX, startY + i * 40, $"{prefix}{i + 1} (TS)");
            }
            AddSingleLed(prefix, "bLampResetButton1_MZ", ledX, startY + count * 40, $"{prefix}{count + 1} (MZ)");
        }

        private void AddSingleLed(string prefix, string variableName, int ledX, int yPosition, string ledText)
        {
            LedSingleControl led = new LedSingleControl
            {
                Width = 35,
                Height = 35,
                Text = ledText,
                Value = false,
                Location = new Point(ledX + 80, yPosition),
                OnColor = Color.Blue
            };

            AddVariableHandle(ledText, variableName);

            leds.Add(led);
            ledStates[ledText] = false;

            this.Controls.Add(led);
        }

        private void StartCyclicReadLedStates()
        {
            Task.Run(async () =>
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(500, cancellationTokenSource.Token);
                    ReadLedStates();
                }
            });
        }

        private void ReadLedStates()
        {
            foreach (var led in leds)
            {
                try
                {
                    uint handle = variableHandleData[led.Text];
                    bool currentValue = localConnector.ReadBool(handle);
                    if (ledStates[led.Text] != currentValue)
                    {
                        ledStates[led.Text] = currentValue;
                        UpdateLedControl(led, currentValue);
                    }
                }
                catch (Exception ex)
                {
                    // Optionally log the exception
                }
            }
        }

        private void UpdateLedControl(LedSingleControl led, bool state)
        {
            context.Post(new SendOrPostCallback(o => led.Value = state), null);
        }

        private void UpdateRows()
        {
            dataGridView1.SuspendLayout();
            foreach (var entry in motordriveDataValues)
            {
                uint index = entry.Key;
                MotorDriveData motorDriveData = entry.Value;

                bool motorDriveInSTO = ShouldShowMotorDriveInSTO(motorDriveData);

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

        private bool ShouldShowMotorDriveInSTO(MotorDriveData motorDriveData)
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

        private void ShowError(string message)
        {
            MessageBox.Show(message);
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
