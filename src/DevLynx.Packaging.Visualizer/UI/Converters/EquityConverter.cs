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
    public class EquityConverter : IValueConverter, IMultiValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool res = Equals(value, parameter);

            if (Invert) res = !res;

            if (targetType == typeof(Visibility))
                return res ? Visibility.Visible : Visibility.Collapsed;

            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }


        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object first = values.FirstOrDefault();
            bool res = true;

            for (int i = 1; i < values.Length && res; i++)
            {
                res &= Equals(first, values[i]);
            }

            if (Invert) res = !res;

            if (targetType == typeof(Visibility))
                return res ? Visibility.Visible : Visibility.Collapsed;

            return res;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
