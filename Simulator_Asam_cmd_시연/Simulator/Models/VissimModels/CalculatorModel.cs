using DTO.OpenSCENARIO_1_0;
using Simulator.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using VISSIMLIB;

using static Simulator.Models.NetworkModel;
using static Simulator.Common.VissimController;

namespace Simulator.Models.VissimModels
{
	/// <summary>
	/// 관련값 산출 시 계산 로직
	/// </summary>
	public static class CalculatorModel
	{

		#region Parse Vehicle Init Data

		/// <summary>
		/// OpenSCENARIO에서 차량 생성정보를 입력받아 vissim에서 생성해야할 데이터 구조체로 변환하여 반환함
		/// </summary>
		/// <param name="vehicleList">OpenSCENARIO에서 정의된 차량 정보</param>
		/// <param name="linkList">Vissim Network의 LINK 정보</param>
		/// <param name="roadList">Vissim Network의 ROAD 정보</param>
		/// <returns>Vissim Simulation에서 생성해야할 차량 정보</returns>
		/// <exception cref="Exception"></exception>
		public static List<VehicleVissimSpawnData> ParseVehicleData(
			Dictionary<string, VehicleOpenSCENARIOSpawnData> vehicleList, 
			Dictionary<int, NetworkModel.LinkData> linkList, 
			Dictionary<(int, NetworkModel.RoadDirection), NetworkModel.RoadData> roadList)
		{
			// 리턴할 데이터 구조체 생성
			List<VehicleVissimSpawnData> vehicleInitList = new List<VehicleVissimSpawnData>();

			// 파싱할 데이터 저장
			int linkNo = 0, laneNo = 0;
			double position = 0;

			/// OpenSCENARIO에 구성되어있는 데이터 기반으로 파싱함
			foreach (var vehicleData in vehicleList)
			{
				// Location Type에 따라서 차량 파싱 로직 변경
				switch(vehicleData.Value.CreateLocation.LocationType)
				{
					case LocationType.WorldPosition:
						(linkNo, laneNo, position) = ParseWorldPositionData(vehicleData.Value, linkList);
						break;

					case LocationType.RoadPosition:
						(linkNo, laneNo, position) = ParseRoadPositionData(vehicleData.Value, linkList, roadList);
						break;

					case LocationType.RelativeRoadPosition:
						(linkNo, laneNo, position) = ParseRoadPositionData(vehicleData.Value, linkList, roadList);
						break;

					case LocationType.LanePosition:
						(linkNo, laneNo, position) = ParseLanePositionData(vehicleData.Value, linkList, roadList);
						break;

					default: throw new Exception("CalculatorModel.GetVehicleInitData occurred error. Not identifyed locationType input");
				}

				// 기본 색상은 검정(0, 0, 0)
				// Ego 기본색상: (0, 128, 255)
				// Target 기본색상: (255, 40, 40)
				int red = 0, green = 0, blue = 0;

				// Color 항목이 입력되어있을 입력된 색상 사용
				if (!vehicleData.Value.Color!.Equals(""))
				{
					string[] colors = vehicleData.Value.Color.Trim().Split(',');

					red = Convert.ToInt32(colors[0]);
					green = Convert.ToInt32(colors[1]);
					blue = Convert.ToInt32(colors[2]);
				}
				// Color 항목이 없고, 자율주행 차량인 경우
				else if (vehicleData.Value.VehicleType == VehicleType.Ego)
				{
					red = 0;
					green = 128;
					blue = 255;
				}
				// Color 항목이 없고, Simulation 차량인 경우
				else if (vehicleData.Value.VehicleType == VehicleType.Simulation)
				{
					red = 214;
					green = 40;
					blue = 40;
				}

				// Vissim에서 생성해야할 차량 데이터 생성
				vehicleInitList.Add(new VehicleVissimSpawnData()
				{
					VehicleType = (int)vehicleData.Value.VehicleType,
					LinkNo = linkNo,
					LaneNo = laneNo,
					Position = position,
					DesiredSpeed = vehicleData.Value.MaxSpeed,
					Color = System.Drawing.Color.FromArgb(red, green, blue).Name,
					VehicleBoundingBox = vehicleData.Value.BoundingBox,
					Name = vehicleData.Value.ScenarioObjectName
				});
			}

			return vehicleInitList;
		}

