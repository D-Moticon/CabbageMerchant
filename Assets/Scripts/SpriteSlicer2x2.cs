#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Editor utility to slice a selected Texture2D into a 2x2 sprite sheet
/// and set each pivot to (0.5, 0.15) normalized.
/// The slice names will be prefixed with the original asset name to ensure uniqueness.
/// </summary>
public static class SpriteSlicer2x2
{
    private const float PivotX = 0.5f;
    private const float PivotY = 0.15f;

    [MenuItem("Assets/Slice Sprite 2x2 with Pivot 0.5,0.15", false, 1000)]
    private static void SliceSelectedSprite()
    {
        var obj = Selection.activeObject;
        string path = AssetDatabase.GetAssetPath(obj);

        if (string.IsNullOrEmpty(path) || !Path.GetExtension(path).Equals(".png", System.StringComparison.OrdinalIgnoreCase))
        {
            EditorUtility.DisplayDialog("Sprite Slicer", "Please select a PNG texture asset.", "OK");
            return;
        }

        string assetName = Path.GetFileNameWithoutExtension(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer == null)
        {
            EditorUtility.DisplayDialog("Sprite Slicer", "Selected asset is not a valid texture importer.", "OK");
            return;
        }

        // Configure importer
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;

        // Load texture to get dimensions
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null)
        {
            EditorUtility.DisplayDialog("Sprite Slicer", "Unable to load texture at path.", "OK");
            return;
        }

        int width = tex.width;
        int height = tex.height;
        int halfW = width / 2;
        int halfH = height / 2;

        // Use the modern sprite data provider interface
        var dataProvider = GetSpriteDataProvider(importer);
        if (dataProvider == null)
        {
            EditorUtility.DisplayDialog("Sprite Slicer", "Unable to get sprite data provider.", "OK");
            return;
        }

        // Get existing sprite data
        dataProvider.InitSpriteEditorDataProvider();
        
        // Create new sprite rectangles
        var spriteRects = new List<SpriteRect>();
        
        spriteRects.Add(CreateSpriteRect(assetName + "_TopLeft", 0, halfH, halfW, halfH));
        spriteRects.Add(CreateSpriteRect(assetName + "_TopRight", halfW, halfH, halfW, halfH));
        spriteRects.Add(CreateSpriteRect(assetName + "_BottomLeft", 0, 0, halfW, halfH));
        spriteRects.Add(CreateSpriteRect(assetName + "_BottomRight", halfW, 0, halfW, halfH));

        // Set the sprite rectangles
        dataProvider.SetSpriteRects(spriteRects.ToArray());
        dataProvider.Apply();

        // Apply and reimport
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        EditorUtility.DisplayDialog("Sprite Slicer", "Sliced into 2x2 with normalized pivots and unique names.", "OK");
    }

    private static ISpriteEditorDataProvider GetSpriteDataProvider(TextureImporter importer)
    {
        var spriteDataProviderFactories = new SpriteDataProviderFactories();
        spriteDataProviderFactories.Init();
        
        var dataProvider = spriteDataProviderFactories.GetSpriteEditorDataProviderFromObject(importer);
        return dataProvider;
    }

    private static SpriteRect CreateSpriteRect(string name, int x, int y, int w, int h)
    {
        var spriteRect = new SpriteRect();
        spriteRect.name = name;
        spriteRect.rect = new Rect(x, y, w, h);
        spriteRect.alignment = SpriteAlignment.Custom;
        spriteRect.pivot = new Vector2(PivotX, PivotY);
        spriteRect.border = Vector4.zero;
        
        return spriteRect;
    }
}
#endif