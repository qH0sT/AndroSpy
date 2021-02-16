using System;
using Android.Content;
using Android.Views;

namespace Task2
{
    public class OrientationChangeCallback : OrientationEventListener
    {
        public OrientationChangeCallback(Context context) : base(context)
        {

        }
        public override void OnOrientationChanged(int orientation)
        {
            int rotation = (int)MainActivity.mDisplay.Rotation;
            if (rotation != MainActivity.mRotation)
            {
                MainActivity.mRotation = rotation;
                try
                {
                    // clean up
                    if (MainActivity.mVirtualDisplay != null) MainActivity.mVirtualDisplay.Release();
                    if (MainActivity.mImageReader != null) MainActivity.mImageReader.SetOnImageAvailableListener(null, null);

                    // re-create virtual display depending on device width / height
                    ((MainActivity)MainActivity.global_activity).createVirtualDisplay();
                }
                catch (Exception)
                {

                }
            }
        }
    }
}