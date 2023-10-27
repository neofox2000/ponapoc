using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ShortCutCommands
{
    static void closeAllOtherScenes(Scene exception, bool remove)
    {
        //Unload all other scenes (but do not remove them)
        Scene scene;
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            scene = EditorSceneManager.GetSceneAt(i);
            if ((scene.isLoaded) && (scene != exception))
                EditorSceneManager.CloseScene(scene, remove);
        }
    }

    [MenuItem("Ponimax/Play #%P")]
	public static void RunPlayMode()
	{
        if (!EditorApplication.isPlaying)
        {
            //Add Loader scene
            Scene loaderScene = EditorSceneManager.GetSceneByPath("Assets/_Project/Scenes/Game/Loader.unity");
            EditorSceneManager.OpenScene(loaderScene.path, OpenSceneMode.Additive);
            EditorSceneManager.SetActiveScene(loaderScene);

            closeAllOtherScenes(loaderScene, false);

            //Add Managers scene
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/Game/Managers.unity", OpenSceneMode.Additive);

            EditorApplication.isPlaying = true;
        }
        else
            Debug.LogWarning("Already playing!");
    }
    [MenuItem("Ponimax/Open Test Scene #%O")]
    public static void OpenTestScene()
    {
        if (EditorApplication.isPlaying)
            EditorApplication.isPlaying = false;

        string testSceneName = "Assets/_Project/Scenes/Utility/Test.unity";
        //Scene testScene = EditorSceneManager.GetSceneByPath(testSceneName);

        Scene testScene = EditorSceneManager.OpenScene(testSceneName, OpenSceneMode.Additive);
        EditorSceneManager.SetActiveScene(testScene);
        closeAllOtherScenes(testScene, false);
    }
    [MenuItem("Ponimax/Set Main Dev")]
    public static void SetMainDevDefines()
    {
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
        defines = AddCompilerDefines(defines, "MAIN_DEV_ENV");

        Debug.Log("Compiling with DEFINE: '" + defines + "'");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
    }
    [MenuItem("Ponimax/UnSet Main Dev")]
    public static void UnSetMainDevDefines()
    {
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
        defines = RemoveCompilerDefines(defines, "MAIN_DEV_ENV");

        Debug.Log("Compiling with DEFINE: '" + defines + "'");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
    }
    [MenuItem("Ponimax/SelectMissing")]
    public static void SelectMissing(MenuCommand command)
    {
        Transform[] ts = Object.FindObjectsOfType<Transform>();
        List<GameObject> selection = new List<GameObject>();
        foreach (Transform t in ts)
        {
            Component[] cs = t.gameObject.GetComponents<Component>();
            foreach (Component c in cs)
            {
                if (c == null)
                {
                    selection.Add(t.gameObject);
                }
            }
        }
        Selection.objects = selection.ToArray();
    }

    //Utility
    private static string AddCompilerDefines(string defines, params string[] toAdd)
    {
        List<string> splitDefines = new List<string>(defines.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
        foreach (var add in toAdd)
            if (!splitDefines.Contains(add))
                splitDefines.Add(add);

        return string.Join(";", splitDefines.ToArray());
    }
    private static string RemoveCompilerDefines(string defines, params string[] toRemove)
    {
        List<string> splitDefines = new List<string>(defines.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
        foreach (var remove in toRemove)
            splitDefines.Remove(remove);

        return string.Join(";", splitDefines.ToArray());
    }
}