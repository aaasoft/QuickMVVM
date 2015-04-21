﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Quick.MVVM.Utils
{
    public class ResourceUtils
    {
        public static String GetResourceText(List<String> fileNameList, Assembly assembly, String baseFolder, String fullFileNameTemplate, params Object[] templateParams)
        {
            //文件是否存在
            Boolean isFileExists = false;
            String filePath = null;
            foreach (String fileName in fileNameList)
            {
                filePath = Path.Combine(baseFolder, fileName);
                isFileExists = File.Exists(filePath);
                if (isFileExists)
                    break;
            }

            String fileContent = null;

            //先尝试从目录加载文件
            if (isFileExists)
            {
                fileContent = File.ReadAllText(filePath);
            }
            //然后尝试从程序集资源中加载
            else
            {
                foreach (String fileName in fileNameList)
                {
                    //"{0}.View.{1}"
                    String resourceName = String.Format(fullFileNameTemplate, templateParams);
                    resourceName = resourceName.Replace("[fileName]", fileName);
                    resourceName = resourceName.Replace("-", "_");
                    Stream resourceStream = assembly.GetManifestResourceStream(resourceName);
                    if (resourceStream != null)
                    {
                        StreamReader streamReader = new StreamReader(resourceStream);
                        fileContent = streamReader.ReadToEnd();
                        streamReader.Close();
                        resourceStream.Close();
                        break;
                    }
                }
            }
            return fileContent;
        }
    }
}
