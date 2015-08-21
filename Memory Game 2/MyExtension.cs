using System;
using System.Collections.Generic;

namespace MyGames
{
    public static class MyExtension
    {
        static readonly Random random = new Random();
        public static void shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                var k = random.Next(n--);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}