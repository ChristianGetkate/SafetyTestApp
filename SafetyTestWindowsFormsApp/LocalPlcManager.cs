using TwinCAT.Ads;
using LedSingleLibrary;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SafetyTestWindowsFormsApp
{
    public class LocalPlcManager
    {
        private TwinCATConnector localConnector;
        private Dictionary<string, uint> variableHandleData = new Dictionary<string, uint>();
        private List<LedSingleControl> leds = new List<LedSingleControl>();
        private Dictionary<string, bool> ledStates = new Dictionary<string, bool>();
        private SynchronizationContext context;
        //private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public LocalPlcManager()
        {
            localConnector = new TwinCATConnector();
        }

        public void InitializeLocalConnector(string amsNetId, int port)
        {
            localConnector.Connect(amsNetId, port);
            context = SynchronizationContext.Current;
        }

        public void AddControlsToTabControl(TabControl tabControl)
        {
            int i = 0;

            foreach(TabPage tab in tabControl.TabPages)
            {
                AddButtonsAndTextBoxesToPage(tab, i, "ESB", "EmergencyStopButtons_TS", 20, 20, 8, Color.LightSalmon);
                AddButtonsAndTextBoxesToPage(tab, i, "LC", "Lightcurtains_TS", 180, 20, 8, Color.LightYellow);
                AddButtonsAndTextBoxesToPage(tab, i, "IG", "InterlockingGuards_TS", 340, 20, 4, Color.LightGray);
                AddButtonsAndTextBoxesToPage(tab, i, "RB", "ResetButtons_TS", 500, 20, 8, Color.LightBlue);
                AddAdditionalButtonsToPage(tab, i);
                AddLedsToPage(tab, i, "LampRB", "LampResetButtons_TS", 580, 15, 8);
                i++;
            }
        }

        private void AddAdditionalButtonsToPage(TabPage tabPage, int trolleyIndex)
        {
            int additionalButtonY = 20 + 8 * 40;
            AddSingleButtonAndTextBoxToPage(tabPage, trolleyIndex, "ESB", "bEmergencyStopButton1_MZ", 20, additionalButtonY, "ESB1 (MZ)", Color.LightSalmon);
            AddSingleButtonAndTextBoxToPage(tabPage, trolleyIndex, "LC", "bLightcurtain1_MZ", 180, additionalButtonY, "LC1 (MZ)", Color.LightYellow);
            AddSingleButtonAndTextBoxToPage(tabPage, trolleyIndex, "RB", "bResetButton1_MZ", 500, additionalButtonY, "RB1 (MZ)", Color.LightBlue);
            AddSingleButtonAndTextBoxToPage(tabPage, trolleyIndex, "IG", "bInterlockingGuard1_MZ", 340, 20 + 4 * 40, "IG1 (MZ)", Color.LightGray);
        }

        private void AddButtonsAndTextBoxesToPage(TabPage tabPage, int trolleyIndex, string prefix, string variableArrayName, int buttonX, int startY, int count, Color buttonColor)
        {
            for (int i = 0; i < count; i++)
            {
                AddSingleButtonAndTextBoxToPage(tabPage, trolleyIndex, prefix, $"arr{variableArrayName}[{i}]", buttonX, startY + i * 40, $"{prefix}{i + 1} (TS)", buttonColor);
            }
        }

        private void AddSingleButtonAndTextBoxToPage(TabPage tabPage, int trolleyIndex, string prefix, string variableName, int buttonX, int yPosition, string buttonText, Color buttonColor)
        {
            var button = CreateButton(trolleyIndex, buttonText, buttonX, yPosition, buttonColor);
            var textBox = CreateTextBox(buttonX + 80, yPosition);

            AddVariableHandle(trolleyIndex, buttonText, variableName);
            ConfigureButtonEvents(button, buttonColor);

            tabPage.Controls.Add(button);
            tabPage.Controls.Add(textBox);
        }

        private Button CreateButton(int trolleyIndex, string buttonText, int x, int y, Color color)
        {
            Button button = new Button
            {
                Text = buttonText,
                Tag = $"{trolleyIndex}:{buttonText}",
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

        private void AddVariableHandle(int trolleyIndex, string buttonText, string variableName)
        {
            try
            {
                uint handle = localConnector.CreateVariableHandle($"MachineObjectsArray.SafetyTestTrolley[{trolleyIndex}].{variableName}");
                if (!variableHandleData.ContainsKey($"{trolleyIndex}:{buttonText}"))
                {
                    variableHandleData.Add($"{trolleyIndex}:{buttonText}", handle);
                }
                else
                {
                    ShowError($"Duplicate key detected for {$"{trolleyIndex}:{buttonText}"}. Please check your configuration.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error creating variable handle for {trolleyIndex}:{buttonText}: {ex.Message}");
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
            uint variableHandle = variableHandleData[(string)button.Tag];
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
            uint variableHandle = variableHandleData[(string)button.Tag];
            localConnector.WriteBool(variableHandle, false);
            button.BackColor = originalColor;
        }

        private void AddLedsToPage(TabPage tabPage, int trolleyIndex, string prefix, string variableArrayName, int ledX, int startY, int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddSingleLedToPage(tabPage, trolleyIndex, prefix, $"arr{variableArrayName}[{i}]", ledX, startY + i * 40, $"{prefix}{i + 1} (TS)");
            }
            AddSingleLedToPage(tabPage, trolleyIndex, prefix, "bLampResetButton1_MZ", ledX, startY + count * 40, $"{prefix}{count + 1} (MZ)");
        }

        private void AddSingleLedToPage(TabPage tabPage, int trolleyIndex, string prefix, string variableName, int ledX, int yPosition, string ledText)
        {
            LedSingleControl led = new LedSingleControl
            {
                Width = 35,
                Height = 35,
                Tag = $"{trolleyIndex}:{ledText}",
                Text = ledText,
                Value = false,
                Location = new Point(ledX + 80, yPosition),
                OnColor = Color.Blue
            };

            AddVariableHandle(trolleyIndex, ledText, variableName);
            leds.Add(led);
            ledStates[(string)led.Tag] = false;

            tabPage.Controls.Add(led);
        }

        public void StartCyclicReadLedStates()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(100);
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
                    uint handle = variableHandleData[(string)led.Tag];
                    bool currentValue = localConnector.ReadBool(handle);
                    if (ledStates[(string)led.Tag] != currentValue)
                    {
                        ledStates[(string)led.Tag] = currentValue;
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

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

    }
}
