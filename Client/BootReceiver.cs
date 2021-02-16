using Android.App;
using Android.Content;
using Android.Content.PM;
using Task2;

namespace izci
{
    [BroadcastReceiver(Exported = true, DirectBootAware = true, Enabled = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionLockedBootCompleted }, Categories = 
        new[] { "android.intent.category.DEFAULT" }, Priority = (int)IntentFilterPriority.HighPriority)]
    public class BootReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action.Equals("android.intent.action.BOOT_COMPLETED"))
            {
                try
                {
                    var pckg = context.PackageManager;
                    ComponentName componentName = new ComponentName(context, Java.Lang.Class.FromType(typeof(MainActivity)).Name);
                    pckg.SetComponentEnabledSetting(componentName, ComponentEnabledState.Enabled, ComponentEnableOption.DontKillApp);
                }
                catch (System.Exception) { }
                Intent start = new Intent(context, Java.Lang.Class.FromType(typeof(MainActivity)));
                start.AddFlags(ActivityFlags.NewTask);
                context.StartActivity(start);
            }

        }
    }
}