using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshUtils))]
public class MeshUtilsEditor : Editor {
	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();

		if (GUILayout.Button ("Load")) {
			(target as MeshUtils).Load ();
		}

        if (GUILayout.Button("Load ALL")) {
            (target as MeshUtils).LoadAllModels();
        }

        if (GUILayout.Button("Save mesh")) {
            (target as MeshUtils).SaveMesh();
        }

        if (GUILayout.Button("ExportImport test")) {
            (target as MeshUtils).ExportImport();
        }

        if (GUILayout.Button("Test texture export")) {
            (target as MeshUtils).ExportTexture();
        }
    }
}
