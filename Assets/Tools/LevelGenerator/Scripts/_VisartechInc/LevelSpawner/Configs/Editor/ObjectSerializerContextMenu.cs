using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(PatternSettings))]
public class ObjectSerializerContextMenu : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PatternSettings myScript = (PatternSettings)target;

        // if (myScript._restileObject != null && myScript._originalObject != null && GUILayout.Button("Restile pattern"))
        // {
        //     myScript.RestilePattern();
        // }
        //
        // if (GUILayout.Button("Update shapes"))
        // {
        //     myScript.UpdateShapes();
        // }

        if (myScript._EntPoint != null && myScript._StartPoint != null && GUILayout.Button("Serialize pattern"))
        {
            SerializeGameObject(myScript.transform);
        }
    }

    void SerializeGameObject(Transform obj)
    {
        if(!obj)
            return;

        var info = ObjectSerializer.SerializeObject(obj);

        var path = EditorUtility.SaveFilePanelInProject("Save file in", Selection.activeGameObject.name + ".txt", "txt",
            "");

        if (path.Length == 0)
            return;

        var writer = new StreamWriter(path, false);
        writer.Write(info);
        writer.Close();

        AssetDatabase.Refresh();
    }
}
