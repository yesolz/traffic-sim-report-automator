using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Diagnostics;

using Simulator.Common;

using VISSIMLIB;

namespace Simulator.Models
{
    /// <summary>
    /// 데이터 측정에 필요한 Node 데이터를 생성
    /// </summary>
    public class NodeModel
    {
        /// <summary>
        /// Node가 겹쳐져서 생성될경우, Vissim Simulation 실행 시 에러 발생, 산출값의 99%만 사용하기 위해 해당 값을 사용함
        /// </summary>
        private const double rate = 0.99;

        /// <summary>
        /// Node 생성 정보, Polygon 형태로 문자열을 생성하기 전, 해당 데이터에 관련 정보를 저장함<br/>
        /// Key: 노드 생성 번호
        /// </summary>
        private Dictionary<int, Node> NodePolygonList { get; set; }
        /// <summary>
        /// Node 생성 지역에 Vehicle tarvel time을 생성함
        /// </summary>
        private List<VehicleTravelTime> VehicleTravelTimeList { get; set; }

        /// <summary>
        /// Node가 생성될 Link 번호 목록
        /// </summary>
        public List<int> NodeLinkList { get; private set; }

        /// <summary>
        /// Junction Id, Sequece로 구성되어있는 노드의 키값 목록
        /// Key: 생성 번호
        /// </summary>
        public Dictionary<int, NodeInfo> NodeInfoList { get; private set; }

        /// <summary>
        /// Node가 생성된 지역에 차량이 통과햇는지 여부를 알기 위해 Signal 정보를 생성함
        /// </summary>
        public Dictionary<(int, int), SimulationResult.EgoSafety.SignalInfo> SignalInfoList { get; private set; }

        /// <summary>
        /// 생성자. 관련 데이터를 저장할 개체를 초기화함
        /// </summary>
        public NodeModel()
        {
            NodePolygonList = new Dictionary<int, Node>();
            VehicleTravelTimeList = new List<VehicleTravelTime>();
            NodeLinkList = new List<int>();
            NodeInfoList = new Dictionary<int, NodeInfo>();
            SignalInfoList = new Dictionary<(int, int), SimulationResult.EgoSafety.SignalInfo>();
        }

        /// <summary>
        /// Delay Measurement 지역을 생성함
        /// </summary>
        /// <returns></returns>
        public async Task AddVissimDelayMesaurements()
        {
            VissimController.SuspendUpdateGUI();

            await Task.Run(() =>
            {
                // DelayMeasurement 기초 데이터 수집
                // TravelTimes를 기반으로 생성됨
                object[] attributes = new object[2] { "No", "Name" };
                object[,] values = VissimController.GetVehicleTravelTimeMeasurementValues(attributes);

                // 생성 데이터 저장 개체
                Dictionary<string, List<int>> delayMeasurementList = new Dictionary<string, List<int>>();

                // Vehicle Travel Time 이름값을 기준으로 관련 개체 정렬
                // 148_1(S), 148_2(L) 형태로 되어있으므로, ( 전 데이터만 사용
                for (int i = 0; i < values.GetLength(0); i++)
                {
                    int number = Convert.ToInt32(values[i, 0]);
                    string name = values[i, 1].ToString()!.Split('(')[0];

                    // 키값이 없을 경우, 키값 추가
                    if (!delayMeasurementList.ContainsKey(name))
                    {
                        delayMeasurementList.Add(name, new List<int>());
                    }

                    // 포함되어있는 vehicle travel time 지역 추가
                    delayMeasurementList[name].Add(number);
                }

                // 필요 데이터 생성 및 추가
                // 입력 형태의 데이터가 148_1, 148_2, 148_3 일 경우
                // "148_1, 148_2, 148_3" 형태로 지정되어야 함
                foreach (var item in delayMeasurementList)
                {
                    string name = item.Key;
                    string travelTimeSequence = "";

                    for (int i = 0; i < item.Value.Count; i++)
                    {
                        // 마지막 데이터인 경우
                        if (item.Value.Count - 1 == i)
                        {
                            travelTimeSequence += item.Value[i].ToString();
                        }
                        else
                        {
                            travelTimeSequence += $"{item.Value[i]}, ";
                        }
                    }

                    VissimController.AddDelayMeasurement(name, travelTimeSequence);
                }

            });

            VissimController.ResumeUpdateGUI();
        }

