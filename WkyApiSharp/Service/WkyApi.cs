using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl;
using Newtonsoft.Json.Linq;
using WkyApiSharp.Service.Model;
using WkyApiSharp.Service.Model.ListPeer;
using WkyApiSharp.Service.Model.GetUsbInfo;
using WkyApiSharp.Service.Model.RemoteDownloadLogin;
using WkyApiSharp.Service.Model.RemoteDownloadList;
using WkyApiSharp.Service.Model.UrlResolve;
using System.IO;
using WkyApiSharp.Service.Model.BtCheck;
using WkyApiSharp.Service.Model.CreateTask;
using WkyApiSharp.Service.Model.CreateTaskResult;
using WkyApiSharp.Service.Model.CreateBatchTask;
using WkyApiSharp.Service.Model.CreateBatchTaskResult;
using WkyApiSharp.Utils;
using WkyApiSharp.Service.Model.GetTurnServerResult;

namespace WkyApiSharp.Service
{
    public class WkyApi : WkyApiBase
    {
        private string _user = ""; //可能是手机号也可能是邮箱
        private string _password = "";

        public WkyApiLoginResultModel UserInfo { set; get; } = new WkyApiLoginResultModel();

        //session过期时间
        const int kCookieMaxAge = 604800; 

        /// <summary>
        /// 从用户名密码初始化
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        public WkyApi(string user, string password)
        {
            _user = user;
            _password = password;
        }

        /// <summary>
        /// 从存储的Session文件内容初始化
        /// </summary>
        /// <param name="filePath"></param>
        public WkyApi(string sessionContent, string user = "", string password = "")
        {
            UserInfo = JsonConvert.DeserializeObject<WkyApiLoginResultModel>(sessionContent);
            _user = user;
            _password = password;
        }

        /// <summary>
        /// 存储的session是否过期
        /// </summary>
        /// <returns></returns>
        public bool IsSessionExpired()
        {
            //检测过期？
            var interval = DateTime.Now - UserInfo.CreateDateTime;
            if (interval.TotalSeconds > kCookieMaxAge)
            {
                //过期 需要重新登录
                return true;
            }
            return false;
        }

        /// <summary>
        /// Login后可调用此接口返回Session，在后续的wkyapi中初始化可用
        /// </summary>
        /// <returns></returns>
        public string GetSessionContent()
        {
            return JsonConvert.SerializeObject(UserInfo);
        }

        private IFlurlRequest BaseHeader(string url)
        {
            Debug.WriteLine(url);
            return url.WithHeader("cache-control", $"no-cache");
        }

        private IFlurlRequest BaseHeaderAndCookie(string url)
        {
            
            return BaseHeader(url)
                //.WithHeader("user-agent", $"MineCrafter3/{kAppVersion} (iPhone; iOS 15.0.1; Scale/3.00)")
                //.WithHeader("cache-control", $"no-cache")
                .WithCookie("userid", UserInfo.UserId)
                .WithCookie("sessionid", UserInfo.SessionId);
        }

        /// <summary>
        /// 登录玩客云
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Login()
        {
            Dictionary<string, string> loginData;

            //判断email
            if (RegexUtilities.IsValidEmail(_user))
            {
                loginData = GenerateBody(new Dictionary<string, string>()
                    { 
                        { "phone_area", "Email" },
                        { "deviceid", GetDevice(_user) },
                        { "imeiid" , GetIMEI(_user)},
                        { "phone" , _user},
                        { "pwd" , GetPassword(_password)},
                        { "account_type" , "5"},
                    });
            }
            else
            {
                loginData = GenerateBody(new Dictionary<string, string>()
                    {
                        { "deviceid", GetDevice(_user) },
                        { "imeiid" , GetIMEI(_user)},
                        { "phone" , _user},
                        { "pwd" , GetPassword(_password)},
                        { "account_type" , "4"},
                    });
            }

            Debug.WriteLine(loginData);

            Debug.WriteLine(JsonConvert.SerializeObject(loginData));

            var result = await BaseHeader(kLoginUrl)
                .WithCookie("origin", "3") //PC?
                .PostStringAsync(DictionaryToString(loginData));

            JsonConvert.SerializeObject(result.Cookies);
            //判断登录成功

            //("Set-Cookie", "userid=**; Expires=Fri, 05-Nov-21 06:46:52 GMT; Max-Age=604800; Domain=.onethingpcs.com; Path=/")
            Debug.WriteLine(JsonConvert.SerializeObject(loginData));

            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                JObject resultRoot = JObject.Parse(resultJson);

                Console.WriteLine(resultJson);
                if (resultRoot.ContainsKey("sMsg") && resultRoot["sMsg"].ToString() == "Success")
                {
                    UserInfo = resultRoot["data"].ToObject<WkyApiLoginResultModel>();
                    UserInfo.CreateDateTime = DateTime.Now;
                    return true;
                }
                else
                {
                    if (resultRoot.ContainsKey("sMsg"))
                    {
                        throw new WkyApiException(resultRoot["sMsg"].ToString());
                    }
                }
            }



