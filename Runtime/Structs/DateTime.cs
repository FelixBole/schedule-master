using UnityEngine;

namespace Slax.Schedule
{
    /// <summary>
    /// The core struct of the Time and Schedule system. It holds information
    /// about the time of day, the date, and offers many helper properties and
    /// methods for the classes using this struct
    /// </summary>
    [System.Serializable]
    public struct DateTime
    {
        #region Properties
        private Days _day;
        /// <summary>The current Day of the week</summary>
        public Days Day => _day;
        private int _date;
        /// <summary>The current Date (day # in the month)</summary>
        public int Date => _date;
        private int _year;
        /// <summary>The current Year</summary>
        public int Year => _year;

        private int _hour;
        /// <summary>Tue current Hour</summary>
        public int Hour => _hour;
        private int _minutes;
        /// <summary>The current minutes</summary>
        public int Minutes => _minutes;

        private Season _season;
        /// <summary>The current season</summary>
        public Season Season => _season;

        private int _totalNumDays;
        public int TotalNumDays => _totalNumDays;
        private int _totalNumWeeks;
        public int TotalNumWeeks => _totalNumWeeks;
        /// <summary>Current week on a 4 seasons of 28 days basis</summary>
        public int CurrentWeek => _totalNumWeeks % 16 == 0 ? 16 : _totalNumWeeks % 16;

        private DayConfiguration _dayConfiguration;
        #endregion

        public DateTime(int date, int season, int year, int hour, int minutes, DayConfiguration dayConfiguration)
        {
            _day = (Days)(date % 7);
            if (_day == 0) _day = (Days)7;
            _date = date;
            _season = (Season)season;
            _year = year;

            _hour = hour;
            _minutes = minutes;

            _totalNumDays = (int)_season > 0 ? date + (28 * (int)_season) : date;
            _totalNumDays = year > 1 ? _totalNumDays + ((28 * 4) * (year - 1)) : _totalNumDays;

            _totalNumWeeks = 1 + _totalNumDays / 7;

            _dayConfiguration = dayConfiguration;
        }

        #region Time Advancement

        /// <summary>
        /// Advances the time by a certain amount of minutes
        /// </summary>
        public void AdvanceMinutes(int minutes)
        {
            if (_minutes + minutes >= 60)
            {
                _minutes = (_minutes + minutes) % 60;
                AdvanceHour();
            }
            else _minutes += minutes;
        }

        /// <summary>
        /// Called when the advance minutes notices
        /// a change in hour is needed
        /// </summary>
        private void AdvanceHour()
        {
            if ((_hour + 1) == 24)
            {
                _hour = 0;
                AdvanceDay();
            }
            else _hour++;
        }

        /// <summary>
        /// Called when the advance hour notices
        /// a change in day is needed
        /// </summary>
        private void AdvanceDay()
        {
            if (_day + 1 > (Days)7)
            {
                _day = (Days)1;
                _totalNumWeeks++;
            }
            else _day++;

            _date++;

            if (_date % 29 == 0)
            {
                AdvanceSeason();
                _date = 1;
            }

            _totalNumDays++;
        }

        /// <summary>
        /// Called when the advance Day notices
        /// a change in seasons is needed
        /// </summary>
        private void AdvanceSeason()
        {
            if (_season == Season.Winter)
            {
                _season = Season.Spring;
                AdvanceYear();
            }
            else _season++;
        }

        /// <summary>
        /// Called when Advance season notices
        /// a change in year is needed
        /// </summary>
        private void AdvanceYear()
        {
            _date = 1;
            _year++;
        }
        #endregion

        #region Bool Checks
        public bool IsNight => _hour >= _dayConfiguration.NightStartHour || _hour < _dayConfiguration.MorningStartHour;
        public bool IsMorning => _hour >= _dayConfiguration.MorningStartHour && _hour < _dayConfiguration.AfternoonStartHour;
        public bool IsAfternoon => _hour >= _dayConfiguration.AfternoonStartHour && _hour < _dayConfiguration.NightStartHour;
        public bool IsWeekend => _day > Days.Fri;
        public bool IsParticularDay(Days day) => day == _day;
        #endregion

        #region Season Start
        public DateTime SeasonStart(Season season, int year)
        {
            season = (Season)Mathf.Clamp((int)season, 0, 3);
            if (year == 0) year = 1;

            return new DateTime(1, (int)season, year, _dayConfiguration.MorningStartHour, 0, _dayConfiguration);
        }

        public DateTime SpringStart(int year) => SeasonStart(Season.Spring, year);
        public DateTime SummerStart(int year) => SeasonStart(Season.Summer, year);
        public DateTime AutumnStart(int year) => SeasonStart(Season.Autumn, year);
        public DateTime WinterStart(int year) => SeasonStart(Season.Winter, year);
        #endregion

        /// <summary>
        /// Returns a Schedule Timestamp. Useful for comparing events to the
        /// current timestamp and check if they should run
        /// </summary>
        public Timestamp GetTimestamp() => new Timestamp(_day, _date, _hour, _minutes, _year, _season);

        public override string ToString()
        {
            return $"Date: {DateToString()} Season: {_season.ToString()} Time: {TimeToString()} " +
                $"\nTotal Days: {_totalNumDays} | Total Weeks: {_totalNumWeeks}";
        }

        public string DateToString() => $"{Day} {Date}";
        public string TimeToString()
        {
            int amPmHour = 0;

            if (_hour == 0) amPmHour = 12;
            else if (_hour == 24) amPmHour = 12;
            else if (_hour >= 13) amPmHour = _hour - 12;
            else amPmHour = _hour;

            string AmPm = _hour < 12 ? "AM" : "PM";

            return $"{amPmHour.ToString("D2")}:{_minutes.ToString("D2")} {AmPm}";
        }
    }

    /// <summary>
    /// Configuration of one in game day
    /// </summary>
    [System.Serializable]
    public struct DayConfiguration
    {
        [Range(0, 23)]
        public int MorningStartHour;
        [Range(0, 23)]
        public int AfternoonStartHour;
        [Range(0, 23)]
        public int NightStartHour;
    }

    /// <summary>
    /// Days of the week
    /// </summary>
    [System.Serializable]
    public enum Days
    {
        NULL = 0,
        Mon = 1,
        Tue = 2,
        Wed = 3,
        Thu = 4,
        Fri = 5,
        Sat = 6,
        Sun = 7
    }

    /// <summary>
    /// Seasons, can be considered as months
    /// </summary>
    [System.Serializable]
    public enum Season
    {
        Spring = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 3
    }
}
