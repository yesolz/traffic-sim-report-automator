using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Models
{
	public class VehicleRemoveArea
	{
		public int LinkNumber { get; set; }
		public int LaneNumber { get; set; }
		public double RemoveStartPosition { get; set; }
		public double RemoveEndPosition { get; set; }

		public double RemovePositionOver { get; set; }
		public double RemovePositionBefore { get; set; }
		public VehicleRemoveType RemoveType { get; set; }

		public enum VehicleRemoveType
		{
			None = -1,
			Between = 0,
			Over = 1,
			Before = 2,
		}
	}


	/// <summary>
	/// Vissim에서 가져온 차량 데이터
	/// </summary>
	public class VehicleData
	{
		/// <summary>
		/// 차량 타입
		/// </summary>
		public int VehicleType { get; set; }
		/// <summary>
		/// 앞 물체와의 거리
		/// </summary>
		public double FollowDistance { get; set; }
		/// <summary>
		/// 속도
		/// </summary>
		public double Speed { get; set; }
		/// <summary>
		/// 가속도
		/// </summary>
		public double Acceleration { get; set; }
		/// <summary>
		/// 차량 앞 좌표
		/// </summary>
		public string CoordFront { get; set; } = "";
		/// <summary>
		/// 차량 뒤 좌표
		/// </summary>
		public string CoordRear { get; set; } = "";
		/// <summary>
		/// 도로
		/// </summary>
		public int Link { get; set; }
		/// <summary>
		/// 차선
		/// </summary>
		public int Lane { get; set; }
		/// <summary>
		/// 도로-차선 위에서의 위치
		/// </summary>
		public double Position { get; set; }
		/// <summary>
		/// 목표 속도
		/// </summary>
		public double DesiredSpeed { get; set; }
		/// <summary>
		/// 목표 차선
		/// </summary>
		public int DestinationLane { get; set; }
		/// <summary>
		/// 앞 차량과의 속력 차이
		/// </summary>
		public double SpeedDifference { get; set; }
		/// <summary>
		/// 무게
		/// </summary>
		public double Weight { get; set; }
		/// <summary>
		/// 센서에 검지된 물체 번호
		/// </summary>
		public int LeadTargetNumber { get; set; }
		/// <summary>
		/// 센서에 검지된 물체 타입
		/// </summary>
		public string LeadTargetType { get; set; } = "";
	}

	public enum VehicleAttribute
	{
		Number,
		VehicleType,
		Headway,
		Speed,
		Acceleration,
		CoordFront,
		CoordRear,
		Lane,
		Position,
		DesiredSpeed,
		DestinationLane,
		SpeedDifference,
		Weight,
		LeadTargetNumber,
		LeadTargetType,
        DesiredLane
    }

	#region Spawn Data

	/// <summary>
	/// Vissim에서 생성할 데이터 개체
	/// </summary>
	public class VehicleVissimSpawnData
	{
		/// <summary>
		/// 차량 타입
		/// </summary>
		public int VehicleType { get; set; }
		/// <summary>
		/// 도로 번호
		/// </summary>
		public int LinkNo { get; set; }
		/// <summary>
		/// 차선 번호
		/// </summary>
		public int LaneNo { get; set; }
		/// <summary>
		/// 생성 위치
		/// </summary>
		public double Position { get; set; }
		/// <summary>
		/// 목표 속도
		/// </summary>
		public double DesiredSpeed { get; set; }
		/// <summary>
		/// 색상
		/// </summary>
		public string? Color { get; set; }
		/// <summary>
		/// 차량 정보
		/// </summary>
		public VehicleBoundingBox VehicleBoundingBox { get; set; }
		public string Name { get; set; }
	}

	/// <summary>
	/// OpenSCENARIO에서 가져올 차량 데이터
	/// </summary>
	public class VehicleOpenSCENARIOSpawnData
	{
		/// <summary>
		/// 차량 이름
		/// </summary>
		public string? Name { get; set; }
		/// <summary>
		/// 차량 카테고리, 사용 안함
		/// </summary>
		public string? Category { get; set; }
		/// <summary>
		/// 최고 속도
		/// </summary>
		public double MaxSpeed { get; set; }
		/// <summary>
		/// 차량 타입
		/// </summary>
		public VehicleType VehicleType { get; set; }
		/// <summary>
		/// 색상
		/// </summary>
		public string? Color { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public VehicleBoundingBox BoundingBox { get; set; }
		/// <summary>
		/// 차량 생성 위치
		/// </summary>
		public VehicleCreateLocation CreateLocation { get; set; }
		public string ScenarioObjectName {  get; set; }
    }

	/// <summary>
	/// 차량 관련 정보
	/// </summary>
	public struct VehicleBoundingBox
	{
		/// <summary>
		/// X값 , 사용안함
		/// </summary>
		public double X { get; set; }
		/// <summary>
		/// Y값, 사용안함
		/// </summary>
		public double Y { get; set; }
		/// <summary>
		/// Z값, 사용안함
		/// </summary>
		public double Z { get; set; }
		/// <summary>
		/// 차량 폭
		/// </summary>
		public double Width { get; set; }
		/// <summary>
		/// 차량 길이
		/// </summary>
		public double Length { get; set; }
		/// <summary>
		/// 차량 높이
		/// </summary>
		public double Height { get; set; }

	}

	/// <summary>
	/// 차량 생성 위치
	/// </summary>
	public struct VehicleCreateLocation
	{
		/// <summary>
		/// 차량 생성시 참고할 타입
		/// </summary>
		public LocationType LocationType { get; set; }
		/// <summary>
		/// WorldPosition X 좌표
		/// </summary>
		public double X { get; set; }
		/// WorldPosition Y 좌표
		public double Y { get; set; }

		/// <summary>
		/// RoadPosition, LanePosition Road ID, vissim의 link
		/// </summary>
		public int RoadId { get; set; }
		/// <summary>
		/// LanePosition Lane ID, vissim의 차선
		/// </summary>
		public int LaneNo { get; set; }
		/// <summary>
		/// RoadPosition, LanePosition S
		/// </summary>
		public double Pos { get; set; }
		/// <summary>
		/// RoadPosition T
		/// </summary>
		public double T { get; set; }

	}

	/// <summary>
	/// 차량 생성시 참고할 타입
	/// </summary>
	public enum LocationType
	{
		WorldPosition,
		LanePosition,
		RoadPosition,
		RelativeRoadPosition,
	}

	#endregion

	/// <summary>
	/// 차량 타입
	/// </summary>
	public enum VehicleType
	{
		/// <summary>
		/// 자율주행차량
		/// </summary>
		Ego = 1001,
		/// <summary>
		/// 시뮬레이션차량
		/// </summary>
		Simulation = 1002,
		Target =1003,
		Drop = 1004
	}

	
}
