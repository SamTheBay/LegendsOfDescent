using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace LegendsOfDescent
{
    public class EnumExtensions
    {
        public static T[] GetValues<T>()
        {
            Type enumType = typeof(T);
      
#if WIN8
            return enumType.GetRuntimeFields().Where(f => f.IsLiteral).Select(f => f.GetValue(enumType)).Cast<T>().ToArray();
#else
            return enumType.GetFields().Where(f => f.IsLiteral).Select(f => f.GetValue(enumType)).Cast<T>().ToArray();
#endif
        }
    }
}
