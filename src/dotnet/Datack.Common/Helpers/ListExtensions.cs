using System;
using System.Collections.Generic;
using System.Linq;

namespace Datack.Common.Helpers
{
    public static class ListExtensions
    {
        public static List<List<T>> ChunkBy<T>(this IEnumerable<T> source, Int32 chunkSize) 
        {
            return source
                   .Select((x, i) => new { Index = i, Value = x })
                   .GroupBy(x => x.Index / chunkSize)
                   .Select(x => x.Select(v => v.Value).ToList())
                   .ToList();
        }
    }
}