using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace DevLynx.Packaging.Visualizer.UI
{
    public class CountToVisibilityConverter : IValueConverter
    {
        #region Properties
        public int HiddenCount { get; set; }
        public bool Inverse { get; set; }
        #endregion

        #region Methods

        #region IValueConverter Implementation

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(bool))
            {
                if (value is int && ((int)value) > HiddenCount)
                    return true && !Inverse;
                return (false && Inverse);
            }


            if (value is int && ((int)value) > HiddenCount)
                return Inverse ? Visibility.Collapsed : Visibility.Visible;
            return Inverse ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion

        #endregion
    }
}
