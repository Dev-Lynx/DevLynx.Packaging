using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevLynx.Packaging.Visualizer.Extensions
{
    internal class FileExtensions
    {
        static ILogger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Easy and safe way to create multiple directories. 
        /// </summary>
        /// <param name="directories">The set of directories to create</param>
        public static void CreateDirectories(params string[] directories)
        {
            if (directories == null || directories.Length <= 0) return;

            foreach (var directory in directories)
                try
                {
                    if (Directory.Exists(directory)) continue;

                    Directory.CreateDirectory(directory);
                    Logger.Info("A new directory has been created ({0})", directory);
                }
                catch (Exception e)
                {
                    Logger.Error("Error while creating directory {0} - {1}", directory, e);
                }
        }
    }
}
