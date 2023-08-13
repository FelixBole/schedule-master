
using UnityEngine;

namespace Slax.Schedule
{
    [CreateAssetMenu(menuName = ScheduleMasterConfiguration.ScriptableObjectAssetMenu + "Event Checks/Is Specific Day")]
    public class CheckIsSpecificDaySO : ScheduleEventCheckBase
    {
        public Days Day;

        public override bool CheckEvent(ScheduleEvent scheduleEvent)
        {
            return scheduleEvent.Timestamp.Day == Day;
        }
    }
}
