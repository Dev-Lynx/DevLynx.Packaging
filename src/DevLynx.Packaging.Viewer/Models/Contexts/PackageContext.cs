using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevLynx.Packaging.Visualizer.Models.Contexts
{
    internal class Dim : BindableBase
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public float Depth { get; set; }

        public Dim(float width, float height, float depth)
        {
            Width = width;
            Height = height;
            Depth = depth;
        }
    }

    internal class NDim : Dim
    {
        public int Count { get; set; }

        public NDim(float width, float height, float depth) : base (width, height, depth)
        {
            Count = 1;
        }

        public NDim(Dim dim) : base(dim.Width, dim.Height, dim.Depth)
        {
            Count = 1;
        }
    }

    internal class PackageContext : BindableBase
    {
        public Dim Container { get; private set; } = new(10, 10, 10);
        public ObservableCollection<NDim> Items { get; } = new();

        public Dim NewItem { get; set; } = new(1, 1, 1);
    }
}
