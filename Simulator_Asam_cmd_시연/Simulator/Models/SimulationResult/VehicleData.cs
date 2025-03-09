using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Models.SimulationResult
{
	/// <summary>
	/// VEHCILE 측정 관련 데이터<br/>
	/// ** 사용안함..??
	/// </summary>
	internal class VehicleData
	{
		#region Vissim Data

		/// <summary>
		/// 시뮬레이션 진행 시간
		/// </summary>
		public double SimulationSecond { get; set; }

		/// <summary>
		/// 차량 타입
		/// </summary>
		public string VehicleType { get; set; }

		/// <summary>
		/// 차량 번호
		/// </summary>
		public int VehicleNumber { get; set; }

		/// <summary>
		/// 도로 번호
		/// </summary>
		public int LinkID { get; set; }

		/// <summary>
		/// 차선 번호
		/// </summary>
		public int LaneID { get; set; }

		/// <summary>
		/// 속도
		/// </summary>
		public double Speed { get; set; }

		/// <summary>
		/// 목표 속도
		/// </summary>
		public double DesiredSpeed { get; set; }

		/// <summary>
		/// 가속도
		/// </summary>
		public double Acceleration { get; set; }

		/// <summary>
		/// 차선 변경
		/// </summary>
		public string LaneChange { get; set; }

		/// <summary>
		/// 차량 앞 좌표
		/// </summary>
		public string CoordFront { get; set; }

		/// <summary>
		/// 차량 뒤 자표
		/// </summary>
		public string CoordRear { get; set; }

		#endregion

		#region Traffic Operation Efficiency

		/// <summary>
		/// 지연시간
		/// </summary>
		public double Delay { get; set; }

		/// <summary>
		/// 평균 통행 속도
		/// </summary>
		public double PassingSpeed { get; set; }

		/// <summary>
		/// 통행 시간
		/// </summary>
		public double TravelTime { get; set; }

		/// <summary>
		/// Travel Time Index
		/// </summary>
		public double TTI { get; set; }

		#endregion

		#region Traffic Safety

		/// <summary>
		/// 속도 편차
		/// </summary>
		public double SpeedDeviation { get; set; }

		/// <summary>
		/// 상충 횟수
		/// </summary>
		public double NumberOfConflict { get; set; }

		/// <summary>
		/// Time To Collision
		/// </summary>
		public double TTC { get; set; }

		/// <summary>
		/// Time To Collision Index
		/// </summary>
		public int TTCI { get; set; }

		/// <summary>
		/// Stopping Distance Index
		/// </summary>
		public int SDI { get; set; }

		/// <summary>
		/// Proportion of Stopping Distance
		/// </summary>
		public double PSD { get; set; }

		#endregion
	}
}
