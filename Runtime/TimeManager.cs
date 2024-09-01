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

        /// <summary>When set to true, the changes during runtime will be saved to the current _timeConfigurationSO</summary>
        protected bool _usingExternalConfiguration = false;

        public bool IsTestingMode => _timeConfiguration != null && !_usingExternalConfiguration;

        protected virtual void Awake()
        {
            _usingExternalConfiguration = false;
        }

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

        /// <summary>
        /// This method is used to initialize the TimeManager with a TimeConfigurationSO.
        /// This is the method that should be used in the real game, instead
        /// of the Initialize method which is used for development & testing.
        /// </summary>
        public virtual void InitializeFromConfiguration(TimeConfigurationSO configuration)
        {
            _timeConfiguration = configuration;
            _usingExternalConfiguration = true;
            Setup();
            CreateDateTime();
        }

        /// <summary>
        /// This method is used to save the current time configuration to the TimeConfigurationSO
        /// that is being used by the TimeManager. Usually used when the TimeManager was
        /// initialized with a provided external TimeConfigurationSO.
        /// </summary>
        public virtual TimeConfigurationSO Save()
        {
            if (_usingExternalConfiguration)
            {
                _timeConfiguration.Setup(_dateTime.Season, _dateTime.Year, _dateTime.Date, _dateTime.Hour, _dateTime.Minutes, _timeConfiguration.DayConfiguration, _timeConfiguration.MaxYears, _timeConfiguration.TickMinutesIncrease, _timeConfiguration.TimeBetweenTicks);
            }
            else
            {
                Debug.LogWarning("TimeManager is not using an external TimeConfigurationSO, changes will not be saved in order to keep the configuration intact.");
            }

            return _timeConfiguration;
        }

        public virtual void Play()
        {
            _isPaused = false;
        }

        public virtual void Pause()
        {
            _isPaused = true;
        }

        public virtual void SetNewDay()
        {
            Pause();
            AdvanceTimeStatus status = _dateTime.SetNewDay();
            if (status.AdvancedDay) OnNewDay?.Invoke(_dateTime);
            if (status.AdvancedSeason) OnNewSeason?.Invoke(_dateTime);
            if (status.AdvancedYear) OnNewYear?.Invoke(_dateTime);
            Play();
        }

        public virtual Timestamp GetTime()
        {
            Timestamp t = new Timestamp
            {
                Season = _season,
                Date = _date,
                Year = _year,
                Hour = _hour,
                Minutes = _minutes
            };
            return t;
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

        /// <summary>
        /// This method is used to change the speed of the time manager during runtime.
        /// 1 is the default speed, to go faster, use a value lower than 1 as it represents
        /// the time between ticks in seconds.
        /// </summary>
        public virtual void ChangeTimeSpeed(float speed)
        {
            _timeBetweenTicks = speed;
        }

        public virtual float GetTimeSpeed()
        {
            return _timeBetweenTicks;
        }

        public virtual void ResetTimeSpeed()
        {
            _timeBetweenTicks = _timeConfiguration.TimeBetweenTicks;
        }

        protected virtual void Tick()
        {
            AdvanceTimeStatus status = _dateTime.AdvanceMinutes(_tickMinutesIncrease);

            if (!IsTestingMode) Save();

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