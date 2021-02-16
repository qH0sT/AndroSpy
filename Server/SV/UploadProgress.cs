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
        Form1.infoAl infoclas = default;
        public UploadProgress(Socket st, Form1.infoAl info, string fname, string ID)
        {
            InitializeComponent();
            client = st;
            infoclas = info;
            Aydi = ID;
            DosyaGonder(fname);
        }
        public async void DosyaGonder(string ayir)
        {
            await Task.Run(async () =>
            {
                try
                {
                    MemoryStream ms = new MemoryStream();
                    byte[] dosya = File.ReadAllBytes(ayir);

                    using (MemoryStream from = new MemoryStream(dosya))
                    {
                        int readCount;
                        byte[] buffer = new byte[4096];
                        while ((readCount = from.Read(buffer, 0, 4096)) != 0)
                        {
                            try
                            {
                                ms.Write(buffer, 0, readCount);
                                byte[] pack = Form1.MyDataPacker("UPLOADING", ms.ToArray());
                                client.Send(pack, 0, pack.Length, SocketFlags.None);
                                ms.Flush(); ms.Close(); ms.Dispose(); ms = new MemoryStream();
                                await Task.Delay(25); //reduce high cpu usage and lighten socket traffic.
                            }
                            catch (Exception) { break; }
                        }

                    }
                    if (ms != null) { ms.Flush(); ms.Close(); ms.Dispose(); }
                    dosya = new byte[] { };
                }
                catch (Exception)
                {

                }
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
