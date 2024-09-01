using UnityEngine;

namespace Slax.Schedule
{
    /// <summary>
    /// This class is here to hold a configuration setting for the TimeManager
    /// this allows to create different assets for different time configuration.
    /// It allows for easier testing of different values in time easily by switching
    /// assets instead of changing the configuration every time on the TimeManager
    /// </summary>
    [CreateAssetMenu(menuName = "Slax/ScheduleMaster/TimeConfiguration", fileName = "NewTimeConfiguration")]
    [System.Serializable]
    public class TimeConfigurationSO : ScriptableObject
    {
        /// <summary>To keep track of what its for</summary>
        [SerializeField] protected string _notes;
        [SerializeField] protected Season _season;
        public Season Season => _season;
        [SerializeField] protected int _maxYears = 99;
        public int MaxYears => _maxYears;
        [SerializeField] protected int _year = 1;
        public int Year => _year;
        [Range(1, 28)]
        [Tooltip("Day in month")]
        [SerializeField] protected int _date = 0;
        public int Date => _date;
        [Range(0, 24)]
        [SerializeField] protected int _hour = 0;
        public int Hour => _hour;
        [SerializeField] protected int _minutes = 0;
        public int Minutes => _minutes;
        [SerializeField] protected DayConfiguration _dayConfiguration;
        public DayConfiguration DayConfiguration => _dayConfiguration;
        [SerializeField] protected int _tickMinutesIncrease = 10;
        public int TickMinutesIncrease => _tickMinutesIncrease;
        [SerializeField] protected float _timeBetweenTicks = 1f;
        public float TimeBetweenTicks => _timeBetweenTicks;

        public void Setup(Season season, int year, int date, int hour, int minutes, DayConfiguration dayConfiguration, int maxYears, int tickMinutesIncrease, float timeBetweenTicks)
        {
            _season = season;
            _year = year;
            _date = date;
            _hour = hour;
            _tickMinutesIncrease = tickMinutesIncrease;
            _minutes = minutes / _tickMinutesIncrease;
            _dayConfiguration = dayConfiguration;
            _maxYears = maxYears;
            _timeBetweenTicks = timeBetweenTicks;
        }
    }
}
