using Android.App;
using Android.Content;
using Android.OS;
using System;

namespace Task2
{
    [BroadcastReceiver(DirectBootAware = true, Enabled = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted }, Categories =
        new[] { "android.intent.category.DEFAULT" }, Priority = (int)IntentFilterPriority.HighPriority)]
    public class BootReceiver : BroadcastReceiver
    {
        private readonly Type SERVICE_TYPE = typeof(ForegroundService);
        private Intent _startServiceIntent;
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action != null)
            {
                if (intent.Action.Equals("android.intent.action.BOOT_COMPLETED"))
                {
                    StartForegroundServiceCompat<ForegroundService>(context);
                }
            }
        }
        private Intent GetIntent(Type type, string action, Context cntXt)
        {
            var intent = new Intent(cntXt, type);
            intent.SetAction(action);
            return intent;
        }

        public void StartForegroundServiceCompat<T>(Context context, Bundle args = null) where T : Service
        {
            _startServiceIntent = GetIntent(SERVICE_TYPE, MainValues.ACTION_START_SERVICE, context);
            if (args != null)
                _startServiceIntent.PutExtras(args);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                context.StartForegroundService(_startServiceIntent);
            else
                context.StartService(_startServiceIntent);
        }
    }
}