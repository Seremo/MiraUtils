using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MiraUI.Converters
{
    /// <summary>
    ///     Source: http://stackoverflow.com/questions/534575/how-do-i-invert-booleantovisibilityconverter
    ///     Implements a Boolean to Visibility converter
    ///     Use ConverterParameter=true to negate the visibility - boolean interpretation.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        ///     Converts a <seealso cref="Boolean" /> value
        ///     into a <seealso cref="Visibility" /> value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var IsInverted = parameter == null ? false : (bool)parameter;
            var IsVisible = value == null ? false : (bool)value;
            if (IsVisible)
                return IsInverted ? Visibility.Hidden : Visibility.Visible;
            return IsInverted ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        ///     Converts a <seealso cref="Visibility" /> value
        ///     into a <seealso cref="Boolean" /> value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visiblility = value == null ? Visibility.Hidden : (Visibility)value;
            var IsInverted = parameter == null ? false : (bool)parameter;

            return visiblility == Visibility.Visible != IsInverted;
        }
    }
}
