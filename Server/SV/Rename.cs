using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class Rename : MetroFramework.Forms.MetroForm
    {
        Socket cl = default;
        string pathfile = default;
        bool isfolder = false;
        string temp = default;
        public Rename(Socket client, string name, string pth, bool fldr)
        {
            InitializeComponent();
            cl = client;
            isfolder = fldr;
            metroTextBox1.Text = name;
            temp = name;
            pathfile = pth;
            MaximizeBox = false;
            MinimizeBox = false;
            Text += " - " + name;
        }

        private void metroTile1_Click(object sender, EventArgs e)
        {
            if (metroTextBox1.Text != string.Empty && metroTextBox1.Text != temp)
            {
                metroTextBox1.Text = metroTextBox1.Text.Replace("<","_")
                    .Replace(">","_").Replace("/","_").Replace(@"\","_").Replace("*","_")
                    .Replace("?","_").Replace('"'.ToString(),"_").Replace("|","_").Replace(":","_");
                /*
                if(temp.Contains(".") && !metroTextBox1.Text.Contains("."))
                {
                    MessageBox.Show(this,"You must write an extension, because you are renaming a file.",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!temp.Contains(".") && metroTextBox1.Text.Contains("."))
                {
                    MessageBox.Show(this, "You can't write an extension, because you are renaming a folder.",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                */
                try
                {
                    byte[] senddata = Form1.MyDataPacker("RENAME", Encoding.UTF8.GetBytes(metroTextBox1.Text
                        + "[VERI]" + pathfile + "[VERI]" + isfolder.ToString()));
                    cl.Send(senddata, 0, senddata.Length, SocketFlags.None);
                }
                catch (Exception) { }               
            }
            Close();
        }
    }
}
