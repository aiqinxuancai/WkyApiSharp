﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var wkyApiGetTurnServerResultModel = WkyApiGetTurnServerResultModel.FromJson(jsonString);

namespace WkyApiSharp.Service.Model.GetTurnServerResult
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class WkyApiGetTurnServerResultModel
    {
        [JsonProperty("turn_server_addr")]
        public TurnServerAddr TurnServerAddr { get; set; }

        [JsonProperty("rtn")]
        public long Rtn { get; set; }

        [JsonProperty("msg")]
        public string Msg { get; set; }
    }

    public partial class TurnServerAddr
    {
        [JsonProperty("aes_key")]
        public long[] AesKey { get; set; }

        [JsonProperty("port")]
        public long Port { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("mode")]
        public long Mode { get; set; }

        [JsonProperty("aes_on")]
        public bool AesOn { get; set; }

        [JsonProperty("sessionid")]
        public string Sessionid { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }

    public partial class WkyApiGetTurnServerResultModel
    {
        public static WkyApiGetTurnServerResultModel FromJson(string json) => JsonConvert.DeserializeObject<WkyApiGetTurnServerResultModel>(json, WkyApiSharp.Service.Model.GetTurnServerResult.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this WkyApiGetTurnServerResultModel self) => JsonConvert.SerializeObject(self, WkyApiSharp.Service.Model.GetTurnServerResult.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
