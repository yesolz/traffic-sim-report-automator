using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator.Models.ConditionModels;
using static Simulator.Models.ConditionModels.ConditionModel;

namespace Simulator.Models.AsamModels
{
    public class EventModel
    {
        public string Priority { get; set; }
        public string Name { get; set; }
        public ObservableCollection<ConditionList> StartTrigger { get; set; } = new ObservableCollection<ConditionList> { };

        public ConditionGroupList ConditionGroups { get; set; } = new ConditionGroupList();
        public ActionList actions { get; set; } = new ActionList();
        public int Maxexec { get; set; }
        public EventModel(object event_elems)
        {
            DTO.OpenSCENARIO_1_0.Event scenario = (DTO.OpenSCENARIO_1_0.Event)event_elems;
            for (int i = 0; i < scenario.Action.Length; i++)
            {
                //ActionModel temp = new ActionModel(scenario.Action[i]);
                actions.Add(new ActionModel(scenario.Action[i]));
            }
            Name = scenario.name;
            Priority = scenario.priority;
            Maxexec = scenario.maximumExecutionCount == null ? 1000 : int.Parse(scenario.maximumExecutionCount);
            TriggerModel trigger = new TriggerModel(scenario.StartTrigger);
            foreach (DTO.OpenSCENARIO_1_0.Condition[] conditiongroup in scenario.StartTrigger)
            {
                ConditionList StartConditions = new ConditionList();
                foreach (DTO.OpenSCENARIO_1_0.Condition condition in conditiongroup)
                    StartConditions.Add(new ConditionModel(condition));
                StartTrigger.Add(StartConditions);
            }
            //Trigger
        }
        public void execActions(int CarNo)
        {
            foreach (ActionModel action in actions)
            {
                action.execAction(CarNo);
            }
        }
        public bool checkEventCondition(object value)
        {
            foreach (ConditionList conditions in StartTrigger)
            {
                foreach (ConditionModel condition in conditions)
                {
                    if (!condition.checkconditions(value)) return false;
                }
            }
            return true;
        }
    }

    public class EventList : ObservableCollection<EventModel>
    {

    }
}
