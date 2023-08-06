using System.Collections.Generic;
using UnityEngine.Events;

namespace Slax.Schedule
{
    /// <summary>
    /// This class acts as a base class for scripts that are event observers
    /// and need to invoke an event for external scripts to handle. Such as
    /// NPC to trigger the action of moving, doing something in the UI etc.
    /// We're using UnityEvent to leverage the inspector's drag n drop features.
    /// </summary>
    public class ScheduleEventAction : ScheduleManagerObserver
    {
        public string EventID;
        public UnityEvent<ScheduleEvent> OnEventStarted = default;

        protected ScheduleEvent _event;

        protected override void HandleTickEvent(List<ScheduleEvent> events)
        {
            _event = events.Find(ev => ev.ID == EventID);
            if (_event == null) return;
            OnEventStarted.Invoke(_event);
        }
    }
}
