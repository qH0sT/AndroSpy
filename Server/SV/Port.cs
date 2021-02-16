using System;
using System.IO;
using System.Net;
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
            CheckForIllegalCrossThreadCalls = false;
            button1.Click += button1_Click;
            button2.Click += button2_Click;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;

        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Contains("<") || textBox1.Text.Contains(">"))
            {
                MessageBox.Show(this, "Please check for < or > char in password textBox, replace with new char.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            System.IO.File.WriteAllText("ConnectionPassword.txt", textBox1.Text);
            Form1.port_no = (int)numericUpDown1.Value;
            Form1.PASSWORD = textBox1.Text;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
            /*
            using (MemoryStream ms = new MemoryStream(File.ReadAllBytes("call.wav")))
            {
                using (FileStream fs = new FileStream("call2.wav", FileMode.Append))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead = ms.ToArray().Length;
                    do
                    {
                        bytesRead = ms.Read(buffer, 0, buffer.Length);
                        fs.Write(buffer, 0, bytesRead);
                        
                    }
                    while (bytesRead > 0);
                }
            }
            */

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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Contains("<"))
            {
                MessageBox.Show(this, "You can't use this char <. because this is special char for Program", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Text = textBox1.Text.Replace("<", "");
            }
            if (textBox1.Text.Contains(">"))
            {
                MessageBox.Show(this, "You can't use this char >. because this is special char for Program", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Text = textBox1.Text.Replace(">", "");
            }
        }
    }
}
