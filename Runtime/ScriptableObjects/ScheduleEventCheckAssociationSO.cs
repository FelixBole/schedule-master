
using System.Collections.Generic;
using UnityEngine;

namespace Slax.Schedule
{
    [CreateAssetMenu(menuName = "Slax/ScheduleMaster/Check Association Data")]
    public class ScheduleEventCheckAssociationSO : ScriptableObject
    {
        public ScheduleEventsSO ScheduleEvents;
        public List<ScheduleEventCheckerAssociation> Associations = new List<ScheduleEventCheckerAssociation>();

        /// <summary>
        /// Runs checks if any exists on the passed in list of events
        /// and returns the same list of checks with their Passed status
        /// </summary>
        public List<PassedCheck> RunChecks(List<ScheduleEvent> events)
        {
            List<PassedCheck> passed = new List<PassedCheck>();

            foreach (var ev in events)
            {
                PassedCheck current = new PassedCheck(ev, false);
                ScheduleEventCheckerAssociation association = Associations.Find(o => o.Event.ID == ev.ID);

                if (association == null)
                {
                    current.Passed = true;
                    passed.Add(current);
                    continue;
                }

                bool passes = association.Logic == ScheduleEventCheckerAssociation.ScheduleEventCheckerLogic.AND;

                foreach (ScheduleEventCheckBase checker in association.Checkers)
                {
                    if (association.Logic == ScheduleEventCheckerAssociation.ScheduleEventCheckerLogic.AND)
                    {
                        // AND logic: All checkers must pass for the event to pass
                        if (!checker.CheckEvent(ev))
                        {
                            passes = false;
                            break;
                        }
                    }
                    else if (association.Logic == ScheduleEventCheckerAssociation.ScheduleEventCheckerLogic.OR)
                    {
                        // OR logic: At least one checker must pass for the event to pass
                        if (checker.CheckEvent(ev))
                        {
                            passes = true;
                            break;
                        }
                    }
                }

                current.Passed = passes;
                passed.Add(current);
            }

            return passed;
        }


        /// <summary>
        /// Runs checks if any exists on the passed in list of events
        /// and returns only the events that passed the checks
        /// </summary>
        public List<ScheduleEvent> RunChecksAndGetPassedEvents(List<ScheduleEvent> events)
        {
            List<ScheduleEvent> passed = new List<ScheduleEvent>();

            foreach (var ev in events)
            {
                ScheduleEventCheckerAssociation association = Associations.Find(o => o.Event.ID == ev.ID);

                if (association == null)
                {
                    passed.Add(ev);
                    continue;
                }

                bool passes = association.Logic == ScheduleEventCheckerAssociation.ScheduleEventCheckerLogic.AND;

                foreach (ScheduleEventCheckBase checker in association.Checkers)
                {
                    if (association.Logic == ScheduleEventCheckerAssociation.ScheduleEventCheckerLogic.AND)
                    {
                        // AND logic: All checkers must pass for the event to pass
                        if (!checker.CheckEvent(ev))
                        {
                            passes = false;
                            break;
                        }
                    }
                    else if (association.Logic == ScheduleEventCheckerAssociation.ScheduleEventCheckerLogic.OR)
                    {
                        // OR logic: At least one checker must pass for the event to pass
                        if (checker.CheckEvent(ev))
                        {
                            passes = true;
                            break;
                        }
                    }
                }

                if (passes) passed.Add(ev);
            }

            return passed;
        }
    }

    [System.Serializable]
    public class ScheduleEventCheckerAssociation
    {
        public enum ScheduleEventCheckerLogic { AND, OR }
        public ScheduleEventCheckerLogic Logic = ScheduleEventCheckerLogic.OR;
        public ScheduleEvent Event;
        public List<ScheduleEventCheckBase> Checkers = new List<ScheduleEventCheckBase>();
    }

    public struct PassedCheck
    {
        public ScheduleEvent Event;
        public bool Passed;

        public PassedCheck(ScheduleEvent ev, bool passed)
        {
            this.Event = ev;
            this.Passed = passed;
        }
    }
}