using Microsoft.Office.Interop.Excel;
using Simulator.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows.Media;

using VISSIMLIB;
using static Simulator.Models.NetworkModel;

namespace Simulator.Models
{
	/// <summary>
	/// Vissim에서 생성된 Link, Road 정보
	/// </summary>
    public class NetworkModel
    {
		/// <summary>
		/// Vissim의 Link 관련 데이터<br/>
		/// Key: Link 번호
		/// </summary>
		public Dictionary<int, LinkData> LinkList { get; set; }

		/// <summary>
		/// OpenDRIVE 기능으로 Vissim에서 생성한 Road 관련 데이터<br/>
		/// Key: Road 번호, Road 방향(Link의 Name에서 확인)
		/// </summary>
		internal Dictionary<(int, RoadDirection), RoadData> RoadList { get; init; }

		/// <summary>
		/// Vissim Network 파일명
		/// </summary>
		public string NetworkFileName { get; set; }

		/// <summary>
		/// From Link, To Link 를 정리한 데이터 목록<br/>
		/// Key: From Link 번호, Value: To Link 번호 목록
		/// </summary>
		internal Dictionary<int, List<int>> vissimFromToLinkList { get; private set; }

		#region LinkList

		/// <summary>
		/// Link 데이터
		/// </summary>
		public struct LinkData
		{
			/// <summary>
			/// 이름
			/// </summary>
			public string Name { get; init; }
			/// <summary>
			/// 길이
			/// </summary>
			public double Length { get; set; }
			/// <summary>
			/// Connector 여부
			/// </summary>
			public bool IsConnector { get; set; }
			/// <summary>
			/// 진입로<br/>
			/// (IsConnector = true인 경우)
			/// </summary>
			public int FromLink { get; set; }
			/// <summary>
			/// 진출로<br/>
			/// (IsConnector = true인 경우)
			/// </summary>
			public int ToLink { get; set; }
			/// <summary>
			/// 진입로에서 시작되는 위치<br/>
			/// (IsConnector = true인 경우)
			/// </summary>
			public double FromPos { get; set; }
			/// <summary>
			/// 진출로에서 시작되는 위치<br/>
			/// (IsConnector = true인 경우)
			/// </summary>
			public double ToPos { get; set; }
			/// <summary>
			/// 차선 수
			/// </summary>
			public int LaneCount { get; set; }
			/// <summary>
			/// 도로 폭, 차선 수 만큼 List 생성
			/// </summary>
			public List<double> LaneWidth { get; set; }
			/// <summary>
			/// Link Poly Point 목록
			/// </summary>
			public List<PolyPoint> PolyPoints { get; set; }
		}

		//public record struct PolyPoint (double X, double Y);
		/// <summary>
		/// Poly Point 구조
		/// </summary>
		public struct PolyPoint
		{
			/// <summary>
			/// 생성자
			/// </summary>
			/// <param name="x">PolyPoint.X</param>
			/// <param name="y">PolyPoint.Y</param>
			public PolyPoint(double x, double y)
			{
				X = x;
				Y = y;
			}

			/// <summary>
			/// PolyPoint의 X좌표
			/// </summary>
			public double X { get; set; }
			/// <summary>
			/// PolyPoint의 Y좌표
			/// </summary>
			public double Y { get; set; }
		}
		#endregion

		#region RoadList

		/// <summary>
		/// Road 방향<br/>
		/// Vissim의 Link.Name 값을 확인하여 해당값으로 사용
		/// </summary>
		public enum RoadDirection
		{
			/// <summary>
			/// Left를 포함하고 있을 경우
			/// </summary>
			Left,
			/// <summary>
			/// Right를 포함하고 있을 경우
			/// </summary>
			Right,
			/// <summary>
			/// 지정되지 않은경우 (OpenDRIVE 파일 구성이 잘못될 경우 name 값에 관련값이 없음)
			/// </summary>
			Default = -1,
		}

		/// <summary>
		/// Road 데이터
		/// </summary>
		public struct RoadData
		{
			/// <summary>
			/// 길이
			/// </summary>
			public double Length { get; set; }
			/// <summary>
			/// Road를 구성하고있는 Link 번호 목록<br/>
			/// Road는 단일 Link 또는 여러개의 Link로 구성됨<br/>
			/// 데이터 추출 후 Position 값 순서로 LinkIndexes를 구성함
			/// </summary>
			public List<int> LinkIndexes { get; set; }
		}
		#endregion

