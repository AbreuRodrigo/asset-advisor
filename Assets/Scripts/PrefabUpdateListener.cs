using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace AssetAdvisor
{
    public class PrefabUpdateListener : AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets (string[] paths)
        {
            Dictionary<string, AssetAdvisorData> assetAdvisorData = AssetAdvisorManager.AssetAdvisorData;

            foreach (string filePath in paths)
            {
                assetAdvisorData.TryGetValue(filePath, out AssetAdvisorData assetData);

                if (!string.IsNullOrWhiteSpace(assetData.m_assetName) &&
                    !string.IsNullOrWhiteSpace(assetData.m_warningMessage))
                {
                    EditorUtility.DisplayDialog($"{filePath} Warning!", assetData.m_warningMessage, "Ok");
                }
            }

            return paths;
        }

        //public static AssetDeleteResult OnWillDeleteAsset (string assetPath, RemoveAssetOptions options)
        //{
        //    return AssetDeleteResult.DidDelete;
        //}

        public static AssetMoveResult OnWillMoveAsset (string sourcePath, string destinationPath)
        {            
            Dictionary<string, AssetAdvisorData> assetAdvisorData = AssetAdvisorManager.AssetAdvisorData;

            if (assetAdvisorData.ContainsKey(sourcePath))
            {
                if (!sourcePath.Equals(destinationPath))
                {
                    Debug.Log($"{sourcePath} is changing to {destinationPath}");
                    AssetAdvisorManager.HandleFileRenamingOrMoving(sourcePath, destinationPath);
                }
                else
                {
                    Debug.Log($"Filename did not change for {sourcePath}");
                }
            }

            File.Move(sourcePath, destinationPath);

            return AssetMoveResult.DidMove;
        }
    }
}
