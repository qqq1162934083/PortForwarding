using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Utils
{
    public static class ReflectUtils
    {
        /// <summary>
        /// 如果指定类型实现Nullable<>，获取Nullable<>中泛型的具体类型，否则直接返回该类型
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <returns></returns>
        public static Type GetNullableType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return type.GetGenericArguments().First();
            else
                return type;
        }

        /// <summary>
        /// 判断属性是否含有这些特性
        /// </summary>
        /// <param name="propInfo">要判断的属性信息</param>
        /// <param name="attrTypes">要判断含有的所有特性</param>
        /// <returns></returns>
        public static bool ContainsAttrs(PropertyInfo propInfo, params Type[] attrTypes) => attrTypes.All(x => propInfo.GetCustomAttributes(x, false).Length != 0);
    }
}
