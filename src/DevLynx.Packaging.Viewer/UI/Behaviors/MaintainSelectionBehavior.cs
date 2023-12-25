using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DevLynx.Packaging.Visualizer.UI
{
    internal class MaintainSelectionBehavior : Behavior<TextBox>
    {
        private int _sel = 0;

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject == null) return;

            AssociatedObject.TextChanged += HandleTextChanged;
            AssociatedObject.SelectionChanged += HandleSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();


            if (AssociatedObject == null) return;
            AssociatedObject.TextChanged -= HandleTextChanged;
            AssociatedObject.SelectionChanged -= HandleSelectionChanged;
        }

        private void HandleTextChanged(object sender, TextChangedEventArgs e)
        {
            if (AssociatedObject.SelectionStart == 0)
                AssociatedObject.SelectionStart = _sel;
        }

        private void HandleSelectionChanged(object sender, RoutedEventArgs e)
        {
            _sel = AssociatedObject.SelectionStart;
        }
    }
}
