using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DM7_PPLUS_Integration.Implementierung.Shared
{
    public static class ArrayExtensions
    {
        public static byte[] Concat(this List<byte[]> source)
        {
            var resultarray = new byte[source.Sum(_ => _.Length)];
            var pos = 0;
            foreach (var ar in source)
            {
                Array.Copy(ar, 0, resultarray, pos, ar.Length);
                pos += ar.Length;
            }
            return resultarray;
        }
    }

}