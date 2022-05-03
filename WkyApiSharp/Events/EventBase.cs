
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WkyApiSharp.Events
{
    /// <summary>
    /// 事件基类
    /// </summary>
    public record EventBase
    {
        protected EventBase()
        {
        }

        /// <summary>
        /// 事件类型
        /// </summary>
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual Events Type { get; set; }


    }
}