        /// <summary>
        /// Vehicle Travel Time 개체 추가
        /// </summary>
        /// <param name="linkList">Vissim에서 생성한 Link Data 목록</param>
        /// <returns></returns>
        public async Task AddVissimVehicleTravelTimes(Dictionary<int, NetworkModel.LinkData> linkList)
        {
            VissimController.SuspendUpdateGUI();

            await Task.Run(() =>
            {
                // 생성된 목록 전체 대상
                foreach (var item in VehicleTravelTimeList)
                {
                    int start = item.StartLinkNumber;
                    int end = item.EndLinkNumber;
                    int signal = item.SignalLinkNumber;

                    double startPosition = 0;
                    double endPosition = 0;

                    // 시작도로 길이가 1 이상일 경우, 1M 지점에 생성
                    if (linkList[start].Length >= 1)
                    {
                        startPosition = 1;
                    }
                    // 그 외 경우, Length / 2 지점에 생성
                    else
                    {
                        startPosition = linkList[start].Length / 2;
                    }

                    // 시작도로 길이가 1 이상일 경우, 1M 지점에 생성
                    if (linkList[end].Length >= 1)
                    {
                        endPosition = 1;
                    }
                    // 그 외 경우, Length / 2 지점에 생성
                    else
                    {
                        endPosition = linkList[end].Length / 2;
                    }

                    // 생성 시작 지점: 신호등이 위치한 도로
                    NetworkModel.LinkData signalLinkData = linkList[signal];
                    int signalPolyCount = signalLinkData.PolyPoints.Count;

                    // 생성 종료 지점: 신호등을 통과해 연결되어있는 도로
                    NetworkModel.LinkData endLinkData = linkList[end];
                    int endPolyCount = endLinkData.PolyPoints.Count;

                    // 생성 시작 도로의 각도 계싼
                    double signalAngle = GetAngle(GetRadian(signalLinkData.PolyPoints[signalPolyCount - 2], signalLinkData.PolyPoints[signalPolyCount - 1]));
                    // 생성 종료 도로의 각도 계산
                    double endAngle = GetAngle(GetRadian(endLinkData.PolyPoints[endPolyCount - 2], endLinkData.PolyPoints[endPolyCount - 1]));

                    double modifyAngle = signalAngle * -1;

                    endAngle += modifyAngle;

                    endAngle = endAngle < 0 ? endAngle + 360 : endAngle;

                    endAngle = endAngle % 360;

                    // 명칭 생성
                    string name = item.Name;
                    // 각도 합산 값이 30도 이내일때 직진 도로
                    if (0 <= endAngle && endAngle <= 30 || 330 <= endAngle && endAngle <= 360)
                    {
                        name += "(S)";
                    }
                    // 각도 합산 값이 30도 이상, 180도 이내일 때 좌회전 도로
                    else if (30 < endAngle && endAngle < 180)
                    {
                        name += "(L)";
                    }
                    // 각도 합산 값이 180도 이상, 330도 이내일 때 우회전 도로
                    else if (180 < endAngle && endAngle < 330)
                    {
                        name += "(R)";
                    }
                    // 그 외 경우 180도 이므로, 유턴 차선.. 아마 없을..
                    else
                    {
                        name += "(U)";
                    }

                    // travel time 개체 생성
                    VissimController.AddVehicleTravelTime(start, startPosition, end, endPosition, name);
                }
            });

            VissimController.ResumeUpdateGUI();
        }

        /// <summary>
        /// Vissim에 Node 추가
        /// </summary>
        /// <returns></returns>
        public async Task AddVissimNodes()
        {
            VissimController.SuspendUpdateGUI();

            await Task.Run(() =>
            {
                // 생성된 정보를 기반으로 wktPolygonString 데이터를 생성, Vissim에 추가
                foreach (var node in NodePolygonList)
                {
                    string wktPolygonString = GetWktPolygonString(node.Value.PolygonList);

                    VissimController.AddNode(node.Key, node.Value.NodeName, wktPolygonString);
                }
            });

            VissimController.ResumeUpdateGUI();
        }

        /// <summary>
        /// Polygon 데이터를 Vissim에서 Node 생성시에 필요한 wktPolygonString 형태로 변환함
        /// </summary>
        /// <param name="polygons">Node 구성 정보</param>
        /// <returns></returns>
        private string GetWktPolygonString(List<Polygon> polygons)
        {
            string wktPolygonString = "MULTIPOLYGON(((";
            for (int i = 0; i < polygons.Count; i++)
            {
                wktPolygonString += $"{polygons[i].Xr} {polygons[i].Yr}, ";
            }

            for (int i = polygons.Count - 1; i >= 0; i--)
            {
                wktPolygonString += $"{polygons[i].Xl} {polygons[i].Yl}, ";
            }

            wktPolygonString += $"{polygons[0].Xr} {polygons[0].Yr})))";

            return wktPolygonString;
        }

