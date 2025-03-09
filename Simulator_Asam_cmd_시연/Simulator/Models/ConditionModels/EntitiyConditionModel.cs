using DTO.OpenSCENARIO_1_0;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Models.ConditionModels
{
    public class EntitiyConditionModel
    {
        public string Typename { get; set; }
        public double Value { get; set; }
        public string Rule { get; set; }
        public double Position { get; set; }
        public string EntityRef { get; set; }
        public double Duration { get; set; }
        public string TTCref { get; set; }

        public EntitiyConditionModel(ByEntityCondition trigger_elem)
        {
            Typename = trigger_elem.EntityCondition.Item.GetType().FullName;

            if (Typename == "OpenSCENARIO_1_0.EndOfRoadCondition")
            {
                EndOfRoadCondition trigger = (EndOfRoadCondition)trigger_elem.EntityCondition.Item;
                Duration = double.Parse(trigger.duration);
            }
            else if (Typename == "DTO.OpenSCENARIO_1_0.CollisionCondition")
            {
                CollisionCondition trigger = (CollisionCondition)trigger_elem.EntityCondition.Item;
                // 추후 구현 예정
            }
            else if (Typename == "DTO.OpenSCENARIO_1_0.TimeHeadwayCondition")
            {
                TimeHeadwayCondition trigger = (TimeHeadwayCondition)trigger_elem.EntityCondition.Item;
                Value = double.Parse(trigger.value);
                Rule = trigger.rule;
                EntityRef = trigger.entityRef;
            }
            else if (Typename == "DTO.OpenSCENARIO_1_0.OffroadCondition")
            {
                OffroadCondition trigger = (OffroadCondition)trigger_elem.EntityCondition.Item;
                Duration = double.Parse(trigger.duration);
            }
            else if (Typename == "DTO.OpenSCENARIO_1_0.TimeToCollisionCondition")
            {
                TimeToCollisionCondition trigger = (TimeToCollisionCondition)trigger_elem.EntityCondition.Item;
                Value = double.Parse(trigger.value);
                Rule = trigger.rule;
                TimeToCollisionConditionTarget TTCtgr = (TimeToCollisionConditionTarget)trigger.TimeToCollisionConditionTarget.Item;
                // 추후에 생긴 모양 보고 마무리 (있을경우)
            }
            else if (Typename == "DTO.OpenSCENARIO_1_0.AccelerationCondition")
            {
                AccelerationCondition trigger = (AccelerationCondition)trigger_elem.EntityCondition.Item;
                Value = double.Parse(trigger.value);
                Rule = trigger.rule;
            }
            else if (Typename == "DTO.OpenSCENARIO_1_0.StandStillCondition")
            {
                StandStillCondition trigger = (StandStillCondition)trigger_elem.EntityCondition.Item;
                Duration = double.Parse(trigger.duration);
            }
            else if (Typename == "DTO.OpenSCENARIO_1_0.SpeedCondition")
            {
                SpeedCondition trigger = (SpeedCondition)trigger_elem.EntityCondition.Item;
                Value = double.Parse(trigger.value);
                Rule = trigger.rule;
            }
            else if (Typename == "DTO.OpenSCENARIO_1_0.RelativeSpeedCondition")
            {
                RelativeSpeedCondition trigger = (RelativeSpeedCondition)trigger_elem.EntityCondition.Item;
                Value = double.Parse(trigger.value);
                Rule = trigger.rule;
                EntityRef = trigger.entityRef;
            }
            else if (Typename == "DTO.OpenSCENARIO_1_0.TraveledDistanceCondition")
            {
                TraveledDistanceCondition trigger = (TraveledDistanceCondition)trigger_elem.EntityCondition.Item;
                Value = double.Parse(trigger.value);
            }
            else if (Typename == "DTO.OpenSCENARIO_1_0.ReachPositionCondition")
            {
                ReachPositionCondition trigger = (ReachPositionCondition)trigger_elem.EntityCondition.Item;
                //추후 추가예정
            }
            else if (Typename == "DTO.OpenSCENARIO_1_0.DistanceCondition")
            {
                DistanceCondition trigger = (DistanceCondition)trigger_elem.EntityCondition.Item;
                Value = double.Parse(trigger.value);
                Rule = trigger.rule;
            }
            else if (Typename == "DTO.OpenSCENARIO_1_0.RelativeDistanceCondition")
            {
                RelativeDistanceCondition trigger = (RelativeDistanceCondition)trigger_elem.EntityCondition.Item;
                Value = double.Parse(trigger.value);
                Rule = trigger.rule;
                EntityRef = trigger.entityRef;
            }
            else
            {
                Rule = "";
            }

        }
        public bool CalcCondition(object target)
        {
            //type파악
            //type에 따른 true false 구하기
            //entity no 파악
            //value일경우 걍 계산
            //duration일 경우 더 오래 있었는지 파악
            //
            if (Rule == "Greater than")
            {
                if ((double)target > Value)
                {
                    return true;
                }
            }
            else if (Rule == "Less than")
            {
                if ((double)target < Value)
                {
                    return true;
                }
            }
            else if (Rule == "Equal to")
            {
                if ((double)target == Value)
                {
                    return true;
                }
            }
            else
            {
                if ((double)target >= Value)
                {
                    return true;
                }
            }
            return false;
        }

    }
    public class EntitiyConditionList : ObservableCollection<EntitiyConditionModel>
    {

    }
}
