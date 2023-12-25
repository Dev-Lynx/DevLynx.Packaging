using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DevLynx.Packaging.Visualizer.Extensions
{
    public static class UIHelper
    {
        public static void NavigateToView<T>(this IRegionManager regionManager, string region, NavigationParameters parameters = null) where T : class
        {
            try
            {
                if (parameters == null)
                    regionManager.RequestNavigate(region, typeof(T).Name);
                else regionManager.RequestNavigate(region, typeof(T).Name, parameters);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An error occured while attempting to navigate to {typeof(T)}.\n{ex}");
            }
        }

        public static Color ParseHexColor(string hex)
        {
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            byte a = 0xFF, r = 0, g = 0, b = 0;
            int nhex = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);

            if (hex.Length == 6)
            {
                r = (byte)(nhex >> 16);
                g = (byte)((nhex >> 8) & 0xFF);
                b = (byte)(nhex & 0xFF);
            }
            else if (hex.Length == 8)
            {
                a = (byte)(nhex >> 24);
                r = (byte)((nhex >> 16) & 0xFF);
                g = (byte)((nhex >> 8) & 0xFF);
                b = (byte)(nhex & 0xFF);
            }
            else throw new FormatException(string.Format("Invalid color string '{0}'", hex));
            return Color.FromArgb(a, r, g, b);
        }
    }
}