            return false;
        }


        

        /// <summary>
        /// 获取玩客云设备列表
        /// </summary>
        /// <returns></returns>
        public async Task<WkyApiListPeerResultModel> ListPeer() 
        {
            ///listPeer?appversion=1.4.5.112&ct=5&v=8&sign=b806ff46fde38c3da6b0be10e86ebd4c
            string data = GetParams(new Dictionary<string, string>()
            {
                { "X-LICENCE-PUB", "1" },
                { "appversion" , kAppVersion},
                { "v" , "2"},
                { "ct" , "9"},
            }, UserInfo.SessionId);

            //&ct=5&v=8(PC)
            Debug.WriteLine(data);
            var result = await BaseHeaderAndCookie(kListPeerURL + data).GetAsync();
            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Debug.WriteLine(resultJson);
                WkyApiListPeerResultModel model = WkyApiListPeerResultModel.FromJson(resultJson);
                return model;
            }
            return null;
        }

        /// <summary>
        /// 获取设备的USB存储设备信息
        /// </summary>
        /// <param name="deviceId">使用ListPeer获取的设备id</param>
        /// <returns></returns>
        public async Task<WkyApiGetUsbInfoResultModel> GetUsbInfo(string deviceId)
        {
            string data = GetParams(new Dictionary<string, string>()
            {
                { "X-LICENCE-PUB","1"},
                {"appversion", kAppVersion },
                {"v", "2"},
                {"ct", "9"},
                {"deviceid", deviceId}
            }, UserInfo.SessionId);
            Debug.WriteLine(data);
            var result = await BaseHeaderAndCookie(kPeerUSBInfoUrl + data).GetAsync();
            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Debug.WriteLine(resultJson);
                WkyApiGetUsbInfoResultModel model = WkyApiGetUsbInfoResultModel.FromJson(resultJson);
                return model;
            }
            return null;
        }

        //"/getturnserver?appversion=1.4.5.112&ct=5&sn=OCPG******&v=3&sign=9b9184e498d8c9120acfb557740c88e9"
        /// <summary>
        /// 设备Turn
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public async Task<WkyApiGetTurnServerResultModel> GetTurnServer(string sn)
        {
            string data = GetParams(new Dictionary<string, string>()
            {
                {"appversion", kAppVersion },
                {"v", "3"},
                {"ct", "5"},
                {"sn", sn}
            }, UserInfo.SessionId);
            Debug.WriteLine(data);
            var result = await BaseHeaderAndCookie(kGetTurnServer + data).GetAsync();
            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Debug.WriteLine(resultJson);
                WkyApiGetTurnServerResultModel model = WkyApiGetTurnServerResultModel.FromJson(resultJson);
                return model;
            }
            return null;
        }
        


        /// <summary>
        /// 登录远程下载
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async Task<WkyApiRemoteDownloadLoginResultModel> RemoteDownloadLogin(string peerId)
        {
            string data = GetParams(new Dictionary<string, string>()
            {
                { "pid", peerId },
                {"appversion", kAppVersion },
                {"v", "1"},
                {"ct", "32"},
            }, UserInfo.SessionId);
            Debug.WriteLine(data);
            var result = await BaseHeaderAndCookie(kLoginRemoteDlUrl + data).GetAsync();
            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Console.WriteLine(resultJson);
                WkyApiRemoteDownloadLoginResultModel model = WkyApiRemoteDownloadLoginResultModel.FromJson(resultJson);
                return model;
            }
            return null;
        }

        /// <summary>
        /// 获取远程设备的下载列表
        /// </summary>
        /// <param name="peerId"></param>
        /// <param name="startPos">开始位置</param>
        /// <param name="count">取出数量</param>
        /// <returns></returns>
        public async Task<WkyApiRemoteDownloadListResultModel> RemoteDownloadList(string peerId, int startPos = 0, int count = 100)
        {
            string data = GetParams(new Dictionary<string, string>()
            {
                { "pid", peerId },
                { "pos", startPos.ToString() },
                { "number", count.ToString() },
                { "appversion", kAppVersion },
                { "type", "4" },
                { "needUrl", "0" },
                { "v", "2"},
                { "ct", "31"},
            }, UserInfo.SessionId);
            Debug.WriteLine(data);
            var result = await BaseHeaderAndCookie(kListRemoteDlInfoUrl + data).GetAsync();
            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Console.WriteLine(resultJson);
                WkyApiRemoteDownloadListResultModel model = WkyApiRemoteDownloadListResultModel.FromJson(resultJson);
                return model;
            }
            return null;
        }

        /// <summary>
        /// 分析URL
        /// </summary>
        /// <param name="peerId"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<WkyApiUrlResolveResultModel> UrlResolve(string peerId, string url)
        {
            var data = new Dictionary<string, string>()
            {
                { "pid", peerId },
                { "v", "1"},
                { "ct", "31"},
            };
            Debug.WriteLine(data);
            var result = await BaseHeaderAndCookie(kUrlResolveUrl + $"?{DictionaryToParamsString(data)}").PostUrlEncodedAsync($"url={url}");
            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Console.WriteLine(resultJson);
                WkyApiUrlResolveResultModel model = WkyApiUrlResolveResultModel.FromJson(resultJson);
                return model;
            }
            return null;
        }

        /// <summary>
        /// 分析bt文件
        /// </summary>
        /// <param name="peerId"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task<WkyApiBtCheckResultModel> BtCheck(string peerId, string filePath)
        {
            var data = new Dictionary<string, string>()
            {
                { "pid", peerId },
                { "v", "2"},
                { "ct", "31"},
            };
            Debug.WriteLine(data);

            System.Net.Http.ByteArrayContent httpContent = new System.Net.Http.ByteArrayContent(File.ReadAllBytes(filePath));

            var result = await BaseHeaderAndCookie(kBtCheckUrl + $"?{DictionaryToParamsString(data)}").PostMultipartAsync(mp => mp.Add("filepath", httpContent));


            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Console.WriteLine(resultJson);
                WkyApiBtCheckResultModel model = WkyApiBtCheckResultModel.FromJson(resultJson);
                return model;
            }
            return null;
        }

        public async Task<WkyApiBtCheckResultModel> BtCheck(string peerId, byte[] fileData)
        {
            var data = new Dictionary<string, string>()
            {
                { "pid", peerId },
                { "v", "2"},
                { "ct", "31"},
            };
            Debug.WriteLine(data);

            System.Net.Http.ByteArrayContent httpContent = new System.Net.Http.ByteArrayContent(fileData);

            var result = await BaseHeaderAndCookie(kBtCheckUrl + $"?{DictionaryToParamsString(data)}")
                .PostMultipartAsync(mp => mp.Add("filepath", httpContent));


            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Console.WriteLine(resultJson);
                WkyApiBtCheckResultModel model = WkyApiBtCheckResultModel.FromJson(resultJson);
                return model;
            }
            return null;
        }

        /// <summary>
        /// 创建任务，全部下载
        /// </summary>
        /// <param name="peerId"></param>
        /// <param name="path"></param>
        /// <param name="urlModel"></param>
        /// <returns></returns>
        public async Task<WkyApiCreateTaskResultModel> CreateTaskWithUrlResolve(string peerId, string path, WkyApiUrlResolveResultModel urlModel)
        {
            var data = new Dictionary<string, string>()
            {
                { "pid", peerId },
                { "v", "1"},
                { "ct", "31"},
            };
            Debug.WriteLine(data);

            WkyApiCreateTaskModel sendModel = new WkyApiCreateTaskModel();
            Model.CreateTask.Task task = new Model.CreateTask.Task();
            task.Filesize = urlModel.TaskInfo.Size;
            task.Infohash = urlModel.Infohash ;
            task.Name = urlModel.TaskInfo.Name;
            task.Url = urlModel.TaskInfo.Url;

            List<Model.CreateTask.Task> tasks = new List<Model.CreateTask.Task>();
            tasks.Add(task);

            sendModel.Path = path;
            sendModel.Tasks = tasks.ToArray();

            var result = await BaseHeaderAndCookie(kCreateTaskUrl + $"?{DictionaryToParamsString(data)}")
                .PostJsonAsync(sendModel);

            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Console.WriteLine(resultJson);
                WkyApiCreateTaskResultModel model = WkyApiCreateTaskResultModel.FromJson(resultJson);
                return model;
            }
            return null;
        }

        /// <summary>
        /// 创建任务，全部下载，从BT文件下载
        /// </summary>
        /// <param name="peerId"></param>
        /// <param name="path"></param>
        /// <param name="urlModel"></param>
        /// <returns></returns>
        public async Task<WkyApiCreateTaskResultModel> CreateTaskWithBtCheck(string peerId, string path, WkyApiBtCheckResultModel urlModel)
        {
            var data = new Dictionary<string, string>()
            {
                { "pid", peerId },
                { "v", "1"},
                { "ct", "31"},
            };
            Debug.WriteLine(data);

            WkyApiCreateTaskModel sendModel = new WkyApiCreateTaskModel();
            Model.CreateTask.Task task = new Model.CreateTask.Task();
            task.Filesize = long.Parse(urlModel.TaskInfo.Size);
            task.Infohash = urlModel.Infohash;
            task.Name = urlModel.TaskInfo.Name;
            task.Url = urlModel.TaskInfo.Url;

            List<Model.CreateTask.Task> tasks = new List<Model.CreateTask.Task>();
            tasks.Add(task);

            sendModel.Path = path;
            sendModel.Tasks = tasks.ToArray();

            var result = await BaseHeaderAndCookie(kCreateTaskUrl + $"?{DictionaryToParamsString(data)}")
                .PostJsonAsync(sendModel);

            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Console.WriteLine(resultJson);
                WkyApiCreateTaskResultModel model = WkyApiCreateTaskResultModel.FromJson(resultJson);
                return model;
            }
            return null;
        }


        /// <summary>
        /// 创建任务 部分下载
        /// </summary>
        /// <param name="peerId"></param>
        /// <param name="path"></param>
        /// <param name="urlModel"></param>
        /// <param name="subTask">如BT内的子文件ID，从解析后的任务中获取，为null则全部下载</param>
        /// <returns></returns>
        public async Task<WkyApiCreateBatchTaskResultModel> CreateBatchTaskWithUrlResolve(string peerId, string path, WkyApiUrlResolveResultModel urlModel, List<long> subTask)
        {
            var data = new Dictionary<string, string>()
            {
                { "pid", peerId },
                { "v", "2"},
                { "ct", "31"},
            };
            Debug.WriteLine(data);

            WkyApiCreateBatchTaskModel sendModel = new WkyApiCreateBatchTaskModel();
            Model.CreateBatchTask.Task task = new Model.CreateBatchTask.Task();
            task.Filesize = urlModel.TaskInfo.Size;
            task.Name = urlModel.TaskInfo.Name;
            task.Url = urlModel.TaskInfo.Url;
            task.Type = urlModel.TaskInfo.Type;

            if (subTask == null)
            {
                List<long> taskIds = new List<long>();
                foreach(var sub in urlModel.TaskInfo.SubList)
                {
                    taskIds.Add(sub.Id);
                }
                task.BtSub = taskIds.ToArray();
            }
            else
            {
                task.BtSub = subTask.ToArray();
            }

            List<Model.CreateBatchTask.Task> tasks = new List<Model.CreateBatchTask.Task>();
            tasks.Add(task);

            sendModel.Path = path;
            sendModel.Tasks = tasks.ToArray();

            var result = await BaseHeaderAndCookie(kCreateBatchTaskUrl + $"?{DictionaryToParamsString(data)}")
                .PostJsonAsync(sendModel);

            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Console.WriteLine(resultJson);
                WkyApiCreateBatchTaskResultModel model = WkyApiCreateBatchTaskResultModel.FromJson(resultJson);
                return model;
            }
            return null;
        }

        /// <summary>
        /// 创建任务 部分下载，使用解析BT的结果
        /// </summary>
        /// <param name="peerId"></param>
        /// <param name="path"></param>
        /// <param name="urlModel"></param>
        /// <param name="subTask">如BT内的子文件ID，从解析后的任务中获取</param>
        /// <returns></returns>
        public async Task<WkyApiCreateBatchTaskResultModel> CreateBatchTaskWithBtCheck(string peerId, string path, WkyApiBtCheckResultModel urlModel, List<long> subTask)
        {
            var data = new Dictionary<string, string>()
            {
                { "pid", peerId },
                { "v", "2"},
                { "ct", "31"},
            };
            Debug.WriteLine(data);

            WkyApiCreateBatchTaskModel sendModel = new WkyApiCreateBatchTaskModel();
            Model.CreateBatchTask.Task task = new Model.CreateBatchTask.Task();
            task.Filesize = long.Parse(urlModel.TaskInfo.Size);
            task.Name = urlModel.TaskInfo.Name;
            task.Url = urlModel.TaskInfo.Url;
            task.Type = urlModel.TaskInfo.Type;

            if (subTask == null)
            {
                List<long> taskIds = new List<long>();
                foreach (var sub in urlModel.TaskInfo.SubList)
                {
                    taskIds.Add(sub.Id);
                }
                task.BtSub = taskIds.ToArray();
            }
            else
            {
                task.BtSub = subTask.ToArray();
            }

            List<Model.CreateBatchTask.Task> tasks = new List<Model.CreateBatchTask.Task>();
            tasks.Add(task);

            sendModel.Path = path;
            sendModel.Tasks = tasks.ToArray();

            var result = await BaseHeaderAndCookie(kCreateBatchTaskUrl + $"?{DictionaryToParamsString(data)}")
                .PostJsonAsync(sendModel);

            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Console.WriteLine(resultJson);
                WkyApiCreateBatchTaskResultModel model = WkyApiCreateBatchTaskResultModel.FromJson(resultJson);
                return model;
            }
            return null;
        }

        // /pause?pid=*&ct=31&clientType=PC-onecloud&ct_ver=1.4.5.112&v=1&tasks=119482758_8_2
        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="peerId"></param>
        /// <param name="taskId">id_state_type 组合而成的字符串</param>
        /// <returns></returns>
        public async Task<bool> PauseTask(string peerId, string taskId)
        {
            var data = new Dictionary<string, string>()
            {
                { "pid", peerId },
                { "v", "1"},
                { "ct", "31"},
                { "tasks", taskId}, //id_state_type
            };
            Debug.WriteLine(data);
            var result = await BaseHeaderAndCookie(kPauseTaskUrl + $"?{DictionaryToParamsString(data)}").GetAsync();
            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                //{"tasks":[],"rtn":0}
                string resultJson = await result.GetStringAsync();
                JObject root = JObject.Parse(resultJson);
                if (root.ContainsKey("rtn") && root["rtn"].ToObject<int>() == 0)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// 开始（继续）下载任务
        /// </summary>
        /// <param name="peerId"></param>
        /// <param name="taskId">id_state_type 组合而成的字符串</param>
        /// <returns></returns>
        public async Task<bool> StartTask(string peerId, string taskId)
        {
            var data = new Dictionary<string, string>()
            {
                { "pid", peerId },
                { "v", "1"},
                { "ct", "31"},
                { "tasks", taskId}, //id_state_type
            };
            Debug.WriteLine(data);
            var result = await BaseHeaderAndCookie(kStartTaskUrl + $"?{DictionaryToParamsString(data)}").GetAsync();
            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                //{"tasks":[],"rtn":0}
                string resultJson = await result.GetStringAsync();
                JObject root = JObject.Parse(resultJson);
                if (root.ContainsKey("rtn") && root["rtn"].ToObject<int>() == 0)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        //del?pid=*&ct=31&clientType=PC-onecloud&ct_ver=1.4.5.112&v=1&tasks=119486928_8_2&deleteFile=true&recycleTask=false
        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="peerId"></param>
        /// <param name="taskId">id_state_type 组合而成的字符串</param>
        /// <param name="deleteFile"></param>
        /// <param name="recycleTask"></param>
        /// <returns></returns>
        public async Task<bool> DeleteTask(string peerId, string taskId, bool deleteFile = false, bool recycleTask = false)
        {
            var data = new Dictionary<string, string>()
            {
                { "pid", peerId },
                { "v", "1"},
                { "ct", "31"},
                { "tasks", taskId}, //id_state_type
                { "deleteFile", deleteFile ? "true" : "false"},
                { "recycleTask", recycleTask ? "true" : "false"},
            };
            Debug.WriteLine(data);
            var result = await BaseHeaderAndCookie(kDelTaskUrl + $"?{DictionaryToParamsString(data)}").GetAsync();
            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                //{"tasks":[],"rtn":0}
                string resultJson = await result.GetStringAsync();
                JObject root = JObject.Parse(resultJson);
                if (root.ContainsKey("rtn") && root["rtn"].ToObject<int>() == 0)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

    }
}
