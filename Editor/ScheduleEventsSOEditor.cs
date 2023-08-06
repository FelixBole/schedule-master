using UnityEngine;
using UnityEditor;
using System.IO;

namespace Slax.Schedule
{
    [CustomEditor(typeof(ScheduleEventsSO))]
    public class ScheduleEventsSOEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // DrawDefaultInspector();

            ScheduleEventsSO eventsSO = (ScheduleEventsSO)target;

            EditorGUILayout.Space();

            if (GUILayout.Button("Open Editor"))
            {
                ScheduleEventsSOEditorWindow.ShowWindow(eventsSO);
            }
        }

        private string GetSaveFilePath(string defaultFilePath)
        {
            if (File.Exists(defaultFilePath))
            {
                return defaultFilePath;
            }

            return EditorUtility.SaveFilePanel(
                "Save Events To JSON",
                "Assets/",
                "schedule_events.json",
                "json");
        }

        private string GetLoadFilePath(string defaultFilePath)
        {
            if (File.Exists(defaultFilePath))
            {
                return defaultFilePath;
            }

            return EditorUtility.OpenFilePanel(
                "Load Events From JSON",
                "Assets/",
                "json");
        }
    }

}