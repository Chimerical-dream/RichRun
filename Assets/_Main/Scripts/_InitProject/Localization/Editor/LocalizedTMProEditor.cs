using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;
namespace Localization
{

    [CustomEditor(typeof(LocalizedTMPro))]
    public class LocalizedTMProEditor : TMP_EditorPanel
    {
        LocalizedTMPro targetLocalizedTMPro;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            targetLocalizedTMPro = (LocalizedTMPro)target;


            serializedObject.Update(); // Sync the serialized properties with the gameobject with GameManager component.


            EditorGUILayout.LabelField("Turkish", EditorStyles.boldLabel);
            targetLocalizedTMPro.textTr = EditorGUILayout.TextArea(targetLocalizedTMPro.textTr);
            EditorGUILayout.LabelField("Russian", EditorStyles.boldLabel);
            targetLocalizedTMPro.textRu = EditorGUILayout.TextArea(targetLocalizedTMPro.textRu);
            EditorGUILayout.LabelField("English", EditorStyles.boldLabel);
            targetLocalizedTMPro.textEn = EditorGUILayout.TextArea(targetLocalizedTMPro.textEn);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(targetLocalizedTMPro);
            }
        }
    }
}
