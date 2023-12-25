using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DevLynx.Packaging.Visualizer.UI
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool Inverse { get; set; }
        public Visibility HiddenValue { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bValue = false;
            if (value is bool) bValue = (bool)value;
            if (Inverse) bValue = !bValue;

            return (bValue) ? Visibility.Visible : HiddenValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bValue = value as Visibility? == Visibility.Visible;
            if (Inverse) bValue = !bValue;
            
            return bValue;
        }
    }
}