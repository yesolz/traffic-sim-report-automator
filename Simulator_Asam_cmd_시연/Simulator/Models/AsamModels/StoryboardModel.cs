using Simulator.Models.ConditionModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Simulator.Models.ConditionModels.ConditionModel;

namespace Simulator.Models.AsamModels
{
    public class StoryboardModel
    {
        public TriggerModel StopTrigger { get; set; }

        public StoryList stories =new StoryList();

        public StoryboardModel(object stotyboard) {
            DTO.OpenSCENARIO_1_0.OpenScenario scenario = (DTO.OpenSCENARIO_1_0.OpenScenario)stotyboard;
            foreach (DTO.OpenSCENARIO_1_0.Story act in scenario.Storyboard.Story)
            {
                stories.Add(new StoryModel(act));
            }
        }
    }
}