        /// <summary>
        /// Node 데이터 생성
        /// </summary>
        /// <param name="linkList">Vissim에서 생성한 Link Data 목록</param>
        /// <param name="vissimFromToLinkList">Vissim의 From Link, To Link 목록</param>
        /// <returns></returns>
        public async Task InitNodeList(Dictionary<int, NetworkModel.LinkData> linkList, Dictionary<int, List<int>> vissimFromToLinkList)
        {

            // signal Name 기준으로 데이터 수집
            // int: signal head link
            // string: signal head name
            // double: signal head position
            List<(int, string, double)> signalLinks = VissimController.GetSignalData();

            // 수집한 데이터를 기준으로 데이터 생성 시작
            foreach (var signalLinkData in signalLinks)
            {
                // 사용하기 편한 형태로 변환
                int signalLinkNumber = signalLinkData.Item1;
                string name = signalLinkData.Item2;
                double signalHeadPosition = signalLinkData.Item3;

                // signal 분석에서 사용할 노드 정보 추가
                string[] split = name.Split('_');
                int junctionId = int.Parse(split[0]);
                SignalModel.VissimSignalType sigType = (SignalModel.VissimSignalType)Enum.Parse(typeof(SignalModel.VissimSignalType), split[1]);
                int sequence = int.Parse(split[2]);

                // node name 생성
                name = $"{junctionId}_{sequence}";

                // signal Link 추가
                NodeLinkList.Add(signalLinkNumber);
                NodeInfoList.Add(signalLinkNumber, new NodeInfo(junctionId, sequence));

                List<int> connectorList = new List<int>();
                List<int> toLinkList = new List<int>();

                // connector, to link 추가
                foreach (var linkData in linkList)
                {
                    if (linkData.Value.IsConnector)
                    {
                        // 신호가 존재하는 링크에서 이어지는 connector, to link 데이터 추가
                        if (linkData.Value.FromLink == signalLinkNumber)
                        {
                            int connectorNumber = linkData.Key;
                            int toLinkNumber = linkData.Value.ToLink;

                            NodeLinkList.Add(connectorNumber);
                            NodeLinkList.Add(toLinkNumber);

                            connectorList.Add(connectorNumber);
                            toLinkList.Add(toLinkNumber);

                            NodeInfoList.Add(connectorNumber, new NodeInfo(junctionId, sequence));
                            NodeInfoList.Add(toLinkNumber, new NodeInfo(junctionId, sequence));
                        }
                    }
                }

                // signal info list 추가
                SignalInfoList.Add((junctionId, sequence), new SimulationResult.EgoSafety.SignalInfo(signalLinkNumber, signalHeadPosition, connectorList, toLinkList, sigType, sequence));

                // Node Polygon 정보 ㅊ추가
                NodePolygonList.Add(signalLinkNumber, new Node(name));
                List<Polygon> polygons = new List<Polygon>();

                // 최초 진입로 데이터 생성
                int originLink = VissimModels.CalculatorModel.GetOriginalFromLink(signalLinkNumber, vissimFromToLinkList);

                // 현재 입력한 값이 최초 진입로 일 경우, 현재값, 아닐경우 최초 진입로 데이터로 수정
                originLink = originLink == 0 ? signalLinkNumber : originLink;

                // 연결된 도로가 있을 경우
                if (originLink != signalLinkNumber)
                {
                    // polyPoint를 검색할 링크 목록 생성
                    List<int> connectedLinks = new List<int>();

                    // 조회를 시작할 link 번호 생성
                    int currentLink = originLink;
                    // 조건을 만족할 떄 까지 실행
                    while (true)
                    {
                        // linkList 데이터의 connector 정보 조회
                        foreach (var connector in linkList)
                        {
                            // connector 필터링
                            if (connector.Value.IsConnector)
                            {
                                // 현재 도로가 from link와 일치할 경우
                                if (currentLink == connector.Value.FromLink)
                                {
                                    // 연결된 도로 목록 추가
                                    connectedLinks.Add(connector.Key);
                                    // 데이터 갱신
                                    currentLink = connector.Value.ToLink;
                                    break;
                                }
                            }
                        }

                        // while문 종료 조건
                        // 현재 도로번호가 신호의 도로 번호와 일치할 경우
                        if (currentLink == signalLinkNumber)
                        {
                            break;
                        }
                    }

                    // polyPoint를 모두 연결
                    List<NetworkModel.PolyPoint> polyPoints = new List<NetworkModel.PolyPoint>();
                    List<double> widths = new List<double>();
                    double width = 0;

                    // 연결된 도로 목록에 대한 데이터 생성
                    for (int i = 0; i < connectedLinks.Count; i++)
                    {
                        // connector 정보
                        NetworkModel.LinkData connector = linkList[connectedLinks[i]];
                        // 진입로 정보
                        NetworkModel.LinkData fromLink = linkList[connector.FromLink];

                        // Connector.PolyPoint[0] 전까지만 계산하여 목록에 추가함
                        double limitLength = fromLink.Length - (fromLink.Length - connector.FromPos);
                        int limitIndex = GetLimitPointIndex(fromLink, limitLength);
                        width = fromLink.LaneWidth.Sum();

                        // Connector.PolyPoint[0] 전 까지의 poly point 데이터를 추가함
                        for (int j = 0; j <= limitIndex; j++)
                        {
                            polyPoints.Add(new NetworkModel.PolyPoint(fromLink.PolyPoints[j].X, fromLink.PolyPoints[j].Y));
                            widths.Add(width);
                        }

                        // connector 계산
                        // ToLink.PolyPoint[0] 전까지만 계산하여 목록에 추가함
                        limitLength = connector.Length - connector.ToPos;
                        limitIndex = GetLimitPointIndex(connector, limitLength);
                        width = connector.LaneWidth.Sum();

                        // connector의 0번 polyPoint 전 까지만 계산하여 추가되므로, 0번 데이터는 필수임.
                        for (int j = 0; j <= limitIndex; j++)
                        {
                            polyPoints.Add(new NetworkModel.PolyPoint(connector.PolyPoints[j].X, connector.PolyPoints[j].Y));
                            widths.Add(width);
                        }
                    }

                    width = linkList[signalLinkNumber].LaneWidth.Sum();
                    // 마지막 링크 데이터 추가
                    for (int i = 1; i < linkList[signalLinkNumber].PolyPoints.Count; i++)
                    {
                        polyPoints.Add(new NetworkModel.PolyPoint(linkList[signalLinkNumber].PolyPoints[i].X, linkList[signalLinkNumber].PolyPoints[i].Y));
                        widths.Add(width);
                    }

                    // polygons 데이터 생성
                    polygons = GetPolygonList(polyPoints, widths);

                    // TravelTime 데이터 추가
                    for (int i = 0; i < vissimFromToLinkList[signalLinkNumber].Count; i++)
                    {
                        VehicleTravelTimeList.Add(new VehicleTravelTime(originLink, vissimFromToLinkList[signalLinkNumber][i], signalLinkNumber, name));
                    }
                }
                // 단독으로 생성되어있는 링크의 경우
                else
                {
                    NetworkModel.LinkData linkData = linkList[signalLinkNumber];
                    double laneWidth = linkData.LaneWidth.Sum();

                    polygons = GetPolygonList(linkData.PolyPoints, laneWidth);

                    // TravelTime 데이터 추가
                    for (int i = 0; i < vissimFromToLinkList[signalLinkNumber].Count; i++)
                    {
                        VehicleTravelTimeList.Add(new VehicleTravelTime(originLink, vissimFromToLinkList[signalLinkNumber][i], signalLinkNumber, name));
                    }
                }

                NodePolygonList[signalLinkNumber].PolygonList = polygons;
            }

        }

