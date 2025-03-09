using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using Microsoft.Office.Interop.Excel;
using Simulator.Common;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Diagnostics;
using System.Windows.Media.Animation;

using VISSIMLIB;

using static Simulator.Models.NodeModel;

namespace Simulator.Models.VissimModels
{
	internal partial class NetworkInfoModel : ObservableObject
	{
		
		#region Chart Value 

		#region Ego Vehicle

		/// <summary>
		/// 자율주행차량 속도
		/// </summary>
		internal double EgoVehicleSpeed { get; set; }
		/// <summary>
		/// 자율주행차량 가속도
		/// </summary>
		internal double EgoVehicleAcceleration { get; set; }
		/// <summary>
		/// 자율주행차량이 측정한 앞 차량과의 거리<br/>
		/// 센서 검지 유형이 VEHICLE인 경우만 생성
		/// </summary>
		internal double EgoVehicleAheadDistance { get; set; }
		/// <summary>
		/// 자율주행차량과 앞 차량의 TTC 값<br/>
		/// 센서 검지 유형이 VEHICLE인 경우만 생성
		/// </summary>
		internal double EgoVehicleTTC { get; set; }

		#endregion

		#region Around Vehicle 

		/// <summary>
		/// 자율주행차량 주변 특정 거리에 있는 차량들의 속력
		/// </summary>
		internal List<int> AroundVehicleSpeed { get; set; }

		/// <summary>
		/// 자율주행차량 주변 특정 거리에 있는 차량들의 가속도
		/// </summary>
		internal List<int> AroundVehicleAcceleration { get; set; }

		/// <summary>
		/// 자율주행차량 주변 특정 거리에 있는 차량들이 측정한 앞 차량과의 거리<br/>
		/// 센서 검지 유형이 VEHICLE인 경우만 생성
		/// </summary>
		internal List<int> AroundVehicleAheadDistance { get; set; }

		/// <summary>
		/// 자율주행차량 주변 특정 거리에 있는 차량들과 앞 차량과의 TTC 값<br/>
		/// 센서 검지 유형이 VEHICLE인 경우만 생성
		/// </summary>
		internal List<int> AroundVehicleTTC { get; set; }

		#endregion

		#region Network Vehicle

		/// <summary>
		/// Vissim Network에 존재하는 모든 차량들의 속도
		/// </summary>
		internal List<int> NetworkVehicleSpeed { get; set; }

		/// <summary>
		/// Vissim Network에 존재하는 모든 차량들의 가속도
		/// </summary>
		internal List<int> NetworkVehicleAcceleration { get; set; }

		/// <summary>
		/// Vissim Network에 존재하는 차량들이 측정한 앞 차량과의 거리<br/>
		/// 센서 검지 유형이 VEHICLE인 경우만 생성
		/// </summary>
		internal List<int> NetworkVehicleAheadDistance { get; set; }

		/// <summary>
		/// Vissim Network에 존재하는 차량들과 앞 차량과의 TTC 값<br/>
		/// 센서 검지 유형이 VEHICLE인 경우만 생성
		/// </summary>
		internal List<int> NetworkVehicleTTC { get; set; }

		#endregion

		/// <summary>
		/// 횡방향 차트값 [ㅣ]
		/// </summary>
		public enum ChartMainType
		{
			/// <summary>
			/// 자율주행 차량 관련 차트
			/// </summary>
			EgoVehicle,
			/// <summary>
			/// 자율주행차량 주변 특정 거리에 있는 차량
			/// </summary>
			AroundVehicle,
			/// <summary>
			/// Vissim Network에 존재하는 모든 차량
			/// </summary>
			NetworkVehicle,
		}

		/// <summary>
		/// 종방향 차트값 [ㅡ]
		/// </summary>
		public enum ChartSubType
		{
			/// <summary>
			/// 앞 차량과의 거리<br/>
			/// 센서 검지 유형이 VEHICLE인 경우만 생성
			/// </summary>
			FollowingDistance,
			/// <summary>
			/// 차량의 속도
			/// </summary>
			Speed,
			/// <summary>
			/// 앞 차량과의 TTC<br/>
			/// 센서 검지 유형이 VEHICLE인 경우만 생성
			/// </summary>
			TTC,
			/// <summary>
			/// 차량의 가속도
			/// </summary>
			Acceleration
		}

		#endregion