		/// <summary>
		/// 생성자, 파일이름 입력 및 초기화
		/// </summary>
		/// <param name="networkFileName">네트워크 파일 이름</param>
		public NetworkModel(string networkFileName)
		{
			NetworkFileName = networkFileName;
			LinkList = new Dictionary<int, LinkData>();
			RoadList = new Dictionary<(int, RoadDirection), RoadData>();
			vissimFromToLinkList = new Dictionary<int, List<int>>();
		}

		/// <summary>
		/// Link list 초기값 생성
		/// </summary>
		/// <returns></returns>
		public async Task InitLinkList()
		{
			VissimController.SuspendUpdateGUI();
			await Task.Run(() =>
			{
				string filepath = "C:\\Users\\user\\Desktop\\Simulator_Asam_cmd_시연\\" + "linklist.txt";
                //// Vissim에서 가져올 link 목록 지정
                //object[] attributes = new object[9] { "No", "Name", "Length2D", "IsConn", "FromLink", "ToLink", "NumLanes", "FromPos", "ToPos" };
                //// Vissim에 link 데이터 요청, 가져옴
                //object[,] linkDatas = VissimController.GetLinkValues(attributes);

                //// 가져온 데이터의 개수
                //int cnt = linkDatas.GetLength(0);
                //StreamWriter writer;
                //writer = File.CreateText("C:\\Users\\EugeneLee\\Desktop\\Simulator_Asam_cmd\\" + "linklist.txt");
                //writer.WriteLine(cnt);
                //// 가져온 데이터를 모두 저장함
                //for ( int i = 0; i < cnt; i++ )
                //{
                //	// connector 여부
                //	bool isConn = Convert.ToBoolean(linkDatas[i, 3]);
                //	//writer.WriteLine(isConn);
                //	// link 번호
                //	int linkNumber = (int)linkDatas[i, 0];

                //	// 저장할 데이터
                //	int from = 0, to = 0;
                //	double fromPos = 0, toPos = 0;

                //	// connector인 경우에만 관련값이 존재함
                //	if ( isConn )
                //	{
                //		from = Convert.ToInt32(linkDatas[i, 4]);
                //		to = Convert.ToInt32(linkDatas[i, 5]);
                //		fromPos = Convert.ToDouble(linkDatas[i, 7]);
                //		toPos = Convert.ToDouble(linkDatas[i, 8]);
                //	}

                //	// Vissim에서 가져올 lane 목록 지정
                //	attributes = new object[2] { "Index", "Width" };
                //	// Vissim에 lane 데이터 요청, 가져옴
                //	object[,] laneDatas = VissimController.GetLaneValues(linkNumber, attributes);

                //	// 도로 폭 관련정보 저장할 개체
                //	List<double> laneWidth = new List<double>();

                //	// vissim에서 가져온 lane 데이터의수 만큼 관련정보 저장
                //	for ( int j = 0; j < laneDatas.GetLength(0); j++ )
                //	{
                //		// connector의 경우, 도로 폭 관련 데이터가 없음
                //		if ( laneDatas[j, 1] is null )
                //		{
                //			// connector의 데이터에서 from link 번호가 있을 경우
                //			if ( from != 0 )
                //			{
                //				/// * link 목록 요청 시 1 ~ 10000 ~ 순서로 가져옴
                //				/// 순차적으로 처리할 시 link의 데이터를 처리한 후
                //				/// connector의 데이터를 처리하므로 관련 처리에 문제가 없었음
                //				/// 관련 상황에대해 문제가 생기면 해당부분 수정 필요함

                //				// fromlink의 도로 폭 값 사용함
                //				laneWidth.Add(LinkList[from].LaneWidth[j]);
                //			}
                //		}
                //		// connector가 아닐 경우 수집해온 데이터 사용함
                //		else
                //			laneWidth.Add((double)laneDatas[j, 1]);
                //	}
                //                writer.WriteLine("{0}${1}${2}${3}", (int)linkDatas[i, 0], (string)linkDatas[i, 1], (double)linkDatas[i, 2], (int)linkDatas[i, 6]);
                //	string lane_width_str = "[$";
                //	foreach (double lane_w_obj in laneWidth)
                //	{
                //		lane_width_str += lane_w_obj.ToString() +"$";
                //	}
                //	lane_width_str += "]";
                //	writer.WriteLine("{0}",lane_width_str);
                //                writer.WriteLine("{0}${1}${2}${3}${4}",isConn,from,to,fromPos,toPos);
                //                // LinkList 추가
                using (var reader = new StreamReader(filepath, Encoding.UTF8))
                {
					int read_index = 0;
					string result = "";
					List<double> width_list = new List<double>();
                    //파일의 마지막까지 읽어 들였는지를 EndOfStream 속성을 보고 조사
                    while (!reader.EndOfStream)
                    {
                        //ReadLine 메서드로 한 행을 읽어 들여 line 변수에 대입
                        var line = reader.ReadLine();
						//string[] result;
                        if(read_index == 0)
						{
							result += line+"$";
							read_index++;
						}
						else if(read_index == 1)
						{
							string[] list_elem = line.Split("$");
							foreach( string elem in list_elem)
							{
								if(elem!="[" && elem!="]")
									width_list.Add(Double.Parse(elem));
							}
                            read_index++;
                        }
						else
						{
                            result += line + "$";
                            string[] link_result;
							link_result = result.Split("$");
							LinkList.Add(int.Parse(link_result[0]), new LinkData
							{
								Name = link_result[1],
								Length = Double.Parse(link_result[2]),
								LaneCount = int.Parse(link_result[3]),
								LaneWidth = width_list,
								IsConnector = bool.Parse(link_result[4]),
								FromLink = int.Parse(link_result[5]),
								ToLink = int.Parse(link_result[6]),
								FromPos = Double.Parse(link_result[7]),
								ToPos = Double.Parse(link_result[8]),
								PolyPoints = new()
							}); ;
							read_index=0;
							result = "";
							width_list.Clear();
                        }
                    }
                }
     //           LinkList.Add(linkNumber, new LinkData
					//{
					//	Name = (string)linkDatas[i, 1],
					//	Length = (double)linkDatas[i, 2],
					//	LaneCount = (int)linkDatas[i, 6],
					//	LaneWidth = laneWidth,
					//	IsConnector = isConn,
					//	FromLink = from,
					//	ToLink = to,
					//	FromPos = fromPos,
					//	ToPos = toPos,
					//	PolyPoints = new()
					//});
				//}
				//writer.Close();
			});
			
			VissimController.ResumeUpdateGUI();
		}

