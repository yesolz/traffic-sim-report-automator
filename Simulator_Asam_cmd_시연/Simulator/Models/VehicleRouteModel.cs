using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Simulator.Common;

namespace Simulator.Models
{
	/// <summary>
	/// Vissim - VehicleRoute 생성시에 필요한 데이터
	/// </summary>
	public class VehicleRouteModel
	{
		/// <summary>
		/// 생성자
		/// </summary>
		public VehicleRouteModel()
		{
			VehicleRouteDecisionList = new List<VehicleRouteDecision>();
		}

		/// <summary>
		/// Vehicle Route 설정할 때 필요한 RouteDecision List 개체<br/>
		/// Vissim에서 VehicleRoute 클릭 후 우클릭할 떄, 처음으로 생성되는 개체임
		/// </summary>
		List<VehicleRouteDecision> VehicleRouteDecisionList { get; set; }

		/// <summary>
		/// Route 목록 생성
		/// </summary>
		/// <param name="linkList">Vissim Link 목록</param>
		/// <param name="vissimFromToLinkList">Vissim Link, To Link 목록</param>
		/// <returns></returns>
		public async Task InitRouteList(Dictionary<int, NetworkModel.LinkData> linkList, Dictionary<int, List<int>> vissimFromToLinkList)
		{
			// 시간이 오래 걸리는 작업. View가 응답할 수 있게 Task를 생성하여 실행함
			await Task.Run(() =>
			{
				// Link, To Link 목록의 개체에서 반복함
				foreach ( var item in vissimFromToLinkList )
				{
					// Link에서 To Link로 연결된 수가 1개 이상일 때
					if ( item.Value.Count > 1 )
					{
						// 이전 링크에서 도로가 이어져있는지 확인함.
						int originLink = VissimModels.CalculatorModel.GetOriginalFromLink(item.Key, vissimFromToLinkList);

						// 리턴값이 0일경우, 현재 link가 최초 지점
						// 이외 경우, 리턴된 link가 최초 link
						originLink = originLink == 0 ? item.Key : originLink;

						// decision을 생성할 도로의 총 길이
						double decisionLength = linkList[originLink].Length;
						// 계산 후 지정할 위치
						double decisionPosition = 0;

						// VehicleRouteDecisionsStats 추가 할 때 POS 정보 필요함
						// 대상 링크 길이가 15M 이상이면, 10M지점에 생성
						// 10M 이상, 10M 생성 시 10.2M인 Route가 생성됨.. Vissim 실행 시 너무 짧은 거리로 이동이 불가능한경우가 존재함.
						// 15M 이하일 경우 Length/2 값으로 Position을 지정
						if ( 15 <= decisionLength )
						{
							decisionPosition = 10;
						}
						else
						{
							decisionPosition = decisionLength / 2;
						}

						// 생성 Link 번호, Position값을 저장
						VehicleRouteDecisionList.Add(new VehicleRouteModel.VehicleRouteDecision(originLink, decisionPosition));

						// RouteDecision에서 Route를 지정할 도로를 탐색함
						// from link를 위에서 고정 후, 연결된 ToLink 값으로 연결된 도로지점을 찾음
						foreach ( var routeLinkNumber in item.Value )
						{
							// route를 지정할 link의 총 길이
							double routeLength = linkList[routeLinkNumber].Length;
							// route를 생성할 포지션 설정
							double routePosition = 3;
							// route를 생성해야할 link의 차선 수
							int laneCount = linkList[routeLinkNumber].LaneCount;

							// route 생성 위치보다 linkLength가 짧은 경우가 생김
							// link의 총 길이보다 긴 위치에 생성할 수 없으므로, 도로 총길이 / 2값을 사용
							if ( routeLength < routePosition )
							{
								routePosition = routeLength / 2;
							}

							// Route 추가함
							VehicleRouteDecisionList[VehicleRouteDecisionList.Count - 1].AddVehicleRoute(routeLinkNumber, routePosition, laneCount);
						}

						// 위의 로직에서 생성된 Vehicle Route가 1개일 경우, VehicleRoute 생성이 의미가 없으므로, 제거함
						if ( VehicleRouteDecisionList[VehicleRouteDecisionList.Count - 1].VehicleRoutes.Count <= 1 )
						{
							VehicleRouteDecisionList.RemoveAt(VehicleRouteDecisionList.Count - 1);
						}
					}
				}
			});
		}

		/// <summary>
		/// Vissim에 VehicleRoute를 추가함
		/// </summary>
		/// <returns></returns>
		public async Task AddVissimVehicleRoutes()
		{
			VissimController.SuspendUpdateGUI();

			await Task.Run(() =>
			{
				foreach ( var item in VehicleRouteDecisionList )
				{
					VissimController.AddVehicleRoute(item.Number, item.Position, item.VehicleRoutes);
				}
			});

			VissimController.ResumeUpdateGUI();
		}
		
		/// <summary>
		/// VehicleRouteDecision 개체를 생성하기위해 필요한 정보
		/// </summary>
        public class VehicleRouteDecision
		{
			/// <summary>
			/// VehicleRouteDecision 번호
			/// </summary>
			public int Number { get; init; }
			/// <summary>
			/// VehicleRouteDecision 생성할 Position
			/// </summary>
			public double Position { get; init; }

			/// <summary>
			/// Vehicle Route Decision에서 연결되는 Vehicle Route 목록
			/// </summary>
			public List<VehicleRoute> VehicleRoutes { get; private set; }

			/// <summary>
			/// 생성자
			/// </summary>
			/// <param name="number"></param>
			/// <param name="position"></param>
			public VehicleRouteDecision(int number, double position)
			{
				Number = number;
				Position = position;

				VehicleRoutes = new List<VehicleRoute>();
			}

			/// <summary>
			/// Vehicle Route 목록 추가
			/// </summary>
			/// <param name="number"></param>
			/// <param name="position"></param>
			/// <param name="laneCount"></param>
			public void AddVehicleRoute(int number, double position, int laneCount)
			{
				VehicleRoutes.Add(new VehicleRoute(number, position, laneCount));
			}
		}

		/// <summary>
		/// Vehicle Route 개체를 생성하기위해 필요한 정보
		/// </summary>
		public class VehicleRoute
		{
			/// <summary>
			/// Vehicle Route Link Number
			/// </summary>
			public int Number { get; init; }
			/// <summary>
			/// Vehicle Route Position
			/// </summary>
			public double Position { get; init; }
			/// <summary>
			/// Vehicle Route가 생성되는 도로의 차선 수
			/// </summary>
			public int LaneCount { get; init; }

			/// <summary>
			/// 생성자
			/// </summary>
			/// <param name="number"></param>
			/// <param name="position"></param>
			/// <param name="laneCount"></param>
			public VehicleRoute(int number, double position, int laneCount)
			{
				Number = number;
				Position = position;
				LaneCount = laneCount;
			}
		}
	}
}
