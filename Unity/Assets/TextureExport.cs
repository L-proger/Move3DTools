using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TextureExport : MonoBehaviour {
    public Texture2D source;

    public InspectorButton SaveTexture;

	void Start () {
		
	}
	
	void Update () {
		
	}

    void OnSaveTexture() {
        if(source != null) {

            string savePath = @"C:\Users\l-pro\Desktop\test.dds";

            var sharpTex = new SharpDX.Direct3D11.Texture2D(source.GetNativeTexturePtr());
            SharpDX.Direct3D11.Texture2D.ToFile(sharpTex.Device.ImmediateContext, sharpTex, SharpDX.Direct3D11.ImageFileFormat.Dds, savePath);
            Debug.Log("Width: " + sharpTex.Description.Width);
        }
     
    }
}
