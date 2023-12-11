using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