		/// <summary>
		/// 자율주행차량 주변 차량을 지정할 때 관리할 거리값 [M]
		/// </summary>
		private double _aroundVehicleDistance { get; set; }

		/// <summary>
		/// 자율주행차량 주변 차량의 수
		/// </summary>
		public int AroundVehicleCount { get; set; }

		/// <summary>
		/// 자율주행차량 존재 유무
		/// </summary>
		public bool EgoVehicleExist 
		{ 
			get { return EgoVehicle.Exist; }
		}

		/// <summary>
		/// 시뮬레이션 평균 데이터<br/>
		/// Key: (Junction Id, 현시 순서)
		/// </summary>
		public Dictionary<(int, int), SimulationResult.AverageData> SimulationAverageData { get; private set; }

		/// <summary>
		/// 자율주행차량 속도 제한, 속도준수율, 신호준수율에 사용
		/// </summary>
		private SimulationResult.EgoSafety.SpeedLimit SpeedLimit { get; set; }
		/// <summary>
		/// 자율주행차량 속도준수율 평균<br/>
		/// Key: 자율주행차량 번호
		/// </summary>
		public Dictionary<int, SimulationResult.EgoSafety.Speed> EgoSpeed { get; set; }
		/// <summary>
		/// 자율주행차량 속도준수율 Raw<br/>
		/// Key: 자율주행차량 번호
		/// </summary>
		public Dictionary<int, List<SimulationResult.EgoSafety.SpeedRaw>> EgoSpeedRaw { get; set; }
		/// <summary>
		/// 자율주행차량 안전거리준수율 평균<br/>
		/// Key: 자율주행차량 번호
		/// </summary>
		public Dictionary<int, SimulationResult.EgoSafety.Distance> EgoDistance { get; set; }
		/// <summary>
		/// 자율주행차량 안전거리준수율 Raw<br/>
		/// Key: 자율주행차량 번호
		/// </summary>
		public Dictionary<int, List<SimulationResult.EgoSafety.DistanceRaw>> EgoDistanceRaw { get; set; }

		/// <summary>
		/// 자율주행차량 신호준수율 평균<br/>
		/// Key: 자율주행차량 번호
		/// </summary>
		public Dictionary<int, SimulationResult.EgoSafety.Signal> EgoSignal { get; set; }
		/// <summary>
		/// 자율주행차량 신호준수율 Raw<br/>
		/// Key: 자율주행차량 번호
		/// </summary>
		public Dictionary<int, List<SimulationResult.EgoSafety.SignalRaw>> EgoSignalRaw { get; set; }

		/// <summary>
		/// Junction, 현시 기반으로 구성되어있는 신호등 정보<br/>
		/// Key: (Jundtion Id, 현시 순서)
		/// </summary>
		private Dictionary<(int, int), SimulationResult.EgoSafety.SignalInfo> SignalInfoList { get; set; }
		
		/// <summary>
		/// Node를 생성해야하는 Link 목록
		/// </summary>
		private List<int> NodeLinkList { get; set; }
		/// <summary>
		/// Node를 생성할 때 필요한 정보<br/>
		/// Key: NodeLinkList의 Link 번호
		/// </summary>
		private Dictionary<int, NodeInfo> NodeInfoList { get; set; }

		/// <summary>
		/// Vissim에 생성되어있는 차량의 정보
		/// </summary>
		private Dictionary<int, VehicleData> VehicleList { get; set; }

		/// <summary>
		/// 자율주행차량 정보
		/// </summary>
		public EgoVehicleModel EgoVehicle { get; set; }

		/// <summary>
		/// 시뮬레이션 시간
		/// </summary>
		private double SimulationSecond { get; set; }

		/// <summary>
		/// 시뮬레이션 해상도
		/// </summary>
		private double SimulationResolution { get; set; }

		/// <summary>
		/// 시뮬레이션 1회 진행시 실행되는 시간
		/// </summary>
		private double SimulationInterval { get; set; }

