using UnityEngine;
using UnityEditor;
using System;

namespace Slax.Schedule
{
    [CustomEditor(typeof(TimeManager))]
    public class TimeManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            TimeManager timeManager = (TimeManager)target;

            EditorGUILayout.HelpBox("The TimeManager is responsible for managing the time in the game. If a TimeConfigurationSO is provided through the inspector and none is provided through the InitializeFromConfiguration method of this script, then it will be used in 'Testing Mode', meaning all changes during playtime will not affect the TimeConfigurationSO, making it re-usable easily for testing purposes.", MessageType.None);

            EditorGUILayout.Space(3);

            SerializedProperty timeConfiguration = serializedObject.FindProperty("_timeConfiguration");
            timeConfiguration.objectReferenceValue = EditorGUILayout.ObjectField("Time Configuration", timeConfiguration.objectReferenceValue, typeof(TimeConfigurationSO), false);
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(3);

            if (timeManager.IsTestingMode)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Currently Running in Testing Mode");
                EditorGUILayout.Space(3);
                GUI.backgroundColor = Color.yellow;
                EditorGUILayout.HelpBox($"If you modify the time configuration of {timeConfiguration.objectReferenceValue.name} during runtime, you must apply the changes to the TimeManager for them to be reflected in the game.", MessageType.None);
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Apply time configuration"))
                {
                    timeManager.Initialize();
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndVertical();
            }

            if (!timeConfiguration.objectReferenceValue)
            {
                GUI.backgroundColor = Color.cyan;
                EditorGUILayout.HelpBox("You are not using any Time Configuration, this is fine but please remember to use the TimeManager's InitializeFromConfiguration method to ensure a TimeConfiguration is provided when the game starts.", MessageType.Info);
            }

            if (Application.isPlaying)
            {
                float timeSpeed = timeManager.GetTimeSpeed();
                if (timeSpeed > .1f)
                {
                    if (GUILayout.Button("Accelerate Time"))
                    {
                        timeManager.ChangeTimeSpeed(timeSpeed - 0.1f);
                    }

                    if (GUILayout.Button("Decelerate Time"))
                    {
                        timeManager.ChangeTimeSpeed(timeSpeed + 0.1f);
                    }
                }
            }
        }
    }

}