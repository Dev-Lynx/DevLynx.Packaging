using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace DevLynx.Packaging.Visualizer.UI
{
    public class ExternalLinkBehavior : Behavior<Hyperlink>
    {
        protected override void OnAttached()
        {
            AssociatedObject.RequestNavigate += OnNavigate;

            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.RequestNavigate -= OnNavigate;

            base.OnDetaching();
        }

        private void OnNavigate(object sender, RequestNavigateEventArgs e)
        {
            e.Handled = true;

            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}
