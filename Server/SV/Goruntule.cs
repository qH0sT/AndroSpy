using System.Windows.Forms;

namespace SV
{
    public partial class Goruntule : MetroFramework.Forms.MetroForm
    {
        public Goruntule(string baslik, string icerik)
        {
            InitializeComponent();
            Text = "You are reading: " + baslik;
            richTextBox1.Text = icerik;
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try { System.Diagnostics.Process.Start(e.LinkText); } catch { }
        }
    }
}
