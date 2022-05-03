using System;
using System.Collections.Generic;
using System.Text;

namespace WkyApiSharp.Events.Account
{
    public record LoginResultEvent : EventBase
    {
        public bool isSuccess { get; set; }


        public string errorMessage { get; set; }

        /// <summary>
        /// 事件类型
        /// </summary>
        public override Events Type { get; set; } = Events.LoginResultEvent;
    }
}
