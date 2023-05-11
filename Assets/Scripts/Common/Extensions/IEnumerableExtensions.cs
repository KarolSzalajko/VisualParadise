using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Common.Extensions
{
  public static class IEnumerableExtensions
  {
    public static HashSet<T> ToSet<T>(this IEnumerable<T> enumerable) => new HashSet<T>(enumerable);

    public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> enumerable, int count)
    {
      var list = enumerable.ToList();
      return list.Take(list.Count - count);
    }

    public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N)
    {
      return source.Skip(Math.Max(0, source.Count() - N));
    }
  }
}
