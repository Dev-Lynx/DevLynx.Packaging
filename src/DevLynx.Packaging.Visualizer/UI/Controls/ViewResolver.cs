using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using Prism.Ioc;

namespace DevLynx.Packaging.Visualizer.UI.Controls
{
    public class ViewResolver : ContentControl
    {
        public Type TargetView
        {
            get { return (Type)GetValue(TargetViewProperty); }
            set { SetValue(TargetViewProperty, value); }
        }

        public static readonly DependencyProperty TargetViewProperty =
            DependencyProperty.Register("TargetView", typeof(Type), typeof(ViewResolver), new FrameworkPropertyMetadata(typeof(Control), (s, e) =>
            {
                if (!(s is ViewResolver resolver)) return;
                if (!(e.NewValue is Type target)) return;

                var container = ContainerLocator.Container;

                if (container == null)
                    resolver.Content = Activator.CreateInstance(target);
                else resolver.Content = container.Resolve(target);
            }));
    }
}
