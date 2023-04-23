using UnityEngine;
using UnityEditor;

public class MeshCombinerEditorWindow : EditorWindow
{
    private GameObject sourceObject;
    private string fileName = "CombinedMesh.asset";

    [MenuItem("Window/Mesh Combiner")]
    public static void ShowWindow()
    {
        GetWindow<MeshCombinerEditorWindow>("Mesh Combiner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Combine Meshes and Save as Asset", EditorStyles.boldLabel);

        sourceObject = (GameObject)EditorGUILayout.ObjectField("Source Object", sourceObject, typeof(GameObject), true);

        fileName = EditorGUILayout.TextField("File Name", fileName);

        if (GUILayout.Button("Combine and Save"))
        {
            if (sourceObject != null)
            {
                CombineAndSaveMeshes();
            }
            else
            {
                Debug.LogError("No source object selected!");
            }
        }
    }

    private void CombineAndSaveMeshes()
    {
        MeshFilter[] meshFilters = sourceObject.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);

        AssetDatabase.CreateAsset(combinedMesh, "Assets/" + fileName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Combined mesh saved as: Assets/" + fileName);
    }
}
