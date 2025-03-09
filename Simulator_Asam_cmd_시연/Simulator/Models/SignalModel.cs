using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Simulator.Common;
using Simulator.Models.SimulationResult.EgoSafety;

using static Simulator.Models.NetworkModel;

namespace Simulator.Models
{
	public class SignalModel
	{
		/// <summary>
		/// OpenDRIVE(*.xodr) 파일에서 신호를 제어하는 Controller에 대한 정보.
		/// 필요한 항목만 추출하여 사용함.
		/// </summary>
		internal class XodrSignalControlIernfo
		{
			/// <summary>
			/// [Unique] Controller Id
			/// </summary>
			internal int ControllerId { get; init; }
			/// <summary>
			/// Controller Sequence. Xodr 파일에서 부여받은 기본 순서
			/// </summary>
			internal int SequenceOriginal { get; init; }
			/// <summary>
			/// Xodr 파일에서 3현시 Sequence값이 0, 1, 3 등의 데이터가 존재하여 수정하기위한 데이터
			/// Vissim에서 SC_Type3 -> 0, 1, 2 로 제어하기 때문에 어긋난 현시데이터를 정렬하기 위해 사용
			/// </summary>
			internal int SequenceChanged { get; set; }

			/// <summary>
			/// OpenDRIVE(*.xodr)파일의 Controller 관련 정보를 입력함.
			/// </summary>
			/// <param name="controllerId">Controller.Id</param>
			/// <param name="sequence">Controller.Sequence</param>
			public XodrSignalControlIernfo(int controllerId, int sequence)
			{
				ControllerId = controllerId;
				SequenceOriginal = sequence;
			}
		}

		/// <summary>
		/// OpenDRIVE(*.xodr) 파일에서 신호등에 대한 정보.
		/// 필요한 항목만 추출하여 사용함.
		/// </summary>
		internal class XodrSignalHeadInfo
		{
			/// <summary>
			/// 신호등이 위치한 Road ID. 
			/// Vissim에서 Import OpenDRIVE 기능으로 호출시 없을 때도 있음
			/// 확인 결과: OpenDRIVE(*.xodr) - Road - Lanes - Lane 속성에 driving이라고 명시된 lane의 데이터만 생성함.
			/// </summary>
			public int RoadId { get; init; }
			/// <summary>
			/// Road의 시작 지점에서 얼마나 떨어져 있는지에 대한 벡터.
			/// </summary>
			public double S { get; init; }
			/// <summary>
			/// S 벡터에서 (360도 기준)+90도 방향으로 이루어진 벡터값. 
			/// T >= 0 : 도로를 그린 방향과 동일
			/// T < 0  : 도로를 그린 방향과 반대
			/// </summary>
			public double T { get; init; }
			/// <summary>
			/// 신호등의 위치가 T벡터 기준 +값인지, -값인지 표기
			/// +값: 도로를 그린 방향과 일치
			/// -값: 도로를 그린 방향과 반대
			/// </summary>
			public string Orientation { get; init; }
			/// <summary>
			/// Vissim Link에 매핑할 때 사용되는 정보임.
			/// T >= 0 (Orientation +) 인 경우: Left
			/// T <  0 (Orientation -) 인 경우: Right
			/// Left, Right의 경우 Vissim에서 Link 생성 시 {Road.Name}-0-Left, {Road.Name}-0-Right 등으로 표기함
			/// ex) Road 0-0-Left, Road 1-0-Right Start, Road 1-0-Right End
			/// </summary>
			public string Direction { get; private set; }
			/// <summary>
			/// XodrSignalType.Signal: 원시 데이터이기 때문에 우선순위를 부여함.
			/// XodrSignalType.SignalReference: 참조 데이터
			/// Signal의 RoadId가 Vissim에서 생성하지 않는 Road일 경우
			/// SignalReference를 기반으로 데이터를 생성함.
			/// </summary>
			public XodrSignalType XodrSignalType { get; set; }
			/// <summary>
			/// OpenDRIVE(*.xodr)파일의 신호 관련 정보를 입력함.
			/// </summary>
			/// <param name="roadId">Road.Id</param>
			/// <param name="s">Signal.S</param>
			/// <param name="t">Signal.T</param>
			/// <param name="orientation">Signal.Orientation</param>
			/// <param name="xodrSignalType">Signal or SignalReference</param>
			public XodrSignalHeadInfo(int roadId, double s, double t, string orientation, XodrSignalType xodrSignalType)
			{
				RoadId = roadId;
				S = s; T = t;
				Orientation = orientation;
				Direction = Orientation == "+" ? "Left" : "Right";
				XodrSignalType = xodrSignalType;
			}
		}

