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
            if (MainActivity.mVirtualDisplay != null) MainActivity.mVirtualDisplay.Release();
            if (MainActivity.mImageReader != null) MainActivity.mImageReader.SetOnImageAvailableListener(null, null);
            if (MainActivity.mOrientationChangeCallback != null) MainActivity.mOrientationChangeCallback.Disable();
            MainActivity.sMediaProjection.UnregisterCallback(this);
        }
    }
}