using System.Collections.Generic;
using UnityEngine;

namespace Slax.Schedule
{
    /// <summary>
    /// This class acts as a base for classes that listen to a tick manager
    /// itself listening to the TimeManager directly.
    ///
    /// Although it is recommended to use a ScheduleManagerObserver instead
    /// to use the central ScheduleManager hub for performance reasons
    /// </summary>
    public abstract class TickManagerObserver : MonoBehaviour
    {
        protected TickObserver _manager;

        protected virtual void Awake()
        {
            _manager = FindObjectOfType<ScheduleManager>();

            if (!_manager) return;
            _manager.OnTickReceived += ProcessScheduleEvent;
        }

        protected virtual void OnDisable()
        {
            if (_manager)
            {
                _manager.OnTickReceived -= ProcessScheduleEvent;
            }
        }

        public abstract void ProcessScheduleEvent(List<ScheduleEvent> events);
    }
}