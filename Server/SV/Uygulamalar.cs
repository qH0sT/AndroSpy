using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class Uygulamalar : MetroFramework.Forms.MetroForm
    {
        Socket socket; public string ID = "";
        List<ListViewItem> lvitem = new List<ListViewItem>();
        public Uygulamalar(Socket sck, string aydi)
        {
            InitializeComponent();
            socket = sck; ID = aydi;
        }
        public void bilgileriIsle(string arg)
        {
            listView1.Items.Clear();
            lvitem.Clear();

            string[] ana_Veriler_ = arg.Split(new[] { "[APPDATA]" }, StringSplitOptions.None);
            for (int k = 0; k < ana_Veriler_.Length; k++)
            {
                try
                {
                    string[] bilgiler = ana_Veriler_[k].Split(new[] { "[VERI]" }, StringSplitOptions.None);
                    ListViewItem item = new ListViewItem(bilgiler[0]);
                    item.SubItems.Add(bilgiler[1]);
                    if (bilgiler[2] != "[NULL]")
                    {
                        try
                        {
                            if (!ımageList1.Images.ContainsKey(bilgiler[1]))
                            {
                                ımageList1.Images.Add(bilgiler[1],
                                    (Image)new ImageConverter().ConvertFrom(Convert.FromBase64String(StringCompressor.DecompressString(bilgiler[2]))));
                            }
                            item.ImageKey = bilgiler[1];
                        }
                        catch (Exception) { }
                    }
                    else
                    {
                        item.ImageKey = "icon_default.png";
                    }
                    lvitem.Add(item);
                }
                catch (Exception) { }
            }
            listView1.Items.AddRange(lvitem.ToArray());
            metroLabel1.Text = "Items: " + listView1.Items.Count.ToString();
        }
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("APPLICATIONS", Encoding.UTF8.GetBytes("ECHO"));
                socket.Send(senddata, 0, senddata.Length, SocketFlags.None);
            }
            catch (Exception) { }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listView1.SelectedItems)
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("OPENAPP", Encoding.UTF8.GetBytes(lvi.SubItems[1].Text));
                    socket.Send(senddata, 0, senddata.Length, SocketFlags.None);
                }
                catch (Exception) { }
                System.Threading.Tasks.Task.Delay(5);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            metroLabel2.Text = "Selected: " + listView1.SelectedItems.Count.ToString();
        }
    }
}
