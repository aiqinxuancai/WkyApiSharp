using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WkyApiSharp.Utils;

namespace WkyApiSharp.Service
{
    public class WkyApiBase
    {
        //PC==1.4.5.112 iOS=2.6.0
        public const string kAppVersion = "1.4.5.112"; // "2.6.0";
        public const string kApiAccountUrlHost = "http://account.onethingpcs.com/";
        public const string kLoginUrl = kApiAccountUrlHost + "user/login?appversion=" + kAppVersion;
        public const string kCheckSession = kApiAccountUrlHost + "user/check-session?appversion=" + kAppVersion;

        public const string kApiControlUrlRoot = "http://control.onethingpcs.com/";
        public const string kListPeerURL = kApiControlUrlRoot + "listPeer";
        public const string kPeerUSBInfoUrl = kApiControlUrlRoot + "getUSBInfo";
        public const string kGetTurnServer = kApiControlUrlRoot + "getturnserver";
        

        public const string kApiRemoteDlUrlRoot = "http://control-remotedl.onethingpcs.com/";
        public const string kUrlResolveUrl = kApiRemoteDlUrlRoot + "urlResolve";
        public const string kBtCheckUrl = kApiRemoteDlUrlRoot + "btCheck";
        public const string kLoginRemoteDlUrl = kApiRemoteDlUrlRoot + "login";
        public const string kListRemoteDlInfoUrl = kApiRemoteDlUrlRoot + "list";
        public const string kCreateTaskUrl = kApiRemoteDlUrlRoot + "createTask";
        public const string kCreateBatchTaskUrl = kApiRemoteDlUrlRoot + "createBatchTask";
        
        public const string kStartTaskUrl = kApiRemoteDlUrlRoot + "start";
        public const string kPauseTaskUrl = kApiRemoteDlUrlRoot + "pause";
        public const string kDelTaskUrl = kApiRemoteDlUrlRoot + "del";


        public static string GetPassword(string password)
        {
            var s = MD5Helper.GetMD5(password).ToLower();
            s = s[0..2] + s[8] + s[3..8] + s[2] + s[9..17] + s[27] + s[18..27] + s[17] + s[28..];
            return MD5Helper.GetMD5(s).ToLower();
        }

        public static string GetDevice(string user)
        {
            var s = MD5Helper.GetMD5(user).ToLower()[..14];
            return s;
        }

        public static string GetPeerId(string user)
        {
            var s = MD5Helper.GetMD5(user).ToUpper();
            return s;
        }

        public static string GetIMEI(string user)
        {
            var s = MD5Helper.GetMD5(user).ToLower()[..16];
            return s;
        }

        public static string GetSign(Dictionary<string, string> dic, string k = "")
        {
            List<string> l = new List<string>();
            foreach (KeyValuePair<string, string> item in dic)
            {
                l.Add(item.Key + "=" + item.Value);
            }

            l.Sort((a,b) => { return PythonSort.ByteArrayCompareV2(a, b); });

            //var alphaStrings = l;
            //var orderedString = alphaStrings.OrderBy(g => new Tuple<int, string>(g.ToCharArray().All(char.IsDigit) ? int.Parse(g) : int.MaxValue, g));

            int t = 0;
            string s = "";

            foreach (string item in l)
            {
                s = s + l[t] + "&";
                t = t + 1;
            }

            var signInput = s + "key=" + k;
            var sign = MD5Helper.GetMD5(signInput).ToLower();

            Debug.WriteLine("使用字符串取Sign：" + signInput);
            Debug.WriteLine("使用字符串取Sign：" + sign);
            return sign;
        }

        public static Dictionary<string, string> GenerateBody(Dictionary<string, string> dic)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();


            foreach (KeyValuePair<string, string> item in dic)
            {
                result[item.Key] = dic[item.Key];
            }

            string sign = GetSign(result);


            foreach (KeyValuePair<string, string> item in dic)
            {
                result[item.Key] = dic[item.Key];
            }
            result["sign"] = sign;

            return result;

        }

        /// <summary>
        /// 转换为连续字符串的参数
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static string DictionaryToString(Dictionary<string, string> dic)
        {
            string s = "";

            foreach (KeyValuePair<string, string> item in dic)
            {
                s = s + "&" + item.Key + "=" + item.Value;
            }
            return s;
        }

        /// <summary>
        /// 转换为连续字符串的参数
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static string DictionaryToParamsString(Dictionary<string, string> dic)
        {
            string s = "";
            foreach (KeyValuePair<string, string> item in dic)
            {
                s = s + item.Key + "=" + item.Value + "&";
            }

            if (s.EndsWith("&"))
            {
                s = s.Substring(0, s.Length - 1);
            }
            return s;
        }
        /// <summary>
        /// 获取Get的参数
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public static string GetParams(Dictionary<string, string> dic, string sessionId)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> item in dic)
            {
                if (item.Key == "pwd")
                {
                    result[item.Key] = GetPassword(item.Value);
                }
                else
                {
                    result[item.Key] = item.Value;
                }
            }

            string sign = GetSign(result, sessionId);
            result["sign"] = sign;


            return "?" + DictionaryToParamsString(result);
        }
    }

}
