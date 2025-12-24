using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Legado.Core.Helps.Login
{
    public class LoginUI
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("action")]
        public string Action { get; set; }
    }
}