		/// <summary>
		/// Vissim에서 사용할 신호등 생성 데이터
		/// </summary>
		internal class VissimSignalHeadInfo
		{
			/// <summary>
			/// [Unique] Signal Id
			/// </summary>
			internal int SignalId { get; init; }
			/// <summary>
			/// 신호가 생성될 Link Number
			/// </summary>
			internal int Link { get; init; }
			/// <summary>
			/// signal lane
			/// </summary>
			internal int Lane { get; set; }
			/// <summary>
			/// 신호가 생성될 Position
			/// </summary>
			internal double Position { get; init; }

			/// <summary>
			/// Vissim Signal 정보 생성
			/// </summary>
			/// <param name="signalId">Signal.No</param>
			/// <param name="link">Signal.Link</param>
			/// <param name="lane">Signal.Lane</param>
			/// <param name="position">Signal.Position</param>
			public VissimSignalHeadInfo(int signalId, int link, double position, int lane = 0)
			{
				SignalId = signalId;
				Link = link;
				Position = position;
				Lane = lane;
			}
		}

		/// <summary>
		/// Vissim에서 제어할 신호 제어 데이터
		/// </summary>
		internal class VissimSignalControllerInfo
		{
			/// <summary>
			/// Key: Sequence, Values: SignalHeadNumbers
			/// </summary>
			internal Dictionary<(int, int), List<int>> SequenceAndSignalHeadNumbers { get; private set; }

			public VissimSignalControllerInfo()
			{
				SequenceAndSignalHeadNumbers = new Dictionary<(int, int), List<int>>();
			}

			/// <summary>
			/// 현시정보와 신호등의 정보를 추가
			/// </summary>
			/// <param name="sequence">현시 번호</param>
			/// <param name="junctionId">Junction ID(*.xodr)</param>
			/// <param name="SignalHeadNumber">신호등 번호</param>
			public void AddSequenceAndSignalHead(int sequence, int junctionId, int SignalHeadNumber)
			{
				// 기존에 가지고있던 목록에 키값이 없을경우 추가함
				if ( !SequenceAndSignalHeadNumbers.ContainsKey((sequence, junctionId)))
				{
					SequenceAndSignalHeadNumbers.Add((sequence, junctionId), new List<int>());
				}

				// 데이터 추가
				SequenceAndSignalHeadNumbers[(sequence, junctionId)].Add((SignalHeadNumber));
			}
		}

		/// <summary>
		/// OpenDrive(*.xodr) 파일에서 존재하는 신호등 관련 데이터 형식
		/// </summary>
		internal enum XodrSignalType
		{
			/// <summary>
			/// 신호등 오리지널 데이터, SignalReference에서 참조함
			/// </summary>
			Signal,
			/// <summary>
			/// 신호등 참조 데이터,
			/// </summary>
			SignalReference,
		}

		/// <summary>
		/// Vissim에서 사용할 현시 키값
		/// </summary>
		public enum VissimSignalType
		{
			/// <summary>
			/// 1현시, 왜 사용되는지 모름.. 주황 점멸 신호로 지정
			/// </summary>
			SCType1 = 1,
			/// <summary>
			/// 2현시
			/// </summary>
			SCType2 = 2,
			/// <summary>
			/// 3현시
			/// </summary>
			SCType3 = 3,
			/// <summary>
			/// 4현시
			/// </summary>
			SCType4 = 4,
		}

