using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Models.SimulationResult
{
	internal class AverageData
	{
		// 교통운영효율성
		#region Traffic Operation Efficiency

		/// <summary>
		/// Delay(avg), 평균 지연
		/// </summary>
		public double Delay { get; set; }

		/// <summary>
		/// Speed(Avg), 평균 통과 속도<br/>
		/// 측정범위(300M) / TravelTime
		/// </summary>
		public double Speed { get; set; }

		/// <summary>
		/// 대기행렬길이(Node)
		/// </summary>
		public double QueueLength { get; set; }

		/// <summary>
		/// 주행 시간(avg)
		/// </summary>
		public double TravelTime { get; set; }

		/// <summary>
		/// 주행 거리(avg)
		/// </summary>
		public double DistanceTraveled { get; set; }

		#endregion

		// 교통 안전성
		#region Traffic Safety

		/// <summary>
		/// 속도 편차
		/// </summary>
		public double SpeedDeviation { get; set; }

		/// <summary>
		/// 상충 횟수
		/// </summary>
		public int NumberOfConflict { get; set; }

		/// <summary>
		/// Time To Collistion
		/// </summary>
		public double TTC { get; set; }

		/// <summary>
		/// Counter of TTC
		/// </summary>
		public int TTCCounter { get; set; }

		/// <summary>
		/// TTC Index
		/// </summary>
		public int TTCI { get; set; }

		/// <summary>
		/// Stopping Distance Index
		/// </summary>
		public int SDI { get; set; }

		#endregion

		// 교통 환경성, Node에서 측정한 값
		#region Traffic Environmental

		/// <summary>
		/// 질소산화물
		/// </summary>
		public double Nox { get; set; }

		/// <summary>
		/// 일산화탄소
		/// </summary>
		public double CO { get; set; }

		/// <summary>
		/// 매연
		/// </summary>
		public double PM { get; set; }

		/// <summary>
		/// 이산화탄소
		/// </summary>
		public double CO2 { get; set; }

		/// <summary>
		/// 연료소모량
		/// </summary>
		public double FuelConsumtion { get; set; }

		#endregion
	}
}
