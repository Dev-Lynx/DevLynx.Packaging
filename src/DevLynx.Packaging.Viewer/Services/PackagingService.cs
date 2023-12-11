using DevLynx.Packaging.Visualizer.Models.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevLynx.Packaging.Visualizer.Services
{
    internal interface IPackagingService
    {
        internal PackageContext Context { get; }
    }

    internal class PackagingService : IPackagingService
    {
        public PackageContext Context { get; } = new();

        public PackagingService()
        {
        }
    }
}
