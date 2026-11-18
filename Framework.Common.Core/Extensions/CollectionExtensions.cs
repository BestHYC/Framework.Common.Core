using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Core
{
    public static class CollectionExtensions
    {
        public static Boolean IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }
        public static Boolean HasValue<T>(this IEnumerable<T> source)
        {
            return source != null && source.Any();
        }
    }
}
