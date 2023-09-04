using Newtonsoft.Json;
using System.IO;
using System.Linq;
using UnityEditor;

namespace AssetAdvisor
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;


    public class AssetAdvisorManager
    {
        //---------------------------------------------------------------------------------------------------
        // Constants
        //---------------------------------------------------------------------------------------------------
        private const string c_assetAdvisorData = "asset_advisor.data";
        private const string c_assetAdvisorDataSample = "asset_advisor_data_sample.data";
        private readonly static string c_dataPath = $"{Application.dataPath}/../AssetAdvisor/";

        //---------------------------------------------------------------------------------------------------
        // Variables
        //---------------------------------------------------------------------------------------------------
        private static Dictionary<string, AssetAdvisorData> s_cachedData = new Dictionary<string, AssetAdvisorData>();

        //---------------------------------------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------------------------------------
        public static Dictionary<string, AssetAdvisorData> AssetAdvisorData => s_cachedData;
        public static List<AssetAdvisorData> AssetAdvisorDataAsList {
            get
            {
                return s_cachedData.Select(kv => new AssetAdvisorData
                {
                    m_assetName = kv.Key,
                    m_warningMessage = kv.Value.m_warningMessage,
                    m_author = kv.Value.m_author
                }).ToList();
            }
        }
        public static string DataFilePath => $"{c_dataPath}{c_assetAdvisorData}";

        //---------------------------------------------------------------------------------------------------
        // Methods
        //---------------------------------------------------------------------------------------------------
        [InitializeOnLoadMethod]
        public static void ReloadAssetAdvisor()
        {
            ValidateDirectory();
            LoadAssetAdvisorData();
        }

        //---------------------------------------------------------------------------------------------------
        public static void HandleFileRenamingOrMoving (string filePath, string newFilePath)
        {
            if (s_cachedData.TryGetValue(filePath, out AssetAdvisorData assetData))
            {
                s_cachedData.Remove(filePath);
                s_cachedData[newFilePath] = assetData;
            }

            SaveCachedData();
        }

        //---------------------------------------------------------------------------------------------------
        public static void UpdateAssetData (AssetAdvisorData assetData)
        {
            string key = assetData.m_assetName;

            if (s_cachedData.ContainsKey(key))
            {
                s_cachedData[key] = assetData;
            }

            SaveCachedData();
        }

        //---------------------------------------------------------------------------------------------------
        private static void ValidateDirectory()
        {
            DirectoryInfo directory = new DirectoryInfo(c_dataPath);

            if (!directory.Exists)
            {
                directory.Create();
            }
        }

        //---------------------------------------------------------------------------------------------------
        private static void LoadAssetAdvisorData()
        { 
            string dataFilePath = Path.Combine(Path.GetFullPath(c_dataPath), c_assetAdvisorData);

            List<AssetAdvisorData> data;

            if (File.Exists(dataFilePath))
            {
                string jsonData = File.ReadAllText(dataFilePath);
                data = JsonConvert.DeserializeObject<List<AssetAdvisorData>>(jsonData);
            }
            else
            {
                data = new List<AssetAdvisorData>();
            }

            s_cachedData.Clear();

            foreach (AssetAdvisorData assetAdvisorData in data)
            {
                s_cachedData[assetAdvisorData.m_assetName] = assetAdvisorData;
            }

            data.Clear();
        }

        //---------------------------------------------------------------------------------------------------
        private static void SaveListOfAssetAdvisorData(string targetLocation, List<AssetAdvisorData> assetDataList)
        {
            string jsonData = JsonConvert.SerializeObject(assetDataList, Formatting.Indented);
            File.WriteAllText(targetLocation, jsonData);
        }

        //---------------------------------------------------------------------------------------------------
        private static void SaveCachedData()
        {
            if (s_cachedData.Count == 0)
            {
                return;
            }

            SaveListOfAssetAdvisorData(Path.Combine(c_dataPath, c_assetAdvisorData), AssetAdvisorDataAsList);
        }

        //---------------------------------------------------------------------------------------------------
        [MenuItem("Tools/AssetAdvisor/Generate Sample Data")]
        private static void SaveJsonFile ()
        {
            GenerateAndSaveAssetAdvisorData(Path.Combine(c_dataPath, c_assetAdvisorDataSample));
        }

        //---------------------------------------------------------------------------------------------------
        public static void GenerateAndSaveAssetAdvisorData (string targetLocation)
        {
            List<AssetAdvisorData> assetDataList = new List<AssetAdvisorData>();

            for (int i = 0; i < 50; i++)
            {
                AssetAdvisorData assetData = new AssetAdvisorData
                {
                    m_assetName = $"Assets/Asset{i}.prefab",
                    m_warningMessage = $"Please make sure to save Asset{i} as disabled ('Active=false')",
                    m_author = System.Environment.UserName
                };

                assetDataList.Add(assetData);
            }

            SaveListOfAssetAdvisorData(targetLocation, assetDataList);
        }
    }
}