		/// <summary>
		/// ASAM OpenSCENARIO(WorldPosition) vehicle data convert to vissim init data
		/// </summary>
		/// <param name="vehicleData">ASAM openSCENARIO worldPosition data</param>
		/// <param name="linkList">Network link data</param>
		/// <returns>LinkNo, LaneNo, Position</returns>
		/// <exception cref="Exception">not managed error case</exception>
		private static (int linkNo, int laneNo, double position) ParseWorldPositionData(VehicleOpenSCENARIOSpawnData vehicleData, Dictionary<int, NetworkModel.LinkData> linkList)
		{
			// return value
			int link = 0, lane = 0;
			double pos = 0;

			// OpenSCENARIO에 지정되어있는 X, Y 위치
			(double x, double y) = (vehicleData.CreateLocation.X, vehicleData.CreateLocation.Y);
			// Vissim에서 찾을 link poly point 값
			(double minX, double minY) = (x - 10, y - 10);
			// 해당위치 찾을 시 저장할 poly point 인덱스
			int polyIndex = 0;

			/// vissim의 모든 link 데이터 조회 후 가장 가까운 위치를 찾음
			foreach (var linkData in linkList)
			{
				// link poly point 개수
				int cnt = linkData.Value.PolyPoints.Count;

				// 모든 link poly point 조회
				for (int i = 0; i < cnt; i++)
				{
					// poly point x
					double vissimX = linkData.Value.PolyPoints[i].X;
					// poly point y
					double vissimY = linkData.Value.PolyPoints[i].Y;

					// link poly point 값이 일정 범위에 들어올 경우
					if ((x - 3 < vissimX && vissimX < x + 3) && y - 3 < vissimY && vissimY < y + 3)
					{
						/// x, y값이 - or + 값을 가지기때문에 좌표값이 일치해야함
						// 좌표값이 동일 (-, -) or (+, +) 한 경우에만 사용
						if (x * vissimX > 0 && y * vissimY > 0)
						{
							// vissim에서 찾은 X좌표값
							double xValue1 = Math.Pow(x, 2) - Math.Pow(vissimX, 2);
							// 기존에 입력되어있던 가장 가까운 X좌표값
							double xValue2 = Math.Pow(x, 2) - Math.Pow(minX, 2);
							// vissim에서 찾은 Y좌표값
							double yValue1 = Math.Pow(y, 2) - Math.Pow(vissimY, 2);
							// 기존에 입력되어있던 가장 가까운 Y좌표값
							double yValue2 = Math.Pow(y, 2) - Math.Pow(minY, 2);

							// 기존에 입력디어있던 좌표값보다 더 가까운 vissim 좌표지점을 찾은 경우
							// 가장 가까운 위치로 지정함
							if (Math.Sqrt(Math.Pow(x - vissimX, 2) + Math.Pow(y - vissimY, 2)) < Math.Sqrt(Math.Pow(x - minX, 2) + Math.Pow(y - minY, 2)))
							{
								// link 번호
								link = linkData.Key;
								// 가장 가까운 위치를 찾기위한 X좌표 갱신
								minX = vissimX;
								// 가장 가까운 위치를 찾기위한 Y좌표 갱신
								minY = vissimY;
								// Link 최초 지점에서 가장 가까운 위치의 거리 계산
								pos = Math.Sqrt(Math.Pow(linkData.Value.PolyPoints[0].X - vissimX, 2) + Math.Pow(linkData.Value.PolyPoints[0].Y - vissimY, 2));
								// poly point index 저장
								polyIndex = i;
							}
						}
					}
				}
			}

			// 도로 방향 계산
			double polyX = 0, polyY = 0;
			double rdx = 0, rdy = 0;
			// 각도값
			double linkDirection = 0;

			// polyIndex값이 마지막인 경우, minX, minY 이전값 사용
			if (linkList[link].PolyPoints.Count - 1 == polyIndex)
			{
				polyX = linkList[link].PolyPoints[polyIndex - 1].X;
				polyY = linkList[link].PolyPoints[polyIndex - 1].Y;

				rdx = minX - polyX;
				rdy = minY - polyY;
			}
			// 그 외 경우 MinX, MinY 다음값 사용
			else
			{
				polyX = linkList[link].PolyPoints[polyIndex + 1].X;
				polyY = linkList[link].PolyPoints[polyIndex + 1].Y;

				rdx = polyX - minX;
				rdy = polyY - minY;
			}

			linkDirection = Math.Atan2(rdy, rdx) * 180 / Math.PI;

			linkDirection = (linkDirection < 0) ? linkDirection += 360 : linkDirection;

			// 도로 방향 기준으로 차량이 우측, 좌측인지 계산
			// 가장 근접한 poly point minX, minY 값으로 차량 위치 계산
			double vdx = 0, vdy = 0;
			double vehicleDirection = 0;

			vdx = vehicleData.CreateLocation.X - minX;
			vdy = vehicleData.CreateLocation.Y - minY;

			vehicleDirection = Math.Atan2(vdy, vdx) * 180 / Math.PI;

			vehicleDirection = (vehicleDirection < 0) ? vehicleDirection += 360 : vehicleDirection;

			// 0도 기준으로 차량 위치 산출
			// 0초과 180미만  : minX, minY 기준 좌측
			// 180초과 360미만: minX, MinY 기준 우측
			// 180 or 360     : minX, MinY 와 같은 위치
			// ASAM s, t 방향 관계에 따라 좌측: +, 우측: -, 중앙: 0값으로 변환
			vehicleDirection = vehicleDirection - linkDirection;

			vehicleDirection = (vehicleDirection < 0) ? vehicleDirection += 360 : vehicleDirection;

			if (0 < vehicleDirection && vehicleDirection < 180)
			{
				vehicleDirection = 1;
			}
			else if (180 < vehicleDirection && vehicleDirection < 360)
			{
				vehicleDirection = -1;
			}
			// 그 외 경우 180, 360 나오는 경우는 polyPoint와 동일 위상, 
			else
			{
				vehicleDirection = 0;
			}

			// 방향 계산 후 차선 산출
			// vissim에서 도로의 중앙 minX, minY
			// scenario에서 차량의 위치 vehicleData.Value.CreateLocation.X, vehicleData.Value.CreateLocation.Y
			double dx = minX - vehicleData.CreateLocation.X;
			double dy = minY - vehicleData.CreateLocation.Y;

			// 차량 생성 지점(거리)
			double distance = Math.Sqrt(dx * dx + dy * dy);

			// LaneCount 짝수, 홀수일때 거리계산방식 다름
			// 2차선:1차선,2차선사이, 4차선:2차선,3차선사이 등 :
			// distance < width: 1차선, distance < width + width: 2차선, ...  0일경우 에러
			// 1차선: 중앙, 3차선: 2차선중앙 등 :
			// distance < width/2: 현재차선, distance < width/2 + width: 2차선 ...

			int startIndex = linkList[link].LaneCount / 2;
			double totalLaneWidth = 0;

			// LaneCount 값이 짝수일경우 계산
			if (linkList[link].LaneCount % 2 == 0)
			{
				// poly point 값 기준으로 좌, 우측 확인
				switch (vehicleDirection)
				{
					// 우측일 경우
					case 1:

						for (int i = startIndex - 1; 0 <= i; i--)
						{
							totalLaneWidth += linkList[link].LaneWidth[i];

							if (distance < totalLaneWidth)
							{
								lane = i + 1;
								break;
							}
						}

						break;
					// 동일할 경우
					case 0: throw new Exception("Calculate Lane Failed.. Lane: even, direction: 0");
					// 좌측일 경우
					case -1:

						for (int i = startIndex + 1; i <= linkList[link].LaneCount; i++)
						{
							totalLaneWidth += linkList[link].LaneWidth[i];

							if (distance < totalLaneWidth)
							{
								lane = i;
								break;
							}
						}

						break;
				}
			}
			// LaneCount값이 홀수일경우 계산
			else
			{
				// poly point 값 기준으로 좌, 우측 확인
				switch ( vehicleDirection)
				{
					// 우측일경우
					case 1:

						for (int i = startIndex; 0 <= i; i--)
						{
							if (i == startIndex)
							{
								totalLaneWidth += linkList[link].LaneWidth[startIndex] / 2;
							}
							else
							{
								totalLaneWidth += linkList[link].LaneWidth[i];
							}

							if (distance < totalLaneWidth)
							{
								lane = i;
								break;
							}
						}

						break;
					// 일치할경우
					case 0:

						lane = startIndex;
						break;

					// 좌측일경우
					case -1:

						for (int i = startIndex; i < linkList[link].LaneCount; i++)
						{
							if (i == startIndex)
							{
								if (i == startIndex)
								{
									totalLaneWidth += linkList[link].LaneWidth[startIndex] / 2;
								}
								else
								{
									totalLaneWidth += linkList[link].LaneWidth[i];
								}

								if (distance < totalLaneWidth)
								{
									lane = i;
									break;
								}
							}
						}

						break;
					default: throw new Exception("");
				}
			}

			// 산출한 lane 값을 vissim 형태로 변환
			lane = ConvertLaneInfoToVissimLane(lane, linkList[link].LaneCount);

			return (link, lane, pos);
		}

