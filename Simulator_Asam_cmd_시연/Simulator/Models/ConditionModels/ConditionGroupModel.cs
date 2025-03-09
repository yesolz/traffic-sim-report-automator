using Simulator.Models.AsamModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Models.ConditionModels
{
    
    public class ConditionGroupModel
    {
        public ConditionList Conditions { get; set; } = new ConditionList();
        public ConditionGroupModel() { }
    }

    public class ConditionGroupList : ObservableCollection<ConditionGroupModel>
    {

    }
}
