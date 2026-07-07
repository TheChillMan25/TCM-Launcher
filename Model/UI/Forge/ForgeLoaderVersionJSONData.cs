using System.Text.Json.Serialization;
using TCM_Launcher.Model.UI.Forge;

namespace TCM_Launcher.Model.UI
{
    class ForgeLoaderVersionJSONData
    {
        [JsonPropertyName("mc_version")]
        public string MCVersion { get; set; }
        [JsonPropertyName("versions")]
        public List<ForgeLoaderJSONData> Versions { get; set; }

        public ForgeLoaderVersionJSONData()
        {
            this.Versions = new List<ForgeLoaderJSONData>();
        }
    }
}