		/// <summary>
		/// Vissim에서 사용할 다음 신호 변경
		/// </summary>
		internal enum VissimSignalActionType
		{
			/// <summary>
			/// 신호를 적색으로 변경함.
			/// </summary>
			TurnRed,
			/// <summary>
			/// 신호를 황색으로 변경함.
			/// </summary>
			TurnAmber,
			/// <summary>
			///  신호를 녹색으로 변경함.
			/// </summary>
			TurnGreen,
		}

		/// <summary>
		/// Key: Junction Id
		/// OpenDRIVE(*.xodr)파일에서 Junction에 Controller가 여러개 존재하므로 junction key에 controller를 여러개 생성하여 데이터를 분류함
		/// Junction Id 는 추후 개별 신호의 현시 조정에서 사용 할 수 있으나, 현재는 별다른 의미 없음.
		/// </summary>
		Dictionary<int, List<XodrSignalControlIernfo>> xodrSignalControllerList { get; set; }

		/// <summary>
		/// Key: Signal Id
		/// OpenDRIVE(*.xodr)파일에서 Signal[Unique], SignalReference가 동일한 Signal ID를 사용함.
		/// 같은 Signal Id를 가진 데이터를 종합하여 Vissim에서 합리적인 위치에 생성하는데 사용함.
		/// </summary>
		Dictionary<int, List<XodrSignalHeadInfo>> xodrSignalHeadList { get; set; }

		/// <summary>
		/// Key: Controller Id
		/// OpenDRIVE(*.xodr)파일에서 Controller 정보는 개별 controller의 현시와, controller가 제어하는 SignalId값을 가지고 있음
		/// 관련 정보를 매핑하여 Signal의 현시를 분석하는데 사용함.
		/// </summary>
		Dictionary<int, List<int>> xodrControllerSignalHeadMap { get; set; }

		/// <summary>
		/// 신호등을 제어시 호출할 신호등 정보, Vissim에서 생성할 신호등 정보
		/// </summary>
		internal List<VissimSignalHeadInfo> VissimSignalHeadList { get; private set; }

		/// <summary>
		/// Key: 최대 현시
		/// 신호를 제어할 때 시간, 신호, 다음 대상 Sequence등을 지정하여
		/// 메인 로직에서 호출만 하도록 구성하기 위해 생성함
		/// </summary>
		internal Dictionary<VissimSignalType, VissimSignalControllerInfo> VissimSignalControllerList { get; set; }

		public SignalModel(string networkFileFullName)
		{
			#region Xodr 파싱 데이터

			xodrSignalControllerList = new Dictionary<int, List<XodrSignalControlIernfo>>();
			xodrSignalHeadList = new Dictionary<int, List<XodrSignalHeadInfo>>();
			xodrControllerSignalHeadMap = new Dictionary<int, List<int>>();

			#endregion

			#region Vissim에서 사용할 데이터

			VissimSignalHeadList = new List<VissimSignalHeadInfo>();
			VissimSignalControllerList = new Dictionary<VissimSignalType, VissimSignalControllerInfo>();

			#endregion

			// xodr Data 가져오기
			InitXodrSignalData(networkFileFullName);
			CheckControllerSequence();
		}

