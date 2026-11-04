using UnityEditor;
namespace Capybara67.Editor {
    public sealed class CapyLifeAtlasImport : AssetPostprocessor {
        void OnPreprocessTexture(){
            if(!assetPath.EndsWith("/Characters/CapyLifeAtlas.png")&&!assetPath.Contains("/Resources/Accessories/")&&!assetPath.Contains("/Resources/Outfits/"))return;
            var importer=(TextureImporter)assetImporter;
            importer.textureType=TextureImporterType.Default;
            importer.alphaSource=TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
            importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=2048;
            importer.filterMode=UnityEngine.FilterMode.Bilinear;
            importer.wrapMode=UnityEngine.TextureWrapMode.Clamp;
            importer.textureCompression=TextureImporterCompression.CompressedHQ;
            importer.isReadable=false;
        }
    }
}