        /// <summary>
        /// Link, Connector, Link 로 구성된 데이터의 경우에 사용함<br/>
        /// 두개의 Poly Point 값 중, 겹쳐지지 않는 마지막 지점을 반환함
        /// </summary>
        /// <param name="linkData">vissim에서 구성되어있는 link data</param>
        /// <param name="limitLength">겹치지 않는 구간의 길이</param>
        /// <returns></returns>
        private int GetLimitPointIndex(NetworkModel.LinkData linkData, double limitLength)
        {
            double currentLength = 0;
            int limitIndex = 0;
            for (int j = 1; j < linkData.PolyPoints.Count; j++)
            {
                double dx = linkData.PolyPoints[j - 1].X - linkData.PolyPoints[j].X;
                double dy = linkData.PolyPoints[j - 1].Y - linkData.PolyPoints[j].Y;

                currentLength += Math.Sqrt(dx * dx + dy * dy);

                if (limitLength <= currentLength)
                {
                    limitIndex = j - 1;
                    break;
                }
            }

            return limitIndex;
        }

        /// <summary>
        /// Link의 처음 Poly Point 좌표값을 계산
        /// </summary>
        /// <param name="aheadPoly">처음 지점 Poly Point</param>
        /// <param name="afterPoly">처음 지점 뒤에 있는 Poly Point</param>
        /// <param name="laneWidth">도로 폭</param>
        /// <returns>Polygon 데이터</returns>
        private Polygon GetFirstPoint(NetworkModel.PolyPoint aheadPoly, NetworkModel.PolyPoint afterPoly, double laneWidth)
        {
            // 앞 지점의 x, y 좌표
            (double x1, double y1) = (aheadPoly.X, aheadPoly.Y);
            // 뒤 지점의 x, y 좌표
            (double x2, double y2) = (afterPoly.X, afterPoly.Y);

            // 각도 계산
            double polyAngle = GetAngle(GetRadian(x1, y1, x2, y2));

            // 데이터 저장 개체
            Polygon result = new Polygon();

            // 데이터 산출지점 X좌표의 Left 방향 좌표 생성
            result.Xl = x1 + ((laneWidth / 2) * Math.Cos(ConvertAngleToRadian(polyAngle + 90)) * rate);
            // 데이터 산출지점 X좌표의 Right 방향 좌표 생성
            result.Xr = x1 + (laneWidth / 2) * Math.Cos(ConvertAngleToRadian(polyAngle - 90));

            // 데이터 산출지점 Y좌표의 Left 방향 좌표 생성
            result.Yl = y1 + ((laneWidth / 2) * Math.Sin(ConvertAngleToRadian(polyAngle + 90)) * rate);
            // 데이터 산출지점 Y좌표의 Right 방향 좌표 생성
            result.Yr = y1 + (laneWidth / 2) * Math.Sin(ConvertAngleToRadian(polyAngle - 90));

            return result;
        }

