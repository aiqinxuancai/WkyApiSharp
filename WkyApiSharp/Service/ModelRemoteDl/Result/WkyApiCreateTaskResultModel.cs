﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var wkyApiCreateTaskResultModel = WkyApiCreateTaskResultModel.FromJson(jsonString);

namespace WkyApiSharp.Service.Model.CreateTaskResult
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class WkyApiCreateTaskResultModel
    {
        [JsonProperty("tasks")]
        public Task[] Tasks { get; set; }

        [JsonProperty("rtn")]
        public long Rtn { get; set; }

        [JsonProperty("msg")]
        public string Msg { get; set; }
    }

    public partial class Task
    {
        [JsonProperty("msg")]
        public string Msg { get; set; }

        [JsonProperty("taskid")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Taskid { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("result")]
        public long Result { get; set; }
    }

    public partial class WkyApiCreateTaskResultModel
    {
        public static WkyApiCreateTaskResultModel FromJson(string json) => JsonConvert.DeserializeObject<WkyApiCreateTaskResultModel>(json, WkyApiSharp.Service.Model.CreateTaskResult.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this WkyApiCreateTaskResultModel self) => JsonConvert.SerializeObject(self, WkyApiSharp.Service.Model.CreateTaskResult.Converter.Settings);
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
