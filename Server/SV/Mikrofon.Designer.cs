namespace SV
{
    partial class Mikrofon
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Mikrofon));
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.comboBox1 = new MetroFramework.Controls.MetroComboBox();
            this.comboBox2 = new MetroFramework.Controls.MetroComboBox();
            this.button1 = new MetroFramework.Controls.MetroTile();
            this.button2 = new MetroFramework.Controls.MetroTile();
            this.SuspendLayout();
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Location = new System.Drawing.Point(47, 84);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(110, 19);
            this.metroLabel1.TabIndex = 8;
            this.metroLabel1.Text = "Sample Rate (Hz)";
            this.metroLabel1.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // metroLabel2
            // 
            this.metroLabel2.AutoSize = true;
            this.metroLabel2.Location = new System.Drawing.Point(188, 84);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(88, 19);
            this.metroLabel2.TabIndex = 9;
            this.metroLabel2.Text = "Audio Source";
            this.metroLabel2.Theme = MetroFramework.MetroThemeStyle.Dark;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.ItemHeight = 23;
            this.comboBox1.Items.AddRange(new object[] {
            "44100",
            "22050",
            "16000",
            "11025",
            "8000"});
            this.comboBox1.Location = new System.Drawing.Point(47, 119);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 29);
            this.comboBox1.Style = MetroFramework.MetroColorStyle.Silver;
            this.comboBox1.TabIndex = 10;
            this.comboBox1.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.comboBox1.UseSelectable = true;
            // 
            // comboBox2
            // 
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.ItemHeight = 23;
            this.comboBox2.Items.AddRange(new object[] {
            "Mic",
            "Default",
            "Call"});
            this.comboBox2.Location = new System.Drawing.Point(188, 119);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(121, 29);
            this.comboBox2.Style = MetroFramework.MetroColorStyle.Silver;
            this.comboBox2.TabIndex = 11;
            this.comboBox2.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.comboBox2.UseSelectable = true;
            // 
            // button1
            // 
            this.button1.ActiveControl = null;
            this.button1.Location = new System.Drawing.Point(47, 154);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(262, 36);
            this.button1.Style = MetroFramework.MetroColorStyle.Silver;
            this.button1.TabIndex = 12;
            this.button1.Text = "Start";
            this.button1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.button1.UseSelectable = true;
            // 
            // button2
            // 
            this.button2.ActiveControl = null;
            this.button2.Location = new System.Drawing.Point(47, 196);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(262, 36);
            this.button2.Style = MetroFramework.MetroColorStyle.Silver;
            this.button2.TabIndex = 13;
            this.button2.Text = "Stop";
            this.button2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.button2.UseSelectable = true;
            // 
            // Mikrofon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(356, 261);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.comboBox2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.metroLabel2);
            this.Controls.Add(this.metroLabel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Mikrofon";
            this.Text = "Live Microphone";
            this.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Mikrofon_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private MetroFramework.Controls.MetroComboBox comboBox1;
        private MetroFramework.Controls.MetroComboBox comboBox2;
        private MetroFramework.Controls.MetroTile button1;
        private MetroFramework.Controls.MetroTile button2;
    }
}