using System.Collections.Generic;
using UnityEngine.Events;

namespace Slax.Schedule
{
    /// <summary>
    /// This class acts as a base class for scripts that are event observers
    /// and need to invoke unity events based on multiple ScheduleEvents
    /// </summary>
    public class ScheduleMultiEventAction : ScheduleManagerObserver
    {
        public List<string> EventIDs = new List<string>();
        public UnityEvent<List<ScheduleEvent>> OnEventStarted = default;

        protected List<ScheduleEvent> _events;

        protected override void HandleTickEvent(List<ScheduleEvent> events)
        {
            _events = new List<ScheduleEvent>();

            foreach (string eventID in EventIDs)
            {
                ScheduleEvent matchedEvent = events.Find(ev => ev.ID == eventID);
                if (matchedEvent != null)
                {
                    _events.Add(matchedEvent);
                }
            }

            if (_events.Count == 0) return;
            OnEventStarted.Invoke(_events);
        }
    }
}
