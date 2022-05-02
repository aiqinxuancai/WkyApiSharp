using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WkyApiSharp.Utils
{
    public class PythonSort
    {
        public static bool ByteArrayCompare(string v, string w)
        {
            byte[] b1 = Encoding.UTF8.GetBytes(v);
            byte[] b2 = Encoding.UTF8.GetBytes(w);

            var a1 = new ReadOnlySpan<byte>(b1);
            var a2 = new ReadOnlySpan<byte>(b2);

            return a1.SequenceEqual(a2);
        }

        public static int ByteArrayCompareV2(string v, string w)
        {
            byte[] b1 = Encoding.UTF8.GetBytes(v);
            byte[] b2 = Encoding.UTF8.GetBytes(w);

            var a1 = new ReadOnlySpan<byte>(b1);
            var a2 = new ReadOnlySpan<byte>(b2);

            int result = a1.SequenceCompareTo(a2);

            //保持和memcmp一致
            if (result > 1)
            {
                result = 1;
            }
            else if (result < -1)
            {
                result = -1;
            }

            return result;
        }

        

    }
}
