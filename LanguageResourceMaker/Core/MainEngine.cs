using LanguageResourceMaker.Core.FileHandlers;
using LanguageResourceMaker.Translator;
using LanguageResourceMaker.Utils;
using Quick.MVVM.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace LanguageResourceMaker.Core
{
    public class MainEngine
    {
        private MainEngineConfig config;
        private Dictionary<String, IFileHandler> fileHandlerDict = new Dictionary<string, IFileHandler>();
        private List<String> allLanguageFileList = new List<String>();

        public MainEngine(MainEngineConfig config)
        {
            this.config = config;
            fileHandlerDict.Add("*.xaml", new XamlFileHandler(config) { OutputLanguageFileAction = outputLanguageFile });
            fileHandlerDict.Add("*.cs", new CsFileHandler() { OutputLanguageFileAction = outputLanguageFile });
        }

        public void Start()
        {
            ThreadPool.QueueUserWorkItem(target =>
                {
                    _Start();
                });
        }

        private void outputLanguageFile(String outputFileNameWithoutExtension, DirectoryInfo projectFolder, Dictionary<String, String> textDict, String language)
        {
            //输出到语言文件
            String outputFolder = null;
            if (String.IsNullOrEmpty(config.OutputFolder))
                outputFolder = Path.Combine(projectFolder.FullName, "View");
            else
                outputFolder = Path.Combine(config.OutputFolder, projectFolder.Name);

            String abstractOutputFolder = Path.Combine(outputFolder, "Language", "{0}");
            String abstractOutputFileName = outputFileNameWithoutExtension + ".txt";

            //写默认语言资源到文件
            outputFolder = String.Format(abstractOutputFolder, language);
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);
            String languageFileName = Path.Combine(outputFolder, abstractOutputFileName);
            File.WriteAllText(languageFileName, LanguageUtils.GetToWriteLanguageText(textDict), Encoding.UTF8);
            allLanguageFileList.Add(languageFileName);
        }

        private void _Start()
        {
            config.PushLogAction("开始");

            DirectoryInfo di = new DirectoryInfo(config.InputFolder);
            var projectFiles = di.GetFiles("*.csproj", SearchOption.AllDirectories);

            config.PushLogAction("搜索中...");
            for (int i = 0; i < projectFiles.Length; i++)
            {
                var projectFile = projectFiles[i];
                DirectoryInfo projectFolder = projectFile.Directory;
                config.UpdateLogAction(String.Format("正在处理第[{0}/{1}]个项目[{2}]", i + 1, projectFiles.Length, projectFolder.Name));
                if (config.ExtractLanguageResource)
                {
                    foreach (String searchPattern in fileHandlerDict.Keys)
                    {
                        IFileHandler fileHandler = fileHandlerDict[searchPattern];
                        DirectoryInfo fileFolder = new DirectoryInfo(Path.Combine(projectFolder.FullName, fileHandler.GetFolderPath()));
                        if (!fileFolder.Exists)
                            continue;
                        foreach (var viewFile in fileFolder.GetFiles(searchPattern, SearchOption.AllDirectories))
                            fileHandler.Handle(viewFile, projectFolder);
                    }
                }
            }

            if (config.AutoTranslate && config.TranslateTarget != null && config.TranslateTarget.Length > 0)
            {
                config.PushLogAction("开始翻译");
                config.PushLogAction("翻译中。。。");
                foreach (String language in config.TranslateTarget)
                {
                    for (int j = 0; j < allLanguageFileList.Count; j++)
                    {
                        String languageFile = allLanguageFileList[j];
                        String languageFileContent = File.ReadAllText(languageFile);
                        Dictionary<Int32, String> languageDict = ResourceUtils.GetLanguageResourceDictionary(languageFileContent);
                        String newFullFileName = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(languageFile)),
                            language, Path.GetFileName(languageFile));
                        
                        Dictionary<String, String> textDict = new Dictionary<string, string>();
                        foreach (Int32 index in languageDict.Keys)
                        {
                            String text = languageDict[index];
                            config.UpdateLogAction(String.Format("正在翻译第[{0}/{1}]个语言文件[{2}]为[{3}]", j + 1, allLanguageFileList.Count, newFullFileName, language));
                            String newText = null;
                            do
                            {
                                newText = config.Translator.Translate(Thread.CurrentThread.CurrentCulture.Name, language, text);
                                if (newText == null)
                                {
                                    config.UpdateLogAction(String.Format("翻译[{0}]中的[{1}]为[{2}]时失败！", languageFile, text, language));
                                    config.PushLogAction("");
                                    for (int s = 5; s > 0; s--)
                                    {
                                        config.UpdateLogAction(String.Format("{0}秒后重试...", s));
                                        Thread.Sleep(1000);
                                    }
                                }
                            } while (newText == null);
                            textDict.Add(index.ToString(), newText);
                        }
                        String newFullFileFolderName = Path.GetDirectoryName(newFullFileName);
                        if (!Directory.Exists(newFullFileFolderName))
                            Directory.CreateDirectory(newFullFileFolderName);
                        File.WriteAllText(newFullFileName, LanguageUtils.GetToWriteLanguageText(textDict), Encoding.UTF8);
                    }
                }
            }
            config.PushLogAction("处理完成");
        }
    }
}
