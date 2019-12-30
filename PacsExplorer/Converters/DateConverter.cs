using System;
using System.Globalization;
using System.Windows.Data;

namespace PacsExplorer.Converters
{
    class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => System.Convert.ToDateTime(value).ToShortDateString();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
