using System.Collections.Generic;
using UnityEngine.Events;

namespace Slax.Schedule
{
    /// <summary>
    /// This class reacts to any event thrown from the ScheduleManager
    /// without questioning its ID. Usually practical for UI reasons or
    /// global gameplay triggers
    /// </summary>
    public class AnyScheduleEventAction : ScheduleManagerObserver
    {
        public UnityEvent<ScheduleEvent> OnEventStarted = default;

        protected override void HandleTickEvent(List<ScheduleEvent> events)
        {
            if (events.Count == 0) return;

            foreach (var ev in events)
            {
                OnEventStarted.Invoke(ev);
            }
        }
    }

}