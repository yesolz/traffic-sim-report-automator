using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Models.SimulationResult
{
	/// <summary>
	///평균 주행시간 산출 데이터 구조체
	/// </summary>
	public class TravelTimeAvg
	{
		/// <summary>
		/// Travel Time 측정 횟수
		/// </summary>
		private int travelTimeCount { get; set; }
		/// <summary>
		/// Distance Traveled 측정 횟수
		/// </summary>
		private int distanceTraveledCount { get; set; }
		/// <summary>
		/// 주행 시간
		/// </summary>
		private double travelTime { get; set; }
		/// <summary>
		/// 주행 시간 값 요청 시 산출된 결과 반영
		/// </summary>
		public double TravelTime { get { return (travelTimeCount == 0) ? 0 : travelTime / travelTimeCount; } }
		/// <summary>
		/// 주행 거리
		/// </summary>
		private double distanceTraveled { get; set; }
		/// <summary>
		/// 주행 거리 값 요청 시 산출된 결과 반영
		/// </summary>
		public double DistanceTraveled { get { return (distanceTraveledCount == 0) ? 0 : distanceTraveled / distanceTraveledCount; } }

		/// <summary>
		/// traveled time 추가, distance traveled 추가
		/// </summary>
		/// <param name="travelTime">주행 시간 데이터</param>
		/// <param name="distanceTraveled">주행 거리 데이터</param>
		public void AddData(object travelTime, object distanceTraveled)
		{
			// 주행시간 입력된 데이터가 있을 경우 관련 데이터 추가
			if ( travelTime is not null )
			{
				travelTimeCount++;
				this.travelTime += (double)travelTime;
			}

			// 주행거리 입력된 데이터가 잇을 경우 관련 데이터 추가
			if ( distanceTraveled is not null )
			{
				distanceTraveledCount++;
				this.distanceTraveled += (double)distanceTraveled;
			}
		}
	}
}
