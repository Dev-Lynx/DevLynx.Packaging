using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace DevLynx.Packaging.Visualizer.UI
{
    /// <summary>
    /// Basic Attached Properties for ContentControl
    ///  <remarks>
    ///  ChangedAnimation: Ingenious idea by Snicker (https://stackoverflow.com/users/4166138/snicker)
    ///  https://stackoverflow.com/a/30543970/8058709
    ///  </remarks>
    /// </summary>
    internal class ContentControlProps
    {
        public static Storyboard GetChangedAnimation(DependencyObject obj)
        {
            return (Storyboard)obj.GetValue(ChangedAnimationProperty);
        }

        public static void SetChangedAnimation(DependencyObject obj, Storyboard value)
        {
            obj.SetValue(ChangedAnimationProperty, value);
        }

        // Using a DependencyProperty as the backing store for ChangedAnimation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChangedAnimationProperty =
            DependencyProperty.RegisterAttached("ChangedAnimation", 
                typeof(Storyboard), typeof(ContentControlProps), 
                new PropertyMetadata(default(Storyboard), ChangedAnimationCallback));

        private static void ChangedAnimationCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ContentControl control) return;

            var desc = DependencyPropertyDescriptor.FromProperty(ContentControl.ContentProperty, typeof(ContentControl));
            desc.AddValueChanged(control, HandleContentChanged);
            //desc.RemoveValueChanged(control, HandleContentChanged);
        }

        private static void HandleContentChanged(object sender, EventArgs eventArgs)
        {
            if (sender is not ContentControl control) return;
            
            Storyboard storyboard = GetChangedAnimation(control);
            if (storyboard == null) return;

            storyboard.Begin(control);
        }
    }
}