		/// <summary>
		/// ASAM OpenSCENARIO(RoadPosition) vehicledata convert to vissim init data
		/// </summary>
		/// <param name="vehicleData">ASAM openSCENARIO roadPosition data</param>
		/// <param name="linkList">Network link data</param>
		/// <param name="roadList">Network road data</param>
		/// <returns>LinkNo, LaneNo, Position</returns>
		private static (int linkNo, int laneNo, double position) ParseRoadPositionData(VehicleOpenSCENARIOSpawnData vehicleData, Dictionary<int, NetworkModel.LinkData> linkList, Dictionary<(int, Models.NetworkModel.RoadDirection), Models.NetworkModel.RoadData> roadList)
		{
			// return value
			int link = 0, lane = 0;
			double pos = 0;

			double tValue = vehicleData.CreateLocation.T;

			Models.NetworkModel.RoadDirection direction;

			/// 진행방향(S)의 -90도 방향의 값이 Left(양수), +90도 방향의 값이 Right(음수)
			if (tValue < 0)
			{
				direction = Models.NetworkModel.RoadDirection.Right;
			}
			else
			{
				direction = Models.NetworkModel.RoadDirection.Left;
			}

			// Vissim에서 OpenDRIVE기능으로 생성한 Road 관련정보 목록을 불러와서 해당 데이터를 사용함
			Models.NetworkModel.RoadData roadData = roadList[(vehicleData.CreateLocation.RoadId, direction)];
			// 도로 총 연장
			double totalLength = 0;

			// 총 연장, 생성위치 확인
			foreach (var item in roadData.LinkIndexes)
			{
				totalLength += linkList[item].Length;

				if (vehicleData.CreateLocation.Pos <= totalLength)
				{
					link = item;

					totalLength -= linkList[item].Length;
					pos = vehicleData.CreateLocation.Pos - totalLength;

					break;
				}
			}

			// 도로 폭 합산값
			double totalWidth = 0;
			for (int i = 0; i < linkList[link].LaneCount; i++)
			{
				totalWidth += linkList[link].LaneWidth[i];

				if (Math.Abs(tValue) < totalWidth)
				{
					lane = i + 1;
					break;
				}
			}

			// 산출한 lane 값을 vissim 형태로 변환
			lane = ConvertLaneInfoToVissimLane(lane, linkList[link].LaneCount);

			return (link, lane, pos);
		}