		/// <summary>
		/// Poly Point 초기값 생성
		/// </summary>
		/// <returns></returns>
		public async Task InitPolyPoints()
		{
			VissimController.SuspendUpdateGUI();
			await Task.Run(() =>
			{
                //// Vissim에서 수집할 Poly Point 값 목록
                //object[] attributes = new object[2] { "X", "Y" };
                //            StreamWriter writer;
                //            writer = File.CreateText("C:\\Users\\EugeneLee\\Desktop\\Simulator_Asam_cmd\\" + "polylist.txt");
                //            // LinkList에 생성되어있는 모든 Link정보에 관련 데이터를 추가함
                //            foreach ( var linkData in LinkList )
                //{
                //	// poly point 값 vissim에서 수집
                //	object[,] polyPoints = VissimController.GetLinkPolyPointValues(linkData.Key, attributes);
                //	string polykey = $"{linkData.Key} ";
                //	string polyvalue = "";
                //	// poly point 개수 확인
                //	int cnt = polyPoints.GetLength(0);

                //	// 개수 만큼 poly point 값 추가함
                //	for ( int i = 0; i < cnt; i++ )
                //	{
                //		polyvalue += $"{polyPoints[i, 0]},{polyPoints[i, 1]} ";
                //		linkData.Value.PolyPoints.Add(new PolyPoint((double)polyPoints[i, 0], (double)polyPoints[i, 1]));
                //	}
                //	writer.WriteLine(polykey + polyvalue);
                //}
                //            writer.Close();
                string filepath = "C:\\Users\\user\\Desktop\\Simulator_Asam_cmd_시연\\" + "polylist.txt";
                using (var reader = new StreamReader(filepath, Encoding.UTF8))
                {
                    List<double> width_list = new List<double>();
                    //파일의 마지막까지 읽어 들였는지를 EndOfStream 속성을 보고 조사
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
						string[] result = line.Split(' ');
						int key = int.Parse(result[0]);	
						for (int i = 0; i < result.Length; i++)
						{
							if(i== 0)
							{
                                continue;
                            }
							else if(i== result.Length - 1)
							{
								break;
							}
                            else
							{
								string[] parsedresult = result[i].Split(",");
								LinkList[i].PolyPoints.Add(new PolyPoint(double.Parse(parsedresult[0]), double.Parse(parsedresult[1])));
                            }
						}
                    }
                }
            });
			VissimController.ResumeUpdateGUI();
		}

