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
        /// <param name="predicate">谓词检索表达式</param>
        /// <returns></returns>
        public static List<TChild> FindChildrens<TChild>(DependencyObject obj, Predicate<FrameworkElement> predicate)
            where TChild : FrameworkElement
        {
            var elem = Convert2FrameworkElement(obj);
            var childrens = GetChildrens<FrameworkElement>(elem);
            return childrens.Where(x => predicate.Invoke(x)).Cast<TChild>().ToList();
        }

        /// <summary>
        /// 递归获取视图元素中符合谓词条件所有的子元素
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <param name="obj">用于提供查找的根元素</param>
        /// <param name="predicate">谓词检索表达式</param>
        /// <returns></returns>
        public static List<TChild> RecursiveFindChildrens<TChild>(DependencyObject obj, Predicate<FrameworkElement> predicate)
            where TChild : FrameworkElement
        {
            var elem = Convert2FrameworkElement(obj);
            var resultList = new List<FrameworkElement>();
            var childrens = GetChildrens<FrameworkElement>(elem);
            foreach (var child in childrens)
            {
                resultList.AddRange(RecursiveFindChildrens<FrameworkElement>(child, predicate));
                if (predicate.Invoke(child)) resultList.Add(child);
            }

            return resultList.Cast<TChild>().ToList();
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
        #endregion

        #region 通用处理
        private static FrameworkElement Convert2FrameworkElement(object obj)
        {
            if (!(obj is FrameworkElement)) throw new ArgumentException("obj is not a FrameworkElement");
            return (FrameworkElement)obj;
        }
        #endregion
    }

    public class VisualTreePrinter
    {
        public void PrintLogicaTree(int depth, object obj)  //输出逻辑树
        {
            test1(new string(' ', depth) + obj);
            if (!(obj is DependencyObject))
            {
                return;
            }
            foreach (object child in LogicalTreeHelper.GetChildren(obj as DependencyObject))
                PrintLogicaTree(depth + 1, child);
        }

        public void PrintVisualTree(int depth, DependencyObject DObj)
        {
            test1(new string(' ', depth) + DObj);
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(DObj); i++)
            {
                PrintVisualTree(depth + 1, VisualTreeHelper.GetChild(DObj, i));
            }
        }
        //需先建立文件夹
        public void test1(string a) //切换用户不会停止当前代码
        {
            //写输出信息      
            //StreamWriter sr = new StreamWriter(@"C:\Users\Public\test\a.txt", true, System.Text.Encoding.Default);  // 保留文件原来的内容
            //sr.WriteLine(DateTime.Now.ToString("\r\n" + "HH:mm:ss"));
            //sr.WriteLine(a);
            //sr.Close();
            Console.WriteLine(a);
        }
    }
}
