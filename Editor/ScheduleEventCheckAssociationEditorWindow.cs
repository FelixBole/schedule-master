using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Slax.Schedule
{

    public class ScheduleEventCheckAssociationEditorWindow : EditorWindow
    {
        [SerializeField] private ScheduleMasterConfigurationEditorSO _editorConfig;

        private string _editorPrefs = "Slax-Schedule-EventCheckManager-";
        private ScheduleEventCheckAssociationSO _target;
        private SerializedObject _serializedTarget;
        private ScheduleEventsSO _data;


        private bool _openedToSpecificEvent = false;


        private string _searchID = "";

        #region Navigation
        private int _currentTab = 0;
        private int _currentManagedEventID;
        private string _eventIDFromBrowse = "";
        private Vector2 _browseScrollPos;
        private Vector2 _managementSingleEventScrollPos;
        private Vector2 _creationScrollPos;
        private Vector2 _scrollPos;
        #endregion

        #region Management
        private ScheduleEventCheckerAssociation _associationInCreation;
        private List<ScheduleEventCheckBase> _checkersInCreation = new List<ScheduleEventCheckBase>();
        private ScheduleEventCheckBase _currentCheckInCreation;
        private bool _attemptedToAssociateExisting = false;
        private ScheduleEvent _eventDisplayingInformation;

        #endregion

        #region UI
        GUIStyle _wordWrapStyle;
        #endregion

        public static void OpenWindow(ScheduleEventCheckAssociationSO targetSO)
        {
            ScheduleEventCheckAssociationEditorWindow window = GetWindow<ScheduleEventCheckAssociationEditorWindow>("Event Check Manager");

            window.LoadTargetSO(targetSO);
        }

        public static void OpenWindowToEvent(ScheduleEventCheckAssociationSO targetSO, string eventID)
        {
            ScheduleEventCheckAssociationEditorWindow window = GetWindow<ScheduleEventCheckAssociationEditorWindow>("Event Check Manager");

            window._openedToSpecificEvent = true;
            window._searchID = eventID;
            window._currentTab = 2;
            window.LoadTargetSO(targetSO);
        }

        private void LoadTargetSO(ScheduleEventCheckAssociationSO targetSO)
        {
            _target = targetSO;

            if (_target != null)
            {
                _serializedTarget = new SerializedObject(_target);
            }
            else
            {
                _serializedTarget = null;
            }
        }

        private void OnEnable()
        {
            string serializedTargetPath = EditorPrefs.GetString(_editorPrefs + "SerializedTarget", "");
            if (!string.IsNullOrEmpty(serializedTargetPath))
            {
                _target = AssetDatabase.LoadAssetAtPath<ScheduleEventCheckAssociationSO>(serializedTargetPath);
                if (_target != null)
                {
                    _serializedTarget = new SerializedObject(_target);
                }
            }

            if (!_openedToSpecificEvent)
            {
                _currentTab = EditorPrefs.GetInt(_editorPrefs + "CurrentTab", 0);
            }
            _searchID = "";
            _associationInCreation = null;
            _checkersInCreation = new List<ScheduleEventCheckBase>();
            _currentCheckInCreation = null;
            _eventDisplayingInformation = null;
            _attemptedToAssociateExisting = false;
        }

        private void OnDisable()
        {
            if (_target != null)
            {
                string serializedTargetPath = AssetDatabase.GetAssetPath(_target);
                EditorPrefs.SetString(_editorPrefs + "SerializedTarget", serializedTargetPath);
            }
            EditorPrefs.SetInt(_editorPrefs + "CurrentTab", _currentTab);
        }

        private void SetupUI()
        {
            _wordWrapStyle = new GUIStyle(GUI.skin.label);
            _wordWrapStyle.wordWrap = true;
        }

        private void OnGUI()
        {
            if (!CheckEventsConfiguration()) return;

            SetupUI();

            _data = _target.ScheduleEvents;

            GUILayout.Label("Schedule Event Associations", EditorStyles.boldLabel);

            DrawTabs();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            switch (_currentTab)
            {
                case 0:
                    GoToMain();
                    DrawOverviewContent();
                    break;
                case 1:
                    DrawManagementContent();
                    break;
                case 2:
                    GoToBrowse();
                    DrawBrowseContent();
                    break;
                case 3:
                    GoToOptions();
                    DrawOptionsContent();
                    break;
                default:
                    break;
            }
            EditorGUILayout.EndScrollView();

            _serializedTarget.ApplyModifiedProperties();
        }

        private bool CheckEventsConfiguration()
        {
            if (_target.ScheduleEvents == null)
            {
                _target.ScheduleEvents = (ScheduleEventsSO)EditorGUILayout.ObjectField("Schedule Events", _target.ScheduleEvents as ScheduleEventsSO, typeof(ScheduleEventsSO), false);
                EditorGUILayout.HelpBox("Please provide a valid Schedule Events configuration", MessageType.Warning);

                return false;
            }
            return true;
        }

        private void DrawTabs()
        {
            int numRows = 2;
            string[] tabsRow1 = new string[] { "Overview", "Manage" };
            string[] tabsRow2 = new string[] { "Browse", "Options" };

            BV(true);
            for (int row = 0; row < numRows; row++)
            {
                BH();
                GUILayout.FlexibleSpace();
                string[] tabs = (row == 0) ? tabsRow1 : tabsRow2;
                DrawTabButtons(tabs, row);
                GUILayout.FlexibleSpace();
                EH();
            }
            EV();

        }

        private void DrawTabButtons(string[] tabs, int rowIndex)
        {
            int offset = 2;
            int activeTab = (rowIndex == 0) ? _currentTab : _currentTab - offset;

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
                        _currentTab = i;
                    }
                    else
                    {
                        _currentTab = i + offset;
                    }
                }
            }

            GUI.backgroundColor = Color.white;
        }

        private void DrawOverviewContent()
        {
            Label($"Overview of {_target.name}");
            Label($"Using {_data.name} Events Dataset which has {_data.Events.Count.ToString()} events.");
            Label($"Total Associations: {_target.Associations.Count}");
            Space();

            BV(true);
            Label("Usage");
            Space();
            Label("The Event Check Manager allows you to add and remove check logics to Events.\n\nThese create 'Associations' which are a List of checkers for an event.\n\nDuring Runtime, the ScheduleManager communicates with the reference it has to a  ScheduleEvents dataset instance and runs the checks that exists on an event before Invoking the list of events that have passed the checks.");
            Space();
            EditorGUILayout.HelpBox("Keep in mind that if the ScheduleManager's reference to ScheduleEventsSO is null the system will simply not run any checks.", MessageType.Info);
            EV();
        }

        private void DrawManagementContent()
        {
            if (_associationInCreation == null)
            {
                if (!string.IsNullOrEmpty(_eventIDFromBrowse))
                {
                    ScheduleEvent ev = _data.Events.Find(o => o.ID == _eventIDFromBrowse);
                    if (ev == null)
                    {
                        EditorGUILayout.HelpBox("Cannot find event with this ID", MessageType.Error);
                        return;
                    }

                    int associations = (_target.Associations.Find(o => o.Event.ID == ev.ID)?.Checkers?.Count) ?? 0;

                    DrawEventForManagement(ev, associations);
                }
                else
                {
                    DrawSearchBar();

                    _creationScrollPos = GUILayout.BeginScrollView(_creationScrollPos);

                    for (int i = 0; i < _data.Events.Count; i++)
                    {
                        ScheduleEvent ev = _data.Events[i];
                        int associations = (_target.Associations.Find(o => o.Event.ID == ev.ID)?.Checkers?.Count) ?? 0;

                        bool isMatch = string.IsNullOrEmpty(_searchID) || ev.ID.Contains(_searchID);

                        if (isMatch)
                        {
                            DrawEventForManagement(ev, associations);
                        }
                    }

                    GUILayout.EndScrollView();
                }


                return;
            }

            List<ScheduleEventCheckBase> checksFromSO = _target.Associations.Find(o => o.Event.ID == _associationInCreation.Event.ID)?.Checkers ?? new List<ScheduleEventCheckBase>();

            GUILayout.Label("Creating associations for event", EditorStyles.boldLabel);
            Label($"{_associationInCreation.Event.Name}");
            Label($"(ID: {_associationInCreation.Event.ID})");
            BV(true);
            GUILayout.Label("Assign a Checker Script", EditorStyles.boldLabel);

            _currentCheckInCreation = (ScheduleEventCheckBase)EditorGUILayout.ObjectField("Event Checker", _currentCheckInCreation as ScheduleEventCheckBase, typeof(ScheduleEventCheckBase), false);

            EV();

            if (_currentCheckInCreation != null)
            {
                bool existsInSO = !!checksFromSO.Find(o => o.name == _currentCheckInCreation.name);
                bool existsInNewlyCreated = !!_checkersInCreation.Find(o => o.name == _currentCheckInCreation.name);

                if (existsInSO || existsInNewlyCreated)
                {
                    _attemptedToAssociateExisting = true;
                    _currentCheckInCreation = null;
                }
                else
                {
                    _attemptedToAssociateExisting = false;
                    if (GUILayout.Button("Add"))
                    {
                        _checkersInCreation.Add(_currentCheckInCreation);
                        _currentCheckInCreation = null;
                    }
                }
            }

            if (_attemptedToAssociateExisting)
            {
                EditorGUILayout.HelpBox($"Cannot associate the same check multiple times", MessageType.Error);
            }

            EditorGUILayout.Space();

            if (checksFromSO.Count > 0)
            {
                BV(true);
                Label("Existing checks on this event");
                DrawRemovableChecks(ref checksFromSO);
                EV();
                Space();
            }

            if (_checkersInCreation.Count > 0)
            {
                BV(true);
                Label("Unsaved checks (remember to save to validate)");
                DrawRemovableChecks(ref _checkersInCreation);
                EV();
                Space();

                if (GUILayout.Button("Save"))
                {
                    if (_associationInCreation.Checkers == null)
                    {
                        _associationInCreation.Checkers = new List<ScheduleEventCheckBase>();
                    }

                    _associationInCreation.Checkers.AddRange(_checkersInCreation);

                    if (!_target.Associations.Contains(_associationInCreation))
                    {
                        _target.Associations.Add(_associationInCreation);
                    }

                    _associationInCreation = null;
                    _currentCheckInCreation = null;
                    _checkersInCreation = new List<ScheduleEventCheckBase>();
                    _attemptedToAssociateExisting = false;
                }

            }

        }

        private void DrawBrowseContent()
        {
            GUILayout.Label("Schedule Event Checks Browser", EditorStyles.boldLabel);

            DrawSearchBar();

            _browseScrollPos = EditorGUILayout.BeginScrollView(_browseScrollPos);

            for (int i = 0; i < _data.Events.Count; i++)
            {
                ScheduleEvent ev = _data.Events[i];
                List<ScheduleEventCheckBase> checks = (_target.Associations.Find(o => o.Event.ID == ev.ID)?.Checkers) ?? new List<ScheduleEventCheckBase>();
                bool isMatch = string.IsNullOrEmpty(_searchID) || ev.ID.Contains(_searchID);

                if (isMatch)
                {
                    BV(true);
                    BH();
                    BV();

                    Label($"Name: {ev.Name}");
                    Label($"ID: {ev.ID}");
                    Label($"{checks.Count} Checks");

                    EV();

                    if (checks.Count != 0)
                    {
                        if (_eventDisplayingInformation != null && _eventDisplayingInformation.ID == ev.ID)
                        {
                            if (GUILayout.Button(_editorConfig.EyeOpened, GUILayout.MaxWidth(35), GUILayout.MaxHeight(25)))
                            {
                                _eventDisplayingInformation = null;
                            }
                        }
                        else
                        {
                            if (GUILayout.Button(_editorConfig.EyeClosed, GUILayout.MaxWidth(35), GUILayout.MaxHeight(25)))
                            {
                                _eventDisplayingInformation = ev;
                            }
                        }

                    }
                    
                    if (GUILayout.Button(_editorConfig.Cog, GUILayout.MaxWidth(35), GUILayout.MaxHeight(25)))
                    {
                        _eventIDFromBrowse = ev.ID;
                        _currentTab = 1;
                    }

                    EH();

                    if (_eventDisplayingInformation != null && ev.ID == _eventDisplayingInformation.ID)
                    {
                        Separator();
                        GUILayout.Label("Associated checkers", EditorStyles.boldLabel);
                        foreach (var check in checks)
                        {
                            BV(true);
                            Label($"{check.name}");
                            EV();
                        }
                    }

                    EV();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawOptionsContent()
        {
            _target.ScheduleEvents = (ScheduleEventsSO)EditorGUILayout.ObjectField("Events Dataset", _target.ScheduleEvents as ScheduleEventsSO, typeof(ScheduleEventsSO), false);
        }

        private void DrawSearchBar()
        {
            BH();
            GUILayout.Label("Search by ID:", GUILayout.Width(100));
            _searchID = GUILayout.TextField(_searchID);
            EH();
        }

        public List<ScheduleEventCheckBase> DrawRemovableChecks(ref List<ScheduleEventCheckBase> checks)
        {
            List<ScheduleEventCheckBase> toRemove = new List<ScheduleEventCheckBase>();
            foreach (var check in checks)
            {
                BV(true);
                BH();
                Label($"{check.name}");
                if (GUILayout.Button("x", GUILayout.MaxWidth(35)))
                {
                    toRemove.Add(check);
                }
                EH();
                EV();
            }

            if (toRemove.Count > 0)
            {
                foreach (var el in toRemove)
                {
                    checks.Remove(el);
                }
            }

            return checks;
        }

        private void DrawEventForManagement(ScheduleEvent ev, int associations)
        {
            BV(true);
            BH();
            BV();
            Label($"Name: {ev.Name}");
            Label($"ID: {ev.ID}");
            Label($"{associations} Checks");
            EV();

            string buttonText = associations > 0 ? "Manage" : "Add checks";

            if (GUILayout.Button(buttonText, GUILayout.MaxWidth(100)))
            {
                _associationInCreation = _target.Associations.Find(o => o.Event.ID == ev.ID);

                if (_associationInCreation == null)
                {
                    _associationInCreation = new ScheduleEventCheckerAssociation();
                    _associationInCreation.Event = ev;
                    _associationInCreation.Checkers = new List<ScheduleEventCheckBase>();
                    _target.Associations.Add(_associationInCreation);
                }
            }

            EH();
            EV();
        }

        #region Navigation
        private void TabExitManage()
        {
            _associationInCreation = null;
            _checkersInCreation = new List<ScheduleEventCheckBase>();
            _currentCheckInCreation = null;
            _attemptedToAssociateExisting = false;
            _eventIDFromBrowse = "";
        }

        private void GoToMain()
        {
            TabExitManage();
        }

        private void GoToOptions()
        {
            TabExitManage();
        }

        private void GoToBrowse()
        {
            TabExitManage();
        }
        #endregion

        #region QuickAccess
        private void BV(bool helpBox = false)
        {
            if (helpBox) EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            else EditorGUILayout.BeginVertical();
        }

        private void EV()
        {
            EditorGUILayout.EndVertical();
        }

        private void BH(bool helpBox = false)
        {
            if (helpBox) EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            else EditorGUILayout.BeginHorizontal();
        }

        private void EH(bool helpBox = false)
        {
            EditorGUILayout.EndHorizontal();
        }

        private void Label(string content)
        {
            GUILayout.Label(content, _wordWrapStyle);
        }

        private void Space()
        {
            EditorGUILayout.Space();
        }

        private void Separator()
        {
            Rect separatorRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(1));
            Color separatorColor = new Color(0.6f, 0.6f, 0.6f);
            EditorGUI.DrawRect(separatorRect, separatorColor);
        }
        #endregion
    }
}