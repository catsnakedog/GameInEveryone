using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

public static class GameSetup
{
    [MenuItem("Tools/Game In Everyone/Create Game Scene")]
    public static void Create()
    {
        string[] names = { "BadGameBlock", "GoodGameBlock", "NormalGameBlock", "GoodFanBlock", "BadFanBlock", "StreamerBlock", "TelevisionBlock" };
        var prefabs = new BlockDefinition[names.Length];
        for (int i=0; i<names.Length; i++)
        {
            var go = new GameObject(names[i]);
            var type = typeof(BlockDefinition).Assembly.GetTypes().First(t => t.Name == names[i]);
            go.AddComponent(type);
            string path = "Assets/Prefabs/" + names[i] + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go,path);
            prefabs[i] = prefab.GetComponent<BlockDefinition>();
            UnityEngine.Object.DestroyImmediate(go);
        }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var root = new GameObject("10x10 Map");
        for (int y=0;y<10;y++) for (int x=0;x<10;x++) { var cell = new GameObject($"Cell {x},{y}"); cell.transform.SetParent(root.transform); cell.transform.position = new Vector3(x,y,0); }
        var game = new GameObject("Game Controller");
        game.AddComponent<GameSession>();
        game.AddComponent<TetrisGame>().catalog = prefabs;
        string scenePath = "Assets/Scenes/TetrisGame.unity";
        EditorSceneManager.SaveScene(scene,scenePath);
        var scenes = EditorBuildSettings.scenes.ToList();
        if (!scenes.Any(s => s.path == scenePath)) scenes.Add(new EditorBuildSettingsScene(scenePath,true));
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
    }
}
