using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace DominoAllFives.Client.WPF.Converters
{
    /// <summary>
    /// Converts multiple PasswordBox controls into an array.
    /// </summary>
    public class PasswordBoxesConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return values.Select(
                value => value as PasswordBox).ToArray();
        }

        public object[] ConvertBack(
            object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}


