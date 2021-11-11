using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace WkyApiSharp.Utils
{
    public class MD5Helper
    {
        public static string GetMD5(string str)
        {
            byte[] result = Encoding.Default.GetBytes(str); 
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            return BitConverter.ToString(output).Replace("-", ""); 
        }


        public static string GetMD5(byte[] bytes)
        {
            byte[] result = bytes;
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            return BitConverter.ToString(output).Replace("-", "");
        }
    }
}
