using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DevToolVaultV2.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                // Retorna Visible se for false, e Collapsed se for true
                return booleanValue ? Visibility.Collapsed : Visibility.Visible;
            }
            // Se não for booleano, assume false (Visible)
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                // Retorna false se for Visible, e true se for Collapsed/Hidden
                return visibility != Visibility.Visible;
            }
            return false;
        }
    }
}