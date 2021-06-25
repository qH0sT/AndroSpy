using Android.Media.Projection;

namespace Task2
{
    class MediaProjectionStopCallback : MediaProjection.Callback
    {
        public override void OnStop()
        {
            run();
            base.OnStop();
        }
        public void run()
        {
            if (ForegroundService.mVirtualDisplay != null) ForegroundService.mVirtualDisplay.Release();
            if (ForegroundService.mImageReader != null) ForegroundService.mImageReader.SetOnImageAvailableListener(null, null);
            if (ForegroundService.mOrientationChangeCallback != null) ForegroundService.mOrientationChangeCallback.Disable();
            ForegroundService.sMediaProjection.UnregisterCallback(this);
        }
    }
}