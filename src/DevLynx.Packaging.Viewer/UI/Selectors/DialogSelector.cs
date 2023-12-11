using DevLynx.Packaging.Visualizer.Models.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DevLynx.Packaging.Visualizer.UI
{
    internal class DialogSelector : DataTemplateSelector
    {
        public DataTemplate EmptyTemplate { get; set; }
        public DataTemplate ItemDialogTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is DialogKind kind)
            {
                switch (kind)
                {
                    case DialogKind.Item:
                        return ItemDialogTemplate;

                    case DialogKind.None:
                        return EmptyTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}
