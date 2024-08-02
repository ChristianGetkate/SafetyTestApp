using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SafetyTestWindowsFormsApp
{
    public partial class AdsConnectionForm : Form
    {
        public string AmsNetId { get; private set; }
        public int Port { get; private set; }
        public AdsConnectionForm()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            AmsNetId = cmbAmsNetId.Text;
            Port = (int)nudPort.Value;
            this.DialogResult = DialogResult.OK; // Close the form and indicate success
            //string amsNetId = cmbAmsNetId.Text;
            //SafetyTestMainForm safetyTestScreen1Form = new SafetyTestMainForm(cmbAmsNetId.Text, (int)nudPort.Value);
            //this.Hide();
            //safetyTestScreen1Form.ShowDialog();

            GC.Collect();
        }

        private void cmbAmsNetId_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void AdsConnectionForm_Load(object sender, EventArgs e)
        {

            // Change default port for systems running TC3 X64
            if (Environment.Is64BitOperatingSystem)
            {
                nudPort.Value = 851;
            }
        }
    }
}
