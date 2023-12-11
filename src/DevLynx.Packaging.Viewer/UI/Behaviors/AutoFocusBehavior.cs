using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DevLynx.Packaging.Visualizer.UI
{
    public class AutoFocusBehavior : Behavior<FrameworkElement>
    {
        public int FocusDelay { get; set; }

        private DispatcherTimer _timer;
        private int _focusAttempt;

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject == null)
                return;

            AssociatedObject.Loaded += OnLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject == null)
                return;

            AssociatedObject.Loaded -= OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement el)
                return;

            _focusAttempt = 0;
            int delay = FocusDelay > 0 ? FocusDelay : 100;

            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(delay), 
                DispatcherPriority.Normal, HandleFocus, 
                Application.Current.Dispatcher);

            _timer.Start();
        }

        private void HandleFocus(object sender, EventArgs e)
        {
            if (AssociatedObject.Focus() || ++_focusAttempt >= 3)
            {
                _timer.Stop();
            }   
        }
    }

    public class AutoHighlightBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject == null)
                return;
            AssociatedObject.GotFocus += HandleFocus;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject == null)
                return;
            AssociatedObject.GotFocus -= HandleFocus;
        }

        private void HandleFocus(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                if (sender is TextBox textBox)
                    textBox.SelectAll();
                else if (sender is PasswordBox passwordBox)
                    passwordBox.SelectAll();
            });
        }
    }
}
