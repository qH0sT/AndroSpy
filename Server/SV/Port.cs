using System;
using System.IO;
using System.Windows.Forms;

namespace SV
{
    public partial class Port : MetroFramework.Forms.MetroForm
    {
        public Port()
        {
            InitializeComponent();
            textBox1.Text = File.ReadAllText("ConnectionPassword.txt").Replace(Environment.NewLine, "");
            button1.DialogResult = DialogResult.OK;
            button1.Click += button1_Click;
            button2.Click += button2_Click;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;            
        }
        private void button1_Click(object sender, EventArgs e)
        {            
            System.IO.File.WriteAllText("ConnectionPassword.txt", textBox1.Text);
            Form1.port_no = (int)numericUpDown1.Value;
            Form1.PASSWORD = textBox1.Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();           
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            string lbl = label2.Text;
            lbl = lbl.Substring(lbl.Length - 1) + lbl.Substring(0, lbl.Length - 1);
            label2.Text = lbl;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.PasswordChar = (checkBox1.Checked) ? '\0' : '*';
        }
    }
}
