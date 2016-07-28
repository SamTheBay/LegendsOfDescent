using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsOfDescent
{
    /// <summary>
    /// Be aware that many of these are less performant than the alternatives if you use lambdas.
    /// </summary>
    public static class IntExtensions
    {
        public static void Times(this int count, Action action)
        {
            for (int i = 0; i < count; i++)
            {
                action();
            }
        }

        public static void Times(this int count, Action<int> action)
        {
            for (int i = 0; i < count; i++)
            {
                action(i);
            }
        }

        public static void PercentChanceTo(this int percent, Action action)
        {
            if (Util.Random.Next(100) < percent)
            {
                action();
            }
        }
    }
}
