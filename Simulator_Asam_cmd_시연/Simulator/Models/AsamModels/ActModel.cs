using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator.Models.ConditionModels;
using static Simulator.Models.ConditionModels.ConditionModel;

namespace Simulator.Models.AsamModels
{
    public class ActModel
    {
        public string Name { get; set; }
        public ManeueverGroupList ManueverGroups { get; set; } = new ManeueverGroupList();
        public ObservableCollection<ConditionList> StartTrigger { get; set; } = new ObservableCollection<ConditionList> { };
        public ObservableCollection<ConditionList> StopTrigger { get; set; } = new ObservableCollection<ConditionList> { };

        //파라미터 선언 부분 추가 할수도?

        public ActModel(object act)
        {
            DTO.OpenSCENARIO_1_0.Act scenario = (DTO.OpenSCENARIO_1_0.Act)act;
            Name = scenario.name;
            foreach (DTO.OpenSCENARIO_1_0.ManeuverGroup Maneuevergroup in scenario.ManeuverGroup)
            {
                ManueverGroups.Add(new ManeueverGroupModel(Maneuevergroup));

            }

            foreach (DTO.OpenSCENARIO_1_0.Condition[] conditiongroup in scenario.StartTrigger)
            {
                ConditionList StartConditions = new ConditionList();
                foreach (DTO.OpenSCENARIO_1_0.Condition condition in conditiongroup)
                    StartConditions.Add(new ConditionModel(condition));
                StartTrigger.Add(StartConditions);
            }

            foreach (DTO.OpenSCENARIO_1_0.Condition[] conditiongroup in scenario.StopTrigger)
            {
                ConditionList StartConditions = new ConditionList();
                foreach (DTO.OpenSCENARIO_1_0.Condition condition in conditiongroup)
                    StartConditions.Add(new ConditionModel(condition));
                StopTrigger.Add(StartConditions);
            }
        }
        public bool CheckAct(object value)
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
        public void ExecAct(object value)
        {
            foreach (ManeueverGroupModel mangroup in ManueverGroups)
            {
                mangroup.ExecManeuver(value);
            }
        }
    }

    public class ActList : ObservableCollection<ActModel>
    {

    }
}
