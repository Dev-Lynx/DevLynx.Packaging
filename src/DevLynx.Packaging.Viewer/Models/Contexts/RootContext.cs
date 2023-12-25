using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace DevLynx.Packaging.Visualizer.Models.Contexts
{
    public enum DialogKind { Item, None }

    public class DialogContext : BindableBase
    {
        public bool IsActive { get; set; }
        public DialogKind Kind { get; set; }
    }

    public class RootContext : BindableBase
    {
        public DialogContext Dialog { get; } = new();


        public Viewport3D Viewport { get; internal set; }
        public Model3DGroup Scene { get; internal set; }
        public Model3DGroup PackedScene { get; internal set; }
    }
}
