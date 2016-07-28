using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LegendsOfDescent
{
    public static class RandomExtensions
    {
        public static T From<T>(this Random random, params T[] options)
        {
            return options.Random();
        }

        public static Color Color(this Random random)
        {
            return new Color(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
        }

        public static float Between(this Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        public static double Between(this Random random, double min, double max)
        {
            return min + random.NextDouble() * (max - min);
        }

        /// <summary>
        /// Get a random int between the specified values.
        /// </summary>
        /// <param name="random"></param>
        /// <param name="min">inclusive</param>
        /// <param name="max">exclusive</param>
        /// <returns></returns>
        public static int Between(this Random random, int min, int max)
        {
            return random.Next(min, max);
        }
        
        /// <summary>
        /// Get a random int in the range of specified values.
        /// </summary>
        /// <param name="random"></param>
        /// <param name="min">inclusive</param>
        /// <param name="max">inclusive</param>
        /// <returns></returns>
        public static int InRange(this Random random, int min, int max)
        {
            return random.Next(min, max + 1);
        }

        public static T Enum<T>(this Random random)
        {
            T[] values = (T[])EnumExtensions.GetValues<T>();
            return values[random.Next(0, values.Length)];
        }

        public static T Random<T>(this IList<T> list)
        {
            if (list.Count == 0) return default(T);
            return list[Util.Random.Next(0, list.Count)];
        }

        public static IEnumerable<T> TakeRandom<T>(this IList<T> list, int needed)
        {
            int remaining = list.Count;

            while (needed > 0 && remaining > 0)
            {
                if (Util.Random.NextDouble() < (needed * 1.0 / remaining))
                {
                    needed--;
                    yield return list[remaining - 1];
                }
                remaining--;
            }
        }
        
        /// <summary>
        /// Shuffles the specified list (Extension Method for any IList<T>).
        /// </summary>
        /// <remarks>
        /// Algorithm described at: http://en.wikipedia.org/wiki/Fisher-Yates_shuffle
        /// </remarks>
        /// <example>list.Shuffle();</example>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Util.Random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
