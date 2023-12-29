using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DevLynx.Packaging.Visualizer.Extensions
{
    internal static class DispatcherExtensions
    {
        public static void DelayedInvoke(this Dispatcher disp,  TimeSpan timespan, Action callback, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send, disp);
            timer.Interval = timespan;

            EventHandler handler = null;
            timer.Tick += handler = (s, e) =>
            {
                timer.Tick -= handler;
                timer.Stop();

                disp.Invoke(callback, priority);
            };

            timer.Start();
        }

        public static void DelayedInvoke(this Dispatcher disp, double ms, Action callback, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send, disp);
            timer.Interval = TimeSpan.FromMilliseconds(ms);

            EventHandler handler = null;
            timer.Tick += handler = (s, e) =>
            {
                timer.Tick -= handler;
                timer.Stop();

                disp.Invoke(callback, priority);
            };

            timer.Start();
        }
    }
}
