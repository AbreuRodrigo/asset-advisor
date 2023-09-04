namespace AssetAdvisor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;

    public class ItemEditorWindow : EditorWindow
    {
        public static void ShowWindow ()
        {
            var window = GetWindow<ItemEditorWindow>(true, "New Asset Advise");
            window.CenterOnMainWin(0, 0);
            window.Show();                        
        }

        void OnGUI ()
        {
            // Add GUI controls for editing item stats here.
        }
    }

    public struct AssetEditData
    {
        public int m_indexInEditMode;
        public AssetAdvisorData? m_data;
    }

    public class AssetAdvisorWindow : EditorWindow
    {
        private const float c_width60 = 60;
        private const float c_width85 = 85;
        private const float c_width150 = 150;

        //---------------------------------------------------------------------------------------------------
        // Variables
        //---------------------------------------------------------------------------------------------------
        private static List<AssetAdvisorData> s_assetDataList;
        private static List<AssetAdvisorData> s_filteredDataList;        
        
        private static GUIStyle s_oddRowStyle;
        private static GUIStyle s_evenRowStyle;
        private static GUIStyle s_titleRowStyle;

        private static Vector2 s_scrollPosition = Vector2.zero;        
        private static SearchField s_searchField;
        private static string s_searchTerm = "";
        private static float s_totalWidth;
        private static bool s_isInEditMode;

        private static AssetEditData s_editData;

        //---------------------------------------------------------------------------------------------------
        // Methods
        //---------------------------------------------------------------------------------------------------
        [MenuItem("Tools/AssetAdvisor/Manage AssetAdvisor")]
        private static void OpenWindow ()
        {
            AssetAdvisorWindow window = GetWindow<AssetAdvisorWindow>(false, "Asset Advisor");
            window.minSize = new Vector2(1024, 400);
            window.maximized = true;
            s_editData = new AssetEditData() { m_indexInEditMode = -1};
        }

        //---------------------------------------------------------------------------------------------------
        private void OnEnable ()
        {
            s_isInEditMode = false;
            s_editData = default;

            s_searchField = new SearchField();
            s_filteredDataList = new List<AssetAdvisorData>();
            s_assetDataList = AssetAdvisorManager.AssetAdvisorDataAsList;
        }

        //---------------------------------------------------------------------------------------------------
        private void OnGUI()
        {
            s_totalWidth = 0;

            CreateStyles();

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                DrawToolbarButton("Create New", c_width85, CreateNew);
                DrawToolbarButton("Save Data", c_width85, SaveData);
                DrawToolbarButton("Open Data Location", c_width150, OpenFileLocation);
            }
            GUILayout.EndHorizontal();

            float tableWidth = position.width;

            // Draw the search input field
            //
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                s_searchTerm = s_searchField.OnGUI(s_searchTerm);
            }
            GUILayout.EndHorizontal();                        

            float indexWidth = 45;
            float actionsWidth = c_width60 * 2;
            float availableWidth = tableWidth - (indexWidth + actionsWidth);

            float assetWidth = availableWidth * 0.15f;
            float messageWidth = availableWidth * 0.75f;
            float authorWidth = availableWidth * 0.10f;

            // Display headers for Asset and Message columns
            //
            GUILayout.BeginHorizontal(s_titleRowStyle);
            {
                GUILayout.Label("", GUILayout.Width(indexWidth));
                GUILayout.Label("Asset", GUILayout.Width(assetWidth));
                GUILayout.Label("Message", GUILayout.Width(messageWidth));
                GUILayout.Label("Author", GUILayout.Width(authorWidth));
                GUILayout.Label("", GUILayout.Width(actionsWidth));
            }
            GUILayout.EndHorizontal();

            if (s_isInEditMode)
            {
                s_assetDataList = AssetAdvisorManager.AssetAdvisorDataAsList;
            }

            // Filter the list of items
            //
            s_filteredDataList.Clear();

            if (string.IsNullOrEmpty(s_searchTerm))
            {
                s_filteredDataList.AddRange(s_assetDataList);
            }
            else
            {
                foreach (AssetAdvisorData dataItem in s_assetDataList)
                {
                    if (dataItem.m_assetName.Contains(s_searchTerm) ||
                        dataItem.m_warningMessage.Contains(s_searchTerm))
                    {
                        s_filteredDataList.Add(dataItem);
                    }
                }
            }

            // Display table rows for AssetAdvisorData list
            //
            s_scrollPosition = GUILayout.BeginScrollView(s_scrollPosition);
            {
                for (int i = 0; i < s_filteredDataList.Count; i++)
                {
                    var assetAdvisorData = s_filteredDataList[i];

                    bool isEditing = s_isInEditMode && s_editData.m_indexInEditMode == i;
                    bool isDisabled = s_isInEditMode && s_editData.m_indexInEditMode != i;

                    GUILayout.BeginHorizontal(GetRowStyle(i));
                    {
                        EditorGUI.BeginDisabledGroup(isDisabled);
                        {
                            GUILayout.Label((i + 1).ToString(), GUILayout.Width(indexWidth));
                            GUILayout.Label(assetAdvisorData.m_assetName, GUILayout.Width(assetWidth));

                            if (isEditing)
                            {
                                var data = s_editData.m_data.Value;
                                data.m_warningMessage = GUILayout.TextField(data.m_warningMessage, GUILayout.Width(messageWidth));
                                s_editData.m_data = data;
                            }
                            else
                            {
                                GUILayout.Label(assetAdvisorData.m_warningMessage, GUILayout.Width(messageWidth));
                            }

                            GUILayout.Label(assetAdvisorData.m_author, GUILayout.MaxWidth(authorWidth));

                            if (isEditing)
                            {
                                if (GUILayout.Button("Save", GUILayout.Width(c_width60)))
                                {
                                    SaveEditedData();
                                }
                                if (GUILayout.Button("Cancel", GUILayout.Width(c_width60)))
                                {
                                    ClearEditMode();
                                }
                            }
                            else
                            {
                                if (GUILayout.Button("Edit", GUILayout.Width(c_width60)))
                                {
                                    EditRow(i, assetAdvisorData);
                                }
                                if (GUILayout.Button("Delete", GUILayout.Width(c_width60)))
                                {
                                    DeleteRow(i);
                                }
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
        }

        //---------------------------------------------------------------------------------------------------
        private void DrawToolbarButton(string label, float width, Action onClick = null)
        {
            s_totalWidth += width;

            if (GUILayout.Button(label, EditorStyles.toolbarButton, GUILayout.Width(width)))
            {
                if (onClick != null)
                {
                    onClick();
                }
            }
        }

        //---------------------------------------------------------------------------------------------------
        private static void ClearEditMode()
        {
            s_editData.m_indexInEditMode = -1;            
            s_editData.m_data = null;
            s_isInEditMode = false;
        }

        //---------------------------------------------------------------------------------------------------
        private static void CreateStyles ()
        {
            if (s_oddRowStyle == null)
            {
                s_oddRowStyle = new GUIStyle(GUI.skin.box);
                s_oddRowStyle.normal.background = MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f, 1f));
                s_oddRowStyle.margin = new RectOffset(0, 0, 0, 0);
                s_oddRowStyle.padding = new RectOffset(0, 0, 0, 0);
                s_oddRowStyle.alignment = TextAnchor.MiddleLeft;
            }

            if (s_evenRowStyle == null)
            {
                s_evenRowStyle = new GUIStyle(GUI.skin.box);
                s_evenRowStyle.normal.background = MakeTex(2, 2, new Color(0.15f, 0.15f, 0.15f, 1f));
                s_evenRowStyle.margin = new RectOffset(0, 0, 0, 0);
                s_evenRowStyle.padding = new RectOffset(0, 0, 0, 0);
                s_evenRowStyle.alignment = TextAnchor.MiddleLeft;
            }

            if (s_titleRowStyle == null)
            {
                s_titleRowStyle = new GUIStyle(GUI.skin.box);
                s_titleRowStyle.normal.background = MakeTex(2, 2, new Color(0.125f, 0.125f, 0.125f, 1f));
                s_titleRowStyle.padding = new RectOffset(0, 0, 5, 5);
                s_titleRowStyle.margin = new RectOffset(0, 0, 1, 0);
                s_titleRowStyle.fontStyle = FontStyle.Bold;
            }
        }

        //---------------------------------------------------------------------------------------------------
        private static GUIStyle GetRowStyle (int index)
        {
            if (index % 2 == 0)
            {
                return s_oddRowStyle;
            }

            return s_evenRowStyle;
        }

        //---------------------------------------------------------------------------------------------------
        private static Texture2D MakeTex (int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = color;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();

            return result;
        }

        //---------------------------------------------------------------------------------------------------
        private static void EditRow (int index, AssetAdvisorData assetAdvisorData)
        {
            if (s_isInEditMode)
            {
                return;
            }

            s_editData.m_indexInEditMode = index;            
            s_editData.m_data = assetAdvisorData;
            s_isInEditMode = true;

            Debug.Log("Editing row " + index);
        }

        private static void SaveEditedData ()
        {
            if (!s_isInEditMode)
            {
                return;
            }

            AssetAdvisorManager.UpdateAssetData(s_editData.m_data.Value);
            s_assetDataList = AssetAdvisorManager.AssetAdvisorDataAsList;

            ClearEditMode();
        }

        //---------------------------------------------------------------------------------------------------
        private static void DeleteRow (int index)
        {
            Debug.Log("Deleting row " + index);
            //m_assetDataList.RemoveAt(index);
        }

        //---------------------------------------------------------------------------------------------------
        private static void CreateNew ()
        {
            ItemEditorWindow.ShowWindow();
        }

        //---------------------------------------------------------------------------------------------------
        private static void SaveData ()
        {
            string path = EditorUtility.SaveFilePanel("Save Asset Advisor Data", "", "AssetAdvisorData", "json");

            if (path.Length > 0)
            {
                SaveListOfAssetAdvisorData(path, s_assetDataList);
            }
        }

        //---------------------------------------------------------------------------------------------------
        private static void OpenFileLocation()
        {
            string dataPath = Path.GetFullPath(AssetAdvisorManager.DataFilePath);
            EditorUtility.RevealInFinder(dataPath);
        }

        //---------------------------------------------------------------------------------------------------
        private static void SaveListOfAssetAdvisorData (
            string targetLocation, 
            List<AssetAdvisorData> assetDataList
        ) {
            string jsonData = JsonUtility.ToJson(new { data = assetDataList });
            System.IO.File.WriteAllText(targetLocation, jsonData);

            Debug.Log($"Successfully saved Asset Advisor data to {targetLocation}");
        }
    }
}