		/// <summary>
		/// ASAM OpenSCENARIO(LanePosition) vehicledata convert to vissim init data
		/// </summary>
		/// <param name="vehicleData"></param>
		/// <param name="linkList"></param>
		/// <param name="roadList"></param>
		/// <returns></returns>
		private static (int linkNo, int laneNo, double position) ParseLanePositionData(VehicleOpenSCENARIOSpawnData vehicleData, Dictionary<int, NetworkModel.LinkData> linkList, Dictionary<(int, Models.NetworkModel.RoadDirection), Models.NetworkModel.RoadData> roadList)
		{
			// return value
			VehicleCreateLocation location= vehicleData.CreateLocation;
			int link = 0;
			int lane = 0;

			(link, lane) = Get_egolink(location.RoadId,location.LaneNo);
			double pos = location.Pos;
			

			return (link, lane, pos);
		}

		/// <summary>
		/// Vissim: 진행방향의 우측 차선에서부터 1차선<br/>
		/// OpenSCENARIO: 중앙선에서 좌, 우측으로 1차선<br/>
		/// 서로 lane을 보는 정보가 다르기 때문에 vissim에서 사용하는 형태로 데이터를 변환함
		/// </summary>
		/// <param name="laneValue">사용할 차선 번호</param>
		/// <param name="laneCount">차선의 수</param>
		/// <returns></returns>
		private static int ConvertLaneInfoToVissimLane(int laneValue, int laneCount)
		{
			List<int> lanes = new List<int>();

			for (int i = 1; i <= laneCount; i++)
			{
				lanes.Add(i);
			}

			int laneIndex = 0;

			for (int i = 0; i < laneCount; i++)
			{
				if (lanes[i] == laneValue)
				{
					laneIndex = i;
					break;
				}
			}

			lanes.Reverse();

			return lanes[laneIndex];
		}

