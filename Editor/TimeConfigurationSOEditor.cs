using UnityEditor;
using UnityEngine;

namespace Slax.Schedule
{
    [CustomEditor(typeof(TimeConfigurationSO))]
    public class TimeConfigurationSOEditor : Editor
    {
        private int _maxMinutes = 60;
        private TimeConfigurationSO _timeConfiguration;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _timeConfiguration = (TimeConfigurationSO)target;

            SerializedProperty notesProperty = serializedObject.FindProperty("_notes");
            EditorGUILayout.LabelField("Notes", EditorStyles.boldLabel);
            notesProperty.stringValue = EditorGUILayout.TextArea(notesProperty.stringValue, GUILayout.Height(100));

            DrawTickConfig();

            DrawDateTime();

            DrawDayConfiguration();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTickConfig()
        {
            EditorGUILayout.LabelField("Tick Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            SerializedProperty tickMinutesIncrease = serializedObject.FindProperty("_tickMinutesIncrease");
            EditorGUILayout.PropertyField(tickMinutesIncrease);
            _maxMinutes = 60 / tickMinutesIncrease.intValue;
            SerializedProperty timeBetweenTicks = serializedObject.FindProperty("_timeBetweenTicks");
            EditorGUILayout.PropertyField(timeBetweenTicks);
            EditorGUILayout.EndVertical();
        }

        private void DrawDateTime()
        {
            EditorGUILayout.LabelField("Start Date & Time", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            SerializedProperty seasonProperty = serializedObject.FindProperty("_season");
            EditorGUILayout.PropertyField(seasonProperty);

            SerializedProperty maxYears = serializedObject.FindProperty("_maxYears");
            EditorGUILayout.PropertyField(maxYears);

            if (maxYears.intValue <= 1) maxYears.intValue = 1;

            SerializedProperty yearProperty = serializedObject.FindProperty("_year");
            yearProperty.intValue = EditorGUILayout.IntSlider(new GUIContent("Year"), yearProperty.intValue, 1, _timeConfiguration.MaxYears);

            SerializedProperty dateProperty = serializedObject.FindProperty("_date");
            EditorGUILayout.PropertyField(dateProperty);

            SerializedProperty hourProperty = serializedObject.FindProperty("_hour");
            EditorGUILayout.PropertyField(hourProperty);

            EditorGUILayout.LabelField("In units of Tick Minutes Increase");
            SerializedProperty minutesProperty = serializedObject.FindProperty("_minutes");
            minutesProperty.intValue = EditorGUILayout.IntSlider(new GUIContent("Minutes", "1 unit = the value set for Tick Minutes Increase. For example, if ticks increase minutes by 10, then 1 unit = 10 minutes"), minutesProperty.intValue, 0, _maxMinutes - 1);
            EditorGUILayout.EndVertical();
        }

        private void DrawDayConfiguration()
        {
            EditorGUILayout.LabelField("Configuration of a Day", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            SerializedProperty dayConfigurationProperty = serializedObject.FindProperty("_dayConfiguration");
            EditorGUILayout.PropertyField(dayConfigurationProperty, true);
            EditorGUILayout.EndVertical();
        }
    }

}