		/// <summary>
		/// OpenDRIVE(*.xodr) 파일을 읽어서 신호에 관련된 데이터 파싱함.
		/// </summary>
		/// <param name="networkFileFullName">네트워크 파일 경로 + 파일명 + 파일확장자</param>
		/// <exception cref="Exception">파싱에 실패할 경우 확인하기 위해 에러 생성하도록 함</exception>
		private void InitXodrSignalData(string networkFileFullName)
		{
			XDocument doc = XDocument.Load(networkFileFullName);

			// Xodr 파일을 읽어서 파싱
			foreach ( XElement xodrElement in doc!.Root!.Descendants() )
			{
				#region xodr 신호등 정보

				// xodr 파일에서 신호에 관련된 정보를 파싱함
				if ( "road" == ParseElementNameLower(xodrElement) )
				{
					// <Road> 항목에서 Road.id 를 파싱
					int roadId = -1;
					foreach ( XAttribute roadAttribute in xodrElement.Attributes() )
					{
						(string name, string value) = ParseAttributeLower(roadAttribute);

						switch ( name )
						{
							case "id": roadId = int.Parse(value); break;
						}
					}

					// Road.id가 없을 경우 값 저장 안함.
					if ( roadId != -1 )
					{
						// Road 내부의 Signal 데이터를 검색함
						foreach ( XElement roadElement in xodrElement.Descendants() )
						{
							// <Road> 내부의 Signals 항목
							if ( "signals" == ParseElementNameLower(roadElement) )
							{
								foreach ( XElement signalElement in roadElement.Descendants() )
								{
									XodrSignalType xodrSigType;
									string signalElementName = ParseElementNameLower(signalElement);

									if ( "signal" == signalElementName )
									{
										xodrSigType = XodrSignalType.Signal;
									}
									else if ( "signalreference" == signalElementName )
									{
										xodrSigType = XodrSignalType.SignalReference;
									}
									else // xodrSignalType에 없는 데이터 형식인 경우 처리하기위한 에러 처리
									{
										continue;
									}

									// 신호등 관련정보 파싱
									int signalId = -1;
									double s = double.NaN;
									double t = double.NaN;
									string orientation = string.Empty;

									foreach ( XAttribute signalAttribute in signalElement.Attributes() )
									{
										(string name, string value) = ParseAttributeLower(signalAttribute);

										switch ( name )
										{
											case "id": signalId = int.Parse(value); break;
											case "s": s = double.Parse(value); break;
											case "t": t = double.Parse(value); break;
											case "orientation": orientation = value; break;
										}
									}

									// 구성이 안된 데이터가 있을 시 처리하기 위한 에러 처리
									if ( -1 == signalId || double.NaN == s || double.NaN == t || string.Empty == orientation )
									{
										throw new Exception("InitSignalData - signalAttribute Parsing Error");
									}

									/// xodrSignalHead 데이터 추가
									// 키값이 없는 경우, 키값 추가
									if ( !xodrSignalHeadList.ContainsKey(signalId) )
									{
										xodrSignalHeadList.Add(signalId, new List<XodrSignalHeadInfo>());
									}

									// 데이터 추가
									xodrSignalHeadList[signalId].Add(new XodrSignalHeadInfo(roadId, s, t, orientation, xodrSigType));
								}
							}
						}
					}
				}


				#endregion

				#region xodr controller 정보

				// 현시 관련 junction Data
				if ( "junction" == ParseElementNameLower(xodrElement) )
				{
					int junctionId = -1;

					foreach ( XAttribute junctionAttribute in xodrElement.Attributes() )
					{
						(string name, string value) = ParseAttributeLower(junctionAttribute);

						switch ( name )
						{
							case "id": junctionId = int.Parse(value); break;
						}
					}

					// junctionId 잘못 매핑한경우 에러 처리
					if ( -1 == junctionId )
					{
						throw new Exception("InitSignalData - junctionAttribute Parsing Error");
					}

					// contoller관련 정보 탐색
					foreach ( XElement junctionElement in xodrElement.Descendants() )
					{
						// junction에서 controller항목일 경우
						if ( "controller" == ParseElementNameLower(junctionElement) )
						{
							// controller에서 필요 정보 저장
							int controllerId = -1;
							int seq = -1;

							foreach ( XAttribute controllerAttribute in junctionElement.Attributes() )
							{
								(string name, string value) = ParseAttributeLower(controllerAttribute);

								switch ( name )
								{
									case "id": controllerId = int.Parse(value); break;
									case "sequence": seq = int.Parse(value); break;
								}
							}

							// controller에서 파싱 실패한경우 찾기위한 에러처리
							if ( -1 == controllerId || -1 == seq )
							{
								throw new Exception("InitSignalData - controllerAttribute Parsing Error");
							}

							/// xodrSignalController 데이터 추가
							// 키값이 없는 경우, 키값 추가
							if ( !xodrSignalControllerList.ContainsKey(junctionId) )
							{
								xodrSignalControllerList.Add(junctionId, new List<XodrSignalControlIernfo>());
							}

							// 데이터 추가
							xodrSignalControllerList[junctionId].Add(new XodrSignalControlIernfo(controllerId, seq));
						}
					}
				}

				#endregion

				#region xodr controller, signal 매핑 정보

				// controller, signal 관련 정보
				if ( "controller" == ParseElementNameLower(xodrElement) )
				{
					// controller항목은 <junction> or <OpenDRIVE> 밑에 2개가 있음.
					// 필요한 항목은 OpenDRIVE 밑 controller - control 정보임.
					int controllerId = -1;
					int controllerSequence = -1;

					foreach ( XAttribute controllerAttribute in xodrElement.Attributes() )
					{
						(string name, string value) = ParseAttributeLower(controllerAttribute);
						{
							switch ( name )
							{
								case "id": controllerId = int.Parse(value); break;
								case "sequence": controllerSequence = int.Parse(value); break;
							}
						}
					}

					if ( -1 == controllerId || -1 == controllerSequence )
					{
						throw new Exception("InitSignalData - controllerAttribute Parsing Error");
					}


					foreach ( XElement controllerElement in xodrElement.Descendants() )
					{
						// <controller> - <control>에 controller와 signal 관련 데이터가 있으므로 관련 데이터만 추출함.
						if ( "control" == ParseElementNameLower(controllerElement) )
						{
							int signalId = -1;

							foreach ( XAttribute controlAttribute in controllerElement.Attributes() )
							{
								(string name, string value) = ParseAttributeLower(controlAttribute);

								switch ( name )
								{
									case "signalid": signalId = int.Parse(value); break;
								}
							}

							if ( -1 == signalId )
							{
								throw new Exception("InitSignalData - controlAttribute Parsing Error");
							}

							// mapping 정보 키(controller) 존재하는지 확인
							if ( !xodrControllerSignalHeadMap.ContainsKey(controllerId) )
							{
								xodrControllerSignalHeadMap.Add(controllerId, new List<int>());
							}

							// mapping정보 중복 확인
							if ( !xodrControllerSignalHeadMap[controllerId].Contains(signalId) )
							{
								xodrControllerSignalHeadMap[controllerId].Add(signalId);
							}
						}
					}
				}

				#endregion
			}
		}

