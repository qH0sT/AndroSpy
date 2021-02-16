using System.Net.Sockets;

namespace Task2
{
    public class UpdateTimeTask 
    {
       public static HiddenCamera _hiddenCamera;
        public static void Kamera(Socket sck)
        {
            _hiddenCamera.TakePhoto(sck);
        }
    }
}