﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var wkyApiUrlResolveResultModel = WkyApiUrlResolveResultModel.FromJson(jsonString);

namespace WkyApiSharp.Service.Model.UrlResolve
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class WkyApiUrlResolveResultModel
    {
        [JsonProperty("infohash")]
        public string Infohash { get; set; }

        [JsonProperty("taskInfo")]
        public TaskInfo TaskInfo { get; set; }

        [JsonProperty("rtn")]
        public long Rtn { get; set; }
    }

    public partial class TaskInfo
    {
        [JsonProperty("size")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Size { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("subList")]
        public SubList[] SubList { get; set; }

        [JsonProperty("type")]
        public long Type { get; set; }
    }

    public partial class SubList
    {
        [JsonProperty("selected")]
        public long Selected { get; set; }

        [JsonProperty("size")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Size { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }
    }

    public partial class WkyApiUrlResolveResultModel
    {
        public static WkyApiUrlResolveResultModel FromJson(string json) => JsonConvert.DeserializeObject<WkyApiUrlResolveResultModel>(json, WkyApiSharp.Service.Model.UrlResolve.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this WkyApiUrlResolveResultModel self) => JsonConvert.SerializeObject(self, WkyApiSharp.Service.Model.UrlResolve.Converter.Settings);
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

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}
