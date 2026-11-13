using System;
using System.Globalization;
using System.Windows.Data;
using MatchService;

namespace DominoAllFives.Client.WPF.Models
{
    public sealed class MatchValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SpecialPower)
            {
                return (DominoTileType)(SpecialPower)value;
            }

            return Localization.Resources.ResourceManager.GetString("Match_end" + value, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
