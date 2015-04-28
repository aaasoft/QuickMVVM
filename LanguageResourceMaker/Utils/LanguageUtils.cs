using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LanguageResourceMaker.Utils
{
    public class LanguageUtils
    {
        public static String GetToWriteLanguageText(List<String> textList)
        {
            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < textList.Count; j++)
            {
                sb.AppendLine(String.Format("{0}={1}", j + 1, textList[j]));
            }
            return sb.ToString();
        }
    }
}
