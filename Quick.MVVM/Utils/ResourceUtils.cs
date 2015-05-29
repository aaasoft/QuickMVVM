using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Quick.MVVM.Utils
{
    public class ResourceUtils
    {
        //搜索文件
        private static String findFilePath(String baseFolder, String fileName)
        {
            String fullFileName = Path.Combine(baseFolder, fileName);
            if (File.Exists(fullFileName))
                return fullFileName;

            String[] nameArray = fileName.Split(new Char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < nameArray.Length; i++)
            {
                String folderName = String.Join(".", nameArray, 0, i);
                String fullFolderPath = Path.Combine(baseFolder, folderName);
                if (!Directory.Exists(fullFolderPath))
                    continue;
                String subFileName = String.Join(".", nameArray, i, nameArray.Length - i);

                fullFileName = findFilePath(fullFolderPath, subFileName);
                if (fullFileName != null)
                    return fullFileName;
            }
            return null;
        }

        /// <summary>
        /// 获取资源
        /// </summary>
        /// <param name="fileNameList"></param>
        /// <param name="assembly"></param>
        /// <param name="baseFolder"></param>
        /// <param name="fullFileNameTemplate"></param>
        /// <param name="templateParams"></param>
        /// <returns></returns>
        public static Stream GetResource(List<String> fileNameList, Assembly assembly, String baseFolder, String fullFileNameTemplate, params Object[] templateParams)
        {
            //文件是否存在
            Boolean isFileExists = false;
            String filePath = null;
            foreach (String fileName in fileNameList)
            {
                //先判断全名文件是否存在
                filePath = findFilePath(baseFolder, fileName);
                isFileExists = filePath != null;
                if (isFileExists)
                    break;
            }

            //先尝试从目录加载
            if (isFileExists)
            {
                return File.Open(filePath, FileMode.Open);
            }
            //然后尝试从程序集资源中加载
            else
            {
                foreach (String fileName in fileNameList)
                {
                    //"{0}.[ThemePathInAssembly].{1}"
                    String resourceName = String.Format(fullFileNameTemplate, templateParams);
                    resourceName = resourceName.Replace("[fileName]", fileName);
                    resourceName = resourceName.Replace("-", "_");
                    Stream resourceStream = assembly.GetManifestResourceStream(resourceName);
                    if (resourceStream != null)
                        return resourceStream;
                }
            }
            return null;
        }

        public static String GetResourceText(List<String> fileNameList, Assembly assembly, String baseFolder, String fullFileNameTemplate, params Object[] templateParams)
        {
            Stream resourceStream = GetResource(fileNameList, assembly, baseFolder, fullFileNameTemplate, templateParams);
            if (resourceStream == null)
                return null;

            using (resourceStream)
            {
                String fileContent = null;
                StreamReader streamReader = new StreamReader(resourceStream);
                fileContent = streamReader.ReadToEnd();
                streamReader.Close();
                resourceStream.Close();
                return fileContent;
            }
        }

        /// <summary>
        /// 获取语言资源字典
        /// </summary>
        /// <param name="languageContent"></param>
        /// <returns></returns>
        public static Dictionary<Int32, String> GetLanguageResourceDictionary(String languageContent)
        {
            Dictionary<Int32, String> languageDict = new Dictionary<int, string>();

            //(?'index'\d+)\s*=(?'value'.+)
            Regex regex = new Regex(@"(?'index'\d+)\s*=(?'value'.+)");
            MatchCollection languageMatchCollection = regex.Matches(languageContent);
            foreach (Match match in languageMatchCollection)
            {
                var indexGroup = match.Groups["index"];
                var valueGroup = match.Groups["value"];

                if (!indexGroup.Success || !valueGroup.Success)
                    continue;
                Int32 key = Int32.Parse(indexGroup.Value);
                String value = valueGroup.Value;
                if (value.EndsWith("\r"))
                    value = value.Substring(0, value.Length - 1);
                if (languageDict.ContainsKey(key))
                    languageDict.Remove(key);
                languageDict.Add(key, value);
            }
            return languageDict;
        }
    }
}
