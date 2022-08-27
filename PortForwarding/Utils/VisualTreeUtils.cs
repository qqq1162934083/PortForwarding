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
        /// <param name="predicate">ν�ʼ������ʽ</param>
        /// <returns></returns>
        public static List<TChild> FindChildrens<TChild>(DependencyObject obj, Predicate<FrameworkElement> predicate)
            where TChild : FrameworkElement
        {
            var elem = Convert2FrameworkElement(obj);
            var childrens = GetChildrens<FrameworkElement>(elem);
            return childrens.Where(x => predicate.Invoke(x)).Cast<TChild>().ToList();
        }

        /// <summary>
        /// �ݹ��ȡ��ͼԪ���з���ν���������е���Ԫ��
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <param name="obj">�����ṩ���ҵĸ�Ԫ��</param>
        /// <param name="predicate">ν�ʼ������ʽ</param>
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
        #endregion

        #region ͨ�ô���
        private static FrameworkElement Convert2FrameworkElement(object obj)
        {
            if (!(obj is FrameworkElement)) throw new ArgumentException("obj is not a FrameworkElement");
            return (FrameworkElement)obj;
        }
        #endregion
    }

    public class VisualTreePrinter
    {
        public void PrintLogicaTree(int depth, object obj)  //����߼���
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
        //���Ƚ����ļ���
        public void test1(string a) //�л��û�����ֹͣ��ǰ����
        {
            //д�����Ϣ      
            //StreamWriter sr = new StreamWriter(@"C:\Users\Public\test\a.txt", true, System.Text.Encoding.Default);  // �����ļ�ԭ��������
            //sr.WriteLine(DateTime.Now.ToString("\r\n" + "HH:mm:ss"));
            //sr.WriteLine(a);
            //sr.Close();
            Console.WriteLine(a);
        }
    }
}
