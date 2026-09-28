using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace DominoAllFives.Client.WPF.Converters
{
    public class PasswordBoxesConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return new[]
            {
                values[0] as PasswordBox,
                values[1] as PasswordBox
            };
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
