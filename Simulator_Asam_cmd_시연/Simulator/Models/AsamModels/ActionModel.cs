using DTO.OpenSCENARIO_1_0;
using LiveChartsCore.Measure;
using Microsoft.Office.Interop.Excel;
using Simulator.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Simulator.Models.AsamModels
{
    public class ActionModel
    {
        public string Typename { get; set; }
        public Type Action_type { get; set; }
        public object Action_value { get; set; }
        public string Relspeed_type { get; set; } = "";
        public string Name { get; set; }
        public PrivateActionList PrivateActions { get; set; } = new PrivateActionList();
        public UserDefinedActionList UserDefinedActions { get; set; } = new UserDefinedActionList();
        public GlobalActionList GlobalActions { get; set; } = new GlobalActionList();

        public ActionModel(object action_elem)
        {
            DTO.OpenSCENARIO_1_0.Action action = (DTO.OpenSCENARIO_1_0.Action)action_elem;
            Typename = action.Item.GetType().FullName;
            if (Typename == "DTO.OpenSCENARIO_1_0.PrivateAction")
            {
                PrivateActions.Add(new PrivateActionModel());
                Action_type = action.Item.GetType();
                PrivateAction action_item = (PrivateAction)action.Item;
                Action_value = findprivate(action_item);
            }
            else if (Typename == "DTO.OpenSCENARIO_1_0.GlobalAction")
            {
                GlobalActions.Add(new GloabalActionModel());
            }
            else if (Typename == "DTO.OpenSCENARIO_1_0.UserDefinedAction")
            {
                UserDefinedActions.Add(new UserDefinedActionModel());
            }

        }

        private object findprivate(PrivateAction action_item)
        {
            var result = action_item.Item;
            string item_type = result.GetType().FullName;
            if (item_type == "DTO.OpenSCENARIO_1_0.LongitudinalAction")
            {
                LongitudinalAction item = (LongitudinalAction)result;
                if (item.Item.GetType().FullName == "DTO.OpenSCENARIO_1_0.SpeedAction")
                {
                    SpeedAction speedaction = (SpeedAction)item.Item;
                    var speedtarget = speedaction.SpeedActionTarget.Item;
                    if (speedtarget.GetType().FullName == "DTO.OpenSCENARIO_1_0.AbsoluteTargetSpeed")
                    {
                        AbsoluteTargetSpeed target = (AbsoluteTargetSpeed)speedtarget;
                        Typename = "DTO.OpenSCENARIO_1_0.AbsoluteTargetSpeed";
                        return target.value;
                    }
                    else
                    {
                        RelativeTargetSpeed target = (RelativeTargetSpeed)speedtarget;
                        Typename = "DTO.OpenSCENARIO_1_0.RelativeTargetSpeed";
                        Relspeed_type = target.speedTargetValueType;
                        return target.value;
                    }
                }
                else
                {
                    return 0;
                }
            }
            else if (item_type == "DTO.OpenSCENARIO_1_0.LateralAction")
            {
                LateralAction item = (LateralAction)result;
                if (item.Item.GetType().FullName == "DTO.OpenSCENARIO_1_0.LaneChangeAction")
                {
                    LaneChangeAction laneaction = (LaneChangeAction)item.Item;
                    var lanetarget = laneaction.LaneChangeTarget.Item;
                    if (lanetarget.GetType().FullName == "DTO.OpenSCENARIO_1_0.AbsoluteTargetLane")
                    {
                        AbsoluteTargetSpeed target = (AbsoluteTargetSpeed)lanetarget;
                        Typename = "DTO.OpenSCENARIO_1_0.AbsoluteTargetLane";
                        return target.value;
                    }
                    else
                    {
                        RelativeTargetLane target = (RelativeTargetLane)lanetarget;
                        Typename = "DTO.OpenSCENARIO_1_0.RelativeTargetLane";
                        return target.value;
                    }
                }
                else
                {
                    return -9999;
                }
            }

            return -9999;
        }

        public void execAction(int CarNo)
        {
            if (Typename == "DTO.OpenSCENARIO_1_0.AbsoluteTargetSpeed")
            {
                VissimController.Set_VehicleInfo(CarNo, "DesSpeed", Action_value);
            }
            else if (Typename == "DTO.OpenSCENARIO_1_0.RelativeTargetSpeed")
            {
                double curspeed = (double)VissimController.Get_TargetVehicleInfo(CarNo, "Speed") * 3.6;//추후수정 target
                if (Relspeed_type == "delta")
                {
                    VissimController.Set_VehicleInfo(CarNo, "DesSpeed", curspeed + (double)Action_value * 3.6);
                }
                else if (Relspeed_type == "factor")
                {
                    VissimController.Set_VehicleInfo(CarNo, "DesSpeed", curspeed * (double)Action_value * 3.6);
                }
            }
            else if (Typename == "DTO.OpenSCENARIO_1_0.AbsoluteTargetLane")
            {
                int curlane = (int)VissimController.Get_TargetVehicleInfo(CarNo, "Lane");
                object[,] lanevalue = VissimController.GetLaneValues(curlane, new object[1] { "NumLanes" });
                if ((int)Action_value > 0)
                {
                    VissimController.Set_VehicleInfo(CarNo, "DesLane", (int)Action_value);
                }
                else
                {
                    if ((int)lanevalue[0, 1] - (int)Action_value + 1 <= (int)lanevalue[0, 1])
                        VissimController.Set_VehicleInfo(CarNo, "DesLane", -(int)lanevalue[0, 1] - (int)Action_value + 1);
                }
            }
            else if (Typename == "DTO.OpenSCENARIO_1_0.RelativeTargetLane")
            {
                int curlane = (int)VissimController.Get_TargetVehicleInfo(CarNo, "Lane");
                object[,] lanevalue = VissimController.GetLaneValues(curlane, new object[1] { "NumLanes" });
                if ((int)Action_value > 0)
                {
                    VissimController.Set_VehicleInfo(CarNo, "DesLane", Math.Min(1, curlane - (int)Action_value));
                }
                else
                {
                    VissimController.Set_VehicleInfo(CarNo, "DesLane", Math.Min((int)lanevalue[0, 1], curlane - (int)Action_value));
                }
            }
        }
    }
    public class ActionList : ObservableCollection<ActionModel>
    {

    }
}
