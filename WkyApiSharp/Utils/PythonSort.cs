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



        [DllImport("msvcrt.dll")]
        public static extern int memcmp(byte[] b1, byte[] b2, UIntPtr count);


        /*
            unsafe_latin_compare(PyObject *v, PyObject *w, MergeState *ms)
            {
                Py_ssize_t len;
                int res;
                assert(Py_IS_TYPE(v, &PyUnicode_Type));
                assert(Py_IS_TYPE(w, &PyUnicode_Type));
                assert(PyUnicode_KIND(v) == PyUnicode_KIND(w));
                assert(PyUnicode_KIND(v) == PyUnicode_1BYTE_KIND);

                len = Py_MIN(PyUnicode_GET_LENGTH(v), PyUnicode_GET_LENGTH(w));
                res = memcmp(PyUnicode_DATA(v), PyUnicode_DATA(w), len);

                res = (res != 0 ?
                    res< 0 :
                    PyUnicode_GET_LENGTH(v) < PyUnicode_GET_LENGTH(w));

                assert(res == PyObject_RichCompareBool(v, w, Py_LT));;
                return res;
            }
         */
        /// <summary>
        /// 有环境依赖，废弃
        /// </summary>
        /// <param name="v"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        public static int CompareString(string v, string w)
        {
            int len = Math.Min(v.Length, w.Length);
            byte[] b1 = Encoding.UTF8.GetBytes(v);
            byte[] b2 = Encoding.UTF8.GetBytes(w);
            int res = memcmp(b1, b2, new UIntPtr((uint)len));

            //res = (res != 0 ? res < 0 : v.Length < w.Length) ? 0 : 1;

            return res;
        }

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
