﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var wkyApiRemoteDownloadLoginResultModel = WkyApiRemoteDownloadLoginResultModel.FromJson(jsonString);

namespace WkyApiSharp.Service.Model.RemoteDownloadLogin
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class WkyApiRemoteDownloadLoginResultModel
    {
        [JsonProperty("pathList")]
        public string[] PathList { get; set; }

        [JsonProperty("clientVersion")]
        public long ClientVersion { get; set; }

        [JsonProperty("rtn")]
        public long Rtn { get; set; }

        [JsonProperty("msg")]
        public string Msg { get; set; }
    }

    public partial class WkyApiRemoteDownloadLoginResultModel
    {
        public static WkyApiRemoteDownloadLoginResultModel FromJson(string json) => JsonConvert.DeserializeObject<WkyApiRemoteDownloadLoginResultModel>(json, WkyApiSharp.Service.Model.RemoteDownloadLogin.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this WkyApiRemoteDownloadLoginResultModel self) => JsonConvert.SerializeObject(self, WkyApiSharp.Service.Model.RemoteDownloadLogin.Converter.Settings);
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
