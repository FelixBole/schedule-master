using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

namespace Slax.Schedule
{
    [CreateAssetMenu(menuName = "Slax/ScheduleMaster/ScheduleEvents", fileName = "ScheduleEvents")]
    public class ScheduleEventsSO : ScriptableObject
    {
        [SerializeField] private List<ScheduleEvent> _events = new List<ScheduleEvent>();
        public List<ScheduleEvent> Events => _events;
        private Dictionary<UniqueTimestamp, List<ScheduleEvent>> _uniqueEventsDict;
        private Dictionary<DailyTimestamp, List<ScheduleEvent>> _dailyEventsDict;
        private Dictionary<WeeklyTimestamp, List<ScheduleEvent>> _weeklyEventsDict;
        private Dictionary<MonthlyTimestamp, List<ScheduleEvent>> _monthlyEventsDict;
        private Dictionary<AnnualTimestamp, List<ScheduleEvent>> _annualEventsDict;

        [Header("Default JSON File Path")]
        [Tooltip("The default path for saving and loading the events as a JSON file.")]
        public string DefaultFilePath = "Assets/schedule_events.json";

        private void OnEnable()
        {
            LoadEventsFromJson(DefaultFilePath);
        }

        public void SaveEventsToJson(string filePath)
        {
            string json = JsonConvert.SerializeObject(_events, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public void LoadEventsFromJson(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                _events = JsonConvert.DeserializeObject<List<ScheduleEvent>>(json);
                GenerateEventsDictionary();
            }
            else
            {
                Debug.LogWarning("There is no JSON file for your ScheduleEvents at : " + DefaultFilePath + ", open the ScheduleEvents editor to generate a JSON file at the path of your choice.");
            }
        }

        /// <summary>
        /// Generates dictionaries to organize the ScheduleEvent objects based on their Timestamp and Frequency.
        /// Events are categorized into unique, daily, weekly, monthly, and annual frequency types for efficient access.
        /// </summary>
        public void GenerateEventsDictionary()
        {
            _uniqueEventsDict = new Dictionary<UniqueTimestamp, List<ScheduleEvent>>();
            _dailyEventsDict = new Dictionary<DailyTimestamp, List<ScheduleEvent>>();
            _weeklyEventsDict = new Dictionary<WeeklyTimestamp, List<ScheduleEvent>>();
            _monthlyEventsDict = new Dictionary<MonthlyTimestamp, List<ScheduleEvent>>();
            _annualEventsDict = new Dictionary<AnnualTimestamp, List<ScheduleEvent>>();

            foreach (var ev in _events)
            {
                switch (ev.Frequency)
                {
                    case ScheduleEventFrequency.UNIQUE:
                        AddToDictionary(ev, _uniqueEventsDict, new UniqueTimestamp(ev.Timestamp.Date, ev.Timestamp.Hour, ev.Timestamp.Minutes, (int)ev.Timestamp.Season, ev.Timestamp.Year));
                        break;
                    case ScheduleEventFrequency.DAILY:
                        AddToDictionary(ev, _dailyEventsDict, new DailyTimestamp(ev.Timestamp.Hour, ev.Timestamp.Minutes));
                        break;
                    case ScheduleEventFrequency.WEEKLY:
                        AddToDictionary(ev, _weeklyEventsDict, new WeeklyTimestamp(ev.Timestamp.Day, ev.Timestamp.Hour, ev.Timestamp.Minutes));
                        break;
                    case ScheduleEventFrequency.MONTHLY:
                        AddToDictionary(ev, _monthlyEventsDict, new MonthlyTimestamp(ev.Timestamp.Date, ev.Timestamp.Hour, ev.Timestamp.Minutes));
                        break;
                    case ScheduleEventFrequency.ANNUAL:
                        AddToDictionary(ev, _annualEventsDict, new AnnualTimestamp(ev.Timestamp.Date, ev.Timestamp.Hour, ev.Timestamp.Minutes, (int)ev.Timestamp.Season));
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Adds a ScheduleEvent to the specified dictionary based on its Timestamp.
        /// If the Timestamp is not already in the dictionary, a new entry is created.
        /// </summary>
        /// <param name="ev">The ScheduleEvent to add to the dictionary.</param>
        /// <param name="dictionary">The dictionary to add the ScheduleEvent to.</param>
        private void AddToDictionary(ScheduleEvent ev, Dictionary<Timestamp, List<ScheduleEvent>> dictionary)
        {
            if (!dictionary.TryGetValue(ev.Timestamp, out var eventsList))
            {
                eventsList = new List<ScheduleEvent>();
                dictionary[ev.Timestamp] = eventsList;
            }
            eventsList.Add(ev);
        }

        /// <summary>
        /// Adds a ScheduleEvent to the specified dictionary based on its DailyTimestamp key.
        /// If the key is not already in the dictionary, a new entry is created.
        /// </summary>
        /// <param name="ev">The ScheduleEvent to add to the dictionary.</param>
        /// <param name="dictionary">The dictionary to add the ScheduleEvent to.</param>
        /// <param name="key">The DailyTimestamp key for the ScheduleEvent.</param>
        private void AddToDictionary(ScheduleEvent ev, Dictionary<DailyTimestamp, List<ScheduleEvent>> dictionary, DailyTimestamp key)
        {
            if (!dictionary.TryGetValue(key, out var eventsList))
            {
                eventsList = new List<ScheduleEvent>();
                dictionary[key] = eventsList;
            }
            eventsList.Add(ev);
        }

        /// <summary>
        /// Adds a ScheduleEvent to the specified dictionary based on its WeeklyTimestamp key.
        /// If the key is not already in the dictionary, a new entry is created.
        /// </summary>
        /// <param name="ev">The ScheduleEvent to add to the dictionary.</param>
        /// <param name="dictionary">The dictionary to add the ScheduleEvent to.</param>
        /// <param name="key">The WeeklyTimestamp key for the ScheduleEvent.</param>
        private void AddToDictionary(ScheduleEvent ev, Dictionary<WeeklyTimestamp, List<ScheduleEvent>> dictionary, WeeklyTimestamp key)
        {
            if (!dictionary.TryGetValue(key, out var eventsList))
            {
                eventsList = new List<ScheduleEvent>();
                dictionary[key] = eventsList;
            }
            eventsList.Add(ev);
        }


        /// <summary>
        /// Adds a ScheduleEvent to the specified dictionary based on its MonthlyTimestamp key.
        /// If the key is not already in the dictionary, a new entry is created.
        /// </summary>
        /// <param name="ev">The ScheduleEvent to add to the dictionary.</param>
        /// <param name="dictionary">The dictionary to add the ScheduleEvent to.</param>
        /// <param name="key">The MonthlyTimestamp key for the ScheduleEvent.</param>
        private void AddToDictionary(ScheduleEvent ev, Dictionary<MonthlyTimestamp, List<ScheduleEvent>> dictionary, MonthlyTimestamp key)
        {
            if (!dictionary.TryGetValue(key, out var eventsList))
            {
                eventsList = new List<ScheduleEvent>();
                dictionary[key] = eventsList;
            }
            eventsList.Add(ev);
        }

        /// <summary>
        /// Adds a ScheduleEvent to the specified dictionary based on its AnnualTimestamp key.
        /// If the key is not already in the dictionary, a new entry is created.
        /// </summary>
        /// <param name="ev">The ScheduleEvent to add to the dictionary.</param>
        /// <param name="dictionary">The dictionary to add the ScheduleEvent to.</param>
        /// <param name="key">The AnnualTimestamp key for the ScheduleEvent.</param>
        private void AddToDictionary(ScheduleEvent ev, Dictionary<AnnualTimestamp, List<ScheduleEvent>> dictionary, AnnualTimestamp key)
        {
            if (!dictionary.TryGetValue(key, out var eventsList))
            {
                eventsList = new List<ScheduleEvent>();
                dictionary[key] = eventsList;
            }
            eventsList.Add(ev);
        }

        /// <summary>
        /// Adds a ScheduleEvent to the specified dictionary based on its UniqueTimestamp key.
        /// If the key is not already in the dictionary, a new entry is created.
        /// </summary>
        /// <param name="ev">The ScheduleEvent to add to the dictionary.</param>
        /// <param name="dictionary">The dictionary to add the ScheduleEvent to.</param>
        /// <param name="key">The UniqueTimestamp key for the ScheduleEvent.</param>
        private void AddToDictionary(ScheduleEvent ev, Dictionary<UniqueTimestamp, List<ScheduleEvent>> dictionary, UniqueTimestamp key)
        {
            if (!dictionary.TryGetValue(key, out var eventsList))
            {
                eventsList = new List<ScheduleEvent>();
                dictionary[key] = eventsList;
            }
            eventsList.Add(ev);
        }


        /// <summary>
        /// Tries to find the events associated to the corresponding timestamp
        /// Returns an empty List if the timestamp is not found
        /// </summary>
        public List<ScheduleEvent> GetEventsForTimestamp(Timestamp timestamp)
        {
            List<ScheduleEvent> eventsToReturn = new List<ScheduleEvent>();

            if (_uniqueEventsDict.TryGetValue(new UniqueTimestamp(timestamp.Date, timestamp.Hour, timestamp.Minutes, (int)timestamp.Season, timestamp.Year), out var uniqueEvents))
            {
                eventsToReturn.AddRange(uniqueEvents);
            }

            if (_dailyEventsDict.TryGetValue(new DailyTimestamp(timestamp.Hour, timestamp.Minutes), out var dailyEvents))
            {
                eventsToReturn.AddRange(dailyEvents);
            }

            if (_weeklyEventsDict.TryGetValue(new WeeklyTimestamp(timestamp.Day, timestamp.Hour, timestamp.Minutes), out var weeklyEvents))
            {
                eventsToReturn.AddRange(weeklyEvents);
            }

            if (_monthlyEventsDict.TryGetValue(new MonthlyTimestamp(timestamp.Date, timestamp.Hour, timestamp.Minutes), out var monthlyEvents))
            {
                eventsToReturn.AddRange(monthlyEvents);
            }

            if (_annualEventsDict.TryGetValue(new AnnualTimestamp(timestamp.Date, timestamp.Hour, timestamp.Minutes, (int)timestamp.Season), out var annualEvents))
            {
                eventsToReturn.AddRange(annualEvents);
            }

            return eventsToReturn;
        }
    }

    public enum ScheduleEventType
    {
        NPC,
        GAMEPLAY,
    }

    public enum ScheduleEventFrequency
    {
        DAILY,
        WEEKLY,
        MONTHLY,
        ANNUAL,
        UNIQUE,
    }

    [System.Serializable]
    public class ScheduleEvent
    {
        /// <summary>Name of the event</summary>
        public string Name;

        /// <summary>Manual given ID of the event. Useful to match with the right NPC / Event</summary>
        public string ID;
        public bool AllSeasons = false;
        public bool SkipSpring = false;
        public bool SkipSummer = false;
        public bool SkipAutumn = false;
        public bool SkipWinter = false;
        public ScheduleEventType Type;
        public ScheduleEventFrequency Frequency;
        public Timestamp Timestamp;
        public bool IgnoreEndsAt;
        /// <summary>If an event running on a frequency ends at a specific date</summary>
        public Timestamp EndsAt;

        /// <summary>
        /// Verifies if the event should be fired or not
        /// </summary>
        public bool IsValid(Timestamp t)
        {
            if (Frequency == ScheduleEventFrequency.UNIQUE)
            {
                if (t.GetHashCode() != Timestamp.GetHashCode()) return false;
            }

            if (
                Timestamp.GetHashCode() > t.GetHashCode() // Event hasn't started
                || (!IgnoreEndsAt && t.GetFullDate() > EndsAt.GetFullDate()) // Event has expired
                || t.GetTime() != Timestamp.GetTime()
            )
                return false;

            if (Frequency == ScheduleEventFrequency.WEEKLY && t.Day != Timestamp.Day)
                return false;

            if (Frequency == ScheduleEventFrequency.MONTHLY && t.Date != Timestamp.Date)
                return false;

            if (Frequency == ScheduleEventFrequency.ANNUAL && t.GetDate() != Timestamp.GetDate())
                return false;

            return true;
        }

        /// <summary>
        /// Checks if the event should skip the current season
        /// </summary>
        public bool SkipSeason(Season s)
        {
            switch (s)
            {
                case Season.Spring:
                    return SkipSpring;
                case Season.Autumn:
                    return SkipAutumn;
                case Season.Summer:
                    return SkipSummer;
                case Season.Winter:
                    return SkipWinter;
                default:
                    return false;
            }
        }
    }
}