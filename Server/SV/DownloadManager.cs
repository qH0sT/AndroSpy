using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class DownloadManager : MetroFramework.Forms.MetroForm
    {
        Socket sck;
        public string ID;
        public DownloadManager(Socket socket, string ident)
        {
            InitializeComponent();
            sck = socket;
            ID = ident;
            button1.Click += button1_Click;
        }
        //
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("DOWNFILE", Encoding.UTF8.GetBytes("[VERI]" + textBox1.Text + "[VERI]" + textBox2.Text));
                sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
        }
    }
}