		#endregion

		#region Parse Conflict Area Data

		#endregion

		/// <summary>
		/// FromLink, ToLink 목록을 조회하여 진입로, 진출로가 1개일 경우<br/>
		/// 최초 진입로를 조회함
		/// </summary>
		/// <param name="toLinkNumber">ToLink 번호</param>
		/// <param name="vissimFromToLinkList">Vissim에서 연결된 도로 목록</param>
		/// <returns>최초 진입로</returns>
		public static int GetOriginalFromLink(int toLinkNumber, Dictionary<int, List<int>> vissimFromToLinkList)
		{
			int toLinkCount = 0;
			int fromLinkNumber = 0;

			// 진입로 정보, 진출로 개수 확인
			foreach ( var item in vissimFromToLinkList )
			{
				foreach ( var subItem in item.Value )
				{
					if ( subItem == toLinkNumber )
					{
						toLinkCount++;
						fromLinkNumber = item.Key;
					}
				}
			}

			/// 조건 1
			// 진출로 개수가 1개이고, 진입로에서 연결된 도로가 1개일 경우
			if ( toLinkCount == 1 && vissimFromToLinkList[fromLinkNumber].Count == 1 )
			{
				// 재귀적으로 조회함
				int result = GetOriginalFromLink(fromLinkNumber, vissimFromToLinkList);

				/// 조건 3
				// 조회한 결과가 0일경우(조건 2) 현재 검색한 결과가 최초 진입로임
				if ( result == 0 )
				{
					// 최초 진입로 값 반환
					return fromLinkNumber;
				}
				/// 조건 4
				// 재귀조회 값이 지정되어 반환된 경우(조건 3) 해당값을 반환함
				else
				{
					return result;
				}
			}
			/// 조건 2
			// 그외 경우
			else
			{
				return 0;
			}
		}

		/// <summary>
		/// Vissim Coord Front, Rear 값의 중위값 반환<br/>
		/// Vissim Coord: [0]: X, [1]: Y, [2]: Z
		/// </summary>
		/// <param name="coordFront">차량 앞 좌표</param>
		/// <param name="coordRear">차량 뒤 좌표</param>
		/// <returns>(X, Y)</returns>
		public static (double x, double y) GetCoord(string coordFront, string coordRear)
		{
			double x = 0, y = 0;
			
			/// 차량 앞 좌표 - ((차량 앞, 뒤 좌표값의 차이 / 2)
			x = Convert.ToDouble(coordFront.Split(' ')[0]) - ((Convert.ToDouble(coordFront.Split(' ')[0]) - Convert.ToDouble(coordRear.Split(' ')[0])) / 2);
			y = Convert.ToDouble(coordFront.Split(' ')[1]) - ((Convert.ToDouble(coordFront.Split(' ')[1]) - Convert.ToDouble(coordRear.Split(' ')[1])) / 2);

			return (x, y);
		}

		/// <summary>
		/// 입력된 좌표값의 차이로 거리를 계산
		/// </summary>
		/// <param name="x1">X좌표 1</param>
		/// <param name="y1">Y좌표 1</param>
		/// <param name="x2">X좌표 2</param>
		/// <param name="y2">Y좌표 2</param>
		/// <returns></returns>
		public static double GetDistance(double x1, double y1, double x2, double y2)
		{
			double dx = x1 - x2;
			double dy = y1 - y2;

			return Math.Sqrt(dx * dx + dy * dy);
		}

