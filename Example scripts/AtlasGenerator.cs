#if UNITYEDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AtlasGenerator : MonoBehaviour {

    public bool createAtlas;
    public bool createdAtlas;
    public string atlasName = "atlas";
    public string path = "Assets/Sprites/Reflections/";
    public Texture2D[] textures;
    void Update () {
        if (createAtlas && !createdAtlas)
        {
            CreateAtlas();
            createAtlas = false;
            createdAtlas = true;
        }
	}

    void CreateAtlas()
    {
        if (!createAtlas) return;
        int widthOfTextures = textures[0].width;
        int maxColumn = Mathf.CeilToInt(Mathf.Sqrt((float)textures.GetLength(0)));
        int column = 0;
        int row = 0;

        Texture2D atlas = new Texture2D(maxColumn * widthOfTextures, Mathf.CeilToInt(textures.GetLength(0) / maxColumn) * widthOfTextures + widthOfTextures, TextureFormat.RGBA32, false);
        print("atlas.width: " + atlas.width + " atlas.height: " + atlas.height);
        for (int id = 0; id < textures.GetLength(0); ++id)
        {
            atlas.SetPixels(column * widthOfTextures, row * widthOfTextures, widthOfTextures, widthOfTextures, textures[id].GetPixels());
            ++column;
            if (column > maxColumn - 1) { column = 0; ++row; }
            
        }
        atlas.Apply();
        System.IO.File.WriteAllBytes(path + atlasName + ".png", atlas.EncodeToPNG());
        Debug.Log("<color=green><b>SUCCESSFULY</b></color> created: " + atlasName);
    }
}
#endif
