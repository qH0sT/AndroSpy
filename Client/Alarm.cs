using Android.Content;
using Android.OS;

namespace Task2
{
    [BroadcastReceiver]
    class Alarm : BroadcastReceiver
    {
        public static Alarm glob_alarm;
        public PowerManager.WakeLock wakelock = null;
        private PowerManager pmanager = null;
        public override void OnReceive(Context context, Intent intent)
        {
            glob_alarm = this;
            if (MainActivity.mySocketConnected == false)
            {
                pmanager = (PowerManager)context.GetSystemService("power");
                wakelock = pmanager.NewWakeLock(WakeLockFlags.Partial, GetType().Name);
                wakelock.SetReferenceCounted(false);
                if (wakelock.IsHeld == false)
                {
                    wakelock.Acquire();
                }
                ((MainActivity)MainActivity.global_activity).cancelAlarm(context);
                ((MainActivity)MainActivity.global_activity).Baglanti_Kur();
                
            }
        }
    }
}