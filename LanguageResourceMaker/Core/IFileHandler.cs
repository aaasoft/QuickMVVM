using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LanguageResourceMaker.Core
{
    public interface IFileHandler
    {
        String GetFolderPath();
        Action<String, DirectoryInfo, List<String>, String> OutputLanguageFileAction { get; set; }
        void Handle(FileInfo viewFile, DirectoryInfo projectFolder);
    }
}
