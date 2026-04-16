namespace Server.Forms
{
    partial class FormMicrophone
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.btnRecord = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelWait = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            //
            // panel1
            //
            this.panel1.Controls.Add(this.comboBox1);
            this.panel1.Controls.Add(this.btnRecord);
            this.panel1.Controls.Add(this.btnSave);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(560, 45);
            this.panel1.TabIndex = 0;
            //
            // comboBox1
            //
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.Enabled = false;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(12, 9);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(300, 28);
            this.comboBox1.TabIndex = 0;
            //
            // btnRecord
            //
            this.btnRecord.Enabled = false;
            this.btnRecord.Location = new System.Drawing.Point(320, 9);
            this.btnRecord.Name = "btnRecord";
            this.btnRecord.Size = new System.Drawing.Size(90, 28);
            this.btnRecord.TabIndex = 1;
            this.btnRecord.Tag = "start";
            this.btnRecord.Text = "Record";
            this.btnRecord.UseVisualStyleBackColor = true;
            this.btnRecord.Click += new System.EventHandler(this.BtnRecord_Click);
            //
            // btnSave
            //
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(418, 9);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 28);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            //
            // labelStatus
            //
            this.labelStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labelStatus.Location = new System.Drawing.Point(0, 130);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(560, 28);
            this.labelStatus.TabIndex = 1;
            this.labelStatus.Text = "Waiting for microphone list...";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // labelWait
            //
            this.labelWait.AutoSize = true;
            this.labelWait.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelWait.Location = new System.Drawing.Point(230, 75);
            this.labelWait.Name = "labelWait";
            this.labelWait.Size = new System.Drawing.Size(78, 29);
            this.labelWait.TabIndex = 2;
            this.labelWait.Text = "Wait...";
            //
            // timer1
            //
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            //
            // FormMicrophone
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 158);
            this.Controls.Add(this.labelWait);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormMicrophone";
            this.Text = "Microphone";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMicrophone_FormClosed);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.ComboBox comboBox1;
        public System.Windows.Forms.Button btnRecord;
        public System.Windows.Forms.Button btnSave;
        public System.Windows.Forms.Label labelStatus;
        public System.Windows.Forms.Label labelWait;
        public System.Windows.Forms.Timer timer1;
    }
}
