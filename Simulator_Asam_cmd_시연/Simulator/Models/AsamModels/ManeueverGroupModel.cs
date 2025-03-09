using DTO.OpenSCENARIO_1_0;
using Simulator.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Models.AsamModels
{
    public class ManeueverGroupModel
    {
        public ManueverList Manuevers { get; set; } = new ManueverList();

        public List<string> Actors { get; set; } = new List<string>();

        public object[] CatalogReferences { get; set; } //안할 예정

        public string Name { get; set; }

        public int MaxExec { get; set; }

        public ManeueverGroupModel(object mangroup)
        {
            ManeuverGroup scenario = (ManeuverGroup)mangroup;
            foreach (Maneuver Maneuever in scenario.Maneuver)
            {
                Manuevers.Add(new ManeueverModel(Maneuever));

            }
            Name = scenario.name;
            MaxExec = int.Parse(scenario.maximumExecutionCount);
            foreach (EntityRef actor in scenario.Actors.EntityRef)
            {
                Actors.Add(actor.entityRef);
            }

        }
        public void ExecManeuver(object value)
        {
            int CarNo = 0;
            object[,] vissimvalue = VissimController.GetVehicleValues(new object[2] { "No", "VehType" });
            for (int i = 0; i < vissimvalue.GetLength(0); i++)
            {
                int type = int.Parse((string)vissimvalue[i, 1]);
                if (type > 1000 && VissimController.GetTypeName(type) == Actors[0])
                {
                    CarNo = (int)vissimvalue[i, 0];
                    foreach (ManeueverModel manuever in Manuevers)
                    {
                        manuever.ExecEvent(value, CarNo);
                    }
                    break;
                }
            }
        }
    }

    public class ManeueverGroupList : ObservableCollection<ManeueverGroupModel>
    {

    }
}
