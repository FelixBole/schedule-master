using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Slax.Schedule
{
    public class ScheduleEventsSOEditorWindow : EditorWindow
    {
        private string editorPrefs = "Slax-Schedule-";
        private ScheduleEventsSO eventsSO;
        private SerializedObject serializedEventsSO;
        private ScheduleEvent newEvent = new ScheduleEvent();

        private string jsonFilePath;
        private string searchId = "*";
        private string errorMessage = "";
        private int currentTab = 0;
        private int nextAvailableID = 1;
        private int selectedYear = 0;
        private Vector2 scrollPosition;
        private Vector2 searchResultsScrollPosition;

        private bool showNoMatchingEventsMessage = false;
        private bool searchOptionsFoldout = true;
        private bool unsavedChanges = false;
        private bool isAddEventButtonEnabled = false;
        private bool isSearchByTimestampPerformed = false;
        private bool successCreatedEvent = false;

        private bool searchByDay = true;
        private bool searchByDate = true;
        private bool searchByHour = true;
        private bool searchByYear = true;
        private bool searchBySeason = true;
        private bool searchBetweenTimestamps = false;

        private Timestamp startTimestamp = new Timestamp(Days.Mon, 0, 0, 0, 0, Season.Spring);
        private Timestamp endTimestamp = new Timestamp(Days.Mon, 0, 0, 0, 0, Season.Spring);

        private List<ScheduleEvent> searchByTimestampResults = new List<ScheduleEvent>();

        private Timestamp searchTimestamp = new Timestamp(Days.Mon, 0, 0, 0, 0, Season.Spring);

        public static void ShowWindow(ScheduleEventsSO targetEventsSO)
        {
            ScheduleEventsSOEditorWindow window = GetWindow<ScheduleEventsSOEditorWindow>("Schedule Events");

            window.LoadEventsSO(targetEventsSO);
        }

        private void OnEnable()
        {
            SetCurrentlyEditedObject();

            unsavedChanges = false;
            isAddEventButtonEnabled = false;
            errorMessage = "";
            currentTab = EditorPrefs.GetInt(editorPrefs + "CurrentTab", 0);
            selectedYear = EditorPrefs.GetInt(editorPrefs + "SelectedYear", 0);
        }

        private void OnDisable()
        {
            if (eventsSO != null)
            {
                string serializedEventsSOPath = AssetDatabase.GetAssetPath(eventsSO);
                EditorPrefs.SetString(editorPrefs + "SerializedEventsSOPath", serializedEventsSOPath);
            }
            EditorPrefs.SetInt(editorPrefs + "SelectedYear", selectedYear);
            EditorPrefs.SetInt(editorPrefs + "CurrentTab", currentTab);
        }


        private void LoadEventsSO(ScheduleEventsSO targetEventsSO)
        {
            eventsSO = targetEventsSO;

            if (eventsSO != null)
            {
                serializedEventsSO = new SerializedObject(eventsSO);
                jsonFilePath = eventsSO.DefaultFilePath;
            }
            else
            {
                serializedEventsSO = null;
            }
        }

        private void OnGUI()
        {
            if (serializedEventsSO == null) return;

            EditorGUI.BeginChangeCheck();
            DrawFilePath();

            bool jsonFileExists = System.IO.File.Exists(jsonFilePath);
            if (!jsonFileExists)
            {
                EditorGUILayout.HelpBox("JSON file does not exist. Fix the file path or Generate a new one by clicking the button below.", MessageType.Warning);
                if (GUILayout.Button("Generate JSON file"))
                {
                    eventsSO.SaveEventsToJson(jsonFilePath);
                    jsonFileExists = true;
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedEventsSO.ApplyModifiedProperties();
                LoadEventsFromJson();
            }

            // Show the rest of the content only if the JSON file exists.
            if (jsonFileExists)
            {
                GUI.Box(new Rect(5, 40, 100, position.height - 50), GUIContent.none, EditorStyles.helpBox);
                GUILayout.BeginArea(new Rect(10, 45, 90, position.height - 60));
                DrawSidebar();
                GUILayout.EndArea();

                GUI.Box(new Rect(110, 40, position.width - 115, position.height - 50), GUIContent.none, EditorStyles.helpBox);
                GUILayout.BeginArea(new Rect(115, 45, position.width - 125, position.height - 60));

                DrawTabs();

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                switch (currentTab)
                {
                    case 0:
                        ResetSearchByTimestamp();
                        successCreatedEvent = false;
                        DrawSearchContent();
                        break;
                    case 1:
                        successCreatedEvent = false;
                        DrawTimestampSearchContent();
                        break;
                    case 2:
                        successCreatedEvent = false;
                        ResetSearchByTimestamp();
                        DrawNewEventInput();
                        break;
                    case 3:
                        ResetSearchByTimestamp();
                        DrawOverviewContent();
                        break;
                    default:
                        break;
                }

                EditorGUILayout.EndScrollView();
                GUILayout.EndArea();

                EditorGUILayout.Space();

                serializedEventsSO.ApplyModifiedProperties();
            }

        }

        private void DrawSidebar()
        {
            string saveHover = unsavedChanges ? "Some changes are not saved to JSON file" : "Save events to JSON file";

            if (unsavedChanges)
            {
                GUI.backgroundColor = Color.yellow;
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }

            if (GUILayout.Button(new GUIContent("Save", saveHover), GUILayout.Width(90)))
            {
                SaveEventsToFile();
            }

            GUI.backgroundColor = Color.white;

            if (GUILayout.Button(new GUIContent("Load", "Load events from JSON file"), GUILayout.Width(90)))
            {
                LoadEventsFromJson();
            }

            if (GUILayout.Button(new GUIContent("Refresh", "Refreshes the events"), GUILayout.Width(90)))
            {
                SetCurrentlyEditedObject();
            }

            GUILayout.Label("Events: " + (eventsSO != null ? eventsSO.Events.Count : 0));

            if (!string.IsNullOrEmpty(errorMessage))
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
                GUILayout.FlexibleSpace();
            }
        }

        private void DrawTabs()
        {
            int numRows = 2;
            string[] tabsRow1 = new string[] { "Search", "By Timestamp" };
            string[] tabsRow2 = new string[] { "Event Creation", "Overview" };

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            for (int row = 0; row < numRows; row++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                string[] tabs = (row == 0) ? tabsRow1 : tabsRow2;
                DrawTabButtons(tabs, row);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawTabButtons(string[] tabs, int rowIndex)
        {
            int offset = 2;
            int activeTab = (rowIndex == 0) ? currentTab : currentTab - offset;

            for (int i = 0; i < tabs.Length; i++)
            {
                if (i == activeTab)
                {
                    GUI.backgroundColor = Color.gray;
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }

                if (GUILayout.Button(tabs[i], GUILayout.MaxWidth(150)))
                {
                    if (rowIndex == 0)
                    {
                        currentTab = i;
                    }
                    else
                    {
                        currentTab = i + offset;
                    }
                }
            }

            GUI.backgroundColor = Color.white;
        }

        private void DrawSearchContent()
        {
            DrawSearchBar();

            if (!string.IsNullOrEmpty(searchId))
            {
                if (eventsSO.Events != null && eventsSO.Events.Count > 0)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    DrawScheduleEvents();
                    GUILayout.EndVertical();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Enter an ID to start searching for events.", MessageType.Info);
            }

            if (!string.IsNullOrEmpty(searchId) && eventsSO.Events != null && eventsSO.Events.Count > 0)
            {
                bool hasMatchingEvent = false;
                foreach (var ev in eventsSO.Events)
                {
                    if (ev.ID.Contains(searchId) || ev.Name.Contains(searchId) || searchId == "*")
                    {
                        hasMatchingEvent = true;
                        break;
                    }
                }
                showNoMatchingEventsMessage = !hasMatchingEvent;
            }
            else
            {
                showNoMatchingEventsMessage = false;
            }

            if (showNoMatchingEventsMessage)
            {
                EditorGUILayout.HelpBox("No events with the matching ID found.", MessageType.Warning);
            }
        }

        private void DrawTimestampFields(ref Timestamp timestamp)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = Color.blue;
            EditorGUILayout.HelpBox("Set the timestamp information for which you want to search the events.", MessageType.Info);
            GUI.backgroundColor = Color.white;
            timestamp.Day = (Days)EditorGUILayout.EnumPopup("Day", timestamp.Day);
            timestamp.Date = EditorGUILayout.IntSlider("Date", timestamp.Date, 0, 28);
            timestamp.Hour = EditorGUILayout.IntSlider("Hour", timestamp.Hour, 0, 23);
            timestamp.Minutes = EditorGUILayout.IntSlider("Minutes", timestamp.Minutes, 0, 59);
            timestamp.Year = EditorGUILayout.IntField("Year", timestamp.Year);
            timestamp.Season = (Season)EditorGUILayout.EnumPopup("Season", timestamp.Season);
            EditorGUILayout.EndVertical();
        }

        private void DrawTimestampSearchContent()
        {
            EditorGUILayout.LabelField("Search by Timestamp", EditorStyles.boldLabel);

            GUI.backgroundColor = Color.magenta;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            searchOptionsFoldout = EditorGUILayout.Foldout(searchOptionsFoldout, "Search Options");
            if (searchOptionsFoldout)
            {
                GUI.backgroundColor = Color.blue;
                EditorGUILayout.HelpBox("Select the options to search by. For example, selecting only Year will search all events in that year ignoring all of the other attributes of the timestamp. You can also combine different search options.", MessageType.Info);
                GUI.backgroundColor = Color.white;
                searchByDay = EditorGUILayout.Toggle("Day", searchByDay);
                searchByDate = EditorGUILayout.Toggle("Date", searchByDate);
                searchByHour = EditorGUILayout.Toggle("Hour", searchByHour);
                searchByYear = EditorGUILayout.Toggle("Year", searchByYear);
                searchBySeason = EditorGUILayout.Toggle("Season", searchBySeason);
                searchBetweenTimestamps = EditorGUILayout.Toggle("Search Between Timestamps", searchBetweenTimestamps);
            }
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;

            if (searchBetweenTimestamps)
            {
                // Draw the input for start and end timestamps
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Start Timestamp");
                DrawTimestampFields(ref startTimestamp);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("End Timestamp");
                DrawTimestampFields(ref endTimestamp);
            }
            else
            {
                // Draw the Timestamp fields for normal search
                DrawTimestampFields(ref searchTimestamp);
            }

            if (GUILayout.Button("Search"))
            {
                if (searchBetweenTimestamps)
                {
                    searchByTimestampResults = eventsSO.GetEventsBetweenTimestamps(startTimestamp, endTimestamp);
                }
                else
                {
                    searchByTimestampResults = eventsSO.GetEventsBySelectiveTimestamp(
                        searchTimestamp, searchByDay, searchByDate, searchByHour, searchByYear, searchBySeason
                    );
                }
                isSearchByTimestampPerformed = true;
            }

            DrawSearchByTimestampResults();
        }

        private void DrawSearchByTimestampResults()
        {
            if (!isSearchByTimestampPerformed) return;

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Matching Events", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"{searchByTimestampResults.Count} events found.");

            searchResultsScrollPosition = EditorGUILayout.BeginScrollView(searchResultsScrollPosition, GUILayout.MinHeight(400));

            if (searchByTimestampResults.Count > 0)
            {
                foreach (var ev in searchByTimestampResults)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField($"Event Name: {ev.Name}");
                    EditorGUILayout.LabelField($"Event ID: {ev.ID}");
                    EditorGUILayout.LabelField($"Type: {ev.Type}");
                    EditorGUILayout.LabelField($"Frequency: {ev.Frequency}");
                    EditorGUILayout.LabelField($"Timestamp: {ev.Timestamp.Day} {ev.Timestamp.Date}/{ev.Timestamp.Season}/{ev.Timestamp.Year} - {ev.Timestamp.Hour}:{ev.Timestamp.Minutes}");
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);
                }
            }
            else
            {
                GUI.backgroundColor = Color.yellow;
                EditorGUILayout.HelpBox("No events found for the selected search criteria.", MessageType.Info);
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawNewEventInput()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Add New Event", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("New event will have ID: " + nextAvailableID.ToString(), EditorStyles.boldLabel);

            newEvent.Name = EditorGUILayout.TextField("Name", newEvent.Name);
            newEvent.ID = nextAvailableID.ToString();
            newEvent.Type = (ScheduleEventType)EditorGUILayout.EnumPopup("Type", newEvent.Type);
            newEvent.Frequency = (ScheduleEventFrequency)EditorGUILayout.EnumPopup("Frequency", newEvent.Frequency);

            if (newEvent.Frequency != ScheduleEventFrequency.UNIQUE)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                newEvent.AllSeasons = EditorGUILayout.Toggle("All Seasons", newEvent.AllSeasons);
                if (!newEvent.AllSeasons)
                {
                    EditorGUI.indentLevel++;
                    newEvent.SkipSpring = EditorGUILayout.Toggle("Skip Spring", newEvent.SkipSpring);
                    newEvent.SkipSummer = EditorGUILayout.Toggle("Skip Summer", newEvent.SkipSummer);
                    newEvent.SkipAutumn = EditorGUILayout.Toggle("Skip Autumn", newEvent.SkipAutumn);
                    newEvent.SkipWinter = EditorGUILayout.Toggle("Skip Winter", newEvent.SkipWinter);
                    EditorGUI.indentLevel--;
                }
                GUILayout.EndVertical();
            }

            // Manually draw the Timestamp properties
            EditorGUILayout.LabelField("Timestamp");
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;

            if (newEvent.Frequency == ScheduleEventFrequency.DAILY)
            {
                newEvent.Timestamp.Day = Days.Null;
            }
            else
            {
                newEvent.Timestamp.Day = (Days)EditorGUILayout.EnumPopup("Day", newEvent.Timestamp.Day);
            }

            newEvent.Timestamp.Date = EditorGUILayout.IntSlider("Date", newEvent.Timestamp.Date, 1, 28);
            newEvent.Timestamp.Hour = EditorGUILayout.IntSlider("Hour", newEvent.Timestamp.Hour, 0, 23);
            newEvent.Timestamp.Minutes = EditorGUILayout.IntSlider("Minutes", newEvent.Timestamp.Minutes, 0, 59);
            newEvent.Timestamp.Year = EditorGUILayout.IntField("Year", newEvent.Timestamp.Year);
            newEvent.Timestamp.Season = (Season)EditorGUILayout.EnumPopup("Season", newEvent.Timestamp.Season);
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("This event has no end date", EditorStyles.boldLabel);
            newEvent.IgnoreEndsAt = EditorGUILayout.Toggle("", newEvent.IgnoreEndsAt);

            if (!newEvent.IgnoreEndsAt)
            {
                // Manually draw the EndsAt properties
                EditorGUILayout.LabelField("EndsAt");
                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;
                newEvent.EndsAt.Day = (Days)EditorGUILayout.EnumPopup("Day", newEvent.EndsAt.Day);
                newEvent.EndsAt.Date = EditorGUILayout.IntSlider("Date", newEvent.EndsAt.Date, 1, 28);
                newEvent.EndsAt.Hour = EditorGUILayout.IntSlider("Hour", newEvent.EndsAt.Hour, 0, 23);
                newEvent.EndsAt.Minutes = EditorGUILayout.IntSlider("Minutes", newEvent.EndsAt.Minutes, 0, 59);
                newEvent.EndsAt.Year = EditorGUILayout.IntField("Year", newEvent.EndsAt.Year);
                newEvent.EndsAt.Season = (Season)EditorGUILayout.EnumPopup("Season", newEvent.EndsAt.Season);
                EditorGUI.indentLevel--;
                GUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            bool hasName = !string.IsNullOrEmpty(newEvent.Name);
            bool isEndDateValid = newEvent.IgnoreEndsAt || newEvent.Timestamp.GetFullDate() <= newEvent.EndsAt.GetFullDate();
            bool isNotDailyAndHasNoDay = newEvent.Frequency != ScheduleEventFrequency.DAILY && newEvent.Timestamp.Day == Days.Null;

            if (!hasName)
            {
                EditorGUILayout.HelpBox("Name cannot be empty.", MessageType.Warning);
            }

            if (!isEndDateValid)
            {
                EditorGUILayout.HelpBox("End Date must be after the Start Date.", MessageType.Warning);
            }

            if (isNotDailyAndHasNoDay)
            {
                EditorGUILayout.HelpBox("Day must be set for non-daily events.", MessageType.Warning);
            }

            isAddEventButtonEnabled = hasName && !string.IsNullOrEmpty(newEvent.ID) && isEndDateValid && !isNotDailyAndHasNoDay;

            GUI.backgroundColor = Color.green;
            GUI.enabled = isAddEventButtonEnabled;
            if (GUILayout.Button("Create Event"))
            {
                AddNewEvent();
                isAddEventButtonEnabled = false;
            }
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;

            GUILayout.EndVertical();
        }

        private void AddNewEvent()
        {
            nextAvailableID++;

            eventsSO.Events.Add(newEvent);
            newEvent = new ScheduleEvent(); // Reset for the next input

            eventsSO.GenerateEventsDictionary();

            EditorUtility.SetDirty(eventsSO);

            // Regenerate the serialized object after adding a new event
            serializedEventsSO = new SerializedObject(eventsSO);

            SaveEventsToFile();
            scrollPosition = new Vector2(0, 0);
            successCreatedEvent = true;
            currentTab = 3;
        }

        private void DeleteEventAtIndex(int index)
        {
            if (index >= 0 && index < eventsSO.Events.Count)
            {
                eventsSO.Events.RemoveAt(index);
                EditorUtility.SetDirty(eventsSO);
            }
        }

        private void DrawFilePath()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("JSON File Path:", GUILayout.Width(100));
            jsonFilePath = EditorGUILayout.TextField(jsonFilePath);
            EditorGUILayout.EndHorizontal();

            eventsSO.DefaultFilePath = jsonFilePath;
        }

        private void LoadEventsFromJson()
        {
            if (System.IO.File.Exists(jsonFilePath))
            {
                eventsSO.LoadEventsFromJson(jsonFilePath);
                EditorUtility.SetDirty(eventsSO);
                AssetDatabase.SaveAssets();

                // Following is needed to be able to "refresh" the window
                serializedEventsSO = new SerializedObject(eventsSO); // Update the serialized object after loading from JSON
                errorMessage = "";
                Repaint(); // Repaint the window to display the loaded values
            }
            else
            {
                errorMessage = "File not found.";
            }
        }

        private void DrawSearchBar()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Search by ID or Name:", GUILayout.Width(130));
            searchId = GUILayout.TextField(searchId).Trim();
            GUI.backgroundColor = Color.blue;
            if (GUILayout.Button("Clear", GUILayout.Width(60)))
            {
                searchId = "*";
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawScheduleEvents()
        {
            EditorGUILayout.LabelField("Found Events", EditorStyles.boldLabel);

            // Use the trimmed searchId to check if it's truly empty
            bool isWildcard = searchId == "*";

            for (int i = 0; i < eventsSO.Events.Count; i++)
            {
                var scheduleEvent = eventsSO.Events[i];
                bool isMatch = isWildcard || scheduleEvent.ID.Contains(searchId) || scheduleEvent.Name.Contains(searchId);

                if (isMatch)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField($"Event {i + 1}", EditorStyles.boldLabel);

                    EditorGUI.BeginChangeCheck();
                    scheduleEvent.Name = EditorGUILayout.TextField("Name", scheduleEvent.Name);
                    scheduleEvent.ID = EditorGUILayout.TextField("ID", scheduleEvent.ID);
                    scheduleEvent.Type = (ScheduleEventType)EditorGUILayout.EnumPopup("Type", scheduleEvent.Type);
                    scheduleEvent.Frequency = (ScheduleEventFrequency)EditorGUILayout.EnumPopup("Frequency", scheduleEvent.Frequency);

                    if (scheduleEvent.Frequency != ScheduleEventFrequency.UNIQUE)
                    {
                        scheduleEvent.AllSeasons = EditorGUILayout.Toggle(new GUIContent("All Seasons", "When set to true, will run all the time"), scheduleEvent.AllSeasons);
                        if (!scheduleEvent.AllSeasons)
                        {
                            EditorGUI.indentLevel++;
                            scheduleEvent.SkipSpring = EditorGUILayout.Toggle("Skip Spring", scheduleEvent.SkipSpring);
                            scheduleEvent.SkipSummer = EditorGUILayout.Toggle("Skip Summer", scheduleEvent.SkipSummer);
                            scheduleEvent.SkipAutumn = EditorGUILayout.Toggle("Skip Autumn", scheduleEvent.SkipAutumn);
                            scheduleEvent.SkipWinter = EditorGUILayout.Toggle("Skip Winter", scheduleEvent.SkipWinter);
                            EditorGUI.indentLevel--;
                        }
                    }

                    DrawTimestampProperty(serializedEventsSO.FindProperty("_events").GetArrayElementAtIndex(i).FindPropertyRelative("Timestamp"), "Timestamp");

                    scheduleEvent.IgnoreEndsAt = EditorGUILayout.Toggle("Ignore End Date", scheduleEvent.IgnoreEndsAt);
                    if (!scheduleEvent.IgnoreEndsAt)
                    {
                        DrawTimestampProperty(serializedEventsSO.FindProperty("_events").GetArrayElementAtIndex(i).FindPropertyRelative("EndsAt"), "EndsAt");
                    }

                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    {
                        DeleteEventAtIndex(i);
                    }
                    GUI.backgroundColor = Color.white;

                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(eventsSO);
                        unsavedChanges = true;
                    }

                    EditorGUILayout.Space();
                }
            }
        }

        private void DrawTimestampProperty(SerializedProperty timestampProperty, string title)
        {
            EditorGUILayout.LabelField(title);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(timestampProperty.FindPropertyRelative("Day"));
            EditorGUILayout.PropertyField(timestampProperty.FindPropertyRelative("Date"));
            EditorGUILayout.PropertyField(timestampProperty.FindPropertyRelative("Hour"));
            EditorGUILayout.PropertyField(timestampProperty.FindPropertyRelative("Minutes"));
            EditorGUILayout.PropertyField(timestampProperty.FindPropertyRelative("Year"));
            EditorGUILayout.PropertyField(timestampProperty.FindPropertyRelative("Season"));
            EditorGUI.indentLevel--;
        }

        private void SaveEventsToFile()
        {
            eventsSO.SaveEventsToJson(jsonFilePath);
            AssetDatabase.Refresh();
            unsavedChanges = false;
        }

        private void CalculateTotalEvents()
        {
            if (eventsSO != null && eventsSO.Events != null && eventsSO.Events.Count > 0)
            {
                int maxID = 0;
                foreach (var ev in eventsSO.Events)
                {
                    int eventID;
                    if (int.TryParse(ev.ID, out eventID))
                    {
                        maxID = Mathf.Max(maxID, eventID);
                    }
                }
                nextAvailableID = maxID + 1;
            }
            else
            {
                nextAvailableID = 1;
            }
        }

        private void DrawOverviewContent()
        {
            if (successCreatedEvent)
            {
                EditorGUILayout.HelpBox("Successfully created new event.", MessageType.Info);
            }
            EditorGUILayout.Space();
            DrawEventsPerSeason();
            EditorGUILayout.Space();
            DrawEventsPerYear();
            EditorGUILayout.Space();
            DrawEventsPerDayOfWeek();
            EditorGUILayout.Space();
            DrawEventCountsByFrequency();
        }

        private void DrawEventsPerSeason()
        {
            if (eventsSO.Events == null || eventsSO.Events.Count == 0)
            {
                EditorGUILayout.HelpBox("No events available.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Events per Season", EditorStyles.boldLabel);

            int[] seasonCounts = new int[4];
            string[] seasonNames = { "Spring", "Summer", "Autumn", "Winter" };

            foreach (var ev in eventsSO.Events)
            {
                int seasonIndex = (int)ev.Timestamp.Season;
                seasonCounts[seasonIndex]++;
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);

            for (int i = 0; i < seasonCounts.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(seasonNames[i], GUILayout.Width(80), GUILayout.Height(20));
                EditorGUILayout.LabelField(seasonCounts[i].ToString(), GUILayout.Width(80), GUILayout.Height(20));
                GUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        private void DrawEventsPerYear()
        {
            EditorGUILayout.LabelField("Events per Year", EditorStyles.boldLabel);

            int[] yearCounts = new int[100];

            foreach (var ev in eventsSO.Events)
            {
                yearCounts[ev.Timestamp.Year]++;
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Select Year", GUILayout.Width(100));
            selectedYear = EditorGUILayout.IntSlider(selectedYear, 0, 99);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField($"Year {selectedYear}: {yearCounts[selectedYear]} Events");
            GUILayout.EndVertical();
        }

        private void DrawEventsPerDayOfWeek()
        {
            EditorGUILayout.LabelField("Events per Day of Week", EditorStyles.boldLabel);

            int[] dayOfWeekCounts = new int[8];
            string[] dayOfWeekNames = { "Null", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

            foreach (var ev in eventsSO.Events)
            {
                int dayOfWeekIndex = (int)ev.Timestamp.Day;
                dayOfWeekCounts[dayOfWeekIndex]++;
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);
            for (int i = 0; i < dayOfWeekNames.Length; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(dayOfWeekNames[i], GUILayout.Width(80), GUILayout.Height(20));
                GUILayout.Label(dayOfWeekCounts[i].ToString(), GUILayout.Width(80), GUILayout.Height(20));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void DrawEventCountsByFrequency()
        {
            EditorGUILayout.LabelField("Events per Frequency", EditorStyles.boldLabel);

            int[] frequencyCounts = new int[5];
            string[] frequencyNames = { "Daily", "Weekly", "Monthly", "Annual", "Unique" };

            foreach (var ev in eventsSO.Events)
            {
                int frequencyIndex = (int)ev.Frequency;
                frequencyCounts[frequencyIndex]++;
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);

            for (int i = 0; i < frequencyCounts.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(frequencyNames[i], GUILayout.Width(80), GUILayout.Height(20));
                EditorGUILayout.LabelField(frequencyCounts[i].ToString(), GUILayout.Width(80), GUILayout.Height(20));
                GUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        private void ResetSearchByTimestamp()
        {
            isSearchByTimestampPerformed = false;
            searchByTimestampResults = new List<ScheduleEvent>();
        }

        private void SetCurrentlyEditedObject()
        {
            string serializedEventsSOPath = EditorPrefs.GetString(editorPrefs + "SerializedEventsSOPath", "");
            if (!string.IsNullOrEmpty(serializedEventsSOPath))
            {
                eventsSO = AssetDatabase.LoadAssetAtPath<ScheduleEventsSO>(serializedEventsSOPath);
                if (eventsSO != null)
                {
                    serializedEventsSO = new SerializedObject(eventsSO);
                    jsonFilePath = eventsSO.DefaultFilePath;
                }
            }

            CalculateTotalEvents();
        }

        #region Kept for reference

        private void LoadEventsSOFromFile()
        {
            string path = EditorUtility.OpenFilePanelWithFilters("Select ScheduleEventsSO", Application.dataPath, new string[] { "Schedule Events", "asset" });
            if (!string.IsNullOrEmpty(path))
            {
                string assetsPath = "Assets" + path.Substring(Application.dataPath.Length);
                eventsSO = AssetDatabase.LoadAssetAtPath<ScheduleEventsSO>(assetsPath);
                if (eventsSO != null)
                {
                    serializedEventsSO = new SerializedObject(eventsSO);
                    jsonFilePath = eventsSO.DefaultFilePath;
                }
                else
                {
                    Debug.LogError("Selected file is not a valid ScheduleEventsSO asset.");
                }
            }
        }

        #endregion
    }

}