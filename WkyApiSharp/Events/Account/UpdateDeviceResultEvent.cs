using System;
using System.Collections.Generic;
using System.Text;
using WkyApiSharp.Service.Model;

namespace WkyApiSharp.Events.Account
{
    /// <summary>
    /// 在更新完所有设备及分区信息后发送此事件
    /// </summary>
    public record UpdateDeviceResultEvent : EventBase
    {
        public bool IsSuccess { get; set; }

        public List<WkyPeer> PeerList { get; set; }
        

        /// <summary>
        /// 事件类型
        /// </summary>
        public override Events Type { get; set; } = Events.UpdateDeviceResultEvent;
    }
}
