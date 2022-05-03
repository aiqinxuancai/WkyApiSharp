using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WkyApiSharp.Service.Model
{
    
    public record WkyDevice
    {
        public WkyDevice(ListPeer.Device device)
        {
            Device = device;
        }


        private ListPeer.Device _device;

        //属于的Peer
        public ListPeer.Device Device
        {
            set
            {
                _device = value;
            }
        }

        /// <summary>
        /// 磁盘对应的分区
        /// </summary>
        public List<GetUsbInfo.Partition> Partitions { get; set; } = new List<GetUsbInfo.Partition>();



        /// <summary>
        /// 磁盘对应的分区
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public async Task UpdateDiskInfo(WkyApi api)
        {
            for (int i = 0; i < 3; i++) //重试3次
            {
                try
                {
                    var diskInfo = await api.GetUsbInfo(_device.DeviceId);
                    if (diskInfo.Rtn == 0)
                    {
                        Partitions.Clear();
                        foreach (var disk in diskInfo.Result)
                        {
                            Partitions.AddRange(disk.ResultClass.Partitions);
                        }
                        break;
                    }
                }
                catch (Exception ex)
                {
                    //获取磁盘信息失败
                }
            }
        }

    }
}
