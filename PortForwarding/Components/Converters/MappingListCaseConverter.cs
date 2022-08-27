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
    public class MappingListCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueType = ReflectUtils.GetNullableType(value.GetType());
            if (parameter != null)
            {
                var caseKey = parameter.ToString();
                if (string.Equals("SwitchEditStatus", caseKey, StringComparison.OrdinalIgnoreCase))
                {
                    if (value != null && valueType == typeof(bool))
                    {
                        var isEditing = (bool)value;
                        return isEditing ? "完成修改" : "开始修改";
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
