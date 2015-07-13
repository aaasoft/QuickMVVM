using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
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
        /// <param name="pathParts"></param>
        /// <returns></returns>
        public static Stream GetResource(List<String> fileNameList, Assembly assembly, String baseFolder, params Object[] pathParts)
        {
            String findedResourcePath;
            return GetResource(fileNameList, assembly, baseFolder, out findedResourcePath, pathParts);
        }

        public static Stream GetResource(List<String> fileNameList, Assembly assembly, String baseFolder, out String findedResourcePath, params Object[] pathParts)
        {
            findedResourcePath = null;
            Uri uri = GetResourceUri(fileNameList, assembly, baseFolder, pathParts);
            if (uri == null)
                return null;
            findedResourcePath = uri.ToString();
            return WebRequest.Create(uri).GetResponse().GetResponseStream();
        }

        public static Uri GetResourceUri(List<String> fileNameList, Assembly assembly, String baseFolder, params Object[] pathParts)
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
                return new Uri(filePath);
            }
            //然后尝试从程序集资源中加载
            else
            {
                String assemblyName = assembly.GetName().Name;

                //先寻找嵌入的资源
                foreach (String fileName in fileNameList)
                {
                    String resourceName = String.Join(".", pathParts);
                    resourceName = resourceName.Replace("[fileName]", fileName);
                    resourceName = resourceName.Replace("-", "_");
                    if (IsEmbedResourceExist(assembly, resourceName))
                        return new Uri(String.Format("embed://{0}/{1}", assemblyName, resourceName));
                }
                //然后寻找Resource资源
                if (IsEmbedResourceExist(assembly, assemblyName + ".g.resources"))
                {
                    foreach (String fileName in fileNameList)
                    {
                        String resourceName = String.Join("/", pathParts);
                        resourceName = resourceName.Replace("[fileName]", fileName);

                        String assemblyNamePrefix = assemblyName + "/";
                        if (resourceName.StartsWith(assemblyNamePrefix))
                            resourceName = resourceName.Substring(assemblyNamePrefix.Length);
                        resourceName = GetResourceResourcePath(assembly, resourceName);
                        if (!String.IsNullOrEmpty(resourceName))
                            return new Uri(String.Format("pack://application:,,,/{0};component/{1}", assemblyName, resourceName));
                    }
                }
            }
            return null;
        }

        //程序集的嵌入的资源字典
        private static Dictionary<Assembly, String[]> assemblyEmbedResourceDict = new Dictionary<Assembly, string[]>();
        //程序集的Resource编译的资源字典
        private static Dictionary<Assembly, Dictionary<String, String>> assemblyResourceResourceDict = new Dictionary<Assembly, Dictionary<String, String>>();

        public static Boolean IsEmbedResourceExist(Assembly assembly, String resourceName)
        {
            lock (assemblyEmbedResourceDict)
            {
                if (!assemblyEmbedResourceDict.ContainsKey(assembly))
                {
                    assemblyEmbedResourceDict[assembly] = assembly.GetManifestResourceNames();
                }
                return assemblyEmbedResourceDict[assembly].Contains(resourceName);
            }
        }

        public static String GetResourceResourcePath(Assembly assembly, String resourceName)
        {
            resourceName = resourceName.Replace('/', '.');
            lock (assemblyResourceResourceDict)
            {
                if (!assemblyResourceResourceDict.ContainsKey(assembly))
                {
                    String[] resourceList = new String[0];
                    string resBaseName = assembly.GetName().Name + ".g.resources";
                    using (var stream = assembly.GetManifestResourceStream(resBaseName))
                    {
                        if (stream != null)
                        {
                            using (var reader = new System.Resources.ResourceReader(stream))
                            {
                                resourceList = reader.Cast<DictionaryEntry>().Select(entry =>
                                         (string)entry.Key).ToArray();
                            }
                        }
                    }
                    assemblyResourceResourceDict[assembly] = new Dictionary<string, string>();
                    foreach (var resource in resourceList)
                    {
                        assemblyResourceResourceDict[assembly][resource.Replace('/', '.')] = resource;
                    }
                }
                resourceName = resourceName.ToLower();
                if (assemblyResourceResourceDict[assembly].ContainsKey(resourceName))
                    return assemblyResourceResourceDict[assembly][resourceName];
                return null;
            }
        }

        public static String GetResourceText(List<String> fileNameList, Assembly assembly, String baseFolder, params Object[] pathParts)
        {
            String findedResourcePath;
            return GetResourceText(fileNameList, assembly, baseFolder, out findedResourcePath, pathParts);
        }

        public static String GetResourceText(List<String> fileNameList, Assembly assembly, String baseFolder, out String findedResourcePath, params Object[] pathParts)
        {
            Stream resourceStream = GetResource(fileNameList, assembly, baseFolder, out findedResourcePath, pathParts);
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
