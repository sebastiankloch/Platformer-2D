#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AtlasGenerator : MonoBehaviour {

    public bool _createAtlas;
    public bool _createdAtlas;
    public string _atlasName = "atlas";
    public string _path = "Assets/_Sprites/Reflections/";
    public Texture2D[] _textures;
    void Update () {
        if (_createAtlas && !_createdAtlas)
        {
            _CreateAtlas();
            _createAtlas = false;
            _createdAtlas = true;
        }
	}

    void _CreateAtlas()
    {
        if (!_createAtlas) return;
        int __widthOfTextures = _textures[0].width;
        int __maxColumn = Mathf.CeilToInt(Mathf.Sqrt((float)_textures.GetLength(0)));
        int __column = 0;
        int __row = 0;

        Texture2D __atlas = new Texture2D(__maxColumn * __widthOfTextures, Mathf.CeilToInt(_textures.GetLength(0) / __maxColumn) * __widthOfTextures + __widthOfTextures, TextureFormat.RGBA32, false);
        print("__atlas.width: " + __atlas.width + " __atlas.height: " + __atlas.height);
        for (int id = 0; id < _textures.GetLength(0); ++id)
        {
            __atlas.SetPixels(__column * __widthOfTextures, __row * __widthOfTextures, __widthOfTextures, __widthOfTextures, _textures[id].GetPixels());
            ++__column;
            if (__column > __maxColumn - 1) { __column = 0; ++__row; }
            
        }
        __atlas.Apply();
        System.IO.File.WriteAllBytes(_path + _atlasName + ".png", __atlas.EncodeToPNG());
        Debug.Log("<color=green><b>SUCCESSFULY</b></color> created: " + _atlasName);
    }
}
#endif