        /// <summary>
        /// Link의 마지막 Poly Point 좌표값을 계산
        /// </summary>
        /// <param name="aheadPoly">마지막 지점 앞에 있는 Poly Point</param>
        /// <param name="afterPoly">마지막 지점 Poly Point</param>
        /// <param name="laneWidth">도로 폭</param>
        /// <returns>Polygon 데이터</returns>
        private Polygon GetLastPoint(NetworkModel.PolyPoint aheadPoly, NetworkModel.PolyPoint afterPoly, double laneWidth)
        {
            // 앞 지점의 x, y 좌표
            (double x1, double y1) = (aheadPoly.X, aheadPoly.Y);
            // 뒤 지점의 x, y 좌표
            (double x2, double y2) = (afterPoly.X, afterPoly.Y);

            // 각도 계산
            double polyAngle = GetAngle(GetRadian(x1, y1, x2, y2));

            // 데이터 저장 개체
            Polygon result = new Polygon();

            // 데이터 산출지점 X좌표의 Left 방향 좌표 생성
            result.Xl = x2 + ((laneWidth / 2) * Math.Cos(ConvertAngleToRadian(polyAngle + 90)) * rate);
            // 데이터 산출지점 X좌표의 Right 방향 좌표 생성
            result.Xr = x2 + (laneWidth / 2) * Math.Cos(ConvertAngleToRadian(polyAngle - 90));

            // 데이터 산출지점 Y좌표의 Left 방향 좌표 생성
            result.Yl = y2 + ((laneWidth / 2) * Math.Sin(ConvertAngleToRadian(polyAngle + 90)) * rate);
            // 데이터 산출지점 Y좌표의 Right 방향 좌표 생성
            result.Yr = y2 + (laneWidth / 2) * Math.Sin(ConvertAngleToRadian(polyAngle - 90));

            return result;
        }

