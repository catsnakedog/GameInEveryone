using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BoardSceneSetup
{
    [MenuItem("Tools/Game In Everyone/Set Up Board Visuals")]
    public static void Create()
    {
        const string spritePath = "Assets/Resources/BoardSquare.png";
        if (!File.Exists(spritePath))
        {
            var texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            var pixels = new Color[64];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            texture.SetPixels(pixels); texture.Apply();
            File.WriteAllBytes(spritePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(spritePath);
        }
        var importer = (TextureImporter)AssetImporter.GetAtPath(spritePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 8;
        importer.filterMode = FilterMode.Point;
        importer.SaveAndReimport();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var root = PrefabUtility.LoadPrefabContents(path);
            var definition = root.GetComponent<BlockDefinition>();
            if (definition != null)
            {
                var old = root.transform.Find("Visuals");
                if (old != null) Object.DestroyImmediate(old.gameObject);
                for (int i = root.transform.childCount - 1; i >= 0; i--)
                    if (root.transform.GetChild(i).name.StartsWith("Block Cell ")) Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
                var shape = definition.Shape;
                for (int i = 0; i < shape.Length; i++)
                {
                    var cell = new GameObject("Block Cell " + i);
                    cell.transform.SetParent(root.transform, false);
                    cell.transform.localPosition = new Vector3(shape[i].x, shape[i].y, -0.1f);
                    cell.transform.localScale = Vector3.one * 0.88f;
                    var renderer = cell.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprite; renderer.color = definition.Color; renderer.sortingOrder = 2;
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            PrefabUtility.UnloadPrefabContents(root);
        }
        var map = GameObject.Find("10x10 Map");
        if (map == null) { Debug.LogError("10x10 Map not found in the active scene."); return; }
        foreach (Transform cell in map.transform)
        {
            var renderer = cell.GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = cell.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.13f, 0.15f, 0.19f);
            renderer.sortingOrder = 0;
            cell.localScale = Vector3.one * 0.96f;
        }
        var camera = Camera.main;
        if (camera != null)
        {
            camera.orthographic = true; camera.orthographicSize = 5f;
            camera.transform.position = new Vector3(4.5f, 4.5f, -10f);
            camera.transform.rotation = Quaternion.identity;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.055f, 0.075f);
        }
        var controller = Object.FindFirstObjectByType<TetrisGame>();
        if (controller != null && controller.GetComponent<BoardView>() == null) controller.gameObject.AddComponent<BoardView>();
        EditorSceneManager.MarkSceneDirty(map.scene);
        EditorSceneManager.SaveScene(map.scene);
        AssetDatabase.SaveAssets();
    }
}
