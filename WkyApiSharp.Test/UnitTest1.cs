using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using WkyApiSharp.Events.Account;
using WkyApiSharp.Service;
using WkyApiSharp.Utils;

namespace WkyApiSharp.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void LoginAndDownloadTest()
        {

            Console.WriteLine("Hello World!");
            if (File.Exists("session.json"))
            {
                //上次登录文件初始化
                WkyApi wkyApi = new WkyApi(File.ReadAllText("session.json"));


                wkyApi.EventReceived
                .OfType<LoginResultEvent>()
                .Subscribe(async r =>
                {

                });


                //var listPeerResult = wkyApi.ListPeer().Result;
                DeviceTest(wkyApi);
            }
            else
            {
                //账号密码初始化
                var user = File.ReadAllText("user.txt");
                var password = File.ReadAllText("password.txt");
                WkyApi wkyApi = new WkyApi(user, password);
                var result = wkyApi.Login().Result;
                if (result)
                {
                    var cookiesString = wkyApi.GetSessionContent();
                    File.WriteAllText("session.json", cookiesString);

                    DeviceTest(wkyApi);

                }
            }
        }


        public void DeviceTest(WkyApi wkyApi)
        {
            //获取设备信息
            var listPeerResult = wkyApi.ListPeer().Result;

            //var checkSessionResult = wkyApi.CheckSession(wkyApi.UserInfo.UserId, wkyApi.UserInfo.SessionId).Result;

            foreach (var item in listPeerResult.Result)
            {
                if (item.ResultClass != null)
                {
                    foreach (var device in item.ResultClass.Devices)
                    {
                        //获取设备的USB信息（存储设备信息）
                        var getUsbInfoResult = wkyApi.GetUsbInfo(device.DeviceId).Result;

                        Console.Write(getUsbInfoResult.ToString());

                        //登录远程下载设备
                        var remoteDownloadLoginResult = wkyApi.RemoteDownloadLogin(device.Peerid).Result;

                        if (remoteDownloadLoginResult != null && remoteDownloadLoginResult.Rtn == 0)
                        {
                            //读取设备当前的下载任务列表
                            var test = wkyApi.RemoteDownloadList(device.Peerid).Result;

                            //获取URL信息（不管是磁力还是http等下载链接均需要）
                            var urlResult = wkyApi.UrlResolve(device.Peerid, "magnet:?xt=urn:btih:1fbd4ead642a5da026d8819b6ada3ad257d0964b&tr=http%3a%2f%2ft.nyaatracker.com%2fannounce&tr=http%3a%2f%2ftracker.kamigami.org%3a2710%2fannounce&tr=http%3a%2f%2fshare.camoe.cn%3a8080%2fannounce&tr=http%3a%2f%2fopentracker.acgnx.se%2fannounce&tr=http%3a%2f%2fanidex.moe%3a6969%2fannounce&tr=http%3a%2f%2ft.acg.rip%3a6699%2fannounce&tr=https%3a%2f%2ftr.bangumi.moe%3a9696%2fannounce&tr=udp%3a%2f%2ftr.bangumi.moe%3a6969%2fannounce&tr=http%3a%2f%2fopen.acgtracker.com%3a1096%2fannounce&tr=udp%3a%2f%2ftracker.opentrackr.org%3a1337%2fannounce").Result;

                            //上传本地BT文件获取信息
                            //var btResult = wkyApi.BtCheck(device.Peerid, @"C:\Users\aiqin\OneDrive\种子\【酢浆草字幕组】[咲-Saki-][天才麻将少女][01-25][GB][848x480][MKV][全].torrent").Result;

                            //获取本地USB的路径生成下载目录
                            var savePath = "";
                            foreach (var disk in getUsbInfoResult.Result)
                            {
                                if (disk.ResultClass != null)
                                {
                                    foreach (var partition in disk.ResultClass.Partitions)
                                    {
                                        savePath = partition.Path + "/onecloud/tddownload";
                                    }
                                }

                            }

                            //创建任务
                            var createResult = wkyApi.CreateTaskWithUrlResolve(device.Peerid, savePath, urlResult).Result;
                            if (createResult.Rtn == 0)
                            {
                                //添加任务成功
                            }

                        }
                    }
                }
            }
        }


        [TestMethod]
        public void CompareStringTest()
        {
            //Debug.WriteLine(PythonSort.CompareString("CRRtyEb9GTKFo", "AnG0uH7X0WFeFy"));

            //Debug.WriteLine(PythonSort.CompareString("hBownxyj4yYx", "psD4O"));

            //Debug.WriteLine(PythonSort.CompareString("cugulmKdciHQZDKB", "Btf3XpMvouvEo0fJ"));

            //Debug.WriteLine(PythonSort.CompareString("6xfA1IyQRYkrG2MN", "bgp3oGHs44YnWc07"));

            //Debug.WriteLine(PythonSort.CompareString("Sn9xQ73jVYSim2B3", "Jl79IGZtqTphlxn5"));

            //Debug.WriteLine(PythonSort.CompareString("Sn9xQ73jVYSim2B3", "Sn9xQ73jVYSim2B3"));

            Debug.WriteLine("---------------------");

            Debug.WriteLine(PythonSort.ByteArrayCompareV2("CRRtyEb9GTKFo", "AnG0uH7X0WFeFy"));

            Debug.WriteLine(PythonSort.ByteArrayCompareV2("hBownxyj4yYx", "psD4O"));

            Debug.WriteLine(PythonSort.ByteArrayCompareV2("cugulmKdciHQZDKB", "Btf3XpMvouvEo0fJ"));

            Debug.WriteLine(PythonSort.ByteArrayCompareV2("6xfA1IyQRYkrG2MN", "bgp3oGHs44YnWc07"));

            Debug.WriteLine(PythonSort.ByteArrayCompareV2("Sn9xQ73jVYSim2B3", "Jl79IGZtqTphlxn5"));

            Debug.WriteLine(PythonSort.ByteArrayCompareV2("Sn9xQ73jVYSim2B3", "Sn9xQ73jVYSim2B3"));
        }
    }
}
