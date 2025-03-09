using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Simulator.Models.VissimModels;

namespace Simulator.Models.SimulationResult.EgoSafety
{
	/// <summary>
	/// 안전거리 통계값 산출을 위해 저장
	/// </summary>
	public class Distance
	{
		/// <summary>
		/// 측정 횟수
		/// </summary>
		public int CheckCount { get; set; }
		/// <summary>
		/// 차량 무게를 고려하지 않음
		/// </summary>
		public int NotConsiderWeight { get; set; }
		/// <summary>
		/// 통신지연을 고려하지 않음
		/// </summary>
		public int NotConsiderLatency { get; set; }
		/// <summary>
		/// 안전거리를 고려하지 않음
		/// </summary>
		public int NotConsiderSafeDistance { get; set; }
	}

	/// <summary>
	/// 시뮬레이션의 안전거리 관련 데이터를 저장
	/// </summary>
	public class DistanceRaw
	{
		/// <summary>
		/// 시뮬레이션 시간
		/// </summary>
		public double SimulationSecond { get; set; }
		/// <summary>
		/// 자율주행차 위치
		/// </summary>
		public double EgoPosition { get; set; }
		/// <summary>
		/// 자율주행차 무게
		/// </summary>
		public double EgoWeight { get; set; }
		/// <summary>
		/// 자율주행차 속력
		/// </summary>
		public double EgoSpeed { get; set; }
		/// <summary>
		/// 자율주행차 가속도
		/// </summary>
		public double EgoAcceleration { get; set; }
		/// <summary>
		/// 자율주행차 센서 검지 물체 타입
		/// </summary>
		public string EgoLeadTargetType { get; set; }
		/// <summary>
		/// 자율주행차와 앞차와의 거리
		/// </summary>
		public double EgoFollowDistance { get; set; }
		/// <summary>
		/// 자율주행차와 앞차와의 속력 차이
		/// </summary>
		public double EgoSpeedDifference { get; set; }

		/// <summary>
		/// 앞 차량의 속도
		/// </summary>
		public double AheadSpeed { get; set; }
		/// <summary>
		/// 앞 차량의 무게
		/// </summary>
		public double AheadWeight { get; set; }

		/// <summary>
		/// 안전거리
		/// </summary>
		public double SafetyDistance { get; set; }
		/// <summary>
		/// 질량차를 고려한 안전거리
		/// </summary>
		public double ConsiderWeightSafetyDistance { get; set; }
		
		/// <summary>
		/// 생성자
		/// </summary>
		/// <param name="simSec">시뮬레이션 시간</param>
		/// <param name="vehData">자율주행차량 데이터</param>
        public DistanceRaw(double simSec, Models.VehicleData vehData)
        {
			SimulationSecond = simSec;

			EgoPosition = vehData.Position;
			EgoWeight = vehData.Weight;
			EgoSpeed = vehData.Speed;
			EgoAcceleration = vehData.Acceleration;
			EgoLeadTargetType = vehData.LeadTargetType;
			EgoFollowDistance = vehData.FollowDistance;

			EgoSpeedDifference = vehData.SpeedDifference;
        }

		/// <summary>
		/// 앞 차량의 데이터 추가
		/// </summary>
		/// <param name="aheadVehicleData">자율주행차의 앞 차량 데이터</param>
		public void AddAheadVehicleData(Models.VehicleData aheadVehicleData)
		{
			AheadSpeed = aheadVehicleData.Speed;
			AheadWeight = aheadVehicleData.Weight;

			SafetyDistance = CalculatorModel.GetSafetyDistance(EgoSpeed, EgoSpeedDifference);
			ConsiderWeightSafetyDistance = (EgoWeight == 0 && aheadVehicleData.Weight == 0) ? 0 : CalculatorModel.GetSafetyDistanceConsiderMassDifference(aheadVehicleData.Weight, EgoWeight, EgoSpeed, EgoSpeedDifference, EgoAcceleration);
		}
    }
}
