using UnityEngine;
using UnityEditor;

namespace Slax.Schedule
{
    [CustomEditor(typeof(ScheduleEventCheckAssociationSO))]
    public class ScheduleEventCheckAssociationSOEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // DrawDefaultInspector();

            ScheduleEventCheckAssociationSO checkAssociationSO = (ScheduleEventCheckAssociationSO)target;

            EditorGUILayout.Space();

            if (GUILayout.Button("Open Editor"))
            {
                ScheduleEventCheckAssociationEditorWindow.OpenWindow(checkAssociationSO);
            }
        }
    }

}