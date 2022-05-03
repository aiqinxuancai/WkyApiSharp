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
        public bool IsSuccess { get; set; }

        //TODO 设备信息
        //TODO 任务信息
        
        /// <summary>
        /// 事件类型
        /// </summary>
        public override Events Type { get; set; } = Events.DownloadSuccessEvent;
    }
}