		/// <summary>
		/// 3현시의 현시 sequence값이 0, 1, 3 등으로 존재하는 경우가 있어서
		/// 0, 1, 2로 수정함
		/// </summary>
		private void CheckControllerSequence()
		{
			foreach ( int key in xodrSignalControllerList.Keys )
			{
				int count = xodrSignalControllerList[key].Count;

				for ( int i = 0; i < count; i++ )
				{
					if ( i != xodrSignalControllerList[key][i].SequenceOriginal )
					{
						xodrSignalControllerList[key][i].SequenceChanged = i + 1;
					}
					else
					{
						xodrSignalControllerList[key][i].SequenceChanged = xodrSignalControllerList[key][i].SequenceOriginal + 1;
					}
				}
			}
		}

		/// <summary>
		/// OpenDRIVE(*.xodr) 파일데이터를 기반으로 vissim에서 사용할 데이터로 변환함
		/// </summary>
		public async Task InitSignalList(Dictionary<int, LinkData> linkList, Dictionary<(int, RoadDirection), RoadData> roadList)
		{
			await Task.Run(() =>
			{
				// Vissim에서 signalHead 추가 시 1부터 시작.. 신호관련 데이터 저쟁하놓기위해 수동으로 지정함
				int signalHeadNumber = 1;

				foreach ( var xodrController in xodrSignalControllerList )
				{
					Console.WriteLine($"juncction: {xodrController.Key}");
					Console.WriteLine($"현시: {xodrController.Value.Count}");

					foreach ( var controllerItem in xodrController.Value )
					{
						Console.WriteLine($"    현시 순서: {controllerItem.SequenceOriginal}, {controllerItem.SequenceChanged}");

						// controller에서 지정한 signal ID 값을 찾아옴
						foreach ( var signalId in xodrControllerSignalHeadMap[controllerItem.ControllerId] )
						{
							// key: signal id
							// 2nd key: xodrSignalType
							// List 구성: OpenDRIVE(*.xodr)파일에서 모든 signal 데이터가 매핑되는게 아니기때문에, 사용 가능한 정보만 추리기 위해 사용
							// 매핑 안되는 이유:
							//  <road>-<lane> 항목에 type:driving이 아닐 경우 vissim에서 link를 생성 안함
							//  <road>를 한 방향의 lane만 생성한 경우( (t >= 0 [Left]) or (t < 0 [Right]) ) left가 생성되었는데, right로 신호의 t 값을 지정한 경우 or 반대의 경우
							//    - 다른 시뮬레이터에서는 연동이 되는지 모르겠으나.. Vissim에서는 반대편 차선에 대한 위치정보를 찾을 실마리가 보이지 않음..
							Dictionary<int, Dictionary<XodrSignalType, List<VissimSignalHeadInfo>>> signalHeadInfoList = new Dictionary<int, Dictionary<XodrSignalType, List<VissimSignalHeadInfo>>>();

							// link와 position 값을 생성함
							int link = 0;
							double pos = 0;

							// 동일한 signalId로 입력된 데이터(signal, signalReference)를 찾아 유효 데이터를 탐색함
							foreach ( var signalHeadItem in xodrSignalHeadList[signalId] )
							{
								Console.WriteLine($"		roadId: {signalHeadItem.RoadId}, signalId:{signalId}, s:{signalHeadItem.S}, t:{signalHeadItem.T}, orientation:{signalHeadItem.Orientation}, dir:{signalHeadItem.Direction}");

								// direction 키값이 다른 경우가 발생하여 관련 데이터 우선 확인 후 매핑
								NetworkModel.RoadDirection direction = (signalHeadItem.Direction.Equals("Right")) ? NetworkModel.RoadDirection.Right : NetworkModel.RoadDirection.Left;
								// 키값 존재유무 검사
								if ( !roadList.ContainsKey((signalHeadItem.RoadId, direction)) )
								{
									Console.WriteLine($"Network RoadId Not Exist, roadId: {signalHeadItem.RoadId}, Direction: {direction}");
									continue;
								}

								// road 값으로 vissim에서 importDRIVE기능을 통해 생성 된 네트워크 정보에서 관련 데이터 추적
								NetworkModel.RoadData roadData = roadList[(signalHeadItem.RoadId, direction)];

								// Vissim에서 생성된 Road 길이보다, OpenDRIVE(*.xodr)파일의 signal s 데이터가 큰 경우
								// Road.S 값으로 수정함
								double signalSValue = double.NaN;
								if ( roadData.Length < signalHeadItem.S )
								{
									signalSValue = roadData.Length;
								}
								else
								{
									signalSValue = signalHeadItem.S;
								}

								// network 구성 데이터에서 signal이 어느 위치에 존재하는지 계산하여 관련 값 저장
								double roadTotalLength = 0;
								foreach ( var linkIndex in roadData.LinkIndexes )
								{
									// road 시작 지점의 link부터 끝 지점의 link 까지 position 값을 계산함.
									roadTotalLength += linkList[linkIndex].Length;

									// OpenDRIVE(*.xodr)파일의 signal.S 값이 현재 link의 총 길이보다 작은 경우 해당 link에 존재함
									if ( signalSValue <= roadTotalLength )
									{
										int currentIndex = linkIndex;
										link = currentIndex;

										// connector에 signal이 존재하는 경우 fromLink 마지막 position 값으로 사용함
										//  - connector에 있을 경우 해당 connector의 진입을 막고, 나머지는 허용되므로 Link에 생성하도록 함.
										if ( linkList[currentIndex].IsConnector )
										{
											// link값 fromLink로 수정
											link = linkList[currentIndex].FromLink;

											// position값 fromlink의 총 길이 - connector의 길이 제외
											// fromPosition값을 추가하여 해당 값으로 변경될 수 있음
											pos = linkList[link].Length - linkList[currentIndex].Length;

											// 현재 linkIndex값을 수정
											currentIndex = link;
										}
										else // road 총 길이 값에서 현재 확인중인 link의 길이 제외
											 // 그 후 signal의 S값에서 road 시작지점까지의 길이를 제외하여 position값 추정 시 link와 동일하게 함
											 // 
										{
											roadTotalLength -= linkList[currentIndex].Length;
											pos = signalSValue - roadTotalLength;
										}

										// road가 그려질 때
										// [ t >= 0 ] direction: Left (Vissim에서 도로 생성 시), pos -> 도로 종료지점에서 pos값 -> link.Length - pos
										// [ t <  0 ] direction: Right (Vissim에서 도로 생성 시), pos -> 도로 시작지점에서 pos값
										if ( direction == NetworkModel.RoadDirection.Left )
										{
											pos = linkList[currentIndex].Length - pos;
										}
										else
										{
											pos = signalSValue;
										}

										if ( linkList[currentIndex].Length < pos )
										{
											pos = linkList[currentIndex].Length;
										}

										if ( !signalHeadInfoList.ContainsKey(signalId) )
										{
											signalHeadInfoList.Add(signalId, new Dictionary<XodrSignalType, List<VissimSignalHeadInfo>>());
										}

										if ( !signalHeadInfoList[signalId].ContainsKey(signalHeadItem.XodrSignalType) )
										{
											signalHeadInfoList[signalId].Add(signalHeadItem.XodrSignalType, new List<VissimSignalHeadInfo>());
										}

										signalHeadInfoList[signalId][signalHeadItem.XodrSignalType].Add(new VissimSignalHeadInfo(signalId, link, pos));
										break;
									}
								}
							}

							// 현재 링크를 connector에서 tolink로 검색하여
							// 이전 링크가 어디인지 확인함
							Dictionary<int, int> linkCounts = new Dictionary<int, int>();
							foreach ( var signalList in signalHeadInfoList )
							{
								foreach ( var signalItem in signalList.Value )
								{
									foreach ( var subSignalItem in signalItem.Value )
									{
										foreach ( var connectorData in linkList )
										{
											if ( !connectorData.Value.IsConnector )
											{
												continue;
											}

											if ( connectorData.Value.ToLink == subSignalItem.Link )
											{
												if ( !linkCounts.ContainsKey(connectorData.Value.FromLink) )
												{
													linkCounts.Add(connectorData.Value.FromLink, 0);
												}

												linkCounts[connectorData.Value.FromLink]++;
											}
										}
									}
								}
							}

							// 제일 많이 사용된fromLink 값을 검색함
							int max = int.MinValue;
							int key = 0;
							int countSum = 0;
							foreach ( var count in linkCounts )
							{
								countSum += count.Value;
								if ( max < count.Value )
								{
									max = count.Value;
									key = count.Key;
								}
							}

							int xodrSignalLink = 0;

							foreach ( var item in signalHeadInfoList )
							{
								foreach ( var subItem in item.Value )
								{
									if ( XodrSignalType.Signal == subItem.Key )
									{
										xodrSignalLink = subItem.Value[0].Link;
									}
								}
							}

							int createLink = 0;
							int sequence = controllerItem.SequenceChanged;
							double createPosition = 0;

							VissimSignalType sigType;

							switch ( xodrController.Value.Count )
							{
								case 1: sigType = VissimSignalType.SCType1; break;
								case 2: sigType = VissimSignalType.SCType2; break;
								case 3: sigType = VissimSignalType.SCType3; break;
								case 4: sigType = VissimSignalType.SCType4; break;
								default: throw new Exception("CreateVissimSignalData - not suspected type.. SCType_1~4 used");
							}

							/// 1순위 : 제일 많이 사용된 from link값이 절반 이상일 때
							// 올림(countSum / 2) <= max
							if ( Math.Ceiling(countSum / (double)2) <= max )
							{
								createLink = key;
								createPosition = (linkList[key].Length < 3) ? linkList[key].Length : linkList[key].Length - 3;
							}
							/// 2순위
							// key값이 XodrSignalType.Signal 값과 일치할 때
							else if ( linkList.ContainsKey(xodrSignalLink) && key == xodrSignalLink )
							{
								createLink = xodrSignalLink;
								createPosition = (linkList[xodrSignalLink].Length < 3) ? linkList[xodrSignalLink].Length : linkList[xodrSignalLink].Length - 3;
							}
							/// 3순위
							// 중복되는 신호의 link값이 1개라도 있을 경우
							else if ( 2 <= max )
							{
								createLink = key;
								createPosition = (linkList[key].Length < 3) ? linkList[key].Length : linkList[key].Length - 3;
								Console.WriteLine("Warning!! CreateType3");
							}
							/// 4순위
							// XodrSignalType.Signal 값 사용
							else if ( linkList.ContainsKey(xodrSignalLink) )
							{
								createLink = xodrSignalLink;
								createPosition = (linkList[xodrSignalLink].Length < 3) ? linkList[xodrSignalLink].Length : linkList[xodrSignalLink].Length - 3;
								Console.WriteLine("Warning!! CreateType4");
							}
							else // 매핑 불가능
							{
								Console.WriteLine("매핑가능한 신호정보없음");
								continue;
							}

							if ( !VissimSignalControllerList.ContainsKey(sigType) )
							{
								VissimSignalControllerList.Add(sigType, new VissimSignalControllerInfo());
							}

							for ( int j = 1; j <= linkList[key].LaneCount; j++ )
							{
								Console.WriteLine($"SignalHeadNumber: {signalHeadNumber}, Link: {createLink}, Lane: {j}, Position: {createPosition}, Sequence: {controllerItem.SequenceChanged}");
								VissimSignalControllerList[sigType].AddSequenceAndSignalHead(sequence, xodrController.Key, signalHeadNumber);
								VissimSignalHeadList.Add(new VissimSignalHeadInfo(signalHeadNumber++, createLink, createPosition, j));
							}
						}
					}
				}
			});
		}

