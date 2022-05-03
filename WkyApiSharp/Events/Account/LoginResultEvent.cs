using System;
using System.Collections.Generic;
using System.Text;

namespace WkyApiSharp.Events.Account
{
    public record LoginResultEvent : EventBase
    {
        public bool IsSuccess { get; set; }


        public string ErrorMessage { get; set; }


        public LoginResultEvent(bool isSuccess, string errorMessage) => (IsSuccess, ErrorMessage) = (isSuccess, errorMessage);

        /// <summary>
        /// 事件类型
        /// </summary>
        public override Events Type { get; set; } = Events.LoginResultEvent;
    }
}
