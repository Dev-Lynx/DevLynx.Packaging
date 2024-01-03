using DevLynx.Packaging.Models;
using DevLynx.Packaging.Visualizer.Extensions;
using DevLynx.Packaging.Visualizer.Models.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using Wpf.Ui.Controls;

namespace DevLynx.Packaging.Visualizer.Services
{
    internal interface IPackagingService
    {
        internal PackageContext Context { get; }
        event EventHandler PackComplete;

        void Start();
    }

    internal class PackagingService : IPackagingService
    {
        public PackageContext Context { get; } = new();
        public event EventHandler PackComplete;

        private readonly List<string> _colors = new List<string>()
        {
            "#ff4244", // Coral
            "#4ebeee", // Light Blue
            "#202020", // Dark Gray
            "#02ad61", // Jade
            "#4e255f", // Dark Purple
            "#00a6a2", // Dark Teal
            "#ff7c00", // Orange
            "#b20092", // Plum (Purple)
            "#0065b9", // Dark Teal,
            "#662e1d", // Cinnamon (Brown)
        };

        private readonly List<PackIteration> _iterations = new();
        private readonly List<PackInstance> _instances = new();

        public PackagingService()
        {
            // TODO: Set the container to LazySingle
            //Context.Container.Width = 40;
            //Context.Container.Height = 50;
            //Context.Container.Depth = 30;

            //Context.Items.Add(new NDim(20, 40, 10)
            //{
            //    Count = 5
            //});

            //Context.Items.Add(new NDim(20, 20, 20)
            //{
            //    Count = 3
            //});

            //Context.Items.Add(new NDim(40, 60, 30)
            //{
            //    Count = 2
            //});
        }

        public void Start()
        {
            var ctx = Context;
            var items = ctx.Items.Select(x => new PackItem(x.Width, x.Height, x.Depth, x.Count));
            var cnt = new PackingContainer(ctx.Container.Width, ctx.Container.Height, ctx.Container.Depth);

            _iterations.Clear();
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                ctx.Iterations.Clear();
            });

            BinPack pack = new BinPack(items, cnt);

            pack.BoxPacked += HandlePack;
            pack.IterationComplete += HandleIterationComplete;

            ctx.Result = pack.Pack();

            ctx.ContainerVolume = ctx.Container.Width * ctx.Container.Height * ctx.Container.Depth;

            int bestItr = -1;
            double bestVol = -1;
            for (int i = 0; i < _iterations.Count; i++)
            {
                double vol = _iterations[i].Volume;

                if (vol > bestVol)
                {
                    bestVol = vol;
                    bestItr = i;
                }
            }

            

            if (bestItr >= 0)
                _iterations[bestItr].IsBest = true;


            ctx.BestIteration = bestItr;
            Console.WriteLine("Best is: {0}", bestItr);

            if (_iterations.Count > 0)
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    for (int i = 0; i < _iterations.Count; i++)
                    {
                        ctx.Iterations.Add(_iterations[i]);
                    }
                });
            }

            PackComplete?.Invoke(this, EventArgs.Empty);
        }

        private void HandleIterationComplete(object sender, IterationEventArgs e)
        {
            PackIteration iteration = new PackIteration(_iterations.Count, e.Container, e.PackedVolume, e.TotalPacked, e.IsFit);

            for (int i = 0; i < _instances.Count; i++)
            {
                iteration.AddInstance(_instances[i]);
            }

            _instances.Clear();
            _iterations.Add(iteration);
        }

        private void HandlePack(object sender, PackedBox box)
        {
            int id = _instances.Count;
            int nColor = _instances.Count % _colors.Count;

            _instances.Add(new PackInstance(id, box)
            {
                Color = _colors[nColor]
            });
        }
    }
}
