using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class MeshUtils : MonoBehaviour {
    public Material material;
    public MeshFilter saveMesh;
    public bool ExportTangents;
    public bool BakeTransform;

    public string FileName;

    public Texture2D hdrSource;

    public void ExportTexture() {
        
        //SharpDX
    }


    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public GameObject GameObjectFromMesh(AMesh mesh) {
        GameObject result = new GameObject();
        var renderer = result.AddComponent<MeshRenderer>();
        var mf = result.AddComponent<MeshFilter>();
        var m = new Mesh();
        mf.mesh = m;

        renderer.materials = new Material[1] { material };
        m.subMeshCount = 1;
        m.vertices = mesh.Position.Select(v=>v.ToVector3()).ToArray();
        mesh.SwapFaceOrder();
        m.SetIndices(mesh.Indices.Select(v=>(int)v).ToArray(), MeshTopology.Triangles, 0);
        mesh.SwapFaceOrder();

        m.uv = mesh.UV0.Select(v => v.ToVector2()).ToArray();
        m.normals = mesh.Normal.Select(v => v.ToVector3()).ToArray();

        if((mesh.Header.Flags & AMesh.AMeshFlags.HaveTangents) != AMesh.AMeshFlags.None) {
            m.tangents = mesh.Tangent.Select(v => v.ToVector3()).ToArray();
        }
        
        m.RecalculateBounds();

        return result;
    }

    public void SaveMesh(string directory, string objectName, string materialName) {
        var meshes = MeshToAMesh(saveMesh.sharedMesh, BakeTransform ? saveMesh.transform : null, ExportTangents ? AMesh.AMeshFlags.HaveTangents : AMesh.AMeshFlags.None);

        for(int i = 0; i < meshes.Length; ++i)
        {
            meshes[i].MaterialName = materialName + "_" + i;
            var stm = File.OpenWrite(Path.Combine(directory, objectName + "_" + i + ".lev"));
            meshes[i].Save(stm);
            stm.Close();
        }
    }

    public string GetMeshDir() {
        return @"D:\Move3D_Car_Demo\resources\models\";
    }

    public void SaveMesh() {
        if(saveMesh != null) {
            SaveMesh(GetMeshDir(), FileName, "Car");
        }
    }

    public AMesh[] MeshToAMesh(Mesh m, Transform meshTransform, AMesh.AMeshFlags flags) {
        AMesh[] resultArray = new AMesh[m.subMeshCount];

        var positions = m.vertices.Select(v => v.ToAVector3()).ToArray();
        var normals = m.normals.Select(v => v.ToAVector3()).ToArray();
        var uvs = m.uv.Select(v => v.ToAVector2()).ToArray();
        var tangents = m.tangents;
        bool exportTangents = (flags & AMesh.AMeshFlags.HaveTangents) != AMesh.AMeshFlags.None;

        AMesh.AVector3 bboxMin = new AMesh.AVector3();
        AMesh.AVector3 bboxMax = new AMesh.AVector3();

        if (meshTransform != null) {
            //transform vertices
            var vertexMatrix = meshTransform.localToWorldMatrix;
            for (int i = 0; i < positions.Length; ++i)
            {
                positions[i] = vertexMatrix.MultiplyPoint3x4(positions[i].ToVector3()).ToAVector3();
            }

            //transform normals
            var normalMatrix = meshTransform.worldToLocalMatrix.transpose;
            for (int i = 0; i < positions.Length; ++i)
            {
                normals[i] = normalMatrix.MultiplyVector(normals[i].ToVector3()).ToAVector3();
            }

            if (exportTangents)
            {
                for (int i = 0; i < positions.Length; ++i)
                {
                    var t = tangents[i];
                    var t3 = new Vector3(t.x, t.y, t.z);
                    t3 = normalMatrix.MultiplyVector(t3);
                    tangents[i] = new Vector4(t3.x, t3.y, t3.z, t.w);
                }
            }

            //recalculate bounds
            bboxMin = positions[0];
            bboxMax = positions[0];

            for (int i = 1; i < positions.Length; ++i)
            {
                bboxMin = AMesh.AVector3.Min(positions[i], bboxMin);
                bboxMax = AMesh.AVector3.Max(positions[i], bboxMax);
            }

           // result.Header.BoundingBox.Min = vMin;
          //  result.Header.BoundingBox.Max = vMax;

        }
        else
        {
            bboxMin = (m.bounds.center - m.bounds.extents).ToAVector3();
            bboxMax = (m.bounds.center + m.bounds.extents).ToAVector3();
        }


        for (int subId = 0; subId < m.subMeshCount; ++subId){
            var submeshIndices = m.GetIndices(subId);

            AMesh result = new AMesh();
            resultArray[subId] = result;
            result.Header.Version = 40;
            result.Header.VerticesCount = (ushort)submeshIndices.Length;
            result.Header.Flags = flags;

            result.Header.BoundingBox.Min = bboxMin;
            result.Header.BoundingBox.Max = bboxMax;

            if (exportTangents)
            {
                result.Tangent = new List<AMesh.AVector4>();
            }

            List<ushort> idx = new List<ushort>();
            for (int i = 0; i < submeshIndices.Length; ++i)
            {
                idx.Add((ushort)i);
                int id = submeshIndices[i];
                result.Position.Add(positions[id]);
                result.Normal.Add(normals[id]);
                result.UV0.Add(uvs[id]);

                if (exportTangents)
                {
                    result.Tangent.Add(tangents[id].ToAVector4());
                }
            }

            result.Indices = idx.ToArray();
            result.Header.PolygonsCount = (ushort)(result.Indices.Length / 3);
            result.SwapFaceOrder();

        }

        return resultArray;
    }

    public void LoadAllModels() {
        string folder = @"D:\Move3D_Car_Demo\resources\models\";

        var files = Directory.GetFiles(folder, "*.lev", SearchOption.TopDirectoryOnly);
        foreach(var file in files) {
            Debug.Log(file);
            LoadMesh(file);
        }

    }

    public void Load() {
        string filePath = GetMeshDir() + FileName;
        LoadMesh(filePath);
    }

    public void ExportImport() {
        SaveMesh(GetMeshDir(), FileName, "Car");
        //LoadMesh(filename);
    }

    public void LoadMesh(string filePath) {
		

		var file = File.OpenRead (filePath);

		var mesh = AMesh.Load (file);


        file.Close ();

		Debug.Log (mesh.Header);
		Debug.Log (mesh.MaterialName);

        GameObjectFromMesh(mesh);


    }
}
