using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TCM_Launcher.Model.UI.Forge
{
    class ForgeLoaderJSONData
    {
        [JsonPropertyName("version")]
        public string VersionName { get; set; }
        [JsonPropertyName("isRecommended")]
        public bool IsRecommended { get; set; }
    }
}
