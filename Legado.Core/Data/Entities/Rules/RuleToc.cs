using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Legado.Core.Data.Entities.Rules
{


    /// <summary>
    /// 目录规则实体
    /// 对应 Kotlin: io.legado.app.data.entities.rule.TocRule
    /// </summary>
    [JsonConverter(typeof(TocRuleConverter))]
    public class TocRule
    {
        [JsonProperty("preUpdateJs")]
        public string PreUpdateJs { get; set; }

        [JsonProperty("chapterList")]
        public string ChapterList { get; set; }

        [JsonProperty("chapterName")]
        public string ChapterName { get; set; }

        [JsonProperty("chapterUrl")]
        public string ChapterUrl { get; set; }

        [JsonProperty("formatJs")]
        public string FormatJs { get; set; }

        [JsonProperty("isVolume")]
        public string IsVolume { get; set; }

        [JsonProperty("isVip")]
        public string IsVip { get; set; }

        [JsonProperty("isPay")]
        public string IsPay { get; set; }

        [JsonProperty("updateTime")]
        public string UpdateTime { get; set; }

        [JsonProperty("nextTocUrl")]
        public string NextTocUrl { get; set; }
    }

    /// <summary>
    /// 自定义 JSON 转换器
    /// 处理 Legado 数据中可能是 Object 也可能是 String 的情况
    /// </summary>
    public class TocRuleConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TocRule);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // 情况 1: 值为 null
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            // 情况 2: 值为 String (Legacy 数据或特殊格式)
            // 对应 Kotlin: json.isJsonPrimitive -> fromJson(json.asString)
            if (reader.TokenType == JsonToken.String)
            {
                string jsonString = reader.Value.ToString();
                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    return null;
                }
                try
                {
                    return JsonConvert.DeserializeObject<TocRule>(jsonString);
                }
                catch
                {
                    return null;
                }
            }

            // 情况 3: 值为 JSON Object (标准格式)
            // 对应 Kotlin: json.isJsonObject -> fromJson(json)
            if (reader.TokenType == JsonToken.StartObject)
            {
                // 为了防止无限递归调用 converter，我们需要加载为 JObject 然后填充
                JObject jObject = JObject.Load(reader);
                var rule = new TocRule();
                serializer.Populate(jObject.CreateReader(), rule);
                return rule;
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // 默认序列化为对象
            serializer.Serialize(writer, value);
        }
    }
}
