using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class Ekle : MetroFramework.Forms.MetroForm
    {
        Socket sckt;
        public Ekle(Socket sck)
        {
            InitializeComponent();
            sckt = sck;
            metroTile1.Click += button1_Click;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("REHBERISIM", Encoding.UTF8.GetBytes(textBox1.Text + "=" + textBox2.Text));
                sckt.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
            Close();
        }
    }
}
