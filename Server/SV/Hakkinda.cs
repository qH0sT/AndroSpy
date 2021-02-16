using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SV
{
    public partial class Hakkinda : MetroFramework.Forms.MetroForm
    {
        public Hakkinda()
        {
            InitializeComponent();
        }
        string alphabet = "_-=#$()+!qwertyuıopğüa0sdfghjklşizxcvbnmöç";
        private async void Hakkinda_Load(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder("     ");
            string coder = "qh0st";
            for (int i = 0; i < coder.Length; i++)
            {
                for (int j = 0; j < alphabet.Length; j++)
                {
                    Application.DoEvents();
                    await Task.Delay(15);
                    sb[i] = (i == 0) ? alphabet[j].ToString().ToUpper().ToCharArray()[0] : alphabet[j]; 
                    label2.Text = sb.ToString();
                    if (alphabet[j].ToString().ToLower() == coder[i].ToString().ToLower())
                    {
                        break;
                    }
                }
                await Task.Delay(15);
            }
        }
    }
}
