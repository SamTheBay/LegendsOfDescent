using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsOfDescent
{
    public static class MiscExtensions
    {
        public static TReturn IfNotNull<TTarget, TReturn>(this TTarget target, Func<TTarget, TReturn> selector) where TReturn : class
        {
            if (target == null) return null;
            return selector(target);
        }
    }
}
