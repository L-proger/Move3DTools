using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class CubemapToLongitudinal : MonoBehaviour {

    public Cubemap source;
    public int OutputWidth;
    public int OutputHeight;
    public bool FlipVertical = true;
    public RenderTexture ResultRT;
    public Texture2D Result;
    public InspectorButton Process;
    public InspectorButton Save;
    public string outputPath;
    public Material RenderMaterial;
    public RenderTextureFormat OutputFormat = RenderTextureFormat.Default;

    public bool compress = false;
    public TextureCompressionQuality CompressionQuality = TextureCompressionQuality.Best;
    public TextureFormat CompressionFormat = TextureFormat.DXT5;

    void OnProcess() {

      

        if (Result != null) {
            DestroyImmediate(Result);
        }
        if (ResultRT!= null)
        {
            DestroyImmediate(ResultRT);
        }
        Debug.Log("Re-render texture at time " + Time.realtimeSinceStartup);
        ResultRT = new RenderTexture(OutputWidth, OutputHeight, 24, RenderTextureFormat.ARGBFloat);
        ResultRT.useMipMap = true;
        ResultRT.autoGenerateMips = false;
        ResultRT.Create();

        Result = new Texture2D(OutputWidth, OutputHeight, TextureFormat.RGBAFloat, true, true);

     

        // RenderTexture.active = Result;

        var tex = new SharpDX.Direct3D11.Texture2D(ResultRT.GetNativeTexturePtr());
       
        var mips = tex.Description.MipLevels;
        tex.Dispose();
        Debug.Log(mips);


        var srcTexture = (Cubemap)RenderMaterial.GetTexture("_Cube");


        for (int i = 0; i < mips; ++i)
        {
            Graphics.SetRenderTarget(ResultRT, i);
            RenderMaterial.SetFloat("_MipOffset", ((float)i / (mips - 1)) * (srcTexture.mipmapCount - 1));
            RenderMaterial.SetFloat("_VerticalUvScale", FlipVertical ? 1 : 0);

            RenderMaterial.SetPass(0);
            GL.PushMatrix();
            GL.LoadOrtho();

            GL.Begin(GL.QUADS);
         
            GL.Clear(true, true, Color.blue);
          
            GL.TexCoord(new Vector2(0, 1));
            GL.Vertex3(0, 0, 0.0f);
            GL.TexCoord(new Vector2(0, 0));
            GL.Vertex3(0, 1, 0.0f);
            GL.TexCoord(new Vector2(1, 0));
            GL.Vertex3(1, 1, 0.0f);
            GL.TexCoord(new Vector2(1, 1));
            GL.Vertex3(1, 0, 0.0f);

            GL.End();
            GL.PopMatrix();

            GL.Flush();
        }

     
        RenderTexture.active = null;


       // EditorUtility.CompressTexture(Result, TextureFormat.RGB24, (int)CompressionQuality);

    }

    void OnSave() {


        var tex = new SharpDX.Direct3D11.Texture2D(ResultRT.GetNativeTexturePtr());
        var tex2 = new SharpDX.Direct3D11.Texture2D(Result.GetNativeTexturePtr());

        //SharpDX.Direct3D11.TextureLoadInformation info = new SharpDX.Direct3D11.TextureLoadInformation();
        //  info.

        tex.Device.ImmediateContext.CopyResource(tex, tex2);
      //  SharpDX.Direct3D11.Texture2D.LoadTextureFromTexture(tex.Device.ImmediateContext, tex, tex2, null);




        SharpDX.Direct3D11.Resource.ToFile(tex2.Device.ImmediateContext, tex2, SharpDX.Direct3D11.ImageFileFormat.Dds, outputPath);
        tex.Dispose();
        tex2.Dispose();
    }
}
