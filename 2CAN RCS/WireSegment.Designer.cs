namespace CtrElectronics.CrossLinkControlSystem
{
    partial class WireSegment
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lblOutput1 = new System.Windows.Forms.Label();
            this.numOffset = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.lblInput1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.numDeadband = new System.Windows.Forms.NumericUpDown();
            this.numScalar = new System.Windows.Forms.NumericUpDown();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.pnlOutput1 = new System.Windows.Forms.Panel();
            this.cboOutput1 = new System.Windows.Forms.ComboBox();
            this.cboInput1 = new System.Windows.Forms.ComboBox();
            this.pnlInput1 = new System.Windows.Forms.Panel();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblHeader = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numOffset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDeadband)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numScalar)).BeginInit();
            this.pnlHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Khaki;
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.pnlHeader);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(900, 90);
            this.panel1.TabIndex = 38;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.lblOutput1);
            this.panel3.Controls.Add(this.numOffset);
            this.panel3.Controls.Add(this.label3);
            this.panel3.Controls.Add(this.lblInput1);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Controls.Add(this.numDeadband);
            this.panel3.Controls.Add(this.numScalar);
            this.panel3.Controls.Add(this.label18);
            this.panel3.Controls.Add(this.label19);
            this.panel3.Controls.Add(this.pnlOutput1);
            this.panel3.Controls.Add(this.cboOutput1);
            this.panel3.Controls.Add(this.cboInput1);
            this.panel3.Controls.Add(this.pnlInput1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(900, 90);
            this.panel3.TabIndex = 46;
            // 
            // lblOutput1
            // 
            this.lblOutput1.AutoSize = true;
            this.lblOutput1.BackColor = System.Drawing.Color.Transparent;
            this.lblOutput1.Location = new System.Drawing.Point(742, 67);
            this.lblOutput1.Name = "lblOutput1";
            this.lblOutput1.Size = new System.Drawing.Size(81, 13);
            this.lblOutput1.TabIndex = 46;
            this.lblOutput1.Text = "Not Selected";
            // 
            // numOffset
            // 
            this.numOffset.DecimalPlaces = 2;
            this.numOffset.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numOffset.Location = new System.Drawing.Point(480, 60);
            this.numOffset.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numOffset.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
            this.numOffset.Name = "numOffset";
            this.numOffset.Size = new System.Drawing.Size(57, 20);
            this.numOffset.TabIndex = 58;
            this.numOffset.ValueChanged += new System.EventHandler(this.numOffset_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(425, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 57;
            this.label3.Text = "Offset :";
            // 
            // lblInput1
            // 
            this.lblInput1.AutoSize = true;
            this.lblInput1.BackColor = System.Drawing.Color.Transparent;
            this.lblInput1.Location = new System.Drawing.Point(99, 67);
            this.lblInput1.Name = "lblInput1";
            this.lblInput1.Size = new System.Drawing.Size(81, 13);
            this.lblInput1.TabIndex = 56;
            this.lblInput1.Text = "Not Selected";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(632, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 55;
            this.label2.Text = "Output:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 54;
            this.label1.Text = "Input:";
            // 
            // numDeadband
            // 
            this.numDeadband.Location = new System.Drawing.Point(480, 11);
            this.numDeadband.Name = "numDeadband";
            this.numDeadband.Size = new System.Drawing.Size(57, 20);
            this.numDeadband.TabIndex = 53;
            this.numDeadband.ValueChanged += new System.EventHandler(this.numDeadband_ValueChanged);
            // 
            // numScalar
            // 
            this.numScalar.DecimalPlaces = 4;
            this.numScalar.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numScalar.Location = new System.Drawing.Point(480, 34);
            this.numScalar.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numScalar.Name = "numScalar";
            this.numScalar.Size = new System.Drawing.Size(68, 20);
            this.numScalar.TabIndex = 52;
            this.numScalar.Value = new decimal(new int[] {
            100,
            0,
            0,
            131072});
            this.numScalar.ValueChanged += new System.EventHandler(this.numScalar_ValueChanged);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(380, 14);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(94, 13);
            this.label18.TabIndex = 51;
            this.label18.Text = "Deadband (%) :";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(362, 41);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(111, 13);
            this.label19.TabIndex = 50;
            this.label19.Text = "Scalar (Multipler) :";
            // 
            // pnlOutput1
            // 
            this.pnlOutput1.Location = new System.Drawing.Point(635, 34);
            this.pnlOutput1.Name = "pnlOutput1";
            this.pnlOutput1.Size = new System.Drawing.Size(248, 23);
            this.pnlOutput1.TabIndex = 49;
            // 
            // cboOutput1
            // 
            this.cboOutput1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboOutput1.FormattingEnabled = true;
            this.cboOutput1.Location = new System.Drawing.Point(706, 11);
            this.cboOutput1.Name = "cboOutput1";
            this.cboOutput1.Size = new System.Drawing.Size(177, 21);
            this.cboOutput1.TabIndex = 47;
            this.cboOutput1.SelectedIndexChanged += new System.EventHandler(this.cboOutput1_SelectedIndexChanged);
            // 
            // cboInput1
            // 
            this.cboInput1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboInput1.FormattingEnabled = true;
            this.cboInput1.Location = new System.Drawing.Point(58, 10);
            this.cboInput1.Name = "cboInput1";
            this.cboInput1.Size = new System.Drawing.Size(298, 21);
            this.cboInput1.TabIndex = 45;
            this.cboInput1.SelectedIndexChanged += new System.EventHandler(this.cboInput1_SelectedIndexChanged);
            // 
            // pnlInput1
            // 
            this.pnlInput1.Location = new System.Drawing.Point(58, 32);
            this.pnlInput1.Name = "pnlInput1";
            this.pnlInput1.Size = new System.Drawing.Size(298, 23);
            this.pnlInput1.TabIndex = 48;
            // 
            // pnlHeader
            // 
            this.pnlHeader.Controls.Add(this.lblHeader);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(900, 0);
            this.pnlHeader.TabIndex = 45;
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Location = new System.Drawing.Point(12, 2);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(199, 13);
            this.lblHeader.TabIndex = 55;
            this.lblHeader.Text = "XXXXXXXXXXXXXXXXXXXXXXXX";
            // 
            // WireSegment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "WireSegment";
            this.Size = new System.Drawing.Size(900, 90);
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numOffset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDeadband)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numScalar)).EndInit();
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblOutput1;
        private System.Windows.Forms.NumericUpDown numOffset;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblInput1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numDeadband;
        private System.Windows.Forms.NumericUpDown numScalar;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Panel pnlOutput1;
        private System.Windows.Forms.ComboBox cboOutput1;
        private System.Windows.Forms.ComboBox cboInput1;
        private System.Windows.Forms.Panel pnlInput1;
        private System.Windows.Forms.Label lblHeader;

    }
}
