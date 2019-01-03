using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using tools.Dev;
using System.IO;
namespace tools
{
    public partial class Form1 : Form
    {

        private string SelectedLeague = string.Empty;
        private Form2 form2;
        private IPlugin Plugin;
        public string NinjaDirectory = ".//NinjaData//";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Show();
            // Make folder if it doesnt exist
            var file = new FileInfo(NinjaDirectory);
            file.Directory?.Create(); // If the directory already exists, this method does nothing.

            string[] leagues = null;
            try
            {
                leagues = Api.GatherLeagueNames();
            }
            catch (System.Net.WebException ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            this.comboBox1.Items.Add("不启用");
            foreach (var v in leagues)
            {
                if (!string.IsNullOrEmpty(v))
                    this.comboBox1.Items.Add(v);
            }
            //使用赛季名称做为约束条件，预防赛季变更带来的问题
            this.comboBox1.SelectedIndex = this.comboBox1.Items.IndexOf(Properties.Settings.Default.League);

            this.comboBox2.Items.Add("简体中文");
            //this.comboBox2.Items.Add("English");
            //this.comboBox2.Items.Add("繁體中文");
            this.comboBox2.SelectedIndex = Properties.Settings.Default.Language;

            this.checkBox_ClipValue.Checked = Properties.Settings.Default.Clipboard;

            this.form2 = new Form2();
            Helper.AddClipboardFormatListener(this.Handle);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Clipboard = this.checkBox_ClipValue.Checked;
            Properties.Settings.Default.Save();

            Helper.RemoveClipboardFormatListener(this.Handle);

            if(form2!=null && !form2.IsDisposed)
                form2.Close();
        }

        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == Helper.WM_CLIPBOARDUPDATE)
            {
                if (Clipboard.ContainsText())
                {
                    string text = Clipboard.GetText();
                    UpdateClipValue(text);
                }
            }
            else
            {
                base.DefWndProc(ref m);
            }
        }

        private void UpdateClipValue(string text)
        {
            if (Plugin == null)
                return;

            //if (!Plugin.FilterClipValue(text)) return;
            //TextResult textResult = await Task.Run(() => { return Plugin.ProcessClipValue(text); });
            TextResult textResult = Plugin.ProcessClipValue(text, this.comboBox1.SelectedIndex<1);
            if (textResult != null)
            {
                if (!string.IsNullOrEmpty(textResult.textTip))
                {
                    form2.Show(textResult.textTip);
                }

                if (!string.IsNullOrEmpty(textResult.textClip) && this.checkBox_ClipValue.Checked)
                {
                    //当有多个进程或线程同时操作剪贴板时，将会冲突出错。
                    try
                    {
                        Clipboard.SetText(textResult.textClip);
                    }
                    catch (System.Runtime.InteropServices.ExternalException)
                    {
                        try
                        {
                            Clipboard.SetDataObject(textResult.textClip, true, 3, 100);
                        }
                        catch (Exception) { }
                    }
                    catch (Exception) { }
                }
            }
        }

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedText = comboBox1.GetItemText(comboBox1.Items[comboBox1.SelectedIndex]);

            if (SelectedLeague != selectedText)
            {
                SelectedLeague = selectedText;
                comboBox1.Enabled = false;

                if(comboBox1.SelectedIndex > 0)
                    await Task.Run(() => { Api.GetJsonData(selectedText); });
                    //await Task.Run(() => { dowloadninja.GetJsonData(selectedText); });
                this.comboBox1.Enabled = true;
                Properties.Settings.Default.League = selectedText;
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox2.SelectedIndex == 0)
                Plugin = new tools.Plugins.PriceCheck_CN();
            else if (comboBox2.SelectedIndex == 1)
                Plugin = new tools.Plugins.PriceCheck_EN();
            else if (comboBox2.SelectedIndex == 2)
                Plugin = new tools.Plugins.PriceCheck_TW();

            Properties.Settings.Default.Language = comboBox2.SelectedIndex;
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
            this.notifyIcon1.Visible = false;
        }


    }




}
