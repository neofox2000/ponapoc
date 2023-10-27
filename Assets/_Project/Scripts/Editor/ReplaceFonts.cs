//Assets/Editor/ReplaceFonts.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System;

public class ReplaceFonts : EditorWindow
{
    [MenuItem("Ponimax/Replace Fonts")]
    static void Init()
    {
        ReplaceFonts window = (ReplaceFonts)EditorWindow.GetWindow(typeof(ReplaceFonts));
        window.Show();
        window.position = new Rect(20, 80, 400, 300);
    }

    bool replaceNullOnly = true;
    int replaceCount;
    Font newFont;
    Vector2 scroll;

    void OnGUI()
    {
        GUILayout.Space(3);
        int oldValue = GUI.skin.window.padding.bottom;
        GUI.skin.window.padding.bottom = -20;
        Rect windowRect = GUILayoutUtility.GetRect(1, 17);
        windowRect.x += 4;
        windowRect.width -= 7;
        //editorMode = GUI.SelectionGrid(windowRect, editorMode, modes, 2, "Window");
        GUI.skin.window.padding.bottom = oldValue;

        //Visual controls
        EditorGUILayout.BeginHorizontal();
        newFont = (Font)EditorGUILayout.ObjectField(newFont, typeof(Font), false);
        replaceNullOnly = EditorGUILayout.Toggle("Replace Broken Fonts Only", replaceNullOnly);
        if (GUILayout.Button("Replace") && (newFont != null))
        {
            //Save any unsaved assets
            AssetDatabase.SaveAssets();

            //Run operation on Prefabs in Asset Bank
            replaceCount = 0;
            string[] allPrefabs = GetAllPrefabsInAssetBank();
            foreach (string prefab in allPrefabs)
                if (replaceFontOnPrefab(prefab, newFont, replaceNullOnly))
                    replaceCount++;
            Debug.Log("Replaced " + replaceCount + " fonts in Asset Bank");

            //Run operation on Open Scenes
            replaceCount = 0;
            UnityEngine.SceneManagement.Scene scene;
            //List<Text> allTextObjects = new List<Text>();

            //Grab all the text objects in all open scenes
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                scene = EditorSceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    int objectsInScene = 0;
                    GameObject[] rootGameObjects = scene.GetRootGameObjects();
                    foreach (GameObject GO in rootGameObjects)
                    {
                        Text[] textObjectsInScene = GO.GetComponentsInChildren<Text>(true);
                        objectsInScene += textObjectsInScene.Length;

                        //Replace fonts on all found objects
                        foreach (Text t in textObjectsInScene)
                            if (replaceFont(t, newFont, replaceNullOnly))
                                replaceCount++;
                    }

                    Debug.Log("Found " + objectsInScene + " Text objects and Replaced " + replaceCount + " fonts in " + scene.name);
                    if (replaceCount > 0) EditorSceneManager.MarkSceneDirty(scene);
                }
            }

            //Save all changes
            //AssetDatabase.SaveAssets();
        }

        EditorGUILayout.EndHorizontal();
    }

    static bool replaceFont(Text textObject, Font newFont, bool nullOnly = true)
    {
        if ((!nullOnly) || ((nullOnly) && (textObject.font == null)))
        {
            //Debug.Log(textObject.name + "> Replacing font: " + textObject.font + " with: " + newFont.name);
            textObject.font = newFont;
            return true;
        }

        return false;
    }
    static bool replaceFontOnPrefab(string prefabAsset, Font newFont, bool nullOnly = true)
    {
        bool somethingWasReplaced = false;
        GameObject GO = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAsset);
        Text[] texts = GO.GetComponentsInChildren<Text>();

        if ((texts != null) && (texts.Length > 0))
        {
            //Debug.Log("Found text components on " + GO.name);
            foreach (Text t in texts)
                somethingWasReplaced = replaceFont(t, newFont, nullOnly);
        }

        if(somethingWasReplaced) EditorUtility.SetDirty(GO);
        //if (somethingWasReplaced) Undo.RecordObject(GO, "Font Replaced in Asset Bank");
        return somethingWasReplaced;
    }
    static string[] GetAllPrefabsInAssetBank()
    {
        string[] temp = AssetDatabase.GetAllAssetPaths();
        List<string> result = new List<string>();
        foreach (string s in temp)
        {
            if (s.Contains(".prefab")) result.Add(s);
        }
        return result.ToArray();
    }
}