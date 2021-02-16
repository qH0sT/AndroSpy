using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class Yuzde : MetroFramework.Forms.MetroForm
    {
        Form1.infoAl infoclas = default;
        public string dosyaAdi = "";
        public FileStream fs = default;
        Socket client = default;
        public Yuzde(string filename, Socket st, Form1.infoAl info)
        {
            InitializeComponent();
            client = st;
            infoclas = info;
            dosyaAdi = filename;
            if (File.Exists(dosyaAdi.Split('|')[1] + "\\" + dosyaAdi.Split('|')[0]))
            {
                File.Delete(dosyaAdi.Split('|')[1] + "\\" + dosyaAdi.Split('|')[0]);
            }
            fs = new FileStream(dosyaAdi.Split('|')[1] + "\\" + dosyaAdi.Split('|')[0], FileMode.Append);
        }

        private void Yuzde_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (infoclas != null)
            {
               
                try
                {
                    infoclas.CloseSocks();
                }
                catch { }
            }
            
            if (fs != null)
            {
                fs.Flush(); fs.Close(); fs.Dispose();
            }
            
        }
    }
}