		/// <summary>
		/// Road data 초기값 생성
		/// </summary>
		/// <returns></returns>
		public async Task InitRoadData()
		{
			VissimController.SuspendUpdateGUI();
			await Task.Run(() =>
			{
				// vissim에서 가져온 데이터를 필요 형태로 변환하기전에 사용할 임시 목록, 시작 지점에 관련된 데이터
				// Key1: Roadid, RoadDirection, Key2: 시작 Link 번호
				List<((int, RoadDirection), int)> roadInfo = new List<((int, RoadDirection), int)>();

				// 내부에서 사용하기위해 생성하는 데이터
				// Key: RoadId, RoadDirection
				Dictionary<(int, RoadDirection), List<int>> roadLinkList = new Dictionary<(int, RoadDirection), List<int>>();

				// link list 내부 데이터를 모두 확인
				foreach ( var item in LinkList )
				{
					// name 값이 지정되어있으면 관련 데이터를 분석함
					if ( !item.Value.Name.Equals("") )
					{
						// road 방향 정보 초기화
						RoadDirection direction = RoadDirection.Default;

						/// RoadId-0(position으로 추정)-방향정보 형태로 되어있음
						// Road Id 관련정보 파싱
						string[] split = item.Value.Name.Split('-');
						split[0] = split[0].Trim().ToLower();

						string result = "";

						// RoadId 관련 정보를 숫자만 추출하여 result에 저장
						foreach ( char c in split[0] )
						{
							if ( char.IsDigit(c) )
							{
								result += c;
							}
						}

						split[0] = result;

						int roadId = Convert.ToInt32(split[0]);

						/// Split[1] == "0": Road의 시작 지점
						/// split[2] == "Right Start" || split[2] == "Right"): 방향 정보
						if ( split[1] == "0" && (split[2] == "Right Start" || split[2] == "Right") )
						{
							// 방향 지정
							direction = RoadDirection.Right;
							// 시작 지점에 관련된 데이터 목록에 추가
							roadInfo.Add(((roadId, direction), item.Key));

                            // RoadList에 Key값 생성
                            try
                            {
                                roadLinkList.Add((roadId, direction), new List<int>());
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        }
						else if ( split[1] == "0" && (split[2] == "Left Start" || split[2] == "Left") )
						{
							// 방향 지정
							direction = RoadDirection.Left;
							// 시작 지점에 관련된 데이터 목록에 추가
							roadInfo.Add(((roadId, direction), item.Key));

                            // RoadList에 Key값 생성
                            try
                            {
                                roadLinkList.Add((roadId, direction), new List<int>());
                            }
                            catch (Exception e)
                            {
								Console.WriteLine(e);
                            }
                            //roadLinkList.Add((roadId, direction), new List<int>());
						}

						// 위의 if, else if 로직에서 시작지점(split[1] == "0")에 관련된 Direction 값만 지정함
						// 나머지 데이터는 도로 방향 지정이 필요함
						if ( direction == RoadDirection.Default )
						{
							direction = split[2].Contains("Right") ? RoadDirection.Right : RoadDirection.Left;
						}

						// Connector가 아닐 경우, 도로 번호를 추가함
						//if ( !item.Value.IsConnector && split[1].Equals("0") && direction != RoadDirection.Default)
						if ( !item.Value.IsConnector )
							roadLinkList[(roadId, direction)].Add(item.Key);
					}
				}

				// 생성된 도로 목록에서 관련 데이터를 분류함
				foreach ( var info in roadInfo )
				{
					// road에 연관된 도로 번호 목록 생성
					List<int> linkNumbers = new List<int>();
					// road 총 연장
					double totalLength = 0;

					// 도로 번호, 방향 데이터 분리
					(int roadId, RoadDirection direction) roadKey = info.Item1;

					// 시작 링크 번호
					int startLink = info.Item2;

					// Road가 1개의 Link로 구성되었을 경우
					if ( roadLinkList[roadKey].Count == 1 )
					{
						// 생성되어있는 모든 link 정보 목록을 조회함
						foreach ( var item in LinkList )
						{
							// road에 연관된 도로 번호 목록 생성
							List<int> linkIndexes = new List<int>();

							// connector에서만 조회하여 관련값을 추가함
							if ( item.Value.IsConnector )
							{
								// connector의 from link값이 roadId 시작 link값과 동일한경우
								if ( item.Value.FromLink == startLink )
								{
									// from link, connector 순서로 추가
									linkIndexes.Add(item.Value.FromLink);
									linkIndexes.Add(item.Key);

									// road 총 연장값 추가
									totalLength += LinkList[item.Value.FromLink].Length;
									totalLength += LinkList[item.Key].Length;

									// 키가 생성되어있으면
									if ( !RoadList.ContainsKey(roadKey) )
										// 단일 Link로 구성된 Road 값 추가
										RoadList.Add(roadKey, new RoadData() { Length = totalLength, LinkIndexes = linkIndexes });

									// 초기화
									totalLength = 0;
								}
							}
						}
					}
					// Road가 여러개의 Link로 구성되었을 경우
					else
					{
						// 연결된 도로를 정리하기위해 순서값 저장
						List<(int, int)> keyMap = new List<(int, int)>();
						// road에 연관된 도로 번호 목록 생성
						List<int> linkIndexes = new List<int>();

						/// 순차적으로 저장된 번호의 목록을 저장함, 
						/// RoadLinkList 값에 Link 번호가 1, 2, 3 순서로 되어있을 경우 아래와 같은 형태로 저장됨
						/// keyMap[0] = (1, 2), keyMap[1] = (2, 3)
						for ( int i = 0; i < roadLinkList[roadKey].Count - 1; i++ )
							keyMap.Add((roadLinkList[roadKey][i], roadLinkList[roadKey][i + 1]));

						// 위에서 저장한 순차적 데이터를 기준으로 데이터 생성
						for ( int i = 0; i < keyMap.Count; i++ )
						{
							// 모든 Link 목록을 조회하여 관련 데이터 산출
							foreach ( var item in LinkList )
							{
								// Connector의 경우에만 관련 데이터 사용
								if ( item.Value.IsConnector )
								{
									// Connector의 FromLink, ToLink 값과, keyMap[i]의 값이 일치하는 경우
									// ex) 1 - 10000 - 2 으로 link가 구성되었을 경우
									//     (1, 2) == keyMap[0](1, 2)
									if ( (item.Value.FromLink, item.Value.ToLink) == keyMap[i] )
									{
										// 최초 시작된 지점인 경우 From Link값을 같이 저장함
										if ( i == 0 )
										{
											linkIndexes.Add(item.Value.FromLink);
											linkIndexes.Add(item.Key);
											linkIndexes.Add(item.Value.ToLink);

											totalLength += LinkList[item.Value.FromLink].Length;
											totalLength += LinkList[item.Key].Length;
											totalLength += LinkList[item.Value.ToLink].Length;
										}
										// 그 외 경우 Connector, ToLink 값만 저장함
										else
										{
											linkIndexes.Add(item.Key);
											linkIndexes.Add(item.Value.ToLink);

											totalLength += LinkList[item.Key].Length;
											totalLength += LinkList[item.Value.ToLink].Length;
										}
									}
								}
							}
						}

						// 생성된 linkIndexes 값을 저장
						if ( !RoadList.ContainsKey(roadKey) )
							RoadList.Add(roadKey, new RoadData() { Length = totalLength, LinkIndexes = linkIndexes });
					}
				}
			});
			VissimController.ResumeUpdateGUI();
		}

		/// <summary>
		/// From Link, To Link 목록을 저장함
		/// </summary>
		/// <returns></returns>
		public async Task InitVissimFromToLinkData()
		{
			await Task.Run(() =>
			{
				// Link 목록을 모두 조회
				foreach ( var linkData in LinkList )
				{
					// Connector 값만 사용
					if ( linkData.Value.IsConnector )
					{
						// Key값(From Link)이 없을 경우 추가
						if ( !vissimFromToLinkList.ContainsKey(linkData.Value.FromLink) )
						{
							vissimFromToLinkList.Add(linkData.Value.FromLink, new List<int>());
						}

						// Key값(From Link)에 연결되어있는 To Link값을 추가
						vissimFromToLinkList[linkData.Value.FromLink].Add(linkData.Value.ToLink);
					}
				}
			});
		}
	}
}
