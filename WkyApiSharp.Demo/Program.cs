using System;
using System.Reactive.Linq;
using WkyApiSharp.Events.Account;
using WkyApiSharp.Service;

namespace WkyApiSharp.Demo // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");


            Console.WriteLine("Hello World!");
            //账号密码初始化
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

            var result = wkyApi.StartLogin().Result;
            if (result)
            {
                //DeviceTest(wkyApi);
            }

            Console.ReadLine();
        }
    }
}