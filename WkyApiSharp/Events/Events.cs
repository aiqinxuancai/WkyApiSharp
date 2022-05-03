using System.ComponentModel;
using System.Runtime.Serialization;

namespace WkyApiSharp.Events
{
    /// <summary>
    /// 事件类型
    /// </summary>
    public enum Events
    {
        /// <summary>
        /// 登录成功
        /// </summary>
        [Description("LoginResultEvent")]
        [EnumMember(Value = "LoginResultEvent")]
        LoginResultEvent,

    }
}
