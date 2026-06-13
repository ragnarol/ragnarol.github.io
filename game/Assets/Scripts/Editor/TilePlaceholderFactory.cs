using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;

namespace FadingSuns.Editor
{
    public static class TilePlaceholderFactory
    {
        private const string SpritesDir = "Assets/Sprites/Placeholders";
        private const string TilesDir   = "Assets/Tiles/Placeholders";

        [MenuItem("Tools/Fading Suns/Generate Placeholder Tiles")]
        public static void GeneratePlaceholderTiles()
        {
            Directory.CreateDirectory(Application.dataPath + "/Sprites/Placeholders");
            Directory.CreateDirectory(Application.dataPath + "/Tiles/Placeholders");
            AssetDatabase.Refresh();

            CreateTile("GroundTile",  new Color(0.70f, 0.55f, 0.35f));
            CreateTile("WallTile",    new Color(0.45f, 0.40f, 0.38f));
            CreateTile("WaterTile",   new Color(0.20f, 0.40f, 0.75f));
            CreateTile("OverlayTile", new Color(1.00f, 1.00f, 0.00f, 0.45f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[TilePlaceholderFactory] Placeholder tiles created in " + TilesDir);
        }

        private static void CreateTile(string tileName, Color color)
        {
            // 1. Build a 32x32 texture
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            var pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();

            // 2. Save PNG
            string pngPath = SpritesDir + "/" + tileName + ".png";
            File.WriteAllBytes(Application.dataPath + "/Sprites/Placeholders/" + tileName + ".png",
                tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.Refresh();

            // 3. Configure import settings as sprite
            var importer = (TextureImporter)AssetImporter.GetAtPath(pngPath);
            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode          = FilterMode.Point;
            importer.textureCompression  = TextureImporterCompression.Uncompressed;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            // 4. Load the imported sprite
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);

            // 5. Create a Tile asset and assign the sprite
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.color  = Color.white;

            string tilePath = TilesDir + "/" + tileName + ".asset";
            AssetDatabase.CreateAsset(tile, tilePath);
        }
    }
}
