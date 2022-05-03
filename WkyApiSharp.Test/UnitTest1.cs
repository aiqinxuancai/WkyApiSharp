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
                //�ϴε�¼�ļ���ʼ��
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
                //�˺������ʼ��
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
            //��ȡ�豸��Ϣ
            var listPeerResult = wkyApi.ListPeer().Result;

            //var checkSessionResult = wkyApi.CheckSession(wkyApi.UserInfo.UserId, wkyApi.UserInfo.SessionId).Result;

            foreach (var item in listPeerResult.Result)
            {
                if (item.ResultClass != null)
                {
                    foreach (var device in item.ResultClass.Devices)
                    {
                        //��ȡ�豸��USB��Ϣ���洢�豸��Ϣ��
                        var getUsbInfoResult = wkyApi.GetUsbInfo(device.DeviceId).Result;

                        Console.Write(getUsbInfoResult.ToString());

                        //��¼Զ�������豸
                        var remoteDownloadLoginResult = wkyApi.RemoteDownloadLogin(device.Peerid).Result;

                        if (remoteDownloadLoginResult != null && remoteDownloadLoginResult.Rtn == 0)
                        {
                            //��ȡ�豸��ǰ�����������б�
                            var test = wkyApi.RemoteDownloadList(device.Peerid).Result;

                            //��ȡURL��Ϣ�������Ǵ�������http���������Ӿ���Ҫ��
                            var urlResult = wkyApi.UrlResolve(device.Peerid, "magnet:?xt=urn:btih:1fbd4ead642a5da026d8819b6ada3ad257d0964b&tr=http%3a%2f%2ft.nyaatracker.com%2fannounce&tr=http%3a%2f%2ftracker.kamigami.org%3a2710%2fannounce&tr=http%3a%2f%2fshare.camoe.cn%3a8080%2fannounce&tr=http%3a%2f%2fopentracker.acgnx.se%2fannounce&tr=http%3a%2f%2fanidex.moe%3a6969%2fannounce&tr=http%3a%2f%2ft.acg.rip%3a6699%2fannounce&tr=https%3a%2f%2ftr.bangumi.moe%3a9696%2fannounce&tr=udp%3a%2f%2ftr.bangumi.moe%3a6969%2fannounce&tr=http%3a%2f%2fopen.acgtracker.com%3a1096%2fannounce&tr=udp%3a%2f%2ftracker.opentrackr.org%3a1337%2fannounce").Result;

                            //�ϴ�����BT�ļ���ȡ��Ϣ
                            //var btResult = wkyApi.BtCheck(device.Peerid, @"C:\Users\aiqin\OneDrive\����\����������Ļ�顿[�D-Saki-][����齫��Ů][01-25][GB][848x480][MKV][ȫ].torrent").Result;

                            //��ȡ����USB��·����������Ŀ¼
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

                            //��������
                            var createResult = wkyApi.CreateTaskWithUrlResolve(device.Peerid, savePath, urlResult).Result;
                            if (createResult.Rtn == 0)
                            {
                                //�������ɹ�
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
