namespace SafetyTestWindowsFormsApp
{
    partial class AdsConnectionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnConnect = new Button();
            groupBox1 = new GroupBox();
            nudPort = new NumericUpDown();
            cmbAmsNetId = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudPort).BeginInit();
            SuspendLayout();
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(260, 127);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(109, 44);
            btnConnect.TabIndex = 0;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(nudPort);
            groupBox1.Controls.Add(cmbAmsNetId);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(label2);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(357, 109);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "ADS Connection";
            // 
            // nudPort
            // 
            nudPort.Location = new Point(80, 70);
            nudPort.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudPort.Name = "nudPort";
            nudPort.Size = new Size(120, 23);
            nudPort.TabIndex = 7;
            nudPort.Value = new decimal(new int[] { 851, 0, 0, 0 });
            // 
            // cmbAmsNetId
            // 
            cmbAmsNetId.FormattingEnabled = true;
            cmbAmsNetId.Location = new Point(80, 29);
            cmbAmsNetId.Name = "cmbAmsNetId";
            cmbAmsNetId.Size = new Size(271, 23);
            cmbAmsNetId.TabIndex = 6;
            cmbAmsNetId.SelectedIndexChanged += cmbAmsNetId_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 32);
            label1.Name = "label1";
            label1.Size = new Size(68, 15);
            label1.TabIndex = 4;
            label1.Text = "AMS Net ID";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 72);
            label2.Name = "label2";
            label2.Size = new Size(54, 15);
            label2.TabIndex = 5;
            label2.Text = "ADS Port";
            // 
            // AdsConnectionForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(381, 180);
            Controls.Add(groupBox1);
            Controls.Add(btnConnect);
            Name = "AdsConnectionForm";
            Text = "AdsConnectionForm";
            Load += AdsConnectionForm_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudPort).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button btnConnect;
        private GroupBox groupBox1;
        private ComboBox cmbAmsNetId;
        private Label label1;
        private Label label2;
        private NumericUpDown nudPort;
    }
}