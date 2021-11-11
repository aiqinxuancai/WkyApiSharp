# WkyApiSharp
玩客云c# api

## 快速开始
```csharp

public void LoginAndDownloadTest()
{
    Console.WriteLine("Hello World!");
    if (File.Exists("session.json"))
    {
        //上次登录文件初始化
        WkyApi wkyApi = new WkyApi(File.ReadAllText("session.json"));
        //var listPeerResult = wkyApi.ListPeer().Result;
        DeviceTest(wkyApi);
    }
    else
    {
        //账号密码初始化
        WkyApi wkyApi = new WkyApi("*", "*");
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
                    var urlResult = wkyApi.UrlResolve(device.Peerid, "magnet:?xt=urn:btih:f55b8dfb173f0506fb09781c4a2e3df27ef240ed").Result;

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

```
