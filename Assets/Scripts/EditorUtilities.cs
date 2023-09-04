#if UNITY_EDITOR

using System;
using System.Collections.Specialized;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class EditorUtilities
{
    public static void DrawEnumBitMaskProperty(SerializedProperty intProperty, Type enumType, int valuesToHide = 0)
    {
        EditorGUILayout.LabelField($"{intProperty.name}:");
            
        string[] names = Enum.GetNames(enumType);
        BitVector32 mask = new BitVector32(intProperty.intValue);

        for (int i = valuesToHide; i < names.Length; ++i)
        {
            int bitIndex = 1 << i;
            string name = names[i];
            bool value = mask[bitIndex];
            value = EditorGUILayout.Toggle(name, value);
            mask[bitIndex] = value;

        }
        intProperty.intValue = mask.Data;
    }
                
    public static void CenterOnMainWin(this EditorWindow window, float width, float height)
    {
        var main = GetEditorMainWindowPos();
        var position = window.position;
        var windowWidth = (main.width - width) * 0.5f;
        var windowHeight = (main.height - height) * 0.5f;
        position.x = main.x + windowWidth;
        position.y = main.y + windowHeight;
        position.width = width;
        position.height = height;
        window.position = position;
    }

    // Indentifying where is the main unity window (monitors)
    private static Rect GetEditorMainWindowPos()
    {
        Type baseType = typeof(ScriptableObject);
        Type containerWinType = null;

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (baseType.IsAssignableFrom(type) && type.Name == "ContainerWindow")
                {
                    containerWinType = type;
                    break;
                }
            }

            if (containerWinType != null)
            {
                break;
            }
        }

        if (containerWinType == null)
        {
            throw new MissingMemberException(
                "Can't find internal type ContainerWindow. Maybe something has changed inside Unity");
        }

        var showModeField = containerWinType.GetField("m_ShowMode", BindingFlags.NonPublic | BindingFlags.Instance);
        var positionProperty = containerWinType.GetProperty("position", BindingFlags.Public | BindingFlags.Instance);

        if (showModeField == null || positionProperty == null)
        {
            throw new MissingFieldException(
                "Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity");
        }

        var windows = Resources.FindObjectsOfTypeAll(containerWinType);

        foreach (var win in windows)
        {
            var showMode = (int) showModeField.GetValue(win);
            if (showMode == 4) // ShowMode ( MainWindow == 4) 
            {
                return (Rect) positionProperty.GetValue(win, null);
            }
        }

        throw new NotSupportedException(
            "Can't find internal main window. Maybe something has changed inside Unity");
    }
}

#endif
