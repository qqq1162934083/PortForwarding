using MyTool.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PortForwarding
{
    public class ReadOnlyPerformanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var type = ReflectUtils.GetNullableType(value.GetType());
                if (typeof(bool) == type)
                {
                    var readOnly = (bool)value;

                    if (targetType == typeof(Brush))
                    {
                        return new SolidColorBrush(readOnly ? Colors.LightGray : Colors.White);
                    }
                    else
                    {
                        MessageBox.Show("NotImplementedException");
                        throw new NotImplementedException();
                    }
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MessageBox.Show("NotImplementedException");
            throw new NotImplementedException();
        }
    }
}
