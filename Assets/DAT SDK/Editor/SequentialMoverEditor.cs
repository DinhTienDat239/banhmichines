using UnityEngine;
using UnityEditor;
using DAT.Core.Motion;

[CustomEditor(typeof(SequentialMover))]
[CanEditMultipleObjects]
public class SequentialMoverEditor : Editor
{
    SerializedProperty targets, offset, moveMode, moveDuration, waitDuration, curveDepth;
    SerializedProperty ease, loop, autoStartOnEnable, snapToFirstTarget;
    SerializedProperty useAnchoredPosition;
    SerializedProperty enableRotateOnArrival, rotationAngle, rotationDuration;
    SerializedProperty enablePulseInOnArrival, enablePulseOutOnArrival;
    SerializedProperty pulseScale, pulseDuration, pulseEase;
    SerializedProperty holdArrivalEffects;
    SerializedProperty OnTargetReached, OnLoopComplete;

    void OnEnable()
    {
        targets = serializedObject.FindProperty("targets");
        offset = serializedObject.FindProperty("offset");
        moveMode = serializedObject.FindProperty("moveMode");
        moveDuration = serializedObject.FindProperty("moveDuration");
        waitDuration = serializedObject.FindProperty("waitDuration");
        curveDepth = serializedObject.FindProperty("curveDepth");

        ease = serializedObject.FindProperty("ease");
        loop = serializedObject.FindProperty("loop");
        autoStartOnEnable = serializedObject.FindProperty("autoStartOnEnable");
        snapToFirstTarget = serializedObject.FindProperty("snapToFirstTarget");

        useAnchoredPosition = serializedObject.FindProperty("useAnchoredPosition");

        enableRotateOnArrival = serializedObject.FindProperty("enableRotateOnArrival");
        rotationAngle = serializedObject.FindProperty("rotationAngle");
        rotationDuration = serializedObject.FindProperty("rotationDuration");

        enablePulseInOnArrival = serializedObject.FindProperty("enablePulseInOnArrival");
        enablePulseOutOnArrival = serializedObject.FindProperty("enablePulseOutOnArrival");
        pulseScale = serializedObject.FindProperty("pulseScale");
        pulseDuration = serializedObject.FindProperty("pulseDuration");
        pulseEase = serializedObject.FindProperty("pulseEase");

        holdArrivalEffects = serializedObject.FindProperty("holdArrivalEffects");

        OnTargetReached = serializedObject.FindProperty("OnTargetReached");
        OnLoopComplete = serializedObject.FindProperty("OnLoopComplete");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // --- Target Settings ---
        DrawHeader("Target Settings");
        EditorGUILayout.PropertyField(targets);
        EditorGUILayout.PropertyField(offset);
        EditorGUILayout.PropertyField(moveMode);
        EditorGUILayout.PropertyField(moveDuration);
        EditorGUILayout.PropertyField(waitDuration);
        if ((MoveMode)moveMode.enumValueIndex == MoveMode.Curve)
        {
            EditorGUILayout.PropertyField(curveDepth);
        }

        // --- Tween Settings ---
        DrawHeader("Tween Settings");
        EditorGUILayout.PropertyField(ease);
        EditorGUILayout.PropertyField(loop);
        EditorGUILayout.PropertyField(autoStartOnEnable);
        if (autoStartOnEnable.boolValue)
        {
            EditorGUILayout.PropertyField(snapToFirstTarget);
        }

        // --- Position Mode ---
        DrawHeader("Position Mode");
        EditorGUILayout.PropertyField(useAnchoredPosition);

        // --- Arrival Effects ---
        DrawHeader("Arrival Effects");
        EditorGUILayout.PropertyField(enableRotateOnArrival);
        if (enableRotateOnArrival.boolValue)
        {
            EditorGUILayout.PropertyField(rotationAngle);
            EditorGUILayout.PropertyField(rotationDuration);
        }

        EditorGUILayout.PropertyField(enablePulseInOnArrival);
        EditorGUILayout.PropertyField(enablePulseOutOnArrival);
        if (enablePulseInOnArrival.boolValue || enablePulseOutOnArrival.boolValue)
        {
            EditorGUILayout.PropertyField(pulseScale);
            EditorGUILayout.PropertyField(pulseDuration);
            EditorGUILayout.PropertyField(pulseEase);
        }

        if (enableRotateOnArrival.boolValue || enablePulseInOnArrival.boolValue || enablePulseOutOnArrival.boolValue)
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(holdArrivalEffects);
        }

        // --- Events ---
        DrawHeader("Events");
        EditorGUILayout.PropertyField(OnTargetReached);
        EditorGUILayout.PropertyField(OnLoopComplete);

        serializedObject.ApplyModifiedProperties();
    }

    // ====== Helpers ======

    private void DrawLine(Color color, float thickness = 1, float padding = 6)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2f;
        r.x -= 2;
        r.width += 4;
        EditorGUI.DrawRect(r, color);
    }

    private void DrawHeader(string label)
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        DrawLine(Color.gray);
        EditorGUILayout.Space(2);
    }
}
