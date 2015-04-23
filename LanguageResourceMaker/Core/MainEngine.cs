﻿using LanguageResourceMaker.Translator;
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
        private Action<String> pushLogAction;
        private Action<String> updateLogAction;
        private ITranslator translator;

        public MainEngine(ITranslator translator,Action<String> pushLogAction, Action<String> updateLogAction)
        {
            this.translator = translator;
            this.pushLogAction = pushLogAction;
            this.updateLogAction = updateLogAction;
        }

        public void Start()
        {
            ThreadPool.QueueUserWorkItem(target =>
                {
                    _Start();
                });
        }

        private String getToWriteLanguageText(List<String> textList)
        {
            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < textList.Count; j++)
            {
                sb.AppendLine(String.Format("{0}={1}", j + 1, textList[j]));
            }
            return sb.ToString();
        }

        private void _Start()
        {
            pushLogAction("开始");
            //所有的语言资源字典，翻译时用
            Dictionary<String, List<String>> allLanguageResourceDict = new Dictionary<string, List<string>>();

            DirectoryInfo di = new DirectoryInfo(Program.InputFolder);
            var projectFiles = di.GetFiles("*.csproj", SearchOption.AllDirectories);

            pushLogAction("搜索中...");
            for (int i = 0; i < projectFiles.Length; i++)
            {
                var projectFile = projectFiles[i];
                DirectoryInfo projectFolder = projectFile.Directory;
                updateLogAction(String.Format("正在处理第[{0}/{1}]个项目[{2}]", i + 1, projectFiles.Length, projectFolder.Name));
                //处理视图文件
                DirectoryInfo viewDi = new DirectoryInfo(Path.Combine(projectFolder.FullName, "View"));
                if (viewDi.Exists)
                {
                    foreach (var viewFile in viewDi.GetFiles("*.xaml"))
                    {
                        List<String> textList = new List<string>();

                        String xamlContent = File.ReadAllText(viewFile.FullName);
                        Boolean isContentChanged = false;

                        //"(?'value'[^"]*?[\u4E00-\u9FA5]+[^"]*?)"
                        Regex regex = new Regex("\"(?'value'[^\"]*?[\u4E00-\u9FA5]+[^\"]*?)\"");
                        xamlContent = regex.Replace(xamlContent, match =>
                            {
                                var valueGroup = match.Groups["value"];
                                if (!valueGroup.Success)
                                    return match.Value;
                                String value = valueGroup.Value;
                                if (value.Contains("<!--"))
                                    return match.Value;

                                if (value.StartsWith("{}"))
                                    value = value.Substring(2);
                                else
                                    isContentChanged = true;
                                textList.Add(value);
                                return String.Format("\"{0}\"", "{}" + value);
                            });
                        if (textList.Count == 0)
                            continue;


                        if (isContentChanged)
                            File.WriteAllText(viewFile.FullName, xamlContent, Encoding.UTF8);

                        //输出到语言文件
                        String outputFolder = null;
                        if (String.IsNullOrEmpty(Program.OutputFolder))
                            outputFolder = projectFolder.FullName;
                        else
                            outputFolder = Program.OutputFolder;

                        String abstractOutputFolder = Path.Combine(outputFolder, projectFolder.Name, "Language", "{0}");
                        String abstractOutputFileName = Path.GetFileNameWithoutExtension(viewFile.Name) + ".txt";

                        allLanguageResourceDict.Add(Path.Combine(abstractOutputFolder, abstractOutputFileName), textList);
                        //写默认语言资源到文件
                        outputFolder = String.Format(abstractOutputFolder, Thread.CurrentThread.CurrentCulture.Name);
                        if (!Directory.Exists(outputFolder))
                            Directory.CreateDirectory(outputFolder);
                        String languageFileName = Path.Combine(outputFolder, abstractOutputFileName);
                        File.WriteAllText(languageFileName, getToWriteLanguageText(textList), Encoding.UTF8);
                    }
                }
                //处理cs文件

            }

            if (Program.AutoTranslate && Program.TranslateTarget != null && Program.TranslateTarget.Length > 0)
            {
                pushLogAction("开始翻译");
                pushLogAction("翻译中。。。");
                foreach (String language in Program.TranslateTarget)
                {
                    String[] allLanguageResourceDictKeys = allLanguageResourceDict.Keys.ToArray();
                    for (int j = 0; j < allLanguageResourceDictKeys.Length; j++)
                    {
                        String abstractFileName = allLanguageResourceDictKeys[j];
                        String newFullFileName = String.Format(abstractFileName, language);

                        List<String> textList = allLanguageResourceDict[abstractFileName];
                        List<String> newList = new List<string>();

                        foreach (String text in textList)
                        {
                            updateLogAction(String.Format("正在翻译第[{0}/{1}]个语言文件[{2}]", j + 1, allLanguageResourceDictKeys.Length, newFullFileName));
                            String newText = null;
                            do
                            {
                                newText = translator.Translate(Thread.CurrentThread.CurrentCulture.Name, language, text);
                                if (newText == null)
                                {
                                    Thread.Sleep(5 * 1000);
                                    pushLogAction(String.Format("翻译[{0}]中的[{1}]为[{2}]时失败，5秒后重试！", abstractFileName, text, language));
                                }
                            } while (newText == null);
                            newList.Add(newText);
                        }
                        String newFullFileFolderName = Path.GetDirectoryName(newFullFileName);
                        if (!Directory.Exists(newFullFileFolderName))
                            Directory.CreateDirectory(newFullFileFolderName);
                        File.WriteAllText(newFullFileName, getToWriteLanguageText(newList), Encoding.UTF8);
                    }
                }
            }
            pushLogAction("处理完成");
        }
    }
}
