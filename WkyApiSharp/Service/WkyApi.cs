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
using System.Net.Http;
using WkyApiSharp.Events;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using Task = System.Threading.Tasks.Task;
using WkyApiSharp.Events.Account;
using System.Threading;

namespace WkyApiSharp.Service
{
    public enum WkyLoginDeviceType
    {
        Mobile = 0, //传递 imeiid + deviceid
        PC = 1, // 传递 peerId（MD5的长度） + product_id=0 本地固定，来源随机？
    }

    public class WkyApi : WkyApiBase
    {
        public string User => _user; //可能是手机号也可能是邮箱

        private string _user = ""; //可能是手机号也可能是邮箱
        private string _password = "";

        private string _sessionName = "";

        private CancellationTokenSource _tokenTaskListSource = new CancellationTokenSource();

        private WkyLoginDeviceType _wkyLoginDeviceType;


        public WkyApiLoginResultModel UserInfo { set; get; } = new WkyApiLoginResultModel();


        //事件
        public IObservable<EventBase> EventReceived => _eventReceivedSubject.AsObservable();

        private readonly Subject<EventBase> _eventReceivedSubject = new();


        /// <summary>
        /// 玩客云设备
        /// </summary>
        public List<WkyPeer> PeerList => _peerList;

        private readonly List<WkyPeer> _peerList = new();


        //session过期时间
        const int kCookieMaxAge = 604800;



        /// <summary>
        /// 从用户名密码初始化
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        public WkyApi(string user, string password, WkyLoginDeviceType wkyLoginDeviceType = WkyLoginDeviceType.Mobile)
        {
            _user = user;
            _password = password;
            _wkyLoginDeviceType = wkyLoginDeviceType;

            _sessionName = @$"{MD5Helper.GetMD5(_user)}.session";

            //检查session是否存在，否则重新登录
            if (File.Exists(_sessionName))
            {
                UserInfo = JsonConvert.DeserializeObject<WkyApiLoginResultModel>(File.ReadAllText(_sessionName));
            }

            //自身的订阅方法
            _eventReceivedSubject
                .OfType<UpdateDeviceResultEvent>()
                .Subscribe(async r =>
                {
                    if (r.IsSuccess)
                    {
                        if (_tokenTaskListSource != null)
                        {
                            _tokenTaskListSource.Cancel();
                        }
                        _tokenTaskListSource = new CancellationTokenSource();
                        Task.Run(async () =>
                        {
                            await UpdateTaskFunc(_tokenTaskListSource.Token);
                        }, _tokenTaskListSource.Token);
                    }

                });
        }



        #region Public

        /// <summary>
        /// 登录，优先使用session，如果不可用，则使用账号密码
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartLogin()
        {
            bool isSuccess = false;
            string errorMessage = string.Empty;
            if (!string.IsNullOrWhiteSpace(UserInfo.Phone) && !IsSessionExpired())
            {
                //自动登录
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        var listPeer = await this.ListPeer(); //检查session是否可用
                        if (listPeer.Rtn == 0)
                        {
                            //检查是否可登录Peer
                            var firstPeer = listPeer.Result.FirstOrDefault(a => a.Peer != null);
                            if (firstPeer.Peer != null)
                            {
                                var peer = new WkyPeer(firstPeer.Peer);
                                var loginPeerResult = await peer.VerifyTaskList(this);

                                if (loginPeerResult)
                                {
                                    isSuccess = true;
                                    break;
                                }
                            }
                        }
                        else //失败 API层失败
                        {
                            await Task.Delay(1000);
                        }
                    }
                    catch (Exception ex) //失败
                    {
                        await Task.Delay(1000);
                    }
                }
            }

