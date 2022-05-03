using System;
using System.Collections.Generic;
using System.Text;
using WkyApiSharp.Service.Model;

namespace WkyApiSharp.Events.Account
{
    /// <summary>
    /// 下载完成时发送事件
    /// </summary>
    public record DownloadSuccessEvent : EventBase
    {
        //TODO 设备信息
        public WkyPeer Peer { get; set; }


        //TODO 任务信息
        public WkyTask Task { get; set; }

        /// <summary>
        /// 事件类型
        /// </summary>
        public override Events Type { get; set; } = Events.DownloadSuccessEvent;
    }
}
