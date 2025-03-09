using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Models.AsamModels
{
    public class ManeueverModel
    {
        public EventList Events { get; set; } = new EventList();

        public string Name { get; set; }

        public object Paramdec { get; set; }

        public ManeueverModel(object man)
        {
            DTO.OpenSCENARIO_1_0.Maneuver scenario = (DTO.OpenSCENARIO_1_0.Maneuver)man;
            foreach (DTO.OpenSCENARIO_1_0.Event event_elem in scenario.Event)
            {
                Events.Add(new EventModel(event_elem));
            }
            Name = scenario.name;
        }
        public void ExecEvent(object value, int CarNo)
        {
            foreach (EventModel event_elem in Events)
            {
                if (event_elem.checkEventCondition(value))
                {
                    event_elem.execActions(CarNo);
                };
            }
        }
    }

    public class ManueverList : ObservableCollection<ManeueverModel>
    {

    }
}
