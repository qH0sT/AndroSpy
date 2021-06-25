using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SV
{
    public partial class UploadProgress : MetroFramework.Forms.MetroForm
    {
        public string Aydi = default;
        Socket client = default;
        Form1.ClientSession infoclas = default;
        public UploadProgress(Socket st, Form1.ClientSession info, string fname, string ID)
        {
            InitializeComponent();
            client = st;
            infoclas = info;
            Aydi = ID;
            DosyaGonder(fname);
        }
        public async void DosyaGonder(string ayir)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (File.Exists(ayir))
                    {
                        lock (ayir)
                        {
                            using (FileStream from = new FileStream(ayir, FileMode.Open, FileAccess.Read))
                            {
                                int readCount;
                                byte[] buffer = new byte[4096];
                                while ((readCount = from.Read(buffer, 0, 4096)) != 0)
                                {
                                    try
                                    {
                                        client.Send(buffer, 0, readCount, SocketFlags.None);
                                        Task.Delay(25).Wait(); //reduce high cpu usage and lighten socket traffic.
                                    }
                                    catch (Exception) { break; }
                                }
                            }
                        }
                    }
                }
                catch (Exception) { }
            });
        }
        private void UploadProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (infoclas != null)
            {

                try
                {
                    infoclas.CloseSocks();
                }
                catch { }
            }
        }
    }
}
