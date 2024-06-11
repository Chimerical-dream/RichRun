using Game.Core;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraFollower))]
public class CameraFollowerCustomEditor : Editor {
    bool previewPOV = false;
    int POVIndex = 0;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        var cameraHolder = (CameraFollower)target;
        var POVs = cameraHolder.CameraPOV_s;

        if (POVs == null) {
            return;
        }

        if (POVIndex > POVs.Length) {
            POVIndex = POVs.Length - 1;
        }
        if (POVIndex < 0) {
            POVIndex = 0;
        }


        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor tools");
        POVIndex = EditorGUILayout.IntField("POV Index", POVIndex);
        POVIndex = Mathf.Clamp(POVIndex, 0, POVs.Length - 1);

        bool previewOldValue = previewPOV;
        previewPOV = EditorGUILayout.Toggle("Preview POV", previewPOV);

        if (previewOldValue == true && previewPOV == false) {
            try {
                PrefabUtility.RevertObjectOverride(cameraHolder.CameraTransform.parent, InteractionMode.AutomatedAction);
            }
            catch { }
        }

        if (previewPOV) {
            cameraHolder.CameraTransform.parent.localPosition = POVs[POVIndex].CameraLocalPosition;
            cameraHolder.CameraTransform.parent.localRotation = POVs[POVIndex].CameraLocalRotation;
            cameraHolder.CameraTransform.GetComponent<Camera>().fieldOfView = POVs[POVIndex].FOV;
        }
        bool wasChanged = false;
        if (GUILayout.Button(new GUIContent("Add POV to end", "Adds POV and sets position and rotation like in MainCamera"))) {
            AddPOV(cameraHolder);
            wasChanged = true;
        }
        if (GUILayout.Button(new GUIContent("Refresh POV index", "Refreshes POV positions and rotations\n by POV Index"))) {
            RefreshPOV(POVIndex, cameraHolder);
            wasChanged = true;
        }

        if (wasChanged) {
            EditorUtility.SetDirty(target);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void AddPOV(CameraFollower cameraFollower) {
        var povs = cameraFollower.CameraPOV_s;
        var newArray = new CameraPOVData[povs.Length + 1];
        for (int i = 0; i < povs.Length; i++) {
            newArray[i] = povs[i];
        }
        newArray[povs.Length] = new CameraPOVData() {
            CameraLocalPosition = cameraFollower.CameraTransform.parent.localPosition,
            CameraLocalRotation = cameraFollower.CameraTransform.parent.localRotation,
            FOV = cameraFollower.CameraTransform.GetComponent<Camera>().fieldOfView,
        };
        cameraFollower.CameraPOV_s = newArray;
    }

    private void RefreshPOV(int index, CameraFollower cameraFollower) {
        cameraFollower.CameraPOV_s[index].CameraLocalPosition = cameraFollower.CameraTransform.parent.localPosition;
        cameraFollower.CameraPOV_s[index].CameraLocalRotation = cameraFollower.CameraTransform.parent.localRotation;
        cameraFollower.CameraPOV_s[index].FOV = cameraFollower.CameraTransform.GetComponent<Camera>().fieldOfView;
    }
}
