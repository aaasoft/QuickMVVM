using LanguageResourceMaker.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace LanguageResourceMaker.Core.FileHandlers
{
    public class XamlFileHandler : AbstractFileHandler
    {
        //第一次："(?'value'[^"]*?[\u4E00-\u9FA5]+[^"]*?)"
        //第二次："(?'value'[^"|\n]*?[\u4E00-\u9FA5]+[^"|\n]*?)"
        private Regex regex = new Regex("\"(?'value'[^\"|\\n]*?[\u4E00-\u9FA5]+[^\"|\\n]*?)\"");
        private MainEngineConfig config;

        public XamlFileHandler(MainEngineConfig config)
        {
            this.config = config;
        }

        public override string GetFolderPath()
        {
            return "View";
        }

        public override void Handle(FileInfo viewFile, DirectoryInfo projectFolder)
        {
            Dictionary<String, String> textDict = new Dictionary<string, string>();

            String xamlContent = File.ReadAllText(viewFile.FullName);
            Boolean isContentChanged = false;

            Int32 index = 1;
            xamlContent = regex.Replace(xamlContent, match =>
            {
                var valueGroup = match.Groups["value"];
                if (!valueGroup.Success)
                    return match.Value;
                String value = valueGroup.Value;

                if (value.StartsWith("{}"))
                    value = value.Substring(2);
                else
                    isContentChanged = true;
                textDict.Add(index.ToString(), value);
                index++;
                return String.Format("\"{0}\"", "{}" + value);
            });
            if (textDict.Count == 0)
                return;

            //如果允许修改XAML文件并且内容发生了变化
            if (config.AllowModifyXamlFile && isContentChanged)
                File.WriteAllText(viewFile.FullName, xamlContent, Encoding.UTF8);

            OutputLanguageFileAction(Path.GetFileNameWithoutExtension(viewFile.Name), projectFolder, textDict, Thread.CurrentThread.CurrentCulture.Name);
        }
    }
}
