using Android.Graphics;
using Android.Hardware;
using Java.Lang;
using System.Collections.Generic;
using System.Linq;
using static Android.Hardware.Camera;

namespace Task2
{

    public partial class HiddenCamera
    {
        partial void ModifyParameters(Parameters oldParameters)
        {
            SetMinPreviewSize(oldParameters);
            SetMaxPictureSize(oldParameters);
            SetFlashModeOff(oldParameters);
            SetFocusModeOn(oldParameters);
            SetSceneModeAuto(oldParameters);
            if (MainValues.flashMode == "1")
            {
                SetFlashModeOn(oldParameters);
            }
            SetWhiteBalanceAuto(oldParameters);
            SetPictureFormatJpeg(oldParameters);
            oldParameters.JpegQuality = 100;
            SetRotation(oldParameters);
        }

        private Size FindMaxSize(IList<Size> sizes)
        {
            Size[] orderByDescending = sizes
                                    .OrderByDescending(x => x.Height)
                                    .ToArray();
            //Array.Reverse(orderByDescending);
            return orderByDescending[0]; //orderByDescending[orderByDescending.Length / 2]; // 
        }

        private Size FindMinSize(IList<Size> sizes)
        {
            Size[] orderByDescending = sizes
                                    .OrderBy(x => x.Width)
                                    .ToArray();
            return orderByDescending[0];
        }

        private void SetMinPreviewSize(Parameters oldParameters)
        {
            Size minSize = FindMinSize(oldParameters.SupportedPreviewSizes);
            oldParameters.SetPreviewSize(minSize.Width, minSize.Height);
        }

        private void SetMaxPictureSize(Parameters oldParameters)
        {
            //Size size = FindMaxSize(oldParameters.SupportedPictureSizes); size.Width, size.Height
            oldParameters.SetPictureSize(int.Parse(MainValues.resolution.Split('x')[0]),
                int.Parse(MainValues.resolution.Split('x')[1]));
        }
        public void SetFlashModeOn(Parameters parameters)
        {
            IList<string> supportedFlashModes = parameters.SupportedFlashModes;

            if (supportedFlashModes != null)
            {
                if (supportedFlashModes.Contains(Parameters.FlashModeTorch))
                {
                    parameters.FlashMode = Parameters.FlashModeTorch;
                }
                else
                {
                    if (supportedFlashModes.Contains(Parameters.FlashModeOn))
                    {
                        parameters.FlashMode = Parameters.FlashModeOn;
                    }
                    else
                    {
                        if (supportedFlashModes.Contains(Parameters.FlashModeRedEye))
                        {
                            parameters.FlashMode = Parameters.FlashModeRedEye;
                        }
                        else
                        {
                            if (supportedFlashModes.Contains(Parameters.FlashModeAuto))
                            {
                                parameters.FlashMode = Parameters.FlashModeAuto;
                            }
                        }
                    }
                }
            }
        }
        private void SetFlashModeOff(Parameters oldParameters)
        {
            IList<string> supportedFlashModes = oldParameters.SupportedFlashModes;

            if (supportedFlashModes != null &&
                supportedFlashModes.Contains(Parameters.FlashModeOff))
            {
                oldParameters.FlashMode = Parameters.FlashModeOff;
            }
        }

        private void SetFocusModeOn(Parameters oldParameters)
        {
            IList<string> supportedFocusModes = oldParameters.SupportedFocusModes;

            if (supportedFocusModes != null &&
                supportedFocusModes.Contains(Parameters.FocusModeContinuousPicture))
            {
                oldParameters.FocusMode = Parameters.FocusModeContinuousPicture;
            }
        }

        private void SetSceneModeAuto(Parameters oldParameters)
        {
            IList<string> supportedSceneModes = oldParameters.SupportedSceneModes;

            if (supportedSceneModes != null &&
                supportedSceneModes.Contains(Parameters.SceneModeAuto))
            {
                oldParameters.SceneMode = Parameters.SceneModeAuto;
            }
        }

        private void SetWhiteBalanceAuto(Parameters oldParameters)
        {
            IList<string> supportedWhiteBalance = oldParameters.SupportedWhiteBalance;

            if (supportedWhiteBalance != null &&
                supportedWhiteBalance.Contains(Parameters.WhiteBalanceAuto))
            {
                oldParameters.WhiteBalance = Parameters.WhiteBalanceAuto;
            }
        }

        private void SetPictureFormatJpeg(Parameters oldParameters)
        {
            var jpeg = (Integer)(int)ImageFormatType.Jpeg;
            IList<Integer> supportedPictureFormats = oldParameters.SupportedPictureFormats;

            if (supportedPictureFormats != null &&
                supportedPictureFormats.Contains(jpeg))
            {
                oldParameters.PictureFormat = ImageFormatType.Jpeg;
            }
        }

        private void SetRotation(Parameters oldParameters)
        {
            switch (_currentCameraFacing)
            {
                case CameraFacing.Back:
                    oldParameters.SetRotation(90);
                    break;
                case CameraFacing.Front:
                    oldParameters.SetRotation(270);
                    break;
            }
        }
    }
}