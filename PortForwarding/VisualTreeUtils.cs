using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PortForwarding
{
    public class VisualTreeUtils
    {
        public static List<TChild> GetChildrens<TChild>(FrameworkElement obj)
            where TChild : FrameworkElement
        {
            return GetChildrens(obj).Where(x => x is TChild).Cast<TChild>().ToList();
        }

        public static TChild FindChildren<TChild>(FrameworkElement obj, string name)
            where TChild : FrameworkElement
        {
            return (TChild)FindChildren(obj, name);
        }

        public static FrameworkElement FindChildren(FrameworkElement obj, string name)
        {
            var childrens = GetChildrens(obj);
            return (FrameworkElement)childrens.FirstOrDefault(x => x.Name == name);
        }

        public static List<FrameworkElement> GetChildrens(FrameworkElement obj)
        {
            var resultList = new List<FrameworkElement>();
            var childCount = VisualTreeHelper.GetChildrenCount(obj);
            for (var i = 0; i < childCount; i++)
            {
                var child = (FrameworkElement)VisualTreeHelper.GetChild(obj, i);
                if (child != null) resultList.Add(child);
            }
            return resultList;
        }
    }
}
