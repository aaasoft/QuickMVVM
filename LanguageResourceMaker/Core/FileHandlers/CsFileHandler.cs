using ICSharpCode.NRefactory.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace LanguageResourceMaker.Core.FileHandlers
{
    public class CsFileHandler : AbstractFileHandler
    {
        //\[(?'attribute'.*?)\s*\(.*?"(?'value'.*?)"\s*\)\s*]
        private Regex regex = new Regex("\\[(?'name'.*?)\\s*\\(.*?\"(?'value'.*?)\"\\s*\\)\\s*]");

        public override void Handle(FileInfo viewFile, DirectoryInfo projectFolder)
        {
            String text = viewFile.FullName;

            StreamReader reader = new StreamReader(viewFile.FullName);
            CSharpParser parser = new CSharpParser();
            SyntaxTree syntaxTree = parser.Parse(reader);
            reader.Close();
            foreach (EntityDeclaration item in syntaxTree.GetTypes(true))
            {
                if (!(item is TypeDeclaration))
                    continue;
                TypeDeclaration type = (TypeDeclaration)item;
                handle(type, projectFolder);
            }
        }

        private void handle(TypeDeclaration type, DirectoryInfo projectFolder)
        {
            List<String> textList = new List<string>();
            foreach (AttributeSection attribute in type.Attributes)
            {
                String attributeText = attribute.ToString();
                Match match = regex.Match(attributeText);
                Group nameGroup = match.Groups["name"];
                Group valueGroup = match.Groups["value"];
                if (!nameGroup.Success || nameGroup.Value != "Text" || !valueGroup.Success)
                    continue;
                String value = valueGroup.Value;
                textList.Add(value);
            }
            if (textList.Count == 0)
                return;

            TypeDeclaration currentType = type;
            String typeFullName = null;
            while (true)
            {
                if (typeFullName == null)
                    typeFullName = type.Name;
                else
                    typeFullName = String.Format("{0}+{1}", currentType.Name, typeFullName);
                if (currentType.Parent is TypeDeclaration)
                {
                    currentType = (TypeDeclaration)currentType.Parent;
                    continue;
                }
                else if (currentType.Parent is NamespaceDeclaration)
                {
                    NamespaceDeclaration namesp = (NamespaceDeclaration)currentType.Parent;
                    typeFullName = String.Format("{0}.{1}", namesp.FullName, typeFullName);
                    break;
                }
                else
                    throw new ApplicationException("Type's parent unknown!");
            }
            handle(typeFullName, textList, projectFolder);
        }

        private void handle(String typeFullName, List<String> textList, DirectoryInfo projectFolder)
        {
            String outFileName;
            String projectName = projectFolder.Name;
            if (typeFullName.StartsWith(projectName + ".ViewModel."))
                outFileName = typeFullName.Substring((projectName + ".ViewModel.").Length);
            else if (typeFullName.StartsWith(projectName + "."))
                outFileName = typeFullName.Substring((projectName + ".").Length);
            else
                outFileName = typeFullName;

            OutputLanguageFileAction(outFileName, projectFolder, textList, Thread.CurrentThread.CurrentCulture.Name);
        }
    }
}
