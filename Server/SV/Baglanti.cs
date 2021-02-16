using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class Baglanti : MetroFramework.Forms.MetroForm
    {
        public string ID = ""; Socket socket;
        public Baglanti(Socket sck, string aydi)
        {
            InitializeComponent();
            ID = aydi;
            socket = sck;
            textBox3.Text = Form1.PASSWORD;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
        }
        bool isNotEmpty = false;

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox3.PasswordChar = checkBox1.Checked ? '\0' : '*';
        }

        private void metroTile1_Click(object sender, EventArgs e)
        {
            isNotEmpty = textBox1.Text != "" && textBox2.Text != "";
            if (isNotEmpty)
            {
                try
                {
                    if (textBox3.Text.Contains("<"))
                    {
                        MessageBox.Show(this, "You can't use this char <. because this is special char for Program", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBox3.Text = textBox3.Text.Replace("<", "");
                        return;
                    }
                    if (textBox3.Text.Contains(">"))
                    {
                        MessageBox.Show(this, "You can't use this char >. because this is special char for Program", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBox3.Text = textBox3.Text.Replace(">", "");
                        return;
                    }
                   
                    byte[] senddata = Form1.MyDataPacker("UPDATE", Encoding.UTF8.GetBytes("[VERI]" + textBox1.Text + "[VERI]" + textBox2.Text + "[VERI]"
                        + numericUpDown1.Value.ToString() + $"[VERI]{textBox3.Text}"));
                    socket.Send(senddata, 0, senddata.Length, SocketFlags.None);
                    Close();
                }
                catch (Exception) { }
            }
        }
    }
}
