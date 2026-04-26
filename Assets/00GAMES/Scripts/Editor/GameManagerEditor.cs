using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8f);
        EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);

        GameManager gameManager = (GameManager)target;

        if (GUILayout.Button("Load Grid Positions"))
        {
            Undo.RecordObject(gameManager, "Load Grid Positions");
            gameManager.LoadGridPositions();
            EditorUtility.SetDirty(gameManager);
        }

        if (GUILayout.Button("Spawn Grid Floors"))
        {
            Undo.RecordObject(gameManager, "Spawn Grid Floors");
            gameManager.SpawnGridFloors();
            EditorUtility.SetDirty(gameManager);
        }
    }
}
