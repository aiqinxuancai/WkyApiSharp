# WkyApiSharp
玩客云c# api

## 快速开始
```csharp

var user = File.ReadAllText("user.txt");
            var password = File.ReadAllText("password.txt");
            WkyApi wkyApi = new WkyApi(user, password, WkyLoginDeviceType.PC);

            wkyApi.EventReceived
                .OfType<LoginResultEvent>()
                .Subscribe(async r =>
                {
                    Console.WriteLine("登录成功");
                });

            wkyApi.EventReceived
                .OfType<UpdateDeviceResultEvent>()
                .Subscribe(async r =>
                {
                    Console.WriteLine("设备更新完毕");
                });

            wkyApi.EventReceived
                .OfType<DownloadSuccessEvent>()
                .Subscribe(async r =>
                {
                    Console.WriteLine("下载完成");
                });

            wkyApi.EventReceived
                .OfType<UpdateTaskListEvent>()
                .Subscribe(async r =>
                {
                    Console.WriteLine("任务列表更新");
                });

            var result = wkyApi.StartLogin().Result;
            if (result)
            {
                //DeviceTest(wkyApi);
      


            }

```
