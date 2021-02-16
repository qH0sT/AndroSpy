using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Hardware;
using Android.Hardware.Camera2;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Task2
{
    
    public class CameraInfo
    {
        CameraManager _cameraManager;
        Camera.CameraInfo _info;

        public CameraInfo(CameraManager cameraManager)
        {
            _cameraManager = cameraManager;
            _info = new Camera.CameraInfo();
        }

        public CameraFacing GetCameraFacing(int cameraID)
        {
            Camera.GetCameraInfo(cameraID, _info);
            return _info.Facing;
        }

        public int NumberOfCameras()
            => _cameraManager.GetCameraIdList().Length;

        public int[] GetCameraIdArray()
        {
            string[] IDs = _cameraManager.GetCameraIdList();
            var array = new int[IDs.Length];

            for (int i = 0; i < IDs.Length; i++)
                array[i] = int.Parse(IDs[i]);

            return array;
        }
    }
}