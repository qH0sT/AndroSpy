using Android.App;
using Android.Content;

namespace Task2
{
    [BroadcastReceiver]
    [IntentFilter(new[] { "MY_ALARM_RECEIVED" })]
    class Alarm : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action != null)
            {
                if (intent.Action == "MY_ALARM_RECEIVED")
                {
                    if (!ForegroundService._globalService.CLOSE_CONNECTION)
                    {
                        try
                        {
                            byte[] checkForConnection = ForegroundService._globalService.MyDataPacker("CHECK", System.Text.Encoding.UTF8.GetBytes("CHECK"));
                            ForegroundService.Soketimiz.Send(checkForConnection, 0, checkForConnection.Length, System.Net.Sockets.SocketFlags.None);
                        }
                        catch (System.Exception)
                        {
                            ForegroundService._globalService.StopServerSession(ForegroundService._globalService.globalMemoryStream);
                            if (!ForegroundService._globalService.CLOSE_CONNECTION)
                                ForegroundService._globalService.Baglanti_Kur();
                        }
                        ForegroundService._globalService.setAlarm(context);
                    }
                    else
                    {
                        ForegroundService._globalService.cancelAlarm(context);
                    }                    
                }
            }
            
        }
    }
}