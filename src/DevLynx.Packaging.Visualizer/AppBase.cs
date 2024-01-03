using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevLynx.Packaging.Visualizer
{
    internal class AppBase
    {
        public const string PRODUCT_NAME = "Packaging SimXD";
        public const string PRODUCT_VERSION = "1.1";
        
        public const string AUTHOR = "Prince Owen";
        public const string COMPANY = "Dev-Lynx Technologies";

        public static readonly string BASE_DIR = Directory.GetCurrentDirectory();
        public static readonly string WORK_DIR = Path.Combine(BASE_DIR, "App");
        public readonly static string LOG_DIR = Path.Combine(WORK_DIR, "Logs");

        public const string MAIN_REGION = nameof(MAIN_REGION);
        public const string MENU_REGION = nameof(MENU_REGION);
        public const string SPACE_REGION = nameof(SPACE_REGION);
    }
}
