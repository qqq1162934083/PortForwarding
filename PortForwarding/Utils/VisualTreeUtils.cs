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
        #region ��װ����
        /// <summary>
        /// ��ȡ��ͼԪ�����ϵݹ���ҵĵ�һ��ָ�����Ƶĸ���Ԫ��
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
        /// ��ȡ��ͼԪ�����ϵݹ���ҵĵ�һ��ָ�����͵ĸ���Ԫ��
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
        /// ��ȡ��ͼԪ����ָ�����Ƶ�����һ����Ԫ��
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <param name="obj">�����ṩ���ҵĸ�Ԫ��</param>
        /// <param name="name">Ҫ���ҵ���Ԫ������</param>
        /// <returns></returns>
        public static List<TChild> FindChildrens<TChild>(DependencyObject obj, string name)
            where TChild : FrameworkElement
        {
            return FindChildrens<TChild>(obj, x => x.Name == name);
        }

        /// <summary>
        /// ��ȡ��ͼԪ����ָ�����͵�����һ����Ԫ��
        /// </summary>
        /// <typeparam name="TChild">Ҫ���ҵ���Ԫ������</typeparam>
        /// <param name="obj">�����ṩ���ҵĸ�Ԫ��</param>
        /// <returns></returns>
        public static List<TChild> FindChildrens<TChild>(DependencyObject obj)
            where TChild : FrameworkElement
        {
            return FindChildrens<TChild>(obj, x => x.GetType() == typeof(TChild));
        }
        #endregion


        #region ��������
        /// <summary>
        /// ��ȡ��ͼԪ�����ϵݹ���ҵĵ�һ������ν�ʼ��������ĸ���Ԫ��
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
        /// ��ȡ��ͼԪ�صĸ�Ԫ��
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
        /// ��ȡ��ͼԪ���з���ν���������е�һ����Ԫ��
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <param name="obj">�����ṩ���ҵĸ�Ԫ��</param>
        /// <param name="predicateExpression">ν�ʼ������ʽ</param>
        /// <returns></returns>
        public static List<TChild> FindChildrens<TChild>(DependencyObject obj, Expression<Predicate<FrameworkElement>> predicateExpression)
            where TChild : FrameworkElement
        {
            var elem = Convert2FrameworkElement(obj);
            var childrens = GetChildrens<FrameworkElement>(elem);
            return childrens.Where(x => (bool)predicateExpression.Compile().DynamicInvoke(x)).Cast<TChild>().ToList();
        }

        /// <summary>
        /// ��ȡ��ͼԪ�ص�������Ԫ��
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

        #region ͨ�ô���
        private static FrameworkElement Convert2FrameworkElement(object obj)
        {
            if (!(obj is FrameworkElement)) throw new ArgumentException("obj is not a FrameworkElement");
            return (FrameworkElement)obj;
        }
        #endregion
    }
}
