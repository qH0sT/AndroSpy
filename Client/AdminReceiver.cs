using Android.App;
using Android.App.Admin;
using Android.Content;

namespace Task2
{
    [BroadcastReceiver(Permission = "android.permission.BIND_DEVICE_ADMIN",
		Name = "izci.AdminReceiver")]
	[MetaData("android.app.device_admin", Resource = "@layout/admin")]
	[IntentFilter(new[] { "android.app.action.DEVICE_ADMIN_ENABLED", Intent.ActionMain })]
	public class AdminReceiver : DeviceAdminReceiver
	{
	}
}