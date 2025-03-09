using DTO.OpenSCENARIO_1_0;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Models.ConditionModels
{
    public class ConditionModel
    {
        public string Name { get; set; }
        public double Delay { get; set; }

        public string ConditionEdge { get; set; }
        public EntitiyConditionList entitiyConditions { get; set; } = new EntitiyConditionList();
        public ValueConditionList valueConditions { get; set; } = new ValueConditionList();

        public ConditionModel(object condition_elem)
        {
            Condition condition = (Condition)condition_elem;
            Name = condition.name;
            Delay = double.Parse(condition.delay);
            ConditionEdge = condition.conditionEdge;
            if (condition.Item.GetType().FullName == "DTO.OpenSCENARIO_1_0.ByValueCondition")
            {
                ByValueCondition trigger = (ByValueCondition)condition.Item;
                valueConditions.Add(new ValueConditionModel(trigger));
            }
            if (condition.Item.GetType().FullName == "DTO.OpenSCENARIO_1_0.ByEntityCondition")
            {
                ByEntityCondition trigger = (ByEntityCondition)condition.Item;
                entitiyConditions.Add(new EntitiyConditionModel(trigger));
            }
        }
        public bool checkconditions(object value)
        {
            foreach (EntitiyConditionModel condition in entitiyConditions)
            {
                if (!condition.CalcCondition(value)) return false;
            }
            foreach (ValueConditionModel condition in valueConditions)
            {
                if (!condition.CalcCondition(value)) return false;
            }
            return true;
        }
        
    }

    public class ConditionList : ObservableCollection<ConditionModel>
    {

    }
}