		/// <summary>
		/// 생성자
		/// </summary>
		/// <param name="simulationResolution">시뮬레이션 해상도</param>
		public NetworkInfoModel(double simulationResolution)
		{
			// 차트 초기값 생성
			#region Chart Value Initialize

			#region Ego Vehicle

			EgoVehicleSpeed = 0;
			EgoVehicleAcceleration = 0;
			EgoVehicleAheadDistance = 0;
			EgoVehicleTTC = 0;

			#endregion

			#region Around Vehicle

			AroundVehicleSpeed = new List<int>(); ;
			AroundVehicleAcceleration = new List<int>();
			AroundVehicleAheadDistance = new List<int>();
			AroundVehicleTTC = new List<int>();

			#endregion

			#region Network Vehicle

			NetworkVehicleSpeed = new List<int>();
			NetworkVehicleAcceleration = new List<int>();
			NetworkVehicleAheadDistance = new List<int>();
			NetworkVehicleTTC = new List<int>();

			#endregion

			InitChartData();

			#endregion

			// 사용되는 데이터, Class 생성
			#region Class Initialize

			EgoVehicle = new EgoVehicleModel();
			EgoSpeed = new Dictionary<int, SimulationResult.EgoSafety.Speed>();
			EgoDistance = new Dictionary<int, SimulationResult.EgoSafety.Distance>();
			EgoSignal = new Dictionary<int, SimulationResult.EgoSafety.Signal>();
			SpeedLimit = new SimulationResult.EgoSafety.SpeedLimit();

			EgoSpeedRaw = new Dictionary<int, List<SimulationResult.EgoSafety.SpeedRaw>>();
			EgoDistanceRaw = new Dictionary<int, List<SimulationResult.EgoSafety.DistanceRaw>>();
			EgoSignalRaw = new Dictionary<int, List<SimulationResult.EgoSafety.SignalRaw>>();

			VehicleList = new Dictionary<int, VehicleData>();

			SimulationAverageData = new Dictionary<(int, int), SimulationResult.AverageData>();

			#endregion

			_aroundVehicleDistance = 100;
			SimulationResolution = simulationResolution;
			SimulationInterval = 1 / SimulationResolution;
		}

		/// <summary>
		/// Vissim의 차량 정보를 분석함
		/// </summary>
		/// <param name="vehicleList">Vissim에서 가져온 차량 데이터</param>
		/// <param name="simulationSecond">차량 데이터를 가져온 시뮬레이션 시간</param>
		public void AnalyzeVissimData(Dictionary<int, VehicleData> vehicleList, double simulationSecond)
		{
			// 전역에서 사용할 변수에 차량정보, 시뮬레이션시간 복사
			VehicleList = vehicleList;
			SimulationSecond = simulationSecond;

			// 표출중인 그래프 초기화
			ClearGraphData();
			// 데이터 분류
			ClassificateVehicleData();
		}

		/// <summary>
		/// Vissim의 차량 데이터 분석
		/// </summary>
		public void ClassificateVehicleData()
		{
			// 매초 정각에 그래프 업데이트
			//bool updateGraph = Math.Round(simulationSecond, 3) % 1 == 0 ? true : false;

			/// 자율주행차량 미등장: Network 그래프 차트만 사용
			/// 자율주행차량 생성: Ego, Around, Network 차트 사용
			/// 자율주행차량 사라짐: Network 그래프 차트만 사용
			// 자율주행차량 등장 유무에 따라 차트에 사용되는 데이터를 분류함
			switch ( EgoVehicle.State)
			{
				// 자율주행차량이 등장하지 않은 경우
				case EgoVehicleModel.States.NotAppear:

					/// 자율주행 차량을 검지할 경우 사용하는 Flag
					bool isStoped = false;

					// Vissim에서 가져온데이터를 확인
					foreach (var vehicleData in VehicleList)
					{
						// 차량의 타입이 자율주행 차량인 경우
						if (vehicleData.Value.VehicleType.Equals((int)VehicleType.Ego))
						{
							// 자율주행차량 정보 갱신
							EgoVehicle.Exist = true;
							EgoVehicle.State = EgoVehicleModel.States.Appear;
							EgoVehicle.Number = vehicleData.Key;

							// 자율주행 차량 검지 flag 설정 및 반복문 중지
							isStoped = true;
							break;
						}

						// Network 차량 데이터 분류 (Graph)
						Classification(vehicleData, ChartMainType.NetworkVehicle);
					}

					if (isStoped)
					{
						ClearGraphData();

						goto case EgoVehicleModel.States.Appear;
					}

					break;

				case EgoVehicleModel.States.Appear:

					if (false == VissimController.IsVehicleExist(EgoVehicle.Number))
					{
						EgoVehicle.State = EgoVehicleModel.States.Disappear;
						EgoVehicle.Exist = false;

						goto case EgoVehicleModel.States.Disappear;
					}

					AroundVehicleCount = 0;

					(double egoX, double egoY) = CalculatorModel.GetCoord(VehicleList[EgoVehicle.Number].CoordFront, VehicleList[EgoVehicle.Number].CoordRear);

					foreach (var vehicleData in VehicleList)
					{
						if (vehicleData.Key.Equals(EgoVehicle.Number))
						{
							Classification(vehicleData, ChartMainType.EgoVehicle);
						}
						else
						{
							(double vehX, double vehY) = CalculatorModel.GetCoord(vehicleData.Value.CoordFront, vehicleData.Value.CoordRear);
							double distance = CalculatorModel.GetDistance(egoX, egoY, vehX, vehY);

							if (distance < _aroundVehicleDistance)
							{
								Classification(vehicleData, ChartMainType.AroundVehicle);
								AroundVehicleCount++;
							}
						}

						Classification(vehicleData, ChartMainType.NetworkVehicle);
					}

					break;

				case EgoVehicleModel.States.Disappear:

					foreach (var vehicleData in VehicleList)
					{
						Classification(vehicleData, ChartMainType.NetworkVehicle);
					}

					break;

			}
		}

