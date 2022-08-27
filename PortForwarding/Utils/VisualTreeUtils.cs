using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PortForwarding
{
    public class VisualTreeUtils
    {
        #region 包装重载
        /// <summary>
        /// 获取视图元素向上递归查找的第一个指定名称的父级元素
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TChild GetFirstParent<TChild>(DependencyObject obj, string name)
            where TChild : FrameworkElement
        {
            return GetFirstParent<TChild>(obj, x => x.Name == name);
        }
        /// <summary>
        /// 获取视图元素向上递归查找的第一个指定类型的父级元素
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static TChild GetFirstParent<TChild>(DependencyObject obj)
            where TChild : FrameworkElement
        {
            return GetFirstParent<TChild>(obj, x => x.GetType() == typeof(TChild));
        }

        /// <summary>
        /// 获取视图元素中指定名称的所有一级子元素
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <param name="obj">用于提供查找的根元素</param>
        /// <param name="name">要查找的子元素名称</param>
        /// <returns></returns>
        public static List<TChild> FindChildrens<TChild>(DependencyObject obj, string name)
            where TChild : FrameworkElement
        {
            return FindChildrens<TChild>(obj, x => x.Name == name);
        }

        /// <summary>
        /// 获取视图元素中指定类型的所有一级子元素
        /// </summary>
        /// <typeparam name="TChild">要查找的子元素类型</typeparam>
        /// <param name="obj">用于提供查找的根元素</param>
        /// <returns></returns>
        public static List<TChild> FindChildrens<TChild>(DependencyObject obj)
            where TChild : FrameworkElement
        {
            return FindChildrens<TChild>(obj, x => x.GetType() == typeof(TChild));
        }
        #endregion


        #region 基础方法
        /// <summary>
        /// 获取视图元素向上递归查找的第一个符合谓词检索条件的父级元素
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static TChild GetFirstParent<TChild>(DependencyObject obj, Expression<Predicate<FrameworkElement>> predicateExpression)
            where TChild : FrameworkElement
        {
            var parent = GetParent<FrameworkElement>(obj);
            if (parent == null || (bool)predicateExpression.Compile().DynamicInvoke(parent)) return (TChild)parent;
            return GetFirstParent<TChild>(parent, predicateExpression);
        }

        /// <summary>
        /// 获取视图元素的父元素
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static TChild GetParent<TChild>(DependencyObject obj)
            where TChild : FrameworkElement
        {
            var elem = Convert2FrameworkElement(obj);
            return (TChild)VisualTreeHelper.GetParent(elem);
        }

        /// <summary>
        /// 获取视图元素中符合谓词条件所有的一级子元素
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <param name="obj">用于提供查找的根元素</param>
        /// <param name="predicateExpression">谓词检索表达式</param>
        /// <returns></returns>
        public static List<TChild> FindChildrens<TChild>(DependencyObject obj, Expression<Predicate<FrameworkElement>> predicateExpression)
            where TChild : FrameworkElement
        {
            var elem = Convert2FrameworkElement(obj);
            var childrens = GetChildrens<FrameworkElement>(elem);
            return childrens.Where(x => (bool)predicateExpression.Compile().DynamicInvoke(x)).Cast<TChild>().ToList();
        }

        /// <summary>
        /// 获取视图元素的所有子元素
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<TChild> GetChildrens<TChild>(DependencyObject obj)
            where TChild : FrameworkElement
        {
            var elem = Convert2FrameworkElement(obj);
            var resultList = new List<FrameworkElement>();
            var childCount = VisualTreeHelper.GetChildrenCount(elem);
            for (var i = 0; i < childCount; i++)
            {
                var child = (FrameworkElement)VisualTreeHelper.GetChild(elem, i);
                if (child != null) resultList.Add(child);
            }
            return resultList.Cast<TChild>().ToList();
        }
        #region

        #region 通用处理
        private static FrameworkElement Convert2FrameworkElement(object obj)
        {
            if (!(obj is FrameworkElement)) throw new ArgumentException("obj is not a FrameworkElement");
            return (FrameworkElement)obj;
        }
        #endregion
    }
}
