using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Slax.Schedule
{
    /// <summary>
    /// This class defines the base implementation for any listener of the
    /// ScheduleManager class. It is mainly used for any class that needs
    /// to implement some custom Handling of a tick event
    /// </summary>
    public abstract class ScheduleManagerObserver : MonoBehaviour
    {
        [Tooltip("If set to true, received In Between Ticks from the Schedule Manager will be used. This is a global event if your TimeManager configuration has time between ticks set to any value above 1.")]
        public bool UseInBetweenTicks = false;
        public UnityEvent OnInBetweenTick = default;

        protected virtual void OnEnable()
        {
            ScheduleManager.OnScheduleEvents += HandleTickEvent;
            ScheduleManager.OnInBetweenTickFired += HandleInBetweenTickEvent;
        }

        protected virtual void OnDisable()
        {
            ScheduleManager.OnScheduleEvents -= HandleTickEvent;
            ScheduleManager.OnInBetweenTickFired -= HandleInBetweenTickEvent;
        }

        protected abstract void HandleTickEvent(List<ScheduleEvent> events);
        protected virtual void HandleInBetweenTickEvent()
        {
            if (UseInBetweenTicks) OnInBetweenTick.Invoke();
        }
    }
}