            if (!isSuccess)
            {
                Console.WriteLine("session无效，使用密码重新登录");
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        var loginResult = await Login();
                        if (loginResult)
                        {
                            //登录成功，保存session
                            Console.WriteLine("登录成功，保存session");
                            var sessionContent = this.GetSessionContent();
                            File.WriteAllText(_sessionName, sessionContent);
                            isSuccess = true;
                            break;
                        }
                    }
                    catch (WkyApiException ex)
                    {
                        errorMessage = ex.Message; //由Login登录接口抛出异常
                        break;
                    }
                    catch (Exception ex)
                    {
                        //网络错误等因素
                    }

                }
            }

            if (isSuccess)
            {
                _eventReceivedSubject.OnNext(new LoginResultEvent(true, User, ""));
                //异步更新设备 更新USB存储设备
                UpdateDevices();
                return true;
            }
            else
            {
                _eventReceivedSubject.OnNext(new LoginResultEvent(false, User, errorMessage));
                return false;
            }
        }





        #endregion


        #region Private 
        private async Task<bool> UpdateDevices()
        {
            bool result = false;
            try
            {
                //获取设备信息
                var listPeerResult = await this.ListPeer();

                if (listPeerResult.Rtn == 0)
                {
                    _peerList.Clear();
                    foreach (var item in listPeerResult.Result)
                    {
                        if (item.Peer != null)
                        {
                            var peer = new WkyPeer(item.Peer);
                            _peerList.Add(peer);
                            await peer.UpdateDiskInfo(this);
                        }
                    }
                    result = true;
                    _eventReceivedSubject.OnNext(new UpdateDeviceResultEvent() { IsSuccess = true, PeerList = _peerList });

                }
                else
                {
                    throw new WkyApiException("获取Peer失败");
                }
            }
            catch (Exception ex)
            {
                _eventReceivedSubject.OnNext(new UpdateDeviceResultEvent() { IsSuccess = false });
            }
            return result;
        }



        /// <summary>
        /// TODO 更新任务列表，获取完设备后开始此方法
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task UpdateTaskFunc(CancellationToken cancellationToken)
        {
            //登录到Peer
            foreach (var peer in _peerList)
            {
                //await this.RemoteDownloadLogin(peer.PeerId);
                await peer.LoginPeer(this);
            }

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.WriteLine("退出Task刷新");
                    break;
                }
                Debug.WriteLine("刷新Task列表");
                try
                {
                    await this.UpdateTask();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }

                TaskHelper.Sleep(5 * 1000, 100, cancellationToken);
            }
        }

        private async Task UpdateTask()
        {
            foreach (var peer in _peerList)
            {
                var result = await peer.UpdateTaskList(this);

                if (result.Item1 == true)
                {
                    _eventReceivedSubject.OnNext(new UpdateTaskListEvent() { Peer = peer });
                }

                if (result.Item1 == true && result.Item2 != null && result.Item2.Count > 0)
                {
                    foreach (var item in result.Item2)
                    {
                        _eventReceivedSubject.OnNext(new DownloadSuccessEvent() { Peer = peer, Task = item });
                    }
                }
            }
        }

        public async Task LoginAllPeer()
        {
            //登录到Peer
            foreach (var peer in _peerList)
            {
                //await this.RemoteDownloadLogin(peer.PeerId);
                await peer.LoginPeer(this);
            }
        }


        #endregion


        #region Public Get

        public WkyDevice GetDeviceWithId(string deviceId)
        {
            if (_peerList != null && _peerList.Count > 0)
            {
                foreach (var peer in _peerList)
                {
                    foreach (var device in peer.Devices)
                    {
                        if (!string.IsNullOrWhiteSpace(deviceId))
                        {
                            if (device.Device.DeviceId == deviceId)
                            {
                                //await UpdateUsbInfo(device.Device.DeviceId);
                                return device;
                            }
                        }
                        else
                        {
                            //返回默认
                            return device;
                        }
                    }
                }
            }
            return null;
        }

        public List<WkyDevice> GetAllDevice()
        {
            List<WkyDevice> devices = new List<WkyDevice>();
            if (_peerList != null && _peerList.Count > 0)
            {
                foreach (var peer in _peerList)
                {
                    foreach (var device in peer.Devices)
                    {
                        if (device != null && !string.IsNullOrEmpty(device.Device.DeviceId))
                        {
                            devices.Add(device);
                        }
                    }
                }
            }
            return devices;
        }

        #endregion

        #region BaseHelper

        /// <summary>
        /// 存储的session是否过期
        /// </summary>
        /// <returns></returns>
        private bool IsSessionExpired()
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
        private string GetSessionContent()
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

        #endregion



        #region API Protocol


        /// <summary>
        /// 登录玩客云
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Login()
        {
            Dictionary<string, string> loginData;

            Dictionary<string, string> args = new Dictionary<string, string>();

            args["account_type"] = "4";

            //判断email
            if (RegexUtilities.IsValidEmail(_user))
            {
                args["phone_area"] = "Email";
                args["account_type"] = "5";
                args["mail"] = _user;
            }
            else
            {
                args["phone"] = _user;
            }

            if (_wkyLoginDeviceType == WkyLoginDeviceType.Mobile)
            {
                args["imeiid"] = GetIMEI(_user);
                args["deviceid"] = GetDevice(_user);
            }
            else if (_wkyLoginDeviceType == WkyLoginDeviceType.PC)
            {
                args["peerid"] = GetPeerId(_user);
                args["product_id"] = "0";
            }


            args["pwd"] = GetPassword(_password);

            loginData = GenerateBody(args);

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

                Debug.WriteLine(resultJson);
                if (resultRoot.ContainsKey("sMsg") && resultRoot["sMsg"].ToString() == "Success")
                {
                    Console.WriteLine("登录成功，更新UserInfo");
                    UserInfo = resultRoot["data"].ToObject<WkyApiLoginResultModel>();
                    UserInfo.CreateDateTime = DateTime.Now;
                    //_eventReceivedSubject.OnNext()
                    return true;
                }
                else
                {
                    //{"sMsg":"Parameter error","iRet":-100} //参数错误
                    if (resultRoot.ContainsKey("iRet"))
                    {
                        int iRet = resultRoot["iRet"].ToObject<int>();
                        var msg = iRet switch
                        {
                            -100 => "参数错误",
                            -129 => "账号或密码不正确",
                            _ => resultRoot.ContainsKey("sMsg") ? resultRoot["sMsg"].ToString() : "未知错误"
                        };

                        throw new WkyApiException(msg);

                    }
                    else if (resultRoot.ContainsKey("sMsg"))
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
            Debug.WriteLine(JsonConvert.SerializeObject(data));
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
            Debug.WriteLine(JsonConvert.SerializeObject(data));
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
            Debug.WriteLine(JsonConvert.SerializeObject(data));
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
                {"pid", peerId },
                {"appversion", kAppVersion },
                {"v", "1"},
                {"ct", "32"},
            }, UserInfo.SessionId);
            Debug.WriteLine(JsonConvert.SerializeObject(data));
            var result = await BaseHeaderAndCookie(kLoginRemoteDlUrl + data).GetAsync();
            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Debug.WriteLine(resultJson);
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
                { "needUrl", "1" },
                { "v", "2"},
                { "ct", "31"},
            }, UserInfo.SessionId);
            Debug.WriteLine(JsonConvert.SerializeObject(data));
            var result = await BaseHeaderAndCookie(kListRemoteDlInfoUrl + data).GetAsync();
            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Debug.WriteLine(resultJson);
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
            Debug.WriteLine(JsonConvert.SerializeObject(data));
            var result = await BaseHeaderAndCookie(kUrlResolveUrl + $"?{DictionaryToParamsString(data)}").PostUrlEncodedAsync($"url={url}");
            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Debug.WriteLine(resultJson);
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
                { "ct_ver", kAppVersion }
            };
            Debug.WriteLine(JsonConvert.SerializeObject(data));

            //System.Net.Http.ByteArrayContent httpContent = new System.Net.Http.ByteArrayContent(File.ReadAllBytes(filePath));
            var memoryStream = new FileStream(filePath, FileMode.Open);
            var result = await BaseHeaderAndCookie(kBtCheckUrl + $"?{DictionaryToParamsString(data)}")
                .PostMultipartAsync(mp => mp.AddFile("filepath", memoryStream, "dell.torrent", "application/octet-stream"));

            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Debug.WriteLine(resultJson);
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
                { "ct_ver", kAppVersion }
            };
            Debug.WriteLine(JsonConvert.SerializeObject(data));

            //System.Net.Http.ByteArrayContent httpContent = new System.Net.Http.ByteArrayContent(fileData);
            var memoryStream = new MemoryStream(fileData);

            var result = await BaseHeaderAndCookie(kBtCheckUrl + $"?{DictionaryToParamsString(data)}")
                .PostMultipartAsync(mp => mp.AddFile("filepath", memoryStream, "dell.torrent", "application/octet-stream"));

            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Debug.WriteLine(resultJson);
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
        /// <returns>有可能返回Task中存在result!=0，是重复添加等错误的返回</returns>
        public async Task<WkyApiCreateTaskResultModel> CreateTaskWithUrlResolve(string peerId, string path, WkyApiUrlResolveResultModel urlModel)
        {
            var data = new Dictionary<string, string>()
            {
                { "pid", peerId },
                { "v", "1"},
                { "ct", "31"},
                { "ct_ver", kAppVersion }
            };
            Debug.WriteLine(JsonConvert.SerializeObject(data));

            WkyApiCreateTaskModel sendModel = new WkyApiCreateTaskModel();
            Model.CreateTask.Task task = new Model.CreateTask.Task();
            task.Filesize = urlModel.TaskInfo.Size;
            task.Infohash = urlModel.Infohash;
            task.Name = urlModel.TaskInfo.Name;
            task.Url = urlModel.TaskInfo.Url;
            if (!string.IsNullOrWhiteSpace(urlModel.Infohash) && string.IsNullOrWhiteSpace(task.Url))
            {
                task.Url = $"magnet:?xt=urn:btih:{urlModel.Infohash}";
            }
            List<Model.CreateTask.Task> tasks = new List<Model.CreateTask.Task>();
            tasks.Add(task);

            sendModel.Path = path;
            sendModel.Tasks = tasks.ToArray();
            Debug.WriteLine(sendModel.ToJson());
            var result = await BaseHeaderAndCookie(kCreateTaskUrl + $"?{DictionaryToParamsString(data)}")
                .PostJsonAsync(sendModel);

            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Debug.WriteLine(resultJson);
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
        /// <returns>有可能返回Task中存在result!=0，是重复添加等错误的返回</returns>
        public async Task<WkyApiCreateTaskResultModel> CreateTaskWithBtCheck(string peerId, string path, WkyApiBtCheckResultModel urlModel)
        {
            var data = new Dictionary<string, string>()
            {
                { "pid", peerId },
                { "v", "1"},
                { "ct", "31"},
                { "ct_ver", kAppVersion }
            };
            Debug.WriteLine(JsonConvert.SerializeObject(data));

            WkyApiCreateTaskModel sendModel = new WkyApiCreateTaskModel();
            Model.CreateTask.Task task = new Model.CreateTask.Task();
            task.Filesize = long.Parse(urlModel.TaskInfo.Size);
            task.Infohash = urlModel.Infohash;
            task.Name = urlModel.TaskInfo.Name;
            task.Url = urlModel.TaskInfo.Url;
            if (!string.IsNullOrWhiteSpace(task.Infohash) && string.IsNullOrWhiteSpace(task.Url))
            {
                task.Url = $"magnet:?xt=urn:btih:{task.Infohash}";
            }

            List<Model.CreateTask.Task> tasks = new List<Model.CreateTask.Task>();
            tasks.Add(task);

            sendModel.Path = path;
            sendModel.Tasks = tasks.ToArray();
            Debug.WriteLine("sendJson:" + sendModel.ToJson());
            var result = await BaseHeaderAndCookie(kCreateTaskUrl + $"?{DictionaryToParamsString(data)}")
                .PostJsonAsync(sendModel);

            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Debug.WriteLine("resultJson:" + resultJson);
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
        /// <returns>有可能返回Task中存在result!=0，是重复添加等错误的返回</returns>
        public async Task<WkyApiCreateBatchTaskResultModel> CreateBatchTaskWithUrlResolve(string peerId, string path, WkyApiUrlResolveResultModel urlModel, List<long> subTask)
        {
            var data = new Dictionary<string, string>()
            {
                { "pid", peerId },
                { "v", "2"},
                { "ct", "31"},
                //{ "ct_ver", kAppVersion }
            };
            Debug.WriteLine(JsonConvert.SerializeObject(data));

            WkyApiCreateBatchTaskModel sendModel = new WkyApiCreateBatchTaskModel();
            Model.CreateBatchTask.Task task = new Model.CreateBatchTask.Task();
            task.Filesize = urlModel.TaskInfo.Size;
            task.Name = urlModel.TaskInfo.Name;
            task.Url = urlModel.TaskInfo.Url;
            task.Type = urlModel.TaskInfo.Type;

            task.RefUrl = "";
            task.Localfile = "";
            task.Gcid = "";
            task.Cid = "";

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

            if (!string.IsNullOrWhiteSpace(urlModel.Infohash) && string.IsNullOrWhiteSpace(task.Url))
            {
                task.Url = $"magnet:?xt=urn:btih:{urlModel.Infohash}";
            }

            List<Model.CreateBatchTask.Task> tasks = new List<Model.CreateBatchTask.Task>();
            tasks.Add(task);

            sendModel.Path = path;
            sendModel.Tasks = tasks.ToArray();
            Debug.WriteLine(sendModel.ToJson());
            var result = await BaseHeaderAndCookie(kCreateBatchTaskUrl + $"?{DictionaryToParamsString(data)}")
                .PostJsonAsync(sendModel);

            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Debug.WriteLine(resultJson);
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
        /// <returns>有可能返回Task中存在result!=0，是重复添加等错误的返回</returns>
        public async Task<WkyApiCreateBatchTaskResultModel> CreateBatchTaskWithBtCheck(string peerId, string path, WkyApiBtCheckResultModel urlModel, List<long> subTask)
        {
            var data = new Dictionary<string, string>()
            {
                { "pid", peerId },
                { "v", "2"},
                { "ct", "31"},
                //{ "ct_ver", kAppVersion }
            };
            Debug.WriteLine(JsonConvert.SerializeObject(data));

            WkyApiCreateBatchTaskModel sendModel = new WkyApiCreateBatchTaskModel();
            Model.CreateBatchTask.Task task = new Model.CreateBatchTask.Task();
            task.Filesize = long.Parse(urlModel.TaskInfo.Size);
            task.Name = urlModel.TaskInfo.Name;
            task.Url = urlModel.TaskInfo.Url;
            task.Type = urlModel.TaskInfo.Type;

            task.RefUrl = "";
            task.Localfile = "";
            task.Gcid = "";
            task.Cid = "";

            if (!string.IsNullOrWhiteSpace(urlModel.Infohash) && string.IsNullOrWhiteSpace(task.Url))
            {
                task.Url = $"magnet:?xt=urn:btih:{urlModel.Infohash}";
            }

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

            Debug.WriteLine(sendModel.ToJson());

            var result = await BaseHeaderAndCookie(kCreateBatchTaskUrl + $"?{DictionaryToParamsString(data)}")
                .PostJsonAsync(sendModel);

            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                string resultJson = await result.GetStringAsync();
                Debug.WriteLine(resultJson);
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
            Debug.WriteLine(JsonConvert.SerializeObject(data));
            var result = await BaseHeaderAndCookie(kPauseTaskUrl + $"?{DictionaryToParamsString(data)}").GetAsync();
            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                //{"tasks":[],"rtn":0}
                string resultJson = await result.GetStringAsync();
                Debug.WriteLine(resultJson);
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
            Debug.WriteLine(JsonConvert.SerializeObject(data));
            var result = await BaseHeaderAndCookie(kStartTaskUrl + $"?{DictionaryToParamsString(data)}").GetAsync();
            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                //{"tasks":[],"rtn":0}
                string resultJson = await result.GetStringAsync();
                Debug.WriteLine(resultJson);
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
            Debug.WriteLine(JsonConvert.SerializeObject(data));
            var result = await BaseHeaderAndCookie(kDelTaskUrl + $"?{DictionaryToParamsString(data)}").GetAsync();
            JsonConvert.SerializeObject(result.Cookies);
            if (result.StatusCode == 200)
            {
                //{"tasks":[],"rtn":0}
                string resultJson = await result.GetStringAsync();
                Debug.WriteLine(resultJson);
                JObject root = JObject.Parse(resultJson);
                if (root.ContainsKey("rtn") && root["rtn"].ToObject<int>() == 0)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        #endregion
    }
}
