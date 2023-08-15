using UnityEngine;
using UnityEngine.Events;

namespace Slax.Schedule
{

    /// <summary>
    /// This class is responsible for handling time progress and invoking Tick update events
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        [SerializeField] protected TimeConfigurationSO _timeConfiguration;
        protected Season _season;
        protected int _maxYears = 99;
        protected int _year = 1;
        protected int _date = 0;
        protected int _hour = 0;
        protected int _minutes = 0;
        protected DayConfiguration _dayConfiguration;
        protected bool _isPaused = true;

        public static event UnityAction<DateTime> OnAwake;
        public static event UnityAction<DateTime> OnNewDay;
        public static event UnityAction<DateTime> OnNewSeason;
        public static event UnityAction<DateTime> OnNewYear;
        public static event UnityAction<DateTime> OnDateTimeChanged;
        /// <summary>Fired if there is time between ticks, can be useful</summary>
        public static event UnityAction OnInBetweenTickFired;
        protected DateTime _dateTime;
        protected int _tickMinutesIncrease = 10;
        protected float _timeBetweenTicks = 1f;
        protected float _currentTimeBetweenTicks = 0;

        protected virtual void Update()
        {
            if (_isPaused) return;
            _currentTimeBetweenTicks += Time.deltaTime;

            if (_currentTimeBetweenTicks >= _timeBetweenTicks)
            {
                _currentTimeBetweenTicks = 0;
                Tick();
            }
            else
            {
                OnInBetweenTickFired.Invoke();
            }
        }

        public virtual void Initialize()
        {
            Setup();
            CreateDateTime();
        }

        public virtual void Start()
        {
            _isPaused = false;
        }

        public virtual void Pause()
        {
            _isPaused = true;
        }

        public virtual void SetTime(Timestamp t)
        {
            _season = t.Season;
            _date = t.Date;
            _year = t.Year;
            _hour = t.Hour;
            _minutes = t.Minutes;

            CreateDateTime();
        }

        protected virtual void Tick()
        {
            AdvanceTimeStatus status = _dateTime.AdvanceMinutes(_tickMinutesIncrease);
            OnDateTimeChanged?.Invoke(_dateTime);
            if (status.AdvancedDay) OnNewDay?.Invoke(_dateTime);
            if (status.AdvancedSeason) OnNewSeason?.Invoke(_dateTime);
            if (status.AdvancedYear) OnNewYear?.Invoke(_dateTime);
        }

        protected virtual void Setup()
        {
            _season = _timeConfiguration.Season;
            _maxYears = _timeConfiguration.MaxYears;
            _year = _timeConfiguration.Year;
            _date = _timeConfiguration.Date;
            _hour = _timeConfiguration.Hour;
            _minutes = _timeConfiguration.Minutes;
            _dayConfiguration = _timeConfiguration.DayConfiguration;
            _tickMinutesIncrease = _timeConfiguration.TickMinutesIncrease;
            _timeBetweenTicks = _timeConfiguration.TimeBetweenTicks;
        }

        protected virtual void CreateDateTime()
        {
            _dateTime = new DateTime(_date, (int)_season, _year, _hour, _minutes * _tickMinutesIncrease, _dayConfiguration);

            // We Invoke here so that other scripts can setup during awake with
            // the starting DateTime
            OnAwake?.Invoke(_dateTime);
            OnDateTimeChanged?.Invoke(_dateTime);
        }
    }
}