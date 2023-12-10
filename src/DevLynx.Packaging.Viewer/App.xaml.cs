using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevLynx.Packaging.Visualizer.Extensions;
using Prism;
using Prism.Ioc;

namespace DevLynx.Packaging.Visualizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
#if DEBUG
            ConsoleManager.Show();
            Console.WriteLine("Welcome to DevLynx.Packaging.Visualizer");
            Console.WriteLine("All errors will show up here");
#endif

            base.OnStartup(e);
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<Shell>();
            //return new Shell();
        }

        protected override void RegisterTypes(IContainerRegistry registry)
        {
        }
    }
}
