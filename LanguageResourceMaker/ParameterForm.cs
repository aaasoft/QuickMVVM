using LanguageResourceMaker.Core;
using LanguageResourceMaker.Translator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LanguageResourceMaker
{
    public partial class ParameterForm : Form
    {
        private ITranslator translator = null;

        public ParameterForm()
        {
            InitializeComponent();
            translator = new BaiduTranslator();

            String currentLanguage = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            foreach (String language in translator.GetSupportLanguages())
            {
                //跳过中文
                if (language == currentLanguage)
                    continue;
                CultureInfo cultureInfo = new CultureInfo(language);
                var lvi = lvLanguages.Items.Add(cultureInfo.DisplayName);
                lvi.Tag = language;
            }
#if DEBUG
            txtInputFolder.Text = @"E:\工作项目\loncomip\DCIMSClient_trunk";
            txtOutputFolder.Text = @"D:\Test\QMVVM_TEST";
#endif
            //MessageBox.Show(translator.Translate("zh-CN", "ja-JP", "你好"));
        }

        private void btnSelectInput_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "选择输入目录...";
            var ret = fbd.ShowDialog();
            if (ret == System.Windows.Forms.DialogResult.Cancel)
                return;
            txtInputFolder.Text = fbd.SelectedPath;
        }

        private void btnSelectOutput_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "选择输出目录...";
            var ret = fbd.ShowDialog();
            if (ret == System.Windows.Forms.DialogResult.Cancel)
                return;
            txtOutputFolder.Text = fbd.SelectedPath;
        }

        private void cbAutoTranslate_CheckedChanged(object sender, EventArgs e)
        {
            lvLanguages.Enabled = cbAutoTranslate.Checked;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Program.InputFolder = txtInputFolder.Text.Trim();
            Program.OutputFolder = txtOutputFolder.Text.Trim();
            Program.AutoTranslate = cbAutoTranslate.Checked;
            if (Program.AutoTranslate)
            {
                List<String> list = new List<string>();
                foreach (ListViewItem lvi in lvLanguages.CheckedItems)
                {
                    list.Add(lvi.Tag.ToString());
                }
                Program.TranslateTarget = list.ToArray();
            }

            tabControl1.TabPages.Remove(tabPage1);
            MainEngine engine = new MainEngine(translator, log => pushLog(log), log => updateLog(log));
            engine.Start();
        }

        private void pushLog(String msg)
        {
            this.BeginInvoke(new Action(() =>
                {
                    txtLog.AppendText(Environment.NewLine + msg);
                    txtLog.ScrollToCaret();
                }));
        }

        private void updateLog(String msg)
        {
            this.BeginInvoke(new Action(() =>
            {
                String[] lines = txtLog.Lines;
                lines[lines.Length - 1] = msg;
                txtLog.Lines = lines;
            }));
        }

        private void ParameterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
