using Android.Media;
using Java.Interop;
using Java.Nio;
using System;
using System.Net.Sockets;

namespace Task2
{
    class ImageAvailableListener : Java.Lang.Object, ImageReader.IOnImageAvailableListener
    {
        public IntPtr Handle;

        public int JniIdentityHashCode;

        public JniObjectReference PeerReference;

        public JniPeerMembers JniPeerMembers;

        public JniManagedPeerStates JniManagedPeerState;

        
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
        public void OnImageAvailable(ImageReader reader)
        {
            Android.Media.Image image = null;

            Android.Graphics.Bitmap bitmap = null;
            try
            {
                image = reader.AcquireLatestImage();
                if (image != null)
                {
                    Image.Plane[] planes = image.GetPlanes();
                    ByteBuffer buffer = planes[0].Buffer;
                    int offset = 0;
                    int pixelStride = planes[0].PixelStride;
                    int rowStride = planes[0].RowStride;
                    int rowPadding = rowStride - pixelStride * ForegroundService.mWidth;
                    // create bitmap
                    bitmap = Android.Graphics.Bitmap.CreateBitmap(ForegroundService.mWidth + rowPadding / pixelStride, ForegroundService.mHeight, Android.Graphics.Bitmap.Config.Argb8888);
                    bitmap.CopyPixelsFromBuffer(buffer);
                    image.Close();
                    using (System.IO.MemoryStream fos = new System.IO.MemoryStream())
                    {
                        bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg, ForegroundService._globalService.kalite, fos);
                        byte[] dataPacker = ForegroundService._globalService.MyDataPacker("LIVESCREEN", StringCompressor.Compress(fos.ToArray()), ForegroundService._globalService.ID);
                        try
                        {
                            if (ForegroundService._globalService.screenSock != null)
                            {
                                ForegroundService._globalService.screenSock.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                                System.Threading.Tasks.Task.Delay(1).Wait();
                            }
                        }
                        catch (Exception) { }
                    }
                }

            }
            catch (Exception ex)
            {
                try
                {
                    byte[] dataPacker = ForegroundService._globalService.MyDataPacker("ERRORLIVESCREEN", System.Text.Encoding.UTF8.GetBytes(ex.Message));
                    ForegroundService.Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                }
                catch (Exception) { }
                ForegroundService._globalService.stopProjection();
            }
            finally
            {
                if (bitmap != null)
                {
                    bitmap.Recycle();
                }

                if (image != null)
                {
                    image.Dispose();
                }
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

        public void UnregisterFromRuntime()
        {

        }
    }
}