using System;
using System.Collections.Generic;
using System.Text;
using WkyApiSharp.Service.Model;
using WkyApiSharp.Service.Model.ListPeer;

namespace WkyApiSharp.Service
{
    partial class WkyApi
    {
        /// <summary>
        /// 获取当前所有的分区信息，只有更新完硬件信息后才可得到数据
        /// </summary>
        /// <returns></returns>
        public List<WkyPartition> GetAllPartitions()
        {
            List <WkyPartition> partitions = new List<WkyPartition>();
            foreach (var peer in _peerList)
            {
                foreach (var device in peer.Devices)
                {
                    partitions.AddRange(device.Partitions);
                }
            }
            return partitions;
        }
    }
}
