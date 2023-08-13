using UnityEngine;

namespace Slax.Schedule
{
    /// <summary>
    /// Base class for Schedule Event Checker Scripts
    /// </summary>
    public abstract class ScheduleEventCheckBase : ScriptableObject, IScheduleEventCheck
    {
        [TextArea]
        public string Notes;
        public abstract bool CheckEvent(ScheduleEvent scheduleEvent);
    }

}