		/// <summary>
		/// 차량 데이터 분류, 데이터 생성
		/// </summary>
		/// <param name="vehicleData">차량 데이터</param>
		/// <param name="mainType">Ego, Around, Network</param>
		private void Classification(KeyValuePair<int, VehicleData> vehicleData, ChartMainType mainType)
		{
			// 그래프 생성을 위한 데이터 분류
			ClassificationForGraph(vehicleData.Value, mainType);
			// 결과값 생성을 위한 데이터 분류
			ClassificationForResult(vehicleData.Value);

			// 자율주행차량인 경우
			if (ChartMainType.EgoVehicle == mainType)
			{
				// 안전지표 측정 데이터 분류
				ClassificationForSafety(vehicleData);
			}
		}

		/// <summary>
		/// Graph에 사용할 데이터 분류
		/// </summary>
		/// <param name="vehicleData">차량 데이터</param>
		/// <param name="mainType">Ego, Around, Network</param>
		private void ClassificationForGraph(VehicleData vehicleData, ChartMainType mainType)
		{
			// 차량의 속도
			double speed = vehicleData.Speed;
			// 차량의 가속도
			double acceleration = vehicleData.Acceleration;

			// 앞 차량과의 거리, 센서에 검지된 물체가 차량이 아닌경우, 100M 설정
			double headway = (vehicleData.LeadTargetType.Equals("VEHICLE")) ? vehicleData.FollowDistance : 100;
			
			// 앞 차량과의 TTC, 센서에 검지된 물체가 차량이 아닌 경우, 5S 설정
			double ttc = (vehicleData.LeadTargetType.Equals("VEHICLE")) ? CalculatorModel.GetTTC(vehicleData.FollowDistance, vehicleData.SpeedDifference) : 5;
			
			// 차트 형식에 따른 데이터 입력
			switch (mainType)
			{
				// 자율주행차량인경우, 1대밖에 없으므로, 해당값 입력함
				case ChartMainType.EgoVehicle:

					EgoVehicleSpeed = speed;
					EgoVehicleAcceleration = acceleration;
					EgoVehicleTTC = ttc;
					EgoVehicleAheadDistance = headway;

					break;

				// 자율주행차량 주변 차량
				case ChartMainType.AroundVehicle:

					AroundVehicleSpeed[GetGraphIndex(speed, ChartSubType.Speed)]++;
					AroundVehicleAcceleration[GetGraphIndex(Math.Abs(acceleration), ChartSubType.Acceleration)]++;
					AroundVehicleTTC[GetGraphIndex(ttc, ChartSubType.TTC)]++;
					AroundVehicleAheadDistance[GetGraphIndex(headway, ChartSubType.FollowingDistance)]++;
						
					break;

				// Vissim Network 모든 차량
				case ChartMainType.NetworkVehicle:

					NetworkVehicleSpeed[GetGraphIndex(speed, ChartSubType.Speed)]++;
					NetworkVehicleAcceleration[GetGraphIndex(Math.Abs(acceleration), ChartSubType.Acceleration)]++;
					NetworkVehicleTTC[GetGraphIndex(ttc, ChartSubType.TTC)]++;
					NetworkVehicleAheadDistance[GetGraphIndex(headway, ChartSubType.FollowingDistance)]++; 

					break;
			}
		}

