using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Models.SimulationResult.EgoSafety
{
	/// <summary>
	/// 제한속도준수 통계값 산출을 위해 저장
	/// </summary>
	public class Speed
	{
		/// <summary>
		/// 측정 횟수
		/// </summary>
		public int CheckCount { get; set; }
		/// <summary>
		/// 과속 횟수
		/// </summary>
		public int OverSpeedCount { get; set; }
		/// <summary>
		/// 패널티 산출 값
		/// </summary>
		public double Penalty { get; set; }
	}

	/// <summary>
	/// 시뮬레이션의 제한속도준수 관련 데이터를 저장
	/// </summary>
	public class SpeedRaw
	{
		/// <summary>
		/// 시뮬레이션 시간
		/// </summary>
		public double SimulationSecond { get; private set; }
		/// <summary>
		/// 자율주행차량 도로 번호
		/// </summary>
		public int EgoLinkNumber { get; private set; }
		/// <summary>
		/// 자율주행차량 차선 번호
		/// </summary>
		public int EgoLaneNumber { get; private set; }
		/// <summary>
		/// 자율주행차량 속도
		/// </summary>
		public double EgoSpeed { get; private set; }
		/// <summary>
		/// 자율주행차량 가속도
		/// </summary>
		public double EgoAcceleration { get; private set; }

		/// <summary>
		/// 자율주행차량 패널티 값
		/// </summary>
		public double EgoPanelty { get; private set; }
		/// <summary>
		/// 과속 유무
		/// </summary>
		public bool OverSpeed { get; private set; }

		/// <summary>
		/// 생성자
		/// </summary>
		/// <param name="simSec">시뮬레이션 시간</param>
		/// <param name="vehicleData">자율주행차량 데이터</param>
        public SpeedRaw(double simSec, Models.VehicleData vehicleData)
        {
			SimulationSecond = simSec;
			EgoLinkNumber = vehicleData.Link;
			EgoLaneNumber = vehicleData.Lane;
			EgoSpeed = vehicleData.Speed;
			EgoAcceleration = vehicleData.Acceleration;
        }

		/// <summary>
		/// 패널티 관련값을 산출 후, 패널티 추가
		/// </summary>
		/// <param name="value">패널티 값</param>
		public void AddPanelty(double value)
		{
			EgoPanelty = value;
			OverSpeed = true;
		}
    }

	/// <summary>
	/// 제한 속도
	/// </summary>
	public class SpeedLimit
	{
		/// <summary>
		/// 최고 속도
		/// </summary>
		public double MaximumSpeed { get; set; }
		/// <summary>
		/// 최저 속도
		/// </summary>
		public double MinimumSpeed { get; set; }

		/// <summary>
		/// 생성자
		/// </summary>
		public SpeedLimit()
		{
			MaximumSpeed = 50;
			MinimumSpeed = 0;

			// 패널티 값
			Over50 = 1;
			Over60 = 2;
			Over70 = 3;
		}

		/// <summary>
		/// 50 초과시 패널티 값
		/// </summary>
		public int Over50 { get; set; }
		/// <summary>
		/// 60 초과시 패널티 값
		/// </summary>
		public int Over60 { get; set; }
		/// <summary>
		/// 70 초과시 패널티 값
		/// </summary>
		public int Over70 { get; set; }
	}
}