        /// <summary>
        /// Polygon 목록을 생성하여 관련 데이터를 반환함<br/>
        /// Vissim의 Link 생성 과정에서, Link가 곡선일 경우, 관련 데이터 산출시에 +, - 되는 데이터가 있음<br/>
        /// 명확하지는 않으나, 관련 데이터와 최대한 비슷한 형태로 구성함
        /// </summary>
        /// <param name="polyPoints">Node를 생성할 Link의 Poly Point 목록</param>
        /// <param name="widths">여러개로 구성되어있는 Link일경우, Poly Point 지점의 Width 폭을 모두 기록하여 사용</param>
        /// <returns></returns>
        private List<Polygon> GetPolygonList(List<NetworkModel.PolyPoint> polyPoints, List<double> widths)
        {
            // 결과값 반환 개체
            List<Polygon> result = new List<Polygon>();

            // Poly Point 값 순서대로 값을 생성
            for (int i = 0; i < polyPoints.Count; i++)
            {
                // Poly Point의 현재 도로 폭 데이터를 지정
                double laneWidth = widths[i];

                // 최초 생성 지점
                if (i == 0)
                {
                    result.Add(GetFirstPoint(polyPoints[i], polyPoints[i + 1], laneWidth));
                }
                // 마지막 생성 지점
                else if (i == polyPoints.Count - 1)
                {
                    result.Add(GetLastPoint(polyPoints[i - 1], polyPoints[i], laneWidth));
                }
                // 그 외 중간에 있는값의 데이터 산출
                else
                {
                    // 데이터 산출지점 앞에 있는 Poly Point 값
                    (double x1, double y1) = (polyPoints[i - 1].X, polyPoints[i - 1].Y);
                    // 데이터 산출지점 Poly Point 값
                    (double x2, double y2) = (polyPoints[i].X, polyPoints[i].Y);
                    // 데이터 산출지점 뒤에 있는 Poly Point 값
                    (double x3, double y3) = (polyPoints[i + 1].X, polyPoints[i + 1].Y);

                    // 데이터 산출지점과, 앞 지점의 각도 계산
                    double beforePointAngle = Math.Round((y2 - y1) / (x2 - x1), 5);
                    // 데이터 산출지점과, 뒤 지점의 각도 계산
                    double afterPointAngle = Math.Round((y3 - y2) / (x3 - x2), 5);

                    // 데이터 산출지점과, 앞 지점의 각도 계산
                    double beforePolyAngle = GetAngle(GetRadian(x1, y1, x2, y2));
                    // 데이터 산출지점과, 뒤 지점의 각도 계산
                    double afterPolyAngle = GetAngle(GetRadian(x2, y2, x3, y3));

                    /*
					double x1l = x1 + ((laneWidth - 0.2) / 2) * Math.Cos(ConvertAngleToRadian(beforePolyAngle + 90));
					double y1l = y1 + ((laneWidth - 0.2) / 2) * Math.Sin(ConvertAngleToRadian(beforePolyAngle + 90));

					double x3l = x3 + ((laneWidth - 0.2) / 2) * Math.Cos(ConvertAngleToRadian(afterPolyAngle + 90));
					double y3l = y3 + ((laneWidth - 0.2) / 2) * Math.Sin(ConvertAngleToRadian(afterPolyAngle + 90));
					*/

                    // 데이터 산출지점 앞에 있는 X좌표의 Left 방향 좌표 생성
                    double x1l = x1 + ((laneWidth / 2) * Math.Cos(ConvertAngleToRadian(beforePolyAngle + 90)) * rate);
                    // 데이터 산출지점 앞에 있는 Y좌표의 Left 방향 좌표 생성
                    double y1l = y1 + ((laneWidth / 2) * Math.Sin(ConvertAngleToRadian(beforePolyAngle + 90)) * rate);

                    // 데이터 산출지점 뒤에 있는 X좌표의 Left 방향 점 좌표 생성
                    double x3l = x3 + ((laneWidth / 2) * Math.Cos(ConvertAngleToRadian(afterPolyAngle + 90)) * rate);
                    // 데이터 산출지점 뒤에 있는 Y좌표의 Left 방향 점 좌표 생성
                    double y3l = y3 + ((laneWidth / 2) * Math.Sin(ConvertAngleToRadian(afterPolyAngle + 90)) * rate);

                    // 데이터 산출지점 앞에 있는 X좌표의 Right 방향 점 좌표 생성
                    double x1r = x1 + (laneWidth / 2) * Math.Cos(ConvertAngleToRadian(beforePolyAngle - 90));
                    // 데이터 산출지점 앞에 있는 Y좌표의 Right 방향 점 좌표 생성
                    double y1r = y1 + (laneWidth / 2) * Math.Sin(ConvertAngleToRadian(beforePolyAngle - 90));

                    // 데이터 산출지점 뒤에 있는 X좌표의 Right 방향 점 좌표 생성
                    double x3r = x3 + (laneWidth / 2) * Math.Cos(ConvertAngleToRadian(afterPolyAngle - 90));
                    // 데이터 산출지점 뒤에 있는 Y좌표의 Right 방향 점 좌표 생성
                    double y3r = y3 + (laneWidth / 2) * Math.Sin(ConvertAngleToRadian(afterPolyAngle - 90));

                    // 데이터 산출 후 저장할 개체 생성
                    (double x, double y) itemL;
                    (double x, double y) itemR;
                    // 두개의 각도가 같지 않을 경우에만 계산
                    // 두개의 각도가 동일할 경우, 평행하며, 다음 점 까지 동일하므로, 다음 점에서 계산 후 추가함
                    if (beforePointAngle != afterPointAngle)
                    {
                        // Left 생성정보
                        itemL = GetPoint(beforePointAngle, afterPointAngle, x1l, y1l, x3l, y3l);
                        // Right 생성정보
                        itemR = GetPoint(beforePointAngle, afterPointAngle, x1r, y1r, x3r, y3r);

                        // 생성 결과 저장
                        result.Add(new Polygon()
                        {
                            Xl = itemL.x,
                            Yl = itemL.y,
                            Xr = itemR.x,
                            Yr = itemR.y,
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Polygon 목록을 생성하여 관련 데이터를 반환함<br/>
        /// /// Vissim의 Link 생성 과정에서, Link가 곡선일 경우, 관련 데이터 산출시에 +, - 되는 데이터가 있음<br/>
        /// 명확하지는 않으나, 관련 데이터와 최대한 비슷한 형태로 구성함
        /// </summary>
        /// <param name="polyPoints">Node를 생성할 Link의 Poly Point 목록</param>
        /// <param name="laneWidth">도로 폭이 일정할 경우</param>
        /// <returns>Polygon을 구성하고있는 데이터를 반환</returns>
        private List<Polygon> GetPolygonList(List<NetworkModel.PolyPoint> polyPoints, double laneWidth)
        {
            // 결과값 반환 개체
            List<Polygon> result = new List<Polygon>();

            // Poly Point 값 순서대로 값을 생성
            for (int i = 0; i < polyPoints.Count; i++)
            {
                // 최초 생성 지점
                if (i == 0)
                {
                    result.Add(GetFirstPoint(polyPoints[i], polyPoints[i + 1], laneWidth));
                }
                // 마지막 생성 지점
                else if (i == polyPoints.Count - 1)
                {
                    result.Add(GetLastPoint(polyPoints[i - 1], polyPoints[i], laneWidth));
                }
                // 그 외 중간에 있는값의 데이터 산출
                else
                {
                    // 데이터 산출지점 앞에 있는 Poly Point 값
                    (double x1, double y1) = (polyPoints[i - 1].X, polyPoints[i - 1].Y);
                    // 데이터 산출지점 Poly Point 값
                    (double x2, double y2) = (polyPoints[i].X, polyPoints[i].Y);
                    // 데이터 산출지점 뒤에 있는 Poly Point 값
                    (double x3, double y3) = (polyPoints[i + 1].X, polyPoints[i + 1].Y);

                    // 데이터 산출지점과, 앞 지점의 각도 계산
                    double beforePointAngle = Math.Round((y2 - y1) / (x2 - x1), 5);
                    // 데이터 산출지점과, 뒤 지점의 각도 계산
                    double afterPointAngle = Math.Round((y3 - y2) / (x3 - x2), 5);

                    // 데이터 산출지점과, 앞 지점의 각도 계산
                    double beforePolyAngle = GetAngle(GetRadian(x1, y1, x2, y2));
                    // 데이터 산출지점과, 뒤 지점의 각도 계산
                    double afterPolyAngle = GetAngle(GetRadian(x2, y2, x3, y3));

                    /*
					double x1l = x1 + ((laneWidth - 0.2) / 2) * Math.Cos(ConvertAngleToRadian(beforePolyAngle + 90));
					double y1l = y1 + ((laneWidth - 0.2) / 2) * Math.Sin(ConvertAngleToRadian(beforePolyAngle + 90));

					double x3l = x3 + ((laneWidth - 0.2) / 2) * Math.Cos(ConvertAngleToRadian(afterPolyAngle + 90));
					double y3l = y3 + ((laneWidth - 0.2) / 2) * Math.Sin(ConvertAngleToRadian(afterPolyAngle + 90));
					*/

                    // 데이터 산출지점 앞에 있는 X좌표의 Left 방향 좌표 생성
                    double x1l = x1 + ((laneWidth / 2) * Math.Cos(ConvertAngleToRadian(beforePolyAngle + 90)) * rate);
                    // 데이터 산출지점 앞에 있는 Y좌표의 Left 방향 좌표 생성
                    double y1l = y1 + ((laneWidth / 2) * Math.Sin(ConvertAngleToRadian(beforePolyAngle + 90)) * rate);

                    // 데이터 산출지점 뒤에 있는 X좌표의 Left 방향 점 좌표 생성
                    double x3l = x3 + ((laneWidth / 2) * Math.Cos(ConvertAngleToRadian(afterPolyAngle + 90)) * rate);
                    // 데이터 산출지점 뒤에 있는 Y좌표의 Left 방향 점 좌표 생성
                    double y3l = y3 + ((laneWidth / 2) * Math.Sin(ConvertAngleToRadian(afterPolyAngle + 90)) * rate);

                    // 데이터 산출지점 앞에 있는 X좌표의 Right 방향 점 좌표 생성
                    double x1r = x1 + (laneWidth / 2) * Math.Cos(ConvertAngleToRadian(beforePolyAngle - 90));
                    // 데이터 산출지점 앞에 있는 Y좌표의 Right 방향 점 좌표 생성
                    double y1r = y1 + (laneWidth / 2) * Math.Sin(ConvertAngleToRadian(beforePolyAngle - 90));

                    // 데이터 산출지점 뒤에 있는 X좌표의 Right 방향 점 좌표 생성
                    double x3r = x3 + (laneWidth / 2) * Math.Cos(ConvertAngleToRadian(afterPolyAngle - 90));
                    // 데이터 산출지점 뒤에 있는 Y좌표의 Right 방향 점 좌표 생성
                    double y3r = y3 + (laneWidth / 2) * Math.Sin(ConvertAngleToRadian(afterPolyAngle - 90));

                    // 데이터 산출 후 저장할 개체 생성
                    (double x, double y) itemL;
                    (double x, double y) itemR;
                    // 두개의 각도가 같지 않을 경우에만 계산
                    // 두개의 각도가 동일할 경우, 평행하며, 다음 점 까지 동일하므로, 다음 점에서 계산 후 추가함
                    if (beforePointAngle != afterPointAngle)
                    {
                        // Left 생성정보
                        itemL = GetPoint(beforePointAngle, afterPointAngle, x1l, y1l, x3l, y3l);
                        // Right 생성정보
                        itemR = GetPoint(beforePointAngle, afterPointAngle, x1r, y1r, x3r, y3r);

                        // 생성 결과 저장
                        result.Add(new Polygon()
                        {
                            Xl = itemL.x,
                            Yl = itemL.y,
                            Xr = itemR.x,
                            Yr = itemR.y,
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 3개의 점 형태로 되어있는 PolyPoint를 대상으로 함<br/>
        /// 정확하게 알 수 있는 지점은 시작 지점, 종료 지점임<br/>
        /// 나머지 중간에 있는 poly point의 좌측, 우측 포인트를 계산시에 사용함<br/>
        /// before: 3개의 Poly Point 중 앞에있는 2개의 데이터<br/>
        /// after: 3개의 Poly Point 중 뒤에있는 2개의 데이터<br/>
        /// ex) 점이 [1, 2, 3]의 형태로 있다고 할 때 before: 1, after: 3, 기준은 2를 기준으로 함
        /// </summary>
        /// <param name="beforeAngle">앞의 두 점이 이루고 있는 각도</param>
        /// <param name="afterAngle">뒤의 두 점이 이루고 있는 각도</param>
        /// <param name="beforeX">앞 X 좌표</param>
        /// <param name="beforeY">앞 Y 좌표</param>
        /// <param name="afterX">뒤 X 좌표</param>
        /// <param name="afterY">뒤 Y 좌표</param>
        /// <returns></returns>
        private (double x, double y) GetPoint(double beforeAngle, double afterAngle, double beforeX, double beforeY, double afterX, double afterY)
        {
            double x, y;

            // 앞, 뒤에 있는 데이터가 평행할 경우
            if (beforeAngle == afterAngle)
            {
                x = 0;
            }
            // X좌표값 계산
            else
            {
                x = ((beforeAngle * beforeX) - (afterAngle * afterX) - beforeY + afterY) / (beforeAngle - afterAngle);
            }
            // Y좌표값 계산
            y = beforeAngle * (x - beforeX) + beforeY;

            return (x, y);
        }

        /// <summary>
        /// Angle 데이터를 Radian 형태로 변환함
        /// </summary>
        /// <param name="angle">Angle</param>
        /// <returns></returns>
        private double ConvertAngleToRadian(double angle)
        {
            return angle * Math.PI / 180;
        }

        /// <summary>
        /// X, Y 좌표로 구성되어있는 데이터의 Radian 값 산출
        /// </summary>
        /// <param name="x1">앞에 있는 X 좌표</param>
        /// <param name="y1">앞에 있는 Y 좌표</param>
        /// <param name="x2">뒤에 있는 X 좌표</param>
        /// <param name="y2">뒤에 있는 Y 좌표</param>
        /// <returns></returns>
        private double GetRadian(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;

            return Math.Atan2(dy, dx);
        }

        /// <summary>
        /// Poly Polint로 구성되어있는 데이터의 Radian 값 산출
        /// </summary>
        /// <param name="ahead">앞에 있는 Poly Point</param>
        /// <param name="after">뒤에 있는 Poly Point</param>
        /// <returns></returns>
        private double GetRadian(NetworkModel.PolyPoint ahead, NetworkModel.PolyPoint after)
        {
            double dx = after.X - ahead.X;
            double dy = after.Y - ahead.Y;

            return Math.Atan2(dy, dx);
        }

        /// <summary>
        /// Radian 데이터를 Angle 형태로 변환함
        /// </summary>
        /// <param name="radian">Radian 값</param>
        /// <returns>Angle</returns>
        private double GetAngle(double radian)
        {
            return radian * 180 / Math.PI;
        }

        /// <summary>
        /// Node 생성시에 필요한 정보 저장
        /// </summary>
        public class Node
        {
            /// <summary>
            /// 노드 명칭, JunctionId_Sequence 형태로 생성
            /// </summary>
            public string NodeName { get; init; }
            /// <summary>
            /// Node Polygon 생성시 필요한 데이터 목록
            /// </summary>
            public List<Polygon> PolygonList { get; set; }

            /// <summary>
            /// 생성자
            /// </summary>
            /// <param name="nodeName">노드 명칭</param>
            public Node(string nodeName)
            {
                NodeName = nodeName;
                PolygonList = new List<Polygon>();
            }
        }

        /// <summary>
        /// 폴리곤 구성시 필요한 정보 저장
        /// </summary>
        public class Polygon
        {
            /// <summary>
            /// Right X 좌표
            /// </summary>
            public double Xr { get; set; }
            /// <summary>
            /// Right Y 좌표
            /// </summary>
            public double Yr { get; set; }
            /// <summary>
            /// Left X 좌표
            /// </summary>
            public double Xl { get; set; }
            /// <summary>
            /// Left Y 좌표
            /// </summary>
            public double Yl { get; set; }
        }

        /// <summary>
        /// Vehicle Travel Time 생성시 필요한 정보 저장
        /// </summary>
        public class VehicleTravelTime
        {
            /// <summary>
            /// 시작지점 Link 번호
            /// </summary>
            public int StartLinkNumber { get; init; }
            /// <summary>
            /// 종료지점 Link 번호
            /// </summary>
            public int EndLinkNumber { get; init; }
            /// <summary>
            /// 포함되어있는 신호 Link 번호
            /// </summary>
            public int SignalLinkNumber { get; init; }
            /// <summary>
            /// 이름, Travel time 측정 지역을 명확하게 표기하기위해 사용
            /// </summary>
            public string Name { get; init; }

            /// <summary>
            /// 생성자
            /// </summary>
            /// <param name="startLinkNumber">시작지점 Link 번호</param>
            /// <param name="endLinkNumber">종료지점 Link 번호</param>
            /// <param name="signalLinkNumber">포함되어있는 신호 Link 번호</param>
            /// <param name="name">이름</param>
            public VehicleTravelTime(int startLinkNumber, int endLinkNumber, int signalLinkNumber, string name)
            {
                StartLinkNumber = startLinkNumber;
                EndLinkNumber = endLinkNumber;
                SignalLinkNumber = signalLinkNumber;
                Name = name;
            }
        }

        /// <summary>
        /// Node Key값 매핑<br/>
        /// Junction Id, 현시 번호 순서로 구성됨
        /// </summary>
        public class NodeInfo
        {
            /// <summary>
            /// Junction ID
            /// </summary>
            public int JunctionId { get; init; }
            /// <summary>
            /// 현시 번호
            /// </summary>
            public int Sequence { get; init; }

            /// <summary>
            /// 생성자
            /// </summary>
            /// <param name="junctionId">Junction Id</param>
            /// <param name="sequence">현시 번호</param>
            public NodeInfo(int junctionId, int sequence)
            {
                JunctionId = junctionId;
                Sequence = sequence;
            }
        }
    }
}
