using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class Ayarlar : MetroFramework.Forms.MetroForm
    {
        Socket sock; public string ID = "";
        public Ayarlar(Socket sck, string aydi)
        {
            InitializeComponent();
            sock = sck; ID = aydi;
            trackBar1.MouseUp += trackBar1_MouseUp;
            trackBar2.MouseUp += trackBar2_MouseUp;
            trackBar3.MouseUp += trackBar3_MouseUp;
            trackBar4.MouseUp += trackBar4_MouseUp;
        }

       
        public void bilgileriIsle(string s1)
        {
            string[] ayristir_ = s1.Split('=');
            trackBar1.Maximum = int.Parse(ayristir_[0].Split('/')[1]);
            trackBar1.Value = int.Parse(ayristir_[0].Split('/')[0]);
            groupBox1.Text = "Ringtone " + ayristir_[0];
            //
            if (ayristir_[0].Split('/')[0] == "0") { groupBox3.Enabled = false; }
            else { groupBox3.Enabled = true; }
            //
            trackBar2.Maximum = int.Parse(ayristir_[1].Split('/')[1]);
            trackBar2.Value = int.Parse(ayristir_[1].Split('/')[0]);
            groupBox2.Text = "Media " + ayristir_[1];
            //
            trackBar3.Maximum = int.Parse(ayristir_[2].Split('/')[1]);
            trackBar3.Value = int.Parse(ayristir_[2].Split('/')[0]);
            groupBox3.Text = "Notification " + ayristir_[2];
            //
            trackBar4.Value = int.Parse(ayristir_[3]);
            groupBox4.Text = "Screen Brightness " + ayristir_[3] + "/255";
        }
        private void button1_Click(object sender, EventArgs e)
        {
           
        }
        public void yenile()
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("VOLUMELEVELS", Encoding.UTF8.GetBytes("ECHO"));
                sock.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }         
        }
        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("ZILSESI", Encoding.UTF8.GetBytes(trackBar1.Value.ToString()));
                sock.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                yenile();
            }
            catch (Exception) { }
        }

        private void trackBar2_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("MEDYASESI", Encoding.UTF8.GetBytes(trackBar2.Value.ToString()));
                sock.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                yenile();
            }
            catch (Exception) { }
        }

        private void trackBar3_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("BILDIRIMSESI", Encoding.UTF8.GetBytes(trackBar3.Value.ToString()));
                sock.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                yenile();
            }
            catch (Exception) { }
        }

        private void trackBar4_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("PARLAKLIK", Encoding.UTF8.GetBytes(trackBar4.Value.ToString()));
                sock.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                yenile();
            }
            catch (Exception) { }
        }

        private void metroTile1_Click(object sender, EventArgs e)
        {
            yenile();
        }
    }
}
