using System.Text.Json.Serialization;

namespace TCM_Launcher.Model.UI
{
    class VanillaVersionJSONData
    {
        [JsonPropertyName("versions")]
        public List<string> Versions { get; set; }
        public VanillaVersionJSONData()
        {
            Versions = new List<string>();
        }
    }
}
