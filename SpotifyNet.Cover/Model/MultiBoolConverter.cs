using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace SpotifyNet.Cover.Model
{
    public enum Operation
    {
        And,
        Or
    }

    public class MultiBoolConverter : IMultiValueConverter
    {
        public Operation Operation { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 0)
                return false;

            if (values.Length == 1 && values[0] is bool)
                return values[0];

            switch (Operation)
            {
                case Operation.And:
                    return values.All(x => x as bool? ?? false);
                case Operation.Or:
                    return values.Any(x => x as bool? ?? false);
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
