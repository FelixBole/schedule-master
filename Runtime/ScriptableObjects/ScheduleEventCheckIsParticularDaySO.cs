using UnityEngine;

namespace Slax.Schedule
{
    [CreateAssetMenu(menuName = ScheduleMasterConfiguration.ScriptableObjectAssetMenu + "Event Checks/Is Specific Day")]
    public class ScheduleEventCheckIsParticularDaySO : ScheduleEventCheckBase
    {
        public Days Day;
        private Days _currentDay;

        void OnEnable()
        {
            TimeManager.OnAwake += SetCurrentDay;
            TimeManager.OnNewDay += SetCurrentDay;
        }

        void OnDisable()
        {
            TimeManager.OnAwake -= SetCurrentDay;
            TimeManager.OnNewDay -= SetCurrentDay;
        }

        void SetCurrentDay(DateTime date)
        {
            _currentDay = date.Day;
        }

        public override bool CheckEvent(ScheduleEvent scheduleEvent)
        {
            bool result = _currentDay == Day;
            Debug.Log($"Ran check on {scheduleEvent.Name} and status is : {result}");
            return _currentDay == Day;
        }
    }
}