		/// <summary>
		/// 시뮬레이션 평균 결과값 산출에 필요한 데이터 분류
		/// </summary>
		/// <param name="data"></param>
		private void ClassificationForResult(VehicleData data)
		{
			// Node에 해당하는 link인지 확인
			if (NodeLinkList.Contains(data.Link))
			{
				// Key값 획득
				(int, int) key = (NodeInfoList[data.Link].JunctionId, NodeInfoList[data.Link].Sequence);

				// TTC 계산
				double ttc = CalculatorModel.GetTTC(data.FollowDistance, data.SpeedDifference);

				SimulationAverageData[key].TTC += ttc;
				SimulationAverageData[key].TTCCounter++;
				SimulationAverageData[key].NumberOfConflict += (ttc <= 0.8) ? 1 : 0;
				SimulationAverageData[key].TTCI += (ttc <= 3) ? 1 : 0;

				// SDI 계산
				SimulationAverageData[key].SDI += CalculatorModel.GetSDI(data.FollowDistance, data.Speed, data.SpeedDifference);
			}
		}

		/// <summary>
		/// 제한속도준수율, 안전거리준수율, 신호준수율 관련지표 산출
		/// </summary>
		/// <param name="vehicleData">차량 데이터</param>
		private void ClassificationForSafety(KeyValuePair<int, VehicleData> vehicleData)
		{
			int key = vehicleData.Key;
			VehicleData data = vehicleData.Value;

			// 제한속도 준수 관련값 추가
			EgoSpeed[key].CheckCount++;
			EgoSpeedRaw[key].Add(new SimulationResult.EgoSafety.SpeedRaw(SimulationSecond, data));

			// 현재 입력한 값의 index 지정
			int rowIndex = EgoSpeedRaw[key].Count - 1;
			
			// 속도값이 제한속도값을 초과할 경우
			if (SpeedLimit.MaximumSpeed < data.Speed)
			{
				// 속도초과횟수 추가
				EgoSpeed[key].OverSpeedCount++;

				// 패널티값 산출
				double speedPanelty = 0;

				if (50 < data.Speed && data.Speed <= 60)
				{
					speedPanelty = (double)SpeedLimit.Over50 / SimulationInterval;
				}
				else if (60 < data.Speed && data.Speed <= 70)
				{
					speedPanelty = (double)SpeedLimit.Over60 / SimulationInterval;
				}
				else if (70 < data.Speed )
				{
					speedPanelty = (double)SpeedLimit.Over70 / SimulationInterval;
				}

				// 패널티 값 추가
				EgoSpeed[key].Penalty += speedPanelty;
				if (speedPanelty > 0)
				{
					EgoSpeedRaw[key][rowIndex].AddPanelty(speedPanelty);
				}
			}

			// 안전거리 데이터 산출
			EgoDistance[key].CheckCount++;
			EgoDistanceRaw[key].Add(new SimulationResult.EgoSafety.DistanceRaw(SimulationSecond, data));
			// 현재 입력한 값의 index 지정
			rowIndex = EgoDistanceRaw[key].Count - 1;

			// 센서검지값이 차량일 경우
			if (data.LeadTargetType.Equals("VEHICLE") && VehicleList.ContainsKey(data.LeadTargetNumber))
			{
				int targetNumber = data.LeadTargetNumber;
				EgoDistanceRaw[key][rowIndex].AddAheadVehicleData(VehicleList[targetNumber]);

				// 두 차량이 충돌하는 경우
				// 0 < Following vehicle speed - ahead vehicle speed : 충돌 가능성 존재
				if (0 < data.SpeedDifference)
				{
					double safetyDistance = EgoDistanceRaw[key][rowIndex].SafetyDistance;
					double safetyDistanceConsiderWeight = (targetNumber == 0) ? 0 : EgoDistanceRaw[key][rowIndex].ConsiderWeightSafetyDistance;
					/*if (VehicleList.ContainsKey(data.LeadTargetNumber))
					{
						safetyDistanceConsiderWeight = (targetNumber == 0) ? 0 :
							CalculatorModel.GetSafetyDistanceConsiderMassDifference(VehicleList[targetNumber].Weight, data.Weight, data.Speed, data.SpeedDifference, data.Acceleration);
					}*/

					// 질량차 안전거리
					if (data.FollowDistance <= safetyDistance + safetyDistanceConsiderWeight)
						EgoDistance[key].NotConsiderWeight++;

					// 안전거리
					if (data.FollowDistance <= safetyDistance)
						EgoDistance[key].NotConsiderSafeDistance++;
				}
			}

			// 신호준수율 데이터 추가
			EgoSignalRaw[key].Add(new SimulationResult.EgoSafety.SignalRaw(SimulationSecond, data));
			rowIndex = EgoSignalRaw[key].Count - 1;

			// 교통신호 준수율, 교차로 통과 대상만 추가
			if ( NodeLinkList.Contains(data.Link) )
			{
				(int, int) nodeKey = (NodeInfoList[data.Link].JunctionId, NodeInfoList[data.Link].Sequence);
				EgoSignalRaw[key][rowIndex].AddSignalInfo(SignalInfoList[nodeKey]);

				// 신호를 통과한 경우
				if ( EgoSignal[key].SignalHeadPassed )
				{
					// 마지막 측정지점을 통과한 후 
					// 다른 측정지점(신호 도달 이전)인 경우
					if ( (SignalInfoList[nodeKey].SignalLinkNumber == data.Link && data.Position < SignalInfoList[nodeKey].SignalHeadPosition) )
					{
						EgoSignal[key].SignalHeadPassed = false;
						return;
					}

					// 데이터 추가
					double greenStartTime = SignalInfoList[nodeKey].GreenStartTime;
					double amberStartTime = SignalInfoList[nodeKey].AmberStartTime;
					double amberEndTime = SignalInfoList[nodeKey].AmberEndTime;
					double currentCycleTime = Math.Round(SimulationSecond % SignalInfoList[nodeKey].SignalCycle, 2);

					// 황색 신호 통과 여부
					if ( amberStartTime <= currentCycleTime && currentCycleTime <= amberEndTime )
					{
						if ( !EgoSignal[key].PassedInAmber )
						{
							EgoSignal[key].PassedInAmber = true;
						}
					}

					EgoSignal[key].CheckCount++;
					double sigPanelty = 0;
					SimulationResult.EgoSafety.SignalPenalty sigPassed;
					// 신호점유 시간 계산
					// 녹색 신호 통과 여부
					if ( greenStartTime <= currentCycleTime && currentCycleTime < amberStartTime )
					{
						sigPassed = SimulationResult.EgoSafety.SignalPenalty.Green;
						sigPanelty = (double)sigPassed;

						EgoSignal[key].InGreen++;
					}
					// 황색신호 변경 후 2초 미만
					else if ( amberStartTime <= currentCycleTime && currentCycleTime < amberStartTime + 2 )
					{
						sigPassed = SimulationResult.EgoSafety.SignalPenalty.AmberSafe;
						sigPanelty = (double)sigPassed / SimulationInterval;

						EgoSignal[key].InAmberAhead++;
					}
					// 황색신호 변경 후 2초 이상
					else if ( amberStartTime + 2 <= currentCycleTime && currentCycleTime < amberEndTime )
					{
						sigPassed = SimulationResult.EgoSafety.SignalPenalty.AmberUnsafe;
						sigPanelty = (double)sigPassed / SimulationInterval;

						EgoSignal[key].InAmberLast++;
					}
					// 적색신호 통과
					else
					{
						sigPassed = SimulationResult.EgoSafety.SignalPenalty.Red;
						sigPanelty = (double)sigPassed / SimulationInterval;

						EgoSignal[key].InRed++;
					}

					// 신호준수율 패널티값 추가
					EgoSignal[key].SigPenalty += sigPanelty;
					EgoSignalRaw[key][rowIndex].AddSignalData(sigPanelty, sigPassed);

					if ( SpeedLimit.MaximumSpeed < data.Speed )
					{
						EgoSignal[key].OverSpeedCount++;

						if ( 50 < data.Speed && data.Speed <= 60 )
							EgoSignal[key].Penalty += (double)SpeedLimit.Over50 / SimulationInterval;
						else if ( 60 < data.Speed && data.Speed <= 70 )
							EgoSignal[key].Penalty += (double)SpeedLimit.Over60 / SimulationInterval;
						else if ( 70 < data.Speed && data.Speed <= 80 )
							EgoSignal[key].Penalty += (double)SpeedLimit.Over70 / SimulationInterval;
					}
				}
				else // 신호를 통과하지 않은 경우
				{
					// 측정 지점에 진입했는지 확인, 측점 지점일 경우 데이터 추가
					if ( (SignalInfoList[nodeKey].SignalLinkNumber == data.Link && SignalInfoList[nodeKey].SignalHeadPosition <= data.Position) || // signal head의 link 이고, 차량의 pos값이 signalhead의 pos값을 넘어선경우 -> 측정 시작
						 (SignalInfoList[nodeKey].ConnectorList.Contains(data.Link) && EgoSignal[key].SignalHeadPassed == false) ) // signal head 다음 connector 이고, signalHeadPassed 값이 false 인 경우 -> 측정 시작
					{
						EgoSignal[key].SignalHeadPassed = true;
						double greenStartTime = SignalInfoList[nodeKey].GreenStartTime;
						double amberStartTime = SignalInfoList[nodeKey].AmberStartTime;
						double amberEndTime = SignalInfoList[nodeKey].AmberEndTime;
						double currentCycleTime = Math.Round(SimulationSecond % SignalInfoList[nodeKey].SignalCycle, 2);

						// 황색 신호 통과 여부
						if ( amberStartTime <= currentCycleTime && currentCycleTime <= amberEndTime )
						{
							if ( !EgoSignal[key].PassedInAmber )
							{
								EgoSignal[key].PassedInAmber = true;
							}
						}

						EgoSignal[key].CheckCount++;
						double panelty = 0;
						SimulationResult.EgoSafety.SignalPenalty sigPassed;
						// 신호점유 시간 계산
						// 녹색 신호 통과 여부
						if ( greenStartTime <= currentCycleTime && currentCycleTime < amberStartTime )
						{
							sigPassed = SimulationResult.EgoSafety.SignalPenalty.Green;
							panelty = (double)sigPassed;

							EgoSignal[key].InGreen++;
						}
						// 황색신호 변경 후 2초 미만
						else if ( amberStartTime <= currentCycleTime && currentCycleTime < amberStartTime + 2 )
						{
							sigPassed = SimulationResult.EgoSafety.SignalPenalty.AmberSafe;
							panelty = (double)sigPassed / SimulationInterval;

							EgoSignal[key].InAmberAhead++;
						}
						// 황색신호 변경 후 2초 이상
						else if ( amberStartTime + 2 <= currentCycleTime && currentCycleTime < amberEndTime )
						{
							sigPassed = SimulationResult.EgoSafety.SignalPenalty.AmberUnsafe;
							panelty = (double)sigPassed / SimulationInterval;

							EgoSignal[key].InAmberLast++;
						}
						// 적색신호 통과
						else
						{
							sigPassed = SimulationResult.EgoSafety.SignalPenalty.Red;
							panelty = (double)sigPassed / SimulationInterval;
							
							EgoSignal[key].InRed++;
						}

						EgoSignal[key].SigPenalty += panelty;
						EgoSignalRaw[key][rowIndex].AddSignalData(panelty, sigPassed);

						if ( SpeedLimit.MaximumSpeed < data.Speed )
						{
							EgoSignal[key].OverSpeedCount++;

							if ( 50 < data.Speed && data.Speed <= 60 )
								EgoSignal[key].Penalty += (double)SpeedLimit.Over50 / SimulationInterval;
							else if ( 60 < data.Speed && data.Speed <= 70 )
								EgoSignal[key].Penalty += (double)SpeedLimit.Over60 / SimulationInterval;
							else if ( 70 < data.Speed && data.Speed <= 80 )
								EgoSignal[key].Penalty += (double)SpeedLimit.Over70 / SimulationInterval;
						}
					}
				}
			}
			else // 마지막 측정지점을 통과한 후 측정 지점이 아닌 Link로 이동한 경우
			{
				// 측정 하도록 지정되어있으면, 해제함
				if ( EgoSignal[key].SignalHeadPassed )
				{
					EgoSignal[key].SignalHeadPassed = false;
				}
			}
		}

