using MyTool.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace PortForwarding
{
    public class ConfigPanelCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var type = ReflectUtils.GetNullableType(value.GetType());
                if (typeof(bool) == type)
                {
                    var enabled = (bool)value;

                    if (targetType == typeof(Brush))
                    {
                        return new SolidColorBrush(enabled ? Colors.LightGreen : Colors.OrangeRed);
                    }
                    else
                    {
                        return enabled ? "停用该项" : "启用该项";
                    }
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
