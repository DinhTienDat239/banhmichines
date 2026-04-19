using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8f);

        if (GUILayout.Button("Load Grid Positions"))
        {
            GameManager gameManager = (GameManager)target;
            Undo.RecordObject(gameManager, "Load Grid Positions");
            gameManager.LoadGridPositions();
            EditorUtility.SetDirty(gameManager);
        }
    }
}
