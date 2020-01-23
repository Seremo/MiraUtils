using MiraUI.ViewModels;
using System;
using System.Windows.Data;

namespace MiraUI.Converters
{
    public class ActiveDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ToolViewModel)
                return value;

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ToolViewModel)
                return value;

            return Binding.DoNothing;
        }
    }
}