		/// <summary>
		/// TTC값 계산, 뒷 차량 기준으로 측정한 값을 입력 함
		/// </summary>
		/// <param name="followingDistance">뒷 차량에서 측정한 앞 차량과의 속도 차이</param>
		/// <param name="speedDifference">뒷 차량에서 측정한 앞 차량과의 거리</param>
		/// <returns>TTC 값</returns>
		public static double GetTTC(double followingDistance, double speedDifference)
		{
			// 속도 차이가 같거나, 음수이면 앞 차량이 더 빠름
			if (speedDifference <= 0)
			{
				return 5;
			}

			// 거리 차이값이 같거나, 음수이면 이미 충돌한 상황임
			if (followingDistance <= 0)
			{
				return 0;
			}

			// 위의 2가지 상황을 제외한 후 TTC값을 계산함
			double ttc = followingDistance / (speedDifference / 3.6);

			// TTC값이 5 이상으로 표출되는 경우, 충돌 가능성이없는 상태, 5 값으로 반환
			// 5값으로 반환: 그래프상에 표출하기 위함
			return ttc > 5 ? 5 : ttc;
		}

		/// <summary>
		/// 안전거리 계산, 뒷 차 기준으로 speedDifference값을 입력함
		/// </summary>
		/// <param name="speed">뒷 차량의 속도</param>
		/// <param name="speedDifference">뒷 차량에서 측정한 앞 차량과의 속도 차이</param>
		/// <returns></returns>
		public static double GetSafetyDistance(double speed, double speedDifference)
		{
			// 계산식에 의해 계산 후 반환
			return (0.22796 * speed) + ((2 * speed * speedDifference) / 177.8);
		}

		/// <summary>
		/// 질량차이를 고려한 안전거리
		/// </summary>
		/// <param name="weightAhead">앞 차량의 무게</param>
		/// <param name="weight">측정 차량의 무게</param>
		/// <param name="speed">측정 차량의 속력</param>
		/// <param name="speedDifference">앞 차량과, 측정 차량의 속력 차이</param>
		/// <param name="acceleration">측정 차량의 가속도</param>
		/// <returns></returns>
		public static double GetSafetyDistanceConsiderMassDifference(double weightAhead, double weight, double speed, double speedDifference, double acceleration)
		{
			// 앞 차량의 상대속도 0, 뒷 차량은 속력 차이만큼 접근
			// 위 상황에서 Va
			double velocityAverage = ((weightAhead * 0) + (weight * speedDifference)) / (weightAhead + weight);
			// 질량차이에 의한 충격량
			double valueDifferentWeight = (weight * Math.Pow(speedDifference, 2) - ((weightAhead + weight) * Math.Pow(velocityAverage, 2))) / 2;

			// 충돌에너지가 같아지기 위한 속도 차이 v
			double v = Math.Sqrt(valueDifferentWeight * 4 / weight);

			// 추가로 감속이 필요한 시간
			double time = 0;
			// 현재 속도로 필요한 추가 이동거리
			double distance = 0;

			// 가속중일경우 7.02초로 나눔
			if (acceleration >= 0)
			{
				time = v / 7.02;
				distance = v * time + (7.02 * Math.Pow(time, 2)) / 2;
			}
			else
			{
				time = -v / acceleration;
				distance = v * time + (acceleration * Math.Pow(time, 2)) / 2;
			}

			return distance;
		}

		/// <summary>
		/// SDI 값 산출
		/// </summary>
		/// <param name="followingDistance">앞 차량과의 거리</param>
		/// <param name="speed">속도</param>
		/// <param name="speedDifference">속도 차이</param>
		/// <returns></returns>
		public static int GetSDI(double followingDistance, double speed, double speedDifference)
		{
			/// 앞 차량과의 거리가 0보다 작거나 같은 경우 
			if ( followingDistance <= 0 )
			{
				return 1;
			}

			// 앞 차량의 값
			double aheadValue = (followingDistance / 1000) + (Math.Pow(speedDifference, 2) / 177.8);
			// 뒤 차량의 값
			double followingValue = (0.22796 * speed) + ((2 * speed * speedDifference) / 177.8);
			
			return (aheadValue <= followingValue) ? 1 : 0;
		}
	}
}