		/// <summary>
		/// Graph에 입력되어있는 데이터를 초기화함
		/// </summary>
		private void ClearGraphData()
		{

			#region AroundVehicle

			if (EgoVehicleExist)
			{
				for (int i = 0; i < 5; i++)
				{
					AroundVehicleSpeed[i] = 0;
					AroundVehicleAcceleration[i] = 0;
					AroundVehicleTTC[i] = 0;
					AroundVehicleAheadDistance[i] = 0;
				}
			}

			#endregion

			#region NetworkVehicle

			for (int i = 0; i < 5; i++)
			{
				NetworkVehicleSpeed[i] = 0;
				NetworkVehicleAcceleration[i] = 0;
				NetworkVehicleTTC[i] = 0;
				NetworkVehicleAheadDistance[i] = 0;
			}

			#endregion
		}

		/// <summary>
		/// Graph 중 Around, Network의 경우, 여러 차량이기 때문에 관련 데이터를 입력할 Index가 필요함<br/>
		/// 데이터를 추가할 index값을 리턴함
		/// </summary>
		/// <param name="value">값</param>
		/// <param name="subType">관련된 차트</param>
		/// <returns>추가해야할 graph index</returns>
		/// <exception cref="Exception"></exception>
		private int GetGraphIndex(double value, ChartSubType subType)
		{
			switch (subType)
			{
				case ChartSubType.Speed:

					if (value <= 10)
						return 0;
					else if (value <= 20)
						return 1;
					else if (value <= 30)
						return 2;
					else if (value <= 40)
						return 3;
					else
						return 4;

				case ChartSubType.Acceleration:
					if ( value <= 0.8 )
						return 0;
					else if ( value <= 1.6 )
						return 1;
					else if ( value <= 3.2 )
						return 2;
					else if ( value <= 4.0 )
						return 3;
					else
						return 4;

				case ChartSubType.FollowingDistance:

					if ( value <= 20 )
						return 4;
					else if ( value <= 40 )
						return 3;
					else if ( value <= 60 )
						return 2;
					else if ( value <= 80 )
						return 1;
					else
						return 0;

				case ChartSubType.TTC:

					if (value <= 1)
						return 4;
					else if (value <= 2)
						return 3;
					else if (value <= 3)
						return 2;
					else if (value <= 4)
						return 1;
					else
						return 0;

				default:
					throw new Exception("GetIndex Error");
			}
		}

