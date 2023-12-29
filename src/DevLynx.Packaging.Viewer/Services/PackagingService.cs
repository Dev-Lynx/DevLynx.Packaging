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

namespace DevLynx.Packaging.Visualizer.Services
{
    internal interface IPackagingService
    {
        internal PackageContext Context { get; }

        void Start();
    }

    internal class PackagingService : IPackagingService
    {
        public PackageContext Context { get; } = new();

        private readonly List<string> _colors = new List<string>()
        {
            "#ff4244", // Coral
            "#4ebeee", // Light Blue
            "#202020", // Dark Gray
            "#02ad61", // Jade
            //"#cd2b28", // Red
            "#4e255f", // Dark Purple
            "#00a6a2", // Dark Teal
            "#ff7c00", // Orange
            "#b20092", // Plum (Purple)
            "#0065b9", // Dark Teal,
            "#662e1d", // Cinnamon (Brown)
        };

        private PackIteration _itr;
        private readonly List<PackIteration> _iterations = new();

        public PackagingService()
        {
            // TODO: Set the container to LazySingle
            Context.Container.Width = 40;
            Context.Container.Height = 50;
            Context.Container.Depth = 30;

            Context.Items.Add(new NDim(20, 40, 10)
            {
                Count = 5
            });

            Context.Items.Add(new NDim(20, 20, 20)
            {
                Count = 3
            });

            Context.Items.Add(new NDim(40, 60, 30)
            {
                Count = 2
            });
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
            pack.NewIteration += HandleNewIteration;

            ctx.Result = pack.Pack();

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                for (int i = 0; i < _iterations.Count; i++)
                {
                    ctx.Iterations.Add(_iterations[i]);
                }
            });
        }

        private void HandleNewIteration(object sender, IterationEventArgs e)
        {
            var cnt = Context.Container;
            
            _itr = new PackIteration(_iterations.Count, new Vector3(e.Point.X, e.Point.Y, e.Point.Z), e.Orientation);
            _iterations.Add(_itr);

            Console.WriteLine("Iteration: [{0}] {1}", _iterations.Count, e.Point);
            

            //var q = QuaternionExtensions.FromOrientation(po);


        }

        private void HandlePack(object sender, PackedBox box)
        {
            int id = _itr.Instances.Count;
            int nColor = _itr.Instances.Count % _colors.Count;

            _itr.AddInstance(new PackInstance(id, box)
            {
                Color = _colors[nColor]
            });
        }
    }
}
