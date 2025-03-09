using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Models.ConditionModels
{
    public class TriggerModel
    {
        public ConditionGroupList ConditionGroups { get; set; } = new ConditionGroupList();
        public TriggerModel(object Trigger) {
           
        }
    }
}
