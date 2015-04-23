using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LanguageResourceMaker
{
    static class Program
    {
        public static String InputFolder, OutputFolder;
        public static Boolean AutoTranslate;
        public static String[] TranslateTarget;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ParameterForm());
        }
    }
}