		/// <summary>
		/// Vissim에 신호등 정보를 입력함
		/// </summary>
		/// <param name="minSequenceTime">최소 현시 유지시간</param>
		/// <returns></returns>
		public async Task AddVissimSignalHeads(double minSequenceTime)
		{
			VissimController.SuspendUpdateGUI();
			await Task.Run( () =>
			{
				foreach ( var signalItem in VissimSignalHeadList )
				{
					VissimController.AddSignalHead(signalItem.SignalId, signalItem.Link, signalItem.Position, signalItem.Lane);
				}

				// 신호 정보 수정
				// SignalController - SignalGroup, IsBlockSignal, SignalHeadName
				foreach ( var signalController in VissimSignalControllerList )
				{
					// SignalController, SignalGroup 설정
					// 신호현시, 최소 4초(적색: 1, 황색: 3, 녹색: 나머지)
					// 임의로 40초로 설정
					double cycle = (int)signalController.Key * minSequenceTime;
					int maxSequence = (int)signalController.Key;

					VissimController.AddSignalController(maxSequence, cycle);

					// Signal Sequence, SignalHeadNumbers
					foreach ( var item in signalController.Value.SequenceAndSignalHeadNumbers )
					{
						int currentSequence = item.Key.Item1;
						int junctionId = item.Key.Item2;
						string signalHeadName = $"{junctionId}_{signalController.Key}_{item.Key.Item1}";

						VissimController.ModifySignalHead(maxSequence, currentSequence, item.Value, signalHeadName);
					}
				}
			});
			VissimController.ResumeUpdateGUI();
		}

		/// <summary>
		/// XML 형태의 데이터에서 Element의 이름을 반환
		/// </summary>
		/// <param name="element">Element 항목</param>
		/// <returns>Element.Name</returns>
		private string ParseElementNameLower(XElement element)
		{
			return element.Name.ToString().ToLower();
		}

		/// <summary>
		/// XML 형태의 데이터에서 Attribute의 이름, 값을 반환
		/// </summary>
		/// <param name="attribute">Attribute 항목</param>
		/// <returns>(Attribute.Name, Attribute.Value)</returns>
		private (string, string) ParseAttributeLower(XAttribute attribute)
		{
			return (attribute.Name.ToString().ToLower(), attribute.Value.ToString());
		}
	}
}
