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

            //DirectoryInfo dirInfo = new DirectoryInfo(@"\\192.168.50.7\872d\hahaha");
            //if (dirInfo.Exists == false)
            //{
            //    dirInfo.Create();
            //}
            //File.WriteAllText(@"\\192.168.50.7\872d\hahaha.txt", "11");


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

            Console.ReadLine();
        }
    }
}