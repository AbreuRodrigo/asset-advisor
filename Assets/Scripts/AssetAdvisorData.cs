using Newtonsoft.Json;
using System;

namespace AssetAdvisor
{
    [Serializable]
    public struct AssetAdvisorData
    {
        [JsonProperty("asset")] public string m_assetName;
        [JsonProperty("message")] public string m_warningMessage;
        [JsonProperty("author")] public string m_author;
    }
}