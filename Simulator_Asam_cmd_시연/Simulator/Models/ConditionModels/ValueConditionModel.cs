using Simulator.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Models.ConditionModels
{
    public class ValueConditionModel
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public string Rule { get; set; }
        public double Position { get; set; }
        public string EntityRef { get; set; }
        public double Duration { get; set; }
        public ValueConditionModel(DTO.OpenSCENARIO_1_0.ByValueCondition trigger_elem)
        {
            if (trigger_elem.Item.GetType().FullName == "DTO.OpenSCENARIO_1_0.SimulationTimeCondition")
            {
                Name = "DTO.OpenSCENARIO_1_0.SimulationTimeCondition";
                DTO.OpenSCENARIO_1_0.SimulationTimeCondition trigger = (DTO.OpenSCENARIO_1_0.SimulationTimeCondition)trigger_elem.Item;
                Value = double.Parse(trigger.value);
                Rule = trigger.rule;
            }
            //else if (trigger_elem.Item.GetType().FullName == "DTO.OpenSCENARIO_1_0.SimulationTimeCondition")
            //{
            //    DTO.OpenSCENARIO_1_0.SimulationTimeCondition trigger = (DTO.OpenSCENARIO_1_0.SimulationTimeCondition)trigger_elem.Item;
            //    Value = Double.Parse(trigger.value);
            //    Rule = trigger.rule;
            //}
        }

        public bool CalcCondition(object target)
        {
            if (Name == "DTO.OpenSCENARIO_1_0.SimulationTimeCondition")
                target = VissimController.GetSimulationTime();
            else return false;
            if (Rule == "greaterThan")
            {
                if ((double)target > Value)
                {
                    return true;
                }
            }
            else if (Rule == "lessThan")
            {
                if ((double)target < Value)
                {
                    return true;
                }
            }
            else
            {
                if ((double)target == Value)
                {
                    return true;
                }
            }
            return false;
        }
    }
    public class ValueConditionList : ObservableCollection<ValueConditionModel>
    {

    }
}
