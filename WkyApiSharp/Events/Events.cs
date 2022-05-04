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
        /// 登录成功结果
        /// </summary>
        [Description("LoginResultEvent")]
        [EnumMember(Value = "LoginResultEvent")]
        LoginResultEvent,


        /// <summary>
        /// 设备信息更新结果
        /// </summary>
        [Description("UpdateDeviceResultEvent")]
        [EnumMember(Value = "UpdateDeviceResultEvent")]
        UpdateDeviceResultEvent,


        /// <summary>
        /// 下载成功
        /// </summary>
        [Description("DownloadSuccessEvent")]
        [EnumMember(Value = "DownloadSuccessEvent")]
        DownloadSuccessEvent,


        /// <summary>
        /// 任务列表更新
        /// </summary>
        [Description("UpdateTaskListEvent")]
        [EnumMember(Value = "UpdateTaskListEvent")]
        UpdateTaskListEvent,

        



    }
}
