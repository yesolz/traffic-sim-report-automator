using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Models.SimulationResult.EgoSafety
{
	/// <summary>
	/// 신호 준수율 통계값 산출을 위해 저장
	/// </summary>
	public class Signal
	{
		/// <summary>
		/// 신호 통과 유무
		/// </summary>
		public bool SignalHeadPassed { get; set; }
		/// <summary>
		/// 통과한 신호의 번호
		/// </summary>
		public int PassedSignalHeadLinkNumber { get; set; }
		/// <summary>
		/// 황색신호에 통과 유무
		/// </summary>
		public bool PassedInAmber { get; set; }
		/// <summary>
		/// 신호 비준수 시 패널티 값
		/// </summary>
		public double SigPenalty { get; set; }
		/// <summary>
		/// 과속 횟수
		/// </summary>
		public int OverSpeedCount { get; set; }
		/// <summary>
		/// 과속 패널티 값
		/// </summary>
		public double Penalty { get; set; }
		/// <summary>
		/// 측정 횟수
		/// </summary>
		public int CheckCount { get; set; }
		/// <summary>
		/// 녹색에 통과한 횟수
		/// </summary>
		public int InGreen { get; set; }
		/// <summary>
		/// 황색신호 전반(안전)에 통과한 횟수
		/// </summary>
		public int InAmberAhead { get; set; }
		/// <summary>
		/// 황색신호 후반(위험)에 통과한 횟수
		/// </summary>
		public int InAmberLast { get; set; }
		/// <summary>
		/// 적색신호에 통과한 횟수
		/// </summary>
		public int InRed { get; set; }
	}

	/// <summary>
	/// 시뮬레이션의 신호 준수율 관련 데이터를 저장
	/// </summary>
	public class SignalRaw
	{
		/// <summary>
		/// 시뮬레이션 시간
		/// </summary>
		public double SimulationSecond { get; set; }
		/// <summary>
		/// 자율주행차 도로 번호
		/// </summary>
		public int EgoLinkNumber { get; set; }
		/// <summary>
		/// 자율주행차 차선 번호
		/// </summary>
		public int EgoLaneNumber { get; set; }
		/// <summary>
		/// 자율주행차 위치
		/// </summary>
		public double EgoPosition { get; set; }

		/// <summary>
		/// 신호등 존재 여부
		/// </summary>
		public bool SignalHeadExist { get; set; }
		/// <summary>
		/// 신호등 도로 번호
		/// </summary>
		public int SignalHeadLinkNumber { get; set; }
		/// <summary>
		/// 신호등 차선 번호
		/// </summary>
		public int SignalHeadLaneNumber { get; set; }
		/// <summary>
		/// 신호등 위치
		/// </summary>
		public double SignalHeadPosition { get; set; }

		/// <summary>
		/// 신호 통과 유무
		/// </summary>
		public bool IsSignalHeadPassed { get; set; }

		/// <summary>
		/// 녹색 현시 시작 시간
		/// </summary>
		public double GreenStartTime { get; set; }
		/// <summary>
		/// 황색 현시 시작 시간
		/// </summary>
		public double AmberStartTime { get; set; }
		/// <summary>
		/// 적색 현시 시작 시간
		/// </summary>
		public double AmberEndTime { get; set; }

		/// <summary>
		/// 패널티 값
		/// </summary>
		public double Panelty { get; set; }
		/// <summary>
		/// 녹색 신호 통과 유무
		/// </summary>
		public bool IsPassedInGreen { get; set; }
		/// <summary>
		/// 황색 신호 전반(안전) 통과 유무
		/// </summary>
		public bool IsPassedInAmberSafe { get; set; }
		/// <summary>
		/// 황색 신호 후반(위험) 통과 유무
		/// </summary>
		public bool IsPassedInAmberUnsafe { get; set; }
		/// <summary>
		/// 적색 신호 통과 유무
		/// </summary>
		public bool IsPassedInRed { get; set; }

		/// <summary>
		/// 생성자
		/// </summary>
		/// <param name="simSec">시뮬레이션 시간</param>
		/// <param name="vehData">자율주행차량 데이터</param>
		public SignalRaw(double simSec, Models.VehicleData vehData)
        {
			SimulationSecond = simSec;

			EgoLinkNumber = vehData.Link;
			EgoLaneNumber = vehData.Lane;
			EgoPosition = vehData.Position;
        }

		/// <summary>
		/// 신호 관련 정보 입력
		/// </summary>
		/// <param name="sigInfo">신호 정보</param>
		public void AddSignalInfo(SignalInfo sigInfo)
		{
			// 해당 로직 호출 시 신호정보가 있는 상태에서 호출됨, 호출되면 신호 존재로 판단
			SignalHeadExist = true;
			SignalHeadLinkNumber = sigInfo.SignalLinkNumber;
			SignalHeadPosition = sigInfo.SignalHeadPosition;

			// 녹색, 황색, 적색 신호 주기를 계산하여 추가함
			int timeValue = (int)((Math.Ceiling((SimulationSecond / sigInfo.SignalCycle)) - 1) * sigInfo.SignalCycle);
			GreenStartTime = timeValue + sigInfo.GreenStartTime;
			AmberStartTime = timeValue + sigInfo.AmberStartTime;
			AmberEndTime = timeValue + sigInfo.AmberEndTime;
		}

		/// <summary>
		/// 신호준수율 관련 데이터를 추가함
		/// </summary>
		/// <param name="value">패널티 산출 값</param>
		/// <param name="sigPassed">신호 통과 유무</param>
		public void AddSignalData(double value, SignalPenalty sigPassed )
		{
			Panelty = value;
			IsSignalHeadPassed = true;

			switch (sigPassed)
			{
				case SignalPenalty.Green: IsPassedInGreen = true; break;
				case SignalPenalty.AmberSafe: IsPassedInAmberSafe = true; break;
				case SignalPenalty.AmberUnsafe: IsPassedInAmberUnsafe = true; break;
				case SignalPenalty.Red: IsPassedInRed = true; break;
			}
		}
    }

	/// <summary>
	/// 신호통과시 신호 색상에 따라 패널티 부여
	/// </summary>
	public enum SignalPenalty
	{
		/// <summary>
		/// 녹색 신호
		/// </summary>
		Green = 0,
		/// <summary>
		/// 황색 전반(안전) 신호
		/// </summary>
		AmberSafe = 3,
		/// <summary>
		/// 황색 후반(위험) 신호
		/// </summary>
		AmberUnsafe = 6,
		/// <summary>
		/// 적색 신호
		/// </summary>
		Red = 12
	}
	
	/// <summary>
	/// 신호등 관련정보 데이터구조체
	/// </summary>
	public class SignalInfo
	{
		/// <summary>
		/// 신호등이 위치한 도로 번호
		/// </summary>
		public int SignalLinkNumber { get; init; }
		/// <summary>
		/// 신호등의 위치
		/// </summary>
		public double SignalHeadPosition { get; init; }
		/// <summary>
		/// 신호 현시 형태
		/// </summary>
		public SignalModel.VissimSignalType SignalType { get; init; }
		/// <summary>
		/// 신호등에서 진출하는 Connector 번호 목록
		/// </summary>
		public List<int> ConnectorList { get; init; }
		/// <summary>
		/// 신호등에서 진출하는 Connector와 연결된 Link 번호 목록
		/// </summary>
		public List<int> ToLinkList { get; init; }
		/// <summary>
		/// 현시 주기
		/// </summary>
		public double SignalCycle { get; init; }
		/// <summary>
		/// 녹색 신호 시작 시간
		/// </summary>
		public double GreenStartTime { get; private set; }
		/// <summary>
		/// 황색 신호 시작 시간
		/// </summary>
		public double AmberStartTime { get; private set; }
		/// <summary>
		/// 황색 신호 종료 시간
		/// </summary>
		public double AmberEndTime { get; private set; }

		/// <summary>
		/// 생성자
		/// </summary>
		/// <param name="signalLinkNumber">신호가 위치한 도로의 번호</param>
		/// <param name="signalHeadPosition">신호의 위치</param>
		/// <param name="connectorList">신호등에서 진출하는 Connector 번호 목록</param>
		/// <param name="toLinkList">신호등에서 진출하는 Connector와 연결된 Link 번호 목록</param>
		/// <param name="signalType">신호 현시 형태</param>
		/// <param name="sequence">현시 번호</param>
		/// <param name="time">현시 시간</param>
		public SignalInfo(int signalLinkNumber,  double signalHeadPosition, List<int> connectorList, List<int> toLinkList, SignalModel.VissimSignalType signalType, int sequence, double time = 40)
        {
			SignalLinkNumber = signalLinkNumber;
			SignalHeadPosition = signalHeadPosition;
			ConnectorList = connectorList;
			ToLinkList = toLinkList;
			SignalType = signalType;
			SignalCycle = (int)signalType * time;

			GreenStartTime = (sequence - 1) * time + 1;
			AmberStartTime = (sequence) * time - 3;
			AmberEndTime = (sequence) * time;
		}
    }


/* 기존 화성시 네트워크 수동구현 부문, 사용안함


	public class SignalInfo
	{
		public Dictionary<int, Dictionary<int, LinkData>> LinkDatas { get; set; }
		public Dictionary<SignalName, Dictionary<int, Dictionary<int, SignalData>>> SignalDatas { get; set; }

		public class LinkData
		{
			public bool IsConnector { get; set; }
			public double SignalHeadPosition { get; set; }

			public SignalName SignalName { get; set; }
			public ushort Hyunsi { get; set; }
			public ushort Number { get; set; }
		}

		public class SignalData
		{
			public int GreenSt { get; set; }
			public int AmberSt { get; set; }
			public int AmberEd { get; set; }
		}

		public enum SignalName
		{
			A,
			B,
			C
		}
		

		public SignalInfo()
		{
			LinkDatas = new Dictionary<int, Dictionary<int, LinkData>>();
			CreateLinkData();

			SignalDatas = new Dictionary<SignalName, Dictionary<int, Dictionary<int, SignalData>>>();
			CreateSignalData();
		}

		private void CreateLinkData()
		{
			Dictionary<int, LinkData> data = new Dictionary<int, LinkData>();

			#region A
			// A
			// 32
			data.Add(2, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 65.62,
				SignalName = SignalName.A,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(3, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 65.62,
				SignalName = SignalName.A,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(4, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 65.62,
				SignalName = SignalName.A,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(5, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 65.62,
				SignalName = SignalName.A,
				Hyunsi = 2,
				Number = 1
			});
			LinkDatas.Add(32, data);

			data = new Dictionary<int, LinkData>();

			// 10006
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.A,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(2, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.A,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(3, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.A,
				Hyunsi = 1,
				Number = 1
			});
			LinkDatas.Add(10006, data);

			data = new Dictionary<int, LinkData>();

			// 10
			data.Add(2, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 80.830,
				SignalName = SignalName.A,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(3, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 80.830,
				SignalName = SignalName.A,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(4, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 80.830,
				SignalName = SignalName.A,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(5, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 80.830,
				SignalName = SignalName.A,
				Hyunsi = 2,
				Number = 1
			});
			LinkDatas.Add(10, data);

			data = new Dictionary<int, LinkData>();

			// 10027
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.A,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(2, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.A,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(3, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.A,
				Hyunsi = 1,
				Number = 1
			});
			LinkDatas.Add(10027, data);

			data = new Dictionary<int, LinkData>();

			// 10028
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.A,
				Hyunsi = 2,
				Number = 1
			});
			LinkDatas.Add(10028, data);

			data = new Dictionary<int, LinkData>();

			// 10007
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.A,
				Hyunsi = 2,
				Number = 1
			});
			LinkDatas.Add(10007, data);

			data = new Dictionary<int, LinkData>();

			// 5
			data.Add(2, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 300.480,
				SignalName = SignalName.A,
				Hyunsi = 3,
				Number = 1
			});
			LinkDatas.Add(5, data);

			data = new Dictionary<int, LinkData>();

			// 10016
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.A,
				Hyunsi = 3,
				Number = 1
			});
			LinkDatas.Add(10016, data);

			data = new Dictionary<int, LinkData>();

			// 10015
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.A,
				Hyunsi = 3,
				Number = 1
			});
			LinkDatas.Add(10015, data);

			data = new Dictionary<int, LinkData>();

			// 4
			data.Add(2, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 396.290,
				SignalName = SignalName.A,
				Hyunsi = 5,
				Number = 1
			});
			LinkDatas.Add(2, data);

			data = new Dictionary<int, LinkData>();

			// 10001
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.A,
				Hyunsi = 5,
				Number = 1
			});
			LinkDatas.Add(10001, data);

			data = new Dictionary<int, LinkData>();

			// 10002
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.A,
				Hyunsi = 5,
				Number = 1
			});
			LinkDatas.Add(10002, data);

			data = new Dictionary<int, LinkData>();
			#endregion

			#region B
			// 22
			data.Add(2, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 90.69,
				SignalName = SignalName.B,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(3, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 90.69,
				SignalName = SignalName.B,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(4, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 90.69,
				SignalName = SignalName.B,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(5, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 90.69,
				SignalName = SignalName.B,
				Hyunsi = 1,
				Number = 2
			});
			LinkDatas.Add(22, data);
			data = new Dictionary<int, LinkData>();

			// 10034
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.B,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(2, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.B,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(3, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.B,
				Hyunsi = 1,
				Number = 1
			});
			LinkDatas.Add(10034, data);
			data = new Dictionary<int, LinkData>();

			// 10045
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.B,
				Hyunsi = 1,
				Number = 2
			});
			LinkDatas.Add(10045, data);
			data = new Dictionary<int, LinkData>();

			// 33
			data.Add(2, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 44,
				SignalName = SignalName.B,
				Hyunsi = 2,
				Number = 1
			});
			data.Add(3, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 44,
				SignalName = SignalName.B,
				Hyunsi = 2,
				Number = 1
			});
			data.Add(4, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 44,
				SignalName = SignalName.B,
				Hyunsi = 2,
				Number = 1
			});
			data.Add(5, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 44,
				SignalName = SignalName.B,
				Hyunsi = 2,
				Number = 2
			});
			LinkDatas.Add(33, data);
			data = new Dictionary<int, LinkData>();

			// 10036
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.B,
				Hyunsi = 2,
				Number = 1
			});
			data.Add(2, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.B,
				Hyunsi = 2,
				Number = 1
			});
			data.Add(3, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.B,
				Hyunsi = 2,
				Number = 1
			});
			LinkDatas.Add(10036, data);
			data = new Dictionary<int, LinkData>();

			// 10065
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.B,
				Hyunsi = 2,
				Number = 2
			});
			LinkDatas.Add(10065, data);
			data = new Dictionary<int, LinkData>();

			// 19
			data.Add(2, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 108,
				SignalName = SignalName.B,
				Hyunsi = 3,
				Number = 1
			});
			data.Add(3, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 108,
				SignalName = SignalName.B,
				Hyunsi = 3,
				Number = 1
			});
			LinkDatas.Add(19, data);
			data = new Dictionary<int, LinkData>();

			// 10044
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.B,
				Hyunsi = 3,
				Number = 1
			});
			LinkDatas.Add(10044, data);
			data = new Dictionary<int, LinkData>();

			// 10041
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.B,
				Hyunsi = 3,
				Number = 1
			});
			LinkDatas.Add(10041, data);
			data = new Dictionary<int, LinkData>();

			// 11
			data.Add(2, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 19,
				SignalName = SignalName.B,
				Hyunsi = 5,
				Number = 1
			});
			data.Add(3, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 19,
				SignalName = SignalName.B,
				Hyunsi = 5,
				Number = 1
			});
			LinkDatas.Add(11, data);
			data = new Dictionary<int, LinkData>();

			// 10039
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.B,
				Hyunsi = 5,
				Number = 1
			});
			LinkDatas.Add(10039, data);
			data = new Dictionary<int, LinkData>();

			// 10035
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.B,
				Hyunsi = 5,
				Number = 1
			});
			LinkDatas.Add(10035, data);
			data = new Dictionary<int, LinkData>();
			#endregion

			#region C

			// 30
			data.Add(2, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 54.5,
				SignalName = SignalName.C,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(3, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 54.5,
				SignalName = SignalName.C,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(4, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 54.5,
				SignalName = SignalName.C,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(5, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 54.5,
				SignalName = SignalName.C,
				Hyunsi = 1,
				Number = 2
			});
			LinkDatas.Add(30, data);
			data = new Dictionary<int, LinkData>();

			// 10052
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.C,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(2, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.C,
				Hyunsi = 1,
				Number = 1
			});
			data.Add(3, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.C,
				Hyunsi = 1,
				Number = 1
			});
			LinkDatas.Add(10052, data);
			data = new Dictionary<int, LinkData>();

			// 10051
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.C,
				Hyunsi = 1,
				Number = 2
			});
			LinkDatas.Add(10051, data);
			data = new Dictionary<int, LinkData>();

			// 34
			data.Add(2, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 46.5,
				SignalName = SignalName.C,
				Hyunsi = 2,
				Number = 1
			});
			data.Add(3, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 46.5,
				SignalName = SignalName.C,
				Hyunsi = 2,
				Number = 1
			});
			data.Add(4, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 46.5,
				SignalName = SignalName.C,
				Hyunsi = 2,
				Number = 1
			});
			data.Add(5, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 46.5,
				SignalName = SignalName.C,
				Hyunsi = 2,
				Number = 2
			});
			LinkDatas.Add(34, data);
			data = new Dictionary<int, LinkData>();

			// 10053
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.C,
				Hyunsi = 2,
				Number = 1
			});
			data.Add(2, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.C,
				Hyunsi = 2,
				Number = 1
			});
			data.Add(3, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.C,
				Hyunsi = 2,
				Number = 1
			});
			LinkDatas.Add(10053, data);
			data = new Dictionary<int, LinkData>();

			// 10058
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.C,
				Hyunsi = 2,
				Number = 2
			});
			LinkDatas.Add(10058, data);
			data = new Dictionary<int, LinkData>();

			// 65
			data.Add(1, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 55.57,
				SignalName = SignalName.C,
				Hyunsi = 3,
				Number = 1
			});
			LinkDatas.Add(65, data);
			data = new Dictionary<int, LinkData>();

			// 10055
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.C,
				Hyunsi = 3,
				Number = 1
			});
			LinkDatas.Add(10055, data);
			data = new Dictionary<int, LinkData>();

			// 10056
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.C,
				Hyunsi = 3,
				Number = 1
			});
			LinkDatas.Add(10056, data);
			data = new Dictionary<int, LinkData>();

			// 16
			data.Add(2, new LinkData()
			{
				IsConnector = false,
				SignalHeadPosition = 250.63,
				SignalName = SignalName.C,
				Hyunsi = 4,
				Number = 1
			});
			LinkDatas.Add(16, data);
			data = new Dictionary<int, LinkData>();

			// 10060
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.C,
				Hyunsi = 4,
				Number = 1
			});
			LinkDatas.Add(10060, data);
			data = new Dictionary<int, LinkData>();

			// 10061
			data.Add(1, new LinkData()
			{
				IsConnector = true,
				SignalHeadPosition = 0,
				SignalName = SignalName.C,
				Hyunsi = 4,
				Number = 1
			});
			LinkDatas.Add(10061, data);
			#endregion
		}
		private void CreateSignalData()
		{
			Dictionary<int, Dictionary<int, SignalData>> pair = new Dictionary<int, Dictionary<int, SignalData>>();
			Dictionary<int, SignalData> data = new Dictionary<int, SignalData>();
			#region A
			// 1현시
			data.Add(1, new SignalData()
			{
				GreenSt = 0,
				AmberSt = 71,
				AmberEd = 75
			});
			pair.Add(1, data);
			data = new Dictionary<int, SignalData>();

			// 2현시
			data.Add(1, new SignalData()
			{
				GreenSt = 75,
				AmberSt = 96,
				AmberEd = 100
			});
			pair.Add(2, data);
			data = new Dictionary<int, SignalData>();

			// 3현시
			data.Add(1, new SignalData()
			{
				GreenSt = 100,
				AmberSt = 122,
				AmberEd = 126
			});
			pair.Add(3, data);
			data = new Dictionary<int, SignalData>();

			// 5현시
			data.Add(1, new SignalData()
			{
				GreenSt = 140,
				AmberSt = 166,
				AmberEd = 0
			});
			pair.Add(5, data);
			data = new Dictionary<int, SignalData>();

			SignalDatas.Add(SignalName.A, pair);

			pair = new Dictionary<int, Dictionary<int, SignalData>>();
			#endregion

			#region B
			// 1현시
			data.Add(1, new SignalData()
			{
				GreenSt = 0,
				AmberSt = 64,
				AmberEd = 68
			});
			data.Add(2, new SignalData()
			{
				GreenSt = 0,
				AmberSt = 26,
				AmberEd = 30
			});
			pair.Add(1, data);
			data = new Dictionary<int, SignalData>();

			// 2현시
			data.Add(1, new SignalData()
			{
				GreenSt = 30,
				AmberSt = 84,
				AmberEd = 88
			});
			data.Add(2, new SignalData()
			{
				GreenSt = 68,
				AmberSt = 84,
				AmberEd = 88
			});
			pair.Add(2, data);
			data = new Dictionary<int, SignalData>();

			// 3현시
			data.Add(1, new SignalData()
			{
				GreenSt = 88,
				AmberSt = 119,
				AmberEd = 123
			});
			pair.Add(3, data);
			data = new Dictionary<int, SignalData>();

			// 5현시
			data.Add(1, new SignalData()
			{
				GreenSt = 128,
				AmberSt = 166,
				AmberEd = 170
			});
			pair.Add(5, data);
			data = new Dictionary<int, SignalData>();

			SignalDatas.Add(SignalName.B, pair);
			pair = new Dictionary<int, Dictionary<int, SignalData>>();
			#endregion

			#region C
			// 1현시
			data.Add(1, new SignalData()
			{
				GreenSt = 0,
				AmberSt = 89,
				AmberEd = 93
			});
			data.Add(2, new SignalData()
			{
				GreenSt = 0,
				AmberSt = 31,
				AmberEd = 35
			});
			pair.Add(1, data);
			data = new Dictionary<int, SignalData>();

			// 2현시
			data.Add(1, new SignalData()
			{
				GreenSt = 35,
				AmberSt = 104,
				AmberEd = 108
			});
			data.Add(2, new SignalData()
			{
				GreenSt = 93,
				AmberSt = 104,
				AmberEd = 108
			});
			pair.Add(2, data);
			data = new Dictionary<int, SignalData>();

			// 3현시
			data.Add(1, new SignalData()
			{
				GreenSt = 108,
				AmberSt = 124,
				AmberEd = 128
			});
			pair.Add(3, data);
			data = new Dictionary<int, SignalData>();

			// 4현시
			data.Add(1, new SignalData()
			{
				GreenSt = 146,
				AmberSt = 166,
				AmberEd = 170
			});
			pair.Add(4, data);

			SignalDatas.Add(SignalName.C, pair);
			#endregion
		}
	}*/
}
