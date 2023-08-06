using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Slax.Schedule
{
    /// <summary>
    /// This class is responsible for handling Schedule events for NPCs
    /// and any other registered gameplay event based on time
    /// </summary>
    public class ScheduleManager : TickObserver
    {
        /// <summary>Fires a static event with all the Schedule Events happening at that tick</summary>
        public static UnityAction<List<ScheduleEvent>> OnScheduleEvents = delegate { };

        [Header("When set to true, will fire received In Between Tick Events")]
        public bool HasInBetweenTickEvents = false;
        public static UnityAction OnInBetweenTickFired = delegate { };

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        /// <summary>
        /// On every tick received from the TimeManager, gets the events
        /// for the corresponding timestamp if any and raises a UnityAction
        /// with the events for any observer to pickup and process
        /// </summary>
        protected override void CheckEventOnTick(DateTime date)
        {
            List<ScheduleEvent> eventsToStart = _scheduleEvents.GetEventsForTimestamp(date.GetTimestamp());

            if (eventsToStart.Count == 0) return;

            OnScheduleEvents.Invoke(eventsToStart);
        }

        /// <summary>
        /// On every in between tick received from the TimeManager, fires
        /// a static void event for any observer to pickup and process.
        /// </summary>
        protected override void FireInBetweenTick()
        {
            if (HasInBetweenTickEvents) OnInBetweenTickFired.Invoke();
        }

        /// <summary>
        /// Retrieves a list of all the schedule events that are supposed to happen
        /// during the day of the provided date time.
        /// </summary>
        [Obsolete("Deprecated in favour of using a dictionary. Kept for reference and debugging")]
        protected virtual List<ScheduleEvent> GetTodayEvents(DateTime date)
        {
            List<ScheduleEvent> eventsToday = _scheduleEvents.Events.FindAll(e =>
                !e.SkipSeason(date.Season) &&
                (
                    e.Frequency == ScheduleEventFrequency.DAILY ||
                    (
                        e.Timestamp.Date == date.Date &&
                        (
                            (e.Frequency == ScheduleEventFrequency.WEEKLY && e.Timestamp.Day == date.Day) ||
                            (e.Frequency == ScheduleEventFrequency.MONTHLY) ||
                            (e.Frequency == ScheduleEventFrequency.ANNUAL && e.Timestamp.Season == date.Season)
                        )
                    )
                )
            );

            return eventsToday;
        }
    }

}