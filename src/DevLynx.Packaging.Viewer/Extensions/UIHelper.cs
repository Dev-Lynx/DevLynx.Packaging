using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
