using Android.Content;
using Android.Views;
using System;

namespace Task2
{
    public class OrientationChangeCallback : OrientationEventListener
    {
        public OrientationChangeCallback(Context context) : base(context)
        {

        }
        public override void OnOrientationChanged(int orientation)
        {
            int rotation = (int)ForegroundService.mDisplay.Rotation;
            if (rotation != ForegroundService.mRotation)
            {
                ForegroundService.mRotation = rotation;
                try
                {
                    // clean up
                    if (ForegroundService.mVirtualDisplay != null) ForegroundService.mVirtualDisplay.Release();
                    if (ForegroundService.mImageReader != null) ForegroundService.mImageReader.SetOnImageAvailableListener(null, null);

                    // re-create virtual display depending on device width / height
                    ForegroundService._globalService.createVirtualDisplay();
                }
                catch (Exception)
                {

                }
            }
        }
    }
}