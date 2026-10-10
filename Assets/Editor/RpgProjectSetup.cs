using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
internal static class RpgProjectSetup
{
    private const string StartupScenePath = "Assets/Scenes/FirstLight.unity";

    static RpgProjectSetup()
    {
        EditorApplication.delayCall += EnsureStartupScene;
    }

    private static void EnsureStartupScene()
    {
        if (EditorApplication.isCompiling)
        {
            EditorApplication.delayCall += EnsureStartupScene;
            return;
        }

        SceneAsset startupScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(StartupScenePath);
        if (startupScene == null)
        {
            if (HasOtherEnabledScene())
            {
                return;
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            Directory.CreateDirectory(Path.Combine(projectRoot, "Assets", "Scenes"));
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Echoes of Aether").AddComponent<RpgGame>();
            if (!EditorSceneManager.SaveScene(scene, StartupScenePath))
            {
                Debug.LogError("Could not save the Echoes of Aether startup scene.");
                return;
            }

            startupScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(StartupScenePath);
        }

        if (startupScene != null && !HasOtherEnabledScene())
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(StartupScenePath, true)
            };
        }
    }

    private static bool HasOtherEnabledScene()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i].enabled && scenes[i].path != StartupScenePath)
            {
                return true;
            }
        }

        return false;
    }
}
