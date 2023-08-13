namespace Slax.Schedule
{
    /// <summary>
    /// This interface is to be implemented by scripts that run checks
    /// on schedule events, allowing custom scripts to be plugged into
    /// the event checking extension of the ScheduleManager
    /// </summary>
    public interface IScheduleEventCheck
    {
        bool CheckEvent(ScheduleEvent scheduleEvent);
    }
}
