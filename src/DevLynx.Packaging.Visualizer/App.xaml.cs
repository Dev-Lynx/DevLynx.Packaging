using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevLynx.Packaging.Visualizer.Extensions;
using DevLynx.Packaging.Visualizer.Services;
using NLog;
using NLog.Config;
using NLog.Fluent;
using NLog.Targets;
using Prism;
using Prism.DryIoc;
using Prism.Ioc;

namespace DevLynx.Packaging.Visualizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public ILogger Log;
        public static IContainerProvider IOC;

        protected override void OnStartup(StartupEventArgs e)
        {
            FileExtensions.CreateDirectories(AppBase.WORK_DIR, AppBase.LOG_DIR);
            ConfigureLogging();

            Log = LogManager.GetCurrentClassLogger();
            
            this.Dispatcher.UnhandledException += (s, ex) => Log.Error($"An unhandled exception occured in the dispatcher\n{ex.Exception}");
            AppDomain.CurrentDomain.UnhandledException += (s, ex) => Log.Error("An unhandled exception occured in the app domain\n{0}", ex.ExceptionObject);
            this.DispatcherUnhandledException += (s, ex) => Log.Error(ex.Exception, "An unhandled exception occured in the application\n{0}", ex.Exception);
            TaskScheduler.UnobservedTaskException += (s, ex) => Log.Error(ex.Exception, "An unhandled exception occured in the task scheduler\n{0}", ex.Exception);


#if DEBUG
            Trace.Listeners.Add(new ConsoleTraceListener(true));
            ConsoleManager.Show();
            Log.Debug("DevLynx.Packaging.Visualizer © 2023");
#endif
            base.OnStartup(e);
        }

        protected override Window CreateShell()
        {
            IOC = Container;
            return Container.Resolve<Shell>();
        }

        protected override void RegisterTypes(IContainerRegistry registry)
        {
            registry.RegisterScoped<IPackagingService, PackagingService>();
            registry.RegisterScoped<IMessageService, UIMessageService>();
            registry.RegisterSingleton<IAppService, AppService>();
            registry.RegisterInstance<ILogger>(Log);
        }

        private void ConfigureLogging()
        {
            LoggingConfiguration config = new LoggingConfiguration();

            Target target;

            const string CONSOLE_NAME = "pack-dbg";
            const string LOG_LAYOUT = "${longdate}|${uppercase:${level}}| ${message} ${exception:format=tostring";
            const string FULL_LOG_LAYOUT = "${longdate} | ${logger}\n${message} ${exception:format=tostring}\n";
#if DEBUG
            target = new ColoredConsoleTarget
            {
                Name = CONSOLE_NAME,
                Layout = LOG_LAYOUT,
                Header = $"{AppBase.PRODUCT_NAME} Debugger"
            };

            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));

            target = new FileTarget()
            {
                Name = "RUNTIME_LOG",
                FileName = Path.Combine(AppBase.LOG_DIR, $"Runtime_{DateTime.Now:MM-yyyy}.log"),
                Layout = LOG_LAYOUT
            };

            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, target));
#endif

            target = new FileTarget()
            {
                Name = "ERROR_LOG",
                FileName = Path.Combine(AppBase.LOG_DIR, $"Error_{DateTime.Now:MM-yyyy}.log"),
                Layout = LOG_LAYOUT
            };

            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Error, target));
            
            LogManager.Configuration = config;
            LogManager.ReconfigExistingLoggers();
        }
    }
}
