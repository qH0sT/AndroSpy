using System;
using System.Net.Sockets;
using System.Text;

namespace SV
{
    public partial class EditFile : MetroFramework.Forms.MetroForm
    {
        Socket client = default;
        public string ID = default;
        public string path = default;
        public EditFile(Socket cl, string id)
        {
            InitializeComponent();
            client = cl;
            ID = id;
        }
        // KAÇ SATIR KAÇ KELİME FİLAN EKLE.
        private void metroTile1_Click(object sender, System.EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("WRITEFILE", Encoding.UTF8.GetBytes(richTextBox1.Text), path);
                client.Send(senddata, 0, senddata.Length, SocketFlags.None);
            }
            catch (Exception) { }
            Close();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            metroLabel1.Text = "Lines: " + richTextBox1.Lines.Length.ToString();
            metroLabel2.Text = "Chars: " + richTextBox1.Text.Length.ToString();
        }

        private void richTextBox1_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
    }
}
