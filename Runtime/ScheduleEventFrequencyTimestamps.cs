using UnityEngine;

namespace Slax.Schedule
{
    /// <summary>
    /// A useful struct for handling events based on certain timestamps
    /// </summary>
    [System.Serializable]
    public struct Timestamp
    {
        public Days Day;
        [Range(1, 28)]
        public int Date;
        [Range(0, 23)]
        public int Hour;
        [Range(0, 59)]
        public int Minutes;
        public int Year;
        public Season Season;

        public Timestamp(Days day, int date, int hour, int minutes, int year, Season season)
        {
            Date = date;
            Day = day;
            Hour = hour;
            Minutes = minutes;
            Year = year;
            Season = season;
        }

        /// <summary>Returns the Time in total minutes</summary>
        public int GetTime() => (Hour * 60) + Minutes;
        /// <summary>Returns the date in the year without the year</summary>
        public int GetDate() => (((int)Season) * 44640) + (Date * 1440);
        /// <summary>Returns the full date, year included</summary>
        public int GetFullDate() => ((Year) * 525600) + (((int)Season) * 44640) + (Date * 1440);

        /// <summary>
        /// Checks if the timestamp is between two other timestamps
        /// </summary>
        public bool IsBetween(Timestamp start, Timestamp end)
        {
            // Convert timestamps to comparable values like ticks or a custom comparison logic
            // Here we use a simple comparison logic based on their numeric fields
            if (Year < start.Year || Year > end.Year) return false;
            if (Year == start.Year && Season < start.Season) return false;
            if (Year == end.Year && Season > end.Season) return false;

            if (Year == start.Year && Season == start.Season && Date < start.Date) return false;
            if (Year == end.Year && Season == end.Season && Date > end.Date) return false;

            if (Year == start.Year && Season == start.Season && Date == start.Date && Hour < start.Hour) return false;
            if (Year == end.Year && Season == end.Season && Date == end.Date && Hour > end.Hour) return false;

            return true;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap around
            {
                int hash = 17;

                hash = hash * 31 + Day.GetHashCode();
                hash = hash * 31 + Date.GetHashCode();
                hash = hash * 31 + Hour.GetHashCode();
                hash = hash * 31 + Minutes.GetHashCode();
                hash = hash * 31 + Year.GetHashCode();
                hash = hash * 31 + Season.GetHashCode();

                return hash;
            }
        }
    }

    public struct DailyTimestamp
    {
        public int Hour;
        public int Minutes;

        public DailyTimestamp(int hour, int minutes)
        {
            Hour = hour;
            Minutes = minutes;
        }

        public override int GetHashCode()
        {
            return Hour * 60 + Minutes;
        }
    }

    // Weekly timestamp, only need to store the day and time
    public struct WeeklyTimestamp
    {
        public Days Day;
        public int Hour;
        public int Minutes;

        public WeeklyTimestamp(Days day, int hour, int minutes)
        {
            Day = day;
            Hour = hour;
            Minutes = minutes;
        }

        public override int GetHashCode()
        {
            return ((int)Day * 1440) + (Hour * 60) + Minutes;
        }
    }

    // Monthly timestamp, store the date, time, and month
    public struct MonthlyTimestamp
    {
        public int Date;
        public int Hour;
        public int Minutes;

        public MonthlyTimestamp(int date, int hour, int minutes)
        {
            Date = date;
            Hour = hour;
            Minutes = minutes;
        }

        public override int GetHashCode()
        {
            return (Date * 1440) + (Hour * 60) + Minutes;
        }
    }

    // Annual timestamp, store the date, time, month (season)
    public struct AnnualTimestamp
    {
        public int Date;
        public int Hour;
        public int Minutes;
        public int Month;

        public AnnualTimestamp(int date, int hour, int minutes, int month)
        {
            Date = date;
            Hour = hour;
            Minutes = minutes;
            Month = month;
        }

        public override int GetHashCode()
        {
            return ((Month) * 44640) + (Date * 1440) + (Hour * 60) + Minutes;
        }
    }

    /// <summary>
    /// We need to build this struct as there are other elements on the Timestamp
    /// that can mess with the search in the dictionnary
    /// </summary>
    public struct UniqueTimestamp
    {
        public int Date;
        public int Hour;
        public int Minutes;
        public int Month;
        public int Year;

        public UniqueTimestamp(int date, int hour, int minutes, int month, int year)
        {
            Date = date;
            Hour = hour;
            Minutes = minutes;
            Month = month;
            Year = year;
        }

        public override int GetHashCode()
        {
            return ((Year) * 525600) + ((Month) * 44640) + (Date * 1440) + (Hour * 60) + Minutes;
        }
    }
}