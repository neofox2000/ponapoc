using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteImportSettings : AssetPostprocessor
{
    void OnPostprocessTexture(Texture2D texture)
    {
        TextureImporter ti = (TextureImporter)assetImporter;
        if (Path.GetDirectoryName(ti.assetPath) == "Assets/_Project/Art/Environment/__Import")
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.mipmapEnabled = false;
            ti.filterMode = FilterMode.Trilinear;
            ti.spritePixelsPerUnit = 50;
            TextureImporterPlatformSettings TIPS = new TextureImporterPlatformSettings();
            TIPS.overridden = true;
            TIPS.format = TextureImporterFormat.DXT5;
            TIPS.name = "Standalone";
            TIPS.maxTextureSize = 2048;
            ti.SetPlatformTextureSettings(TIPS);
            ti.spriteImportMode = SpriteImportMode.Single;
            TextureImporterSettings texSettings = new TextureImporterSettings();

            ti.ReadTextureSettings(texSettings);
            texSettings.spriteAlignment = (int)SpriteAlignment.BottomLeft;
            ti.SetTextureSettings(texSettings);

            Debug.Log("Environment texture Settings Applied to " + ti.assetPath);
        }
    }
}