		/// <summary>
		/// 차트 데이터 초기 생성
		/// </summary>
		private void InitChartData()
		{
			for (int i = 0; i < 5; i ++)
			{
				AroundVehicleSpeed.Add(0);
				AroundVehicleAcceleration.Add(0);
				AroundVehicleAheadDistance.Add(0);
				AroundVehicleTTC.Add(0);

				NetworkVehicleSpeed.Add(0);
				NetworkVehicleAcceleration.Add(0);
				NetworkVehicleAheadDistance.Add(0);
				NetworkVehicleTTC.Add(0);
			}
		}

		/// <summary>
		/// Node 데이터 설정 및 평균값 데이터 측정 지역 설정
		/// </summary>
		/// <param name="nodeLinkList">Node를 생성해야하는 Link 목록</param>
		/// <param name="nodeInfoList">Node를 생성할 때 필요한 정보</param>
		/// <param name="signalInfoList">Junction, 현시 기반으로 구성되어있는 신호등 정보</param>
		public void SetNodeData(List<int> nodeLinkList, Dictionary<int, NodeInfo> nodeInfoList, Dictionary<(int, int), SimulationResult.EgoSafety.SignalInfo> signalInfoList)
		{
			// Node 데이터 설정
			NodeLinkList = nodeLinkList;
			NodeInfoList = nodeInfoList;
			SignalInfoList = signalInfoList;

			// 평균값 데이터 측정 지역 설정
			InitSimulationAverageData();
		}

		/// <summary>
		/// 평균값 데이터를 산출해야하는 목록을 생성
		/// </summary>
		private void InitSimulationAverageData()
		{
			// 필요한데이터가 없을 경우, 에러 처리
			if ( NodeLinkList.Count == 0 || NodeInfoList.Count == 0 )
			{
				//MessageBox.Show("NodeLinkList, NodeInfoList의 데이터를 확인할 수 없습니다..");
				return;
			}

			// 목록 생성
			for ( int i = 0; i < NodeInfoList.Count; i++ )
			{
				int linkNumber = NodeLinkList[i];
				(int, int) key = (NodeInfoList[linkNumber].JunctionId, NodeInfoList[linkNumber].Sequence);

				// 해당값이 목록에 없으면
				if ( !SimulationAverageData.ContainsKey(key) )
				{
					// 목록 생성
					SimulationAverageData.Add(key, new SimulationResult.AverageData());
				}
			}
		}
	}
}
