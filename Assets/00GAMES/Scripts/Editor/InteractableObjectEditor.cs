using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InteractableObject), true)]
public class InteractableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8f);

        if (GUILayout.Button("Rotate +90 (Design Level)"))
        {
            InteractableObject interactableObject = (InteractableObject)target;

            Undo.RecordObject(interactableObject.transform, "Rotate InteractableObject +90");
            Undo.RecordObject(interactableObject, "Update InteractableObject Direction");

            interactableObject.RotateDesignLevel90();

            EditorUtility.SetDirty(interactableObject.transform);
            EditorUtility.SetDirty(interactableObject);
        }
    }
}
