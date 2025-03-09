using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using static Simulator.Models.ConditionModels.ConditionModel;

namespace Simulator.Models.AsamModels
{
    public class StoryModel
    {
        public string CurrentAct { get; set; }

        public ActList Acts { get; set; } = new ActList();

        public string name { get; set; }

        public object paramdec { get; set; }

        public StoryModel(object story)
        {
            DTO.OpenSCENARIO_1_0.Story scenario = (DTO.OpenSCENARIO_1_0.Story)story;
            name = scenario.name;
            foreach (DTO.OpenSCENARIO_1_0.Act act in scenario.Act)
            {
                Acts.Add(new ActModel(act));
            }
        }

        public bool CheckAct(object value)
        {
            if (CurrentAct == "")
            {
                foreach (ActModel act in Acts)
                {
                    if (act.CheckAct(value))
                    {
                        CurrentAct = act.Name;
                        return true;
                    }

                }
            }
            else return true;
            return false;
        }
        public void ExecAct(object value)
        {
            ActModel act = Acts.Where(x => x.Name == CurrentAct).FirstOrDefault();
            act.ExecAct(value);
        }
    }

    public class StoryList : ObservableCollection<StoryModel>
    {

    }
}
