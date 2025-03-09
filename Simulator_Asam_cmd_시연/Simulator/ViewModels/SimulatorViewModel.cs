using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;

using Simulator.Common;
using Simulator.Models;
using Simulator.Models.AsamModels;
using Simulator.Models.SimulationResult;

using SkiaSharp;

using VISSIMLIB;

namespace Simulator.ViewModels
{
    public partial class SimulatorViewModel : BaseViewModel
    {
        #region CHART

        #region Ego Vehicle

        /// <summary>
        /// 자율주행차량 속도 그래프
        /// </summary>
        [ObservableProperty]
        private ISeries[]? _egoVehicleSpeedSeries;
        /// <summary>
        /// 자율주행차량 가속도 그래프
        /// </summary>
        [ObservableProperty]
        private ISeries[]? _egoVehicleAccelerationSeries;
        /// <summary>
        /// 자율주행차량 앞차량과의 거리 그래프
        /// </summary>
        [ObservableProperty]
        private ISeries[]? _egoVehicleAheadDistanceSeries;
        /// <summary>
        /// 자율주행차량 앞차량과의 TTC 그래프
        /// </summary>
        [ObservableProperty]
        private ISeries[]? _egoVehicleTTCSeries;

        /// <summary>
        /// 자율주행차량 X 좌표
        /// </summary>
        [ObservableProperty]
        private Axis[]? _egoXAxis;
        /// <summary>
        /// 자율주행차량 Y 좌표
        /// </summary>
        [ObservableProperty]
        private Axis[]? _egoYAxis;

        /// <summary>
        /// 자율주행차량 속도 그래프에 표출될 데이터
        /// </summary>
        private ObservableCollection<ObservablePoint>? _egoVehicleSpeedData { get; set; }
        /// <summary>
        /// 자율주행차량 가속도 그래프에 표출될 데이터
        /// </summary>
        private ObservableCollection<ObservablePoint>? _egoVehicleAccelerationData { get; set; }
        /// <summary>
        /// 자율주행차량 앞 차량과의 거리 그래프에 표출될 데이터
        /// </summary>
        private ObservableCollection<ObservablePoint>? _egoVehicleAheadDistanceData { get; set; }
        /// <summary>
        /// 자율주행차량 앞차량과의 TTC 그래프에 표출될 데이터
        /// </summary>
        private ObservableCollection<ObservablePoint>? _egoVehicleTTCData { get; set; }

        #endregion

        /// <summary>
        /// label 색상
        /// </summary>
        [ObservableProperty]
        private SolidColorPaint _labelTextPaint;

        /// <summary>
        /// margin 설정
        /// </summary>
        [ObservableProperty]
        private Margin _margin;

        /// <summary>
        /// 그래프 표출 속성
        /// </summary>
        [ObservableProperty]
        private Visibility _egoGraphVisibility;

        #region Around Vehicle

        /// <summary>
        /// 자율주행차 주변차량 속도 그래프
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ISeries>? _aroundVehicleSpeedSeries;
        /// <summary>
        /// 자율주행차 주변차량 가속도 그래프
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ISeries>? _aroundVehicleAccelerationSeries;
        /// <summary>
        /// 자율주행차 주변차량과 앞차량과의 거리 그래프
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ISeries>? _aroundVehicleAheadDistanceSeries;
        /// <summary>
        /// 자율주행차 주변차량과 앞차량과의 TTC 그래프
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ISeries>? _aroundVehicleTTCSeries;

        /// <summary>
        /// 자율주행차 주변차량 속도 그래프에 표출될 데이터
        /// </summary>
        private ObservableCollection<ObservableCollection<ObservableValue>>? _aroundVehicleSpeedData { get; set; }
        /// <summary>
        /// 자율주행차 주변차량 가속도 그래프에 표출될 데이터
        /// </summary>
        private ObservableCollection<ObservableCollection<ObservableValue>>? _aroundVehicleAccelerationData { get; set; }
        /// <summary>
        /// 자율주행차 주변차량과 앞차량과의 거리 그래프에 표출될 데이터
        /// </summary>
        private ObservableCollection<ObservableCollection<ObservableValue>>? _aroundVehicleAheadDistanceData { get; set; }
        /// <summary>
        /// 자율주행차 주변차량과 앞차량과의 TTC 그래프에 표출될 데이터
        /// </summary>
        private ObservableCollection<ObservableCollection<ObservableValue>>? _aroundVehicleTTCData { get; set; }

        #endregion

        #region Network Vehicle

        /// <summary>
        /// Vissim network 속도 그래프
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ISeries>? _networkVehicleSpeedSeries;
        /// <summary>
        /// vissim network 가속도 그래프
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ISeries>? _networkVehicleAccelerationSeries;
        /// <summary>
        /// vissim network 앞차량과의 거리 그래프
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ISeries>? _networkVehicleAheadDistanceSeries;
        /// <summary>
        /// vissim network 앞차량과의 TTC 그래프
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ISeries>? _networkVehicleTTCSeries;

        /// <summary>
        /// vissim network 속도 그래프에 표출될 데이터
        /// </summary>
        private ObservableCollection<ObservableCollection<ObservableValue>>? _networkVehicleSpeedData { get; set; }
        /// <summary>
        /// Vissim network 가속도 그래프에 표출될 데이터
        /// </summary>
        private ObservableCollection<ObservableCollection<ObservableValue>>? _networkVehicleAccelerationData { get; set; }
        /// <summary>
        /// vissim entwork 앞차량과의 거리 그래프에 표출될 데이터
        /// </summary>
        private ObservableCollection<ObservableCollection<ObservableValue>>? _networkVehicleAheadDistanceData { get; set; }
        /// <summary>
        /// vissim network 앞차량과의 TTC 그래프에 표출될 데이터
        /// </summary>
        private ObservableCollection<ObservableCollection<ObservableValue>>? _networkVehicleTTCData { get; set; }

        #endregion

        #endregion

        #region View Binding

        /// <summary>
        /// 버튼 사용가능 여부
        /// </summary>
        public bool IsButtonEnable
        {
            get { return Shared.IsButtonEnable; }
            set
            {
                if (value != Shared.IsButtonEnable)
                {
                    if (value == true)
                    {
                        SimulationStateColor = (Brush)Application.Current.FindResource("LightGreen");
                    }
                    else
                    {
                        SimulationStateColor = (Brush)Application.Current.FindResource("Label");
                    }

                    Shared.IsButtonEnable = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 화면 하단 시뮬레이션 상태 표출 
        /// </summary>
        private string _simulationState { get; set; }
        /// <summary>
        /// 화면 하단 시뮬레이션 상태 표출 
        /// </summary>
        public string SimulationState
        {
            get { return _simulationState; }
            set
            {
                if (value != _simulationState)
                {
                    _simulationState = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// vissim network에 존재하는 차량 수
        /// </summary>
        private int _networkVehicleCount { get; set; }
        /// <summary>
        /// vissim network에 존재하는 차량 수
        /// </summary>
        public int NetworkVehicleCount
        {
            get { return _networkVehicleCount; }
            set
            {
                if (value != _networkVehicleCount)
                {
                    _networkVehicleCount = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 자율주행차 주변차량의 수
        /// </summary>
        private int _aroundVehicleCount { get; set; }
        /// <summary>
        /// 자율주행차 주변차량의 수
        /// </summary>
        public int AroundVehicleCount
        {
            get { return _aroundVehicleCount; }
            set
            {
                if (value != _aroundVehicleCount)
                {
                    _aroundVehicleCount = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 자율주행차 존재 여부
        /// </summary>
        private bool _egoVehicleExist { get; set; }
        /// <summary>
        /// 자율주행차 존재 여부
        /// </summary>
        public bool EgoVehicleExist
        {
            get { return _egoVehicleExist; }
            set
            {
                if (value != _egoVehicleExist)
                {
                    _egoVehicleExist = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 시뮬레이션 상태 색상
        /// </summary>
        [ObservableProperty]
        public Brush _simulationStateColor;
        /// <summary>
        /// 시뮬레이션 실행버튼 툴팁
        /// </summary>
        public string SimulationButtonToolTip
        {
            get { return Shared.SimulationToolTip; }
        }

        /// <summary>
        /// 실행해야할 시뮬레이션 목록
        /// </summary>
        public Models.SimulationList SimulationItems { get; init; }

        #endregion

        /// <summary>
        /// 시뮬레이션 시간
        /// </summary>
        private double _simulationSecond { get; set; }

        /// <summary>
        /// 실행되고있는 시뮬레이션 정보
        /// </summary>
        private string _simulationInfo { get; set; }

        /// <summary>
        /// vissim network 정보
        /// </summary>
        private Models.NetworkModel Network { get; set; }
        /// <summary>
        /// vissim에서 network 정보 관련하여 데이터를 정리하는 클래스
        /// </summary>
        private Models.VissimModels.NetworkInfoModel? NetworkInfo { get; set; }
        /// <summary>
        /// 시뮬레이션 실행 횟수
        /// </summary>
        private int _simulationCount { get; set; }

        /// <summary>
        /// 신호 현시 최소 시간
        /// </summary>
        private double _signalSequenceMinTime { get; set; } = 40;

        /// <summary>
        /// 시뮬레이션 아이템 번호
        /// </summary>
        private int _selectedIndex { get; set; }
        /// <summary>
        /// 시뮬레이션 아이템 번호
        /// </summary>
        /// 
        private bool _active { get; set; } = false;

        private string _scenario { get; set; } = "cut-in";

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (value != _selectedIndex)
                {
                    _selectedIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public SimulatorViewModel()
        {
            SimulationItems = Shared.SimulationList;

            _selectedIndex = -1;
            _simulationState = "실행 대기";
            _simulationInfo = "";
            _simulationCount = 0;

            Network = new("");
        }

        private static async Task ActivateTasks(List<Task> tasks)
        {

            await Task.WhenAll(tasks.ToArray());

        }

        /// <summary>
        /// 시뮬레이션 실행 버튼
        /// </summary>
        [RelayCommand]
        public async Task ActivateSimulation(string los_string)
        {


            // 실행할 시뮬레이션 번호
            int itemIndex = 8;
            // 선택된 시뮬레이션 번호
            SelectedIndex = itemIndex;
            Console.WriteLine("VissimRunning");
            UpdateSimulationState("Vissim을 실행중입니다.");
            await VissimController.ActivateVissimAsync();

            // 유저가 추가한 시뮬레이션 목록을 실행
            foreach (var simulationItem in Shared.SimulationList)
            {
                UpdateSimulationState("Vissim 시뮬레이션 초기 설정을 진행중입니다.");
                _scenario = simulationItem.ScenarioFileName;
                simulationItem.UseVissimNetwork = true;
                // 화면하단 실행중인 시뮬레이션 파일정보 업데이트
                _simulationInfo = $"{simulationItem.ScenarioFileName}_{simulationItem.NetworkFileName} | ";

                // 수집 데이터를 처리할 NetworkInfo 개체 생성
                NetworkInfo = new Models.VissimModels.NetworkInfoModel(10);

                // 현재 선택된 List 번호 설정
                SelectedIndex = itemIndex;

                // 네트워크 분석, 시뮬레이션 설정을 진행할 Task 개체 생성
                Task initNetworkData;
                Task initSimulationSetting;

                List<Task> tasks = new List<Task>();

                UpdateSimulationState("네트워크 초기 정보를 분석중입니다. 시간이 오래 걸릴 수 있습니다.");

                // Vissim Network 정보 갱신
                if (true)//!Network.NetworkFileName.Equals(simulationItem.NetworkFileName))
                {
                    // Vissim Network 사용 체크되어있을 경우 vissim파일 로드
                    if (simulationItem.UseVissimNetwork)
                    {
                        await VissimController.LoadNetworkAsync(Shared.VissimFileDirectory + $"{simulationItem.NetworkFileName}");
                    }
                    // 그외 경우 ImportDRIVE기능으로 xodr 파일 로드
                    else
                    {
                        await VissimController.ImportOpenDriveAsync(simulationItem.NetworkFileFullName);
                    }

                    // vissim network 정보 개체 생성
                    Network = new NetworkModel(simulationItem.NetworkFileName);
                    initNetworkData = Task.Run(async () =>
                    {
                        //최적화 보기
                        await Network.InitLinkList();//최적화 보기
                        Console.WriteLine("InitLinkList done");
                        await Network.InitPolyPoints();//최적화 보기
                        Console.WriteLine("InitPolyPoints done");
                        await Network.InitRoadData();
                        Console.WriteLine("InitRoadData done");
                        await Network.InitVissimFromToLinkData();
                        Console.WriteLine("InitVissimFromToLinkData done");

                        // OpenDRIVE사용으로 되어있을 경우, network 파일에 필요한 데이터들을 추가함
                        if (!simulationItem.UseVissimNetwork)
                        {
                            await VissimController.SetDefaultVehicleColorSettingAsync();

                            UpdateSimulationState("신호 데이터를 분석중입니다.");
                            SignalModel signal = new SignalModel(simulationItem.NetworkFileFullName);
                            await signal.InitSignalList(Network.LinkList, Network.RoadList);
                            UpdateSimulationState("신호를 생성중입니다.");
                            await signal.AddVissimSignalHeads(_signalSequenceMinTime);
                            Console.WriteLine("signal Initiation done");

                            UpdateSimulationState("경로 데이터를 분석중입니다.");
                            VehicleRouteModel vehicleRoute = new VehicleRouteModel();
                            await vehicleRoute.InitRouteList(Network.LinkList, Network.vissimFromToLinkList);
                            UpdateSimulationState("경로를 생성중입니다.");
                            await vehicleRoute.AddVissimVehicleRoutes();
                            Console.WriteLine("route Initiation done");

                            UpdateSimulationState("Conflict Area를 생성중입니다.");
                            await VissimController.ModifyConflictAreaAsync(Network.LinkList);
                            Console.WriteLine("conflict area Initiation done");

                            UpdateSimulationState("교차로 데이터를 분석중입니다.");
                            NodeModel node = new NodeModel();
                            await node.InitNodeList(Network.LinkList, Network.vissimFromToLinkList);
                            UpdateSimulationState("Node를 생성중입니다.");
                            await node.AddVissimNodes();
                            UpdateSimulationState("Travel Time 측정지역을 생성중입니다.");
                            await node.AddVissimVehicleTravelTimes(Network.LinkList);
                            UpdateSimulationState("Delay 측정지역을 생성중입니다.");
                            await node.AddVissimDelayMesaurements();
                            Console.WriteLine("measures Initiation done");

                            Console.WriteLine("Saving temporary");
                            // vissim 설정 완료 후 시뮬레이션 임시 파일 저장, OpenDrive로 열린 파일의 경우, 저장하지 않으면 진행이 안됨.
                            VissimController.SaveVissimFileTemp();


                            // vissim record 수정
                            VissimController.AddVehicleRecordAttribute(VissimController.VissimNetworkTempPath + VissimController.VissimNetworkTempFileName + ".inpx");
                            Console.WriteLine("recording Initiation done");
                            // Conflict Area, Vehicle Route 안보이게 수정
                            VissimController.ModifyVissimLayoutFile(VissimController.VissimNetworkTempPath + VissimController.VissimNetworkTempFileName + ".layx");

                            // vissim network 다시 로드
                            await VissimController.LoadNetworkAsync(VissimController.VissimNetworkTempPath + VissimController.VissimNetworkTempFileName);
                            Console.WriteLine("VissimLoaded");
                        }
                        await VissimController.SetDefaultVehicleColorSettingAsync();
                    });

                    tasks.Add(initNetworkData);
                }

                // Vissim Simulation 설정
                initSimulationSetting = Task.Run(async () =>
                {
                    await VissimController.SetSimulationSettingAsync(simulationItem.RandomSeed, simulationItem.Resolution, simulationItem.BreakAt, 100);
                    if (simulationItem.WriteRawFiles)
                    {
                        await VissimController.InitSimulationSettingAsync(simulationItem.BreakAt, 100);
                    }
                });
                tasks.Add(initSimulationSetting);

                await Task.WhenAll(tasks.ToArray());
                Console.WriteLine("initialization task done");

                // OpenSCENARIO 차량 초기정보 Vissim 차량 데이터로 변경
                List<Models.VehicleVissimSpawnData> scenarioVehicleVissimData = Models.VissimModels.CalculatorModel.ParseVehicleData(simulationItem.VehicleList, Network.LinkList, Network.RoadList);

                Console.WriteLine("OpenSCENARIO 차량 정보를 Vissim에 입력중입니다.");
                // Vissim 차량 데이터 초기정보 입력
                foreach (var vehicleData in scenarioVehicleVissimData)
                {
                    if (Shared.ScenarioIndex ==5 || Shared.ScenarioIndex == 6|| Shared.ScenarioIndex == 7)
                    {
                        Models.VehicleVissimSpawnData temp = (Models.VehicleVissimSpawnData)vehicleData;
                        temp.LinkNo = 1528;
                        temp.Position = 0;
                        temp.LaneNo = vehicleData.LaneNo == -1 ? 2 : vehicleData.LaneNo;
                        await VissimController.InitScenarioVehicleAsync(temp);
                    }
                    else
                        await VissimController.InitScenarioVehicleAsync(vehicleData);
                }
                Console.WriteLine("scenario veh loaded");


                List<bool> losList = new List<bool>()
                {
                    true,
                    false, false, false, false, false
                };


                // 실행해야할 LOS 정보 획득
                foreach (string los_str_val in los_string.Split(","))
                {
                    if (los_str_val == "A") losList[0] = true;
                    else if (los_str_val == "B") losList[1] = true;
                    else if (los_str_val == "C") losList[2] = true;
                    else if (los_str_val == "D") losList[3] = true;
                    else if (los_str_val == "E") losList[4] = true;
                    else if (los_str_val == "F") losList[5] = true;
                }

                int losIndex = 0;
                foreach (var los in losList)
                {
                    string losName;
                    int losValue = 0;

                    switch (losIndex)
                    {
                        case 0: losName = "LosA"; losValue = 1; break;
                        case 1: losName = "LosB"; losValue = 2; break;
                        case 2: losName = "LosC"; losValue = 3; break;
                        case 3: losName = "LosD"; losValue = 4; break;
                        case 4: losName = "LosE"; losValue = 5; break;
                        case 5: losName = "LosF"; losValue = 6; break;
                        default: losName = ""; losValue = 0; break;
                    }

                    // LOS 설정값에서 true로 설정되어 있을 경우, 분석 시작
                    if (los.Equals(true))
                    {
                        // 시뮬레이션 데이터를 분류할 개체 생성
                        NetworkInfo = new Models.VissimModels.NetworkInfoModel(simulationItem.Resolution);
                        // node 생성
                        NodeModel node = new NodeModel();
                        // node 데이터 생성
                        Console.WriteLine("Node Initiation");
                        await node.InitNodeList(Network.LinkList, Network.vissimFromToLinkList);
                        Console.WriteLine("Node Loaded");
                        // node 데이터 입력
                        NetworkInfo.SetNodeData(node.NodeLinkList, node.NodeInfoList, node.SignalInfoList);

                        // 시뮬레이션 하단 표출 데이터에 파일명 + LOS 정보 입력
                        _simulationInfo = $"{simulationItem.ScenarioFileName}_{simulationItem.NetworkFileName}, {losName} | ";

                        UpdateSimulationState("Vissim에 차량을 생성중입니다.");
                        // 임시 파일 저장 후 시뮬레이션 1회 진행
                        // 차량을 특정 Link에 생성하는 기능은 시뮬레이션 도중에만 가능함.
                        VissimController.SimulationRunSingleStep();
                        VissimController.SetRecord();

                        // 기본 차량 생성
                        List<Models.VehicleVissimSpawnData> normalVehicleSpawnDataList = new List<VehicleVissimSpawnData>();

                        // 차량 생성 데이터 생성
                        foreach (var link in Network.LinkList)
                        {
                            // Connector 추가 안함
                            if (link.Value.IsConnector)
                            {
                                continue;
                            }

                            // 50M 이상인 도로만 대상으로 추가함
                            if (link.Value.Length >= 50)
                            {
                                // 도로에서 생성할 차량의 최초 position
                                double currentPosition = 0;
                                // Los값 별로 생성 위치를 다르게함
                                double positionAdded = 240 / losValue;

                                // 최초값 0: 생성, 이후 currentPosition + positionAdded 값이 입력 가능할경우 차량을 추가로 입력
                                while (currentPosition < link.Value.Length)
                                {
                                    // 1차선 ~ 최대 차선 수 까지 차량을 추가함
                                    for (int lane = 1; lane <= link.Value.LaneCount; lane++)
                                    {
                                        normalVehicleSpawnDataList.Add(new VehicleVissimSpawnData()
                                        {
                                            VehicleType = 100,
                                            LinkNo = link.Key,
                                            LaneNo = lane,
                                            Position = currentPosition,
                                            // TODO: Desired Speed 관련하여 어떻게 추가할 지 협의 필요함..
                                            DesiredSpeed = 50,
                                        });
                                    }

                                    currentPosition += positionAdded;
                                }
                            }
                        }
                        Console.WriteLine("Adding Normal Veh");
                        // 차량 데이터 추가
                        VissimController.SuspendUpdateGUI();
                        for (int i = 0; i < normalVehicleSpawnDataList.Count; i++)
                        {
                            int type = normalVehicleSpawnDataList[i].VehicleType;
                            int linkNumber = normalVehicleSpawnDataList[i].LinkNo;
                            int laneNumber = normalVehicleSpawnDataList[i].LaneNo;
                            double position = normalVehicleSpawnDataList[i].Position;
                            double desiredSpeed = normalVehicleSpawnDataList[i].DesiredSpeed;

                            await VissimController.AddVehicle(type, linkNumber, laneNumber, position, desiredSpeed);
                        }
                        Console.WriteLine("Normal Veh Added");
                        VissimController.ResumeUpdateGUI();
                        Console.WriteLine("Warmup for" + simulationItem.BreakAt + "s Started");
                        // Break At 존재시
                        if (simulationItem.BreakAt > 0)
                        {
                            UpdateSimulationState($"{simulationItem.BreakAt}초 까지 시뮬레이션 워밍업을 진행 합니다.");
                            await VissimController.SimulationWarmUpAsync(simulationItem.BreakAt);
                        }
                        Console.WriteLine("Warmup ended");


                        Console.WriteLine("Adding Scenario Veh");
                        // xosc 파일에 추가되어있는 시나리오 차량 추가
                        foreach (var vehicleData in scenarioVehicleVissimData)
                        {
                            await VissimController.AddScenarioVehicleAsync(vehicleData, 50);
                            await VissimController.setBoundingBox(vehicleData);
                        }
                        Console.WriteLine("Scenario Veh Added");
                        UpdateSimulationState($"{simulationItem.Period}초 까지 시뮬레이션 데이터를 수집합니다.");

                        // 차트 생성
                        InitChart();
                        EgoGraphVisibility = Visibility.Visible;

                        await Simulate(simulationItem);

                        // 시뮬레이션 중단, 측정완료
                        VissimController.SimulationStop();

                        UpdateSimulationState("Simulation 결과를 저장중입니다.");
                        SaveResult(simulationItem, losName);
                        
                    }

                    losIndex++;
                }

                InitChart();
                EgoGraphVisibility = Visibility.Hidden;
                itemIndex++;
            }

            SelectedIndex = -1;
            _simulationInfo = "";

            //UpdateSimulationState("시뮬레이션이 실행 완료되었습니다.");
            //IsButtonEnable = true;
            VissimController.DeactivateVissim();
            
        }


        public void jsonconvert(string python, string filename, string tgr)
        {
            try
            {  
                string pythonPath = "";
                if (python.Length <= 0)
                    pythonPath = @"C:\Users\EugeneLee\AppData\Local\Programs\Python\Python310\python.exe";
                else
                    pythonPath = python;

                string arguments = "";
                if (filename.Length <= 0)
                    arguments = @"C:\Users\EugeneLee\Documents\Simulator\Resources\vissim\report_psi\chart-server\server\service\jsonconverter.py";
                else
                    arguments = filename;

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = pythonPath,
                    Arguments = $"{arguments} {Shared.ResultFileName} {tgr} {_scenario}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false
                };
                using (Process process = Process.Start(startInfo))
                {
                    // 표준 출력 및 오류를 읽음
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    Console.WriteLine("Python Output: " + output);
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("Python Error: " + error);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Python 스크립트를 실행하는 중 오류가 발생했습니다: " + ex.Message);
            }
        }

        /// <summary>
        /// 시뮬레이션 하단 상태정보 업데이트
        /// </summary>
        /// <param name="message">업데이트할 메시지</param>
        private void UpdateSimulationState(string message)
        {
            SimulationState = _simulationInfo + message;
        }

        /// <summary>
        /// 설정완료 후 시뮬레이션 진행
        /// </summary>
        /// <param name="simulationItem">진행할 시뮬레이셔네 대한 정보</param>
        /// <returns></returns>
        private async Task Simulate(Models.SimulationModel simulationItem)
        {
            // 차량 데이터에서 수집해야 하는 항목 지정
            object[] vehicleAttributes = new object[] { "No", "VehType", "FollowDistNet", "Speed", "Acceleration", "CoordFront", "CoordRear", "Lane", "Pos", "DesSpeed", "DestLane", "SpeedDiff", "Weight", "LeadTargNo", "LeadTargType", "DesLane" };
            // Vissim에서 수집한 데이터
            object[,] vehicleData = new object[0, 0];
            // 시뮬레이션 종료 시간
            _simulationSecond = VissimController.GetSimulationTime();
            Dictionary<string, int> typedict = VissimController.GetVehicleTypeValues();
            int scenario_index = Shared.ScenarioIndex;
            int action_count = 0;
            do
            {

                vehicleData = VissimController.GetVehicleValues(vehicleAttributes);

                NetworkInfo!.AnalyzeVissimData(ConvertVehicleData(vehicleData), _simulationSecond);

                NetworkVehicleCount = (int)vehicleData.GetLength(0);
                AroundVehicleCount = NetworkInfo.AroundVehicleCount;
                EgoVehicleExist = NetworkInfo.EgoVehicleExist;

                action_count = ScenarioAction(vehicleData, scenario_index, typedict, action_count);

                //실사용시
                //UpdateChart();

                // 테스트 환경 그래픽카드 X, 10회에 한번만 업데이트 하도록 함
                if (_simulationCount % 10 == 0 && _simulationCount != 0)
                {
                    _simulationCount = 0;
                    UpdateChart();
                }

                SyncronizeSimulation();

                if (action_count == -1)
                {
                    break;
                }
            }
            while (_simulationSecond <= 100);
        }

        /// <summary>
        /// Vissim에서 가져온 데이터를 사용하기 편한 형태로 재구성함
        /// </summary>
        /// <param name="vissimVehicleData">vissim에서 수집한 데이터</param>
        /// <returns></returns>
        private Dictionary<int, Models.VehicleData> ConvertVehicleData(object[,] vissimVehicleData)
        {
            // 가져온 차량 데이터 수 확인
            int vehicleCount = vissimVehicleData.GetLength(0);
            // 새로운 데이터로 저장할 개체 생성
            Dictionary<int, Models.VehicleData> vehicleData = new Dictionary<int, Models.VehicleData>();

            // 모든차량 정보 수정
            for (int i = 0; i < vehicleCount; i++)
            {
                // link, lane 값 변환
                string[] linkLane = vissimVehicleData[i, (int)Models.VehicleAttribute.Lane].ToString()!.Split('-');
                int link = Convert.ToInt32(linkLane[0]);
                int lane = Convert.ToInt32(linkLane[1]);
                // 목표 차선 변환
                object destinationLane = (object)vissimVehicleData[i, (int)Models.VehicleAttribute.DestinationLane];
                // 센서검지 번호 변환
                object leadTargetNumber = (object)vissimVehicleData[i, (int)Models.VehicleAttribute.LeadTargetNumber];
                // 센서검지타입 변환
                object leadTargetType = (object)vissimVehicleData[i, (int)Models.VehicleAttribute.LeadTargetType];
                // 차량 타입 변환
                int vehType = Convert.ToInt32(vissimVehicleData[i, (int)Models.VehicleAttribute.VehicleType]);
                // 차량 번호 변환
                int vehNumber = (int)vissimVehicleData[i, (int)Models.VehicleAttribute.Number];

                // 자율주행차량일경우 분석할 데이터의 키가 없으면, 관련 키값을 생성함
                if (vehType == (int)Models.VehicleType.Ego)
                {
                    // 분석 로직에 키값 추가
                    if (!NetworkInfo!.EgoSpeed.ContainsKey(vehNumber))
                    {
                        NetworkInfo.EgoSpeed.Add(vehNumber, new Models.SimulationResult.EgoSafety.Speed());
                        NetworkInfo.EgoSpeedRaw.Add(vehNumber, new List<Models.SimulationResult.EgoSafety.SpeedRaw>());
                    }
                    // 분석 로직에 키값 추가
                    if (!NetworkInfo.EgoDistance.ContainsKey(vehNumber))
                    {
                        NetworkInfo.EgoDistance.Add(vehNumber, new Models.SimulationResult.EgoSafety.Distance());
                        NetworkInfo.EgoDistanceRaw.Add(vehNumber, new List<Models.SimulationResult.EgoSafety.DistanceRaw>());
                    }
                    // 분석 로직에 키값 추가
                    if (!NetworkInfo.EgoSignal.ContainsKey(vehNumber))
                    {
                        NetworkInfo.EgoSignal.Add(vehNumber, new Models.SimulationResult.EgoSafety.Signal());
                        NetworkInfo.EgoSignalRaw.Add(vehNumber, new List<Models.SimulationResult.EgoSafety.SignalRaw>());
                    }
                }

                // 반환할 데이터에 데이터 추가
                vehicleData.Add(vehNumber, new Models.VehicleData()
                {
                    VehicleType = vehType,
                    FollowDistance = (double)vissimVehicleData[i, (int)Models.VehicleAttribute.Headway],
                    Speed = (double)vissimVehicleData[i, (int)Models.VehicleAttribute.Speed],
                    Acceleration = (double)vissimVehicleData[i, (int)Models.VehicleAttribute.Acceleration],
                    CoordFront = (string)vissimVehicleData[i, (int)Models.VehicleAttribute.CoordFront],
                    CoordRear = (string)vissimVehicleData[i, (int)Models.VehicleAttribute.CoordRear],
                    Link = link,
                    Lane = lane,
                    Position = (double)vissimVehicleData[i, (int)Models.VehicleAttribute.Position],
                    DesiredSpeed = (double)vissimVehicleData[i, (int)Models.VehicleAttribute.DesiredSpeed],
                    DestinationLane = (destinationLane is null) ? 0 : (int)destinationLane,
                    SpeedDifference = (double)vissimVehicleData[i, (int)Models.VehicleAttribute.SpeedDifference],
                    Weight = (double)vissimVehicleData[i, (int)Models.VehicleAttribute.Weight],
                    LeadTargetNumber = leadTargetNumber is null ? 0 : (int)leadTargetNumber,
                    LeadTargetType = leadTargetType is null ? string.Empty : (string)leadTargetType
                });
            }

            return vehicleData;
        }

        /// <summary>
        /// Story 구성시 관련 행위 지정
        /// </summary>
        private int ScenarioAction(object[,] vehicles, int index, Dictionary<string, int> typedict, int action_count)
        {
            if (index == 0)
            {
                int ego_num = -1;
                if (action_count == 0)
                {
                    int veh_found = 0;
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["NPC_1"])
                        {
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "Speed", 50);
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesSpeed", 50);
                            veh_found += 1;
                            _active = true;
                        }
                        else if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesSpeed", 75);
                            veh_found += 1;
                        }

                        if (veh_found == 2)
                        {
                            action_count = 1;
                            break;
                        }
                    }
                }
                else if (action_count == 1)
                {
                    double ego_pos = 0;
                    double npc_pos = 0;
                    int npc_num = 0;
                    int veh_found = 0;
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["NPC_1"])
                        {
                            npc_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            npc_pos = (double)vehicles[i, Convert.ToInt32(VehicleAttribute.Position)];
                            veh_found += 1;
                        }
                        else if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_pos = (double)vehicles[i, Convert.ToInt32(VehicleAttribute.Position)];
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            veh_found += 1;
                        }

                        if (veh_found == 2)
                        {
                            if (ego_pos - npc_pos > 12)
                            {
                                VissimController.Set_VehicleInfo(ego_num, "DesLane", 3);
                                action_count = 2;
                            }
                            break;
                        }
                    }

                }
                else if (action_count == 2)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            if (Convert.ToInt32(vehicles[i, Convert.ToInt32(VehicleAttribute.Lane)].ToString().Split("-")[1]) == (int)vehicles[i, Convert.ToInt32(VehicleAttribute.DesiredLane)])
                            {
                                VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesSpeed", 50);
                                action_count = 3;
                            }
                            break;
                        }
                    }
                }

                else if (action_count == 3)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        { ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)]; break; }
                    }
                }

                if ((double)VissimController.Get_TargetVehicleInfo(ego_num, "DistTravTot") > 260)
                {
                    return -1;
                }

            } //cut -in

            else if (index == 1)
            {
                int ego_num = -1;
                if (action_count == 0)
                {
                    int veh_found = 0;
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["NPC_1"])
                        {
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "Speed", 50);
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesSpeed", 50);
                            veh_found += 1;
                            _active = true;
                        }
                        else if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "Speed", 75);
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesSpeed", 75);
                            veh_found += 1;
                        }

                        if (veh_found == 2)
                        {
                            action_count = 1;
                            break;
                        }
                    }
                }
                else if (action_count == 1)
                {
                    double ego_pos = 0;
                    double npc_pos = 0;
                    int npc_num = 0;
                    int veh_found = 0;
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["NPC_1"])
                        {
                            npc_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            npc_pos = (double)vehicles[i, Convert.ToInt32(VehicleAttribute.Position)];
                            veh_found += 1;
                        }
                        else if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_pos = (double)vehicles[i, Convert.ToInt32(VehicleAttribute.Position)];
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            veh_found += 1;
                        }

                        if (veh_found == 2)
                        {
                            if (npc_pos - ego_pos < 15)
                            {
                                VissimController.Set_VehicleInfo(ego_num, "DesLane", 2);
                                action_count = 2;
                            }
                            break;
                        }
                    }

                }
                else if (action_count == 2)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            if (Convert.ToInt32(vehicles[i, Convert.ToInt32(VehicleAttribute.Lane)].ToString().Split("-")[1]) == (int)vehicles[i, Convert.ToInt32(VehicleAttribute.DesiredLane)])
                            {
                                VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesSpeed", 50);
                                action_count = 3;
                            }
                            break;
                        }
                    }
                }

                if (action_count == 3)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        { ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)]; break; }
                    }
                }

                if ((double)VissimController.Get_TargetVehicleInfo(ego_num, "DistTravTot") >= 250)
                {
                    return -1;
                }
            }//cut-out

            else if (index == 2)
            {
                int ego_num = -1;
                if (action_count == 0)
                {
                    int veh_found = 0;
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Bicycle"])
                        {
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "Speed", 15);
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesSpeed", 15);
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesLane", 1);
                            veh_found += 1;
                            _active = true;
                        }
                        else if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "Speed", 70);
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesSpeed", 70);
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesLane", 1);
                            veh_found += 1;
                        }

                        if (veh_found == 2)
                        {
                            action_count = 1;
                            break;
                        }
                    }
                }
                else if (action_count == 1)
                {
                    double ego_pos = 0;
                    double npc_pos = 0;
                    int npc_num = 0;
                    int veh_found = 0;
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Bicycle"])
                        {
                            npc_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            npc_pos = (double)vehicles[i, Convert.ToInt32(VehicleAttribute.Position)];
                            veh_found += 1;
                        }
                        else if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_pos = (double)vehicles[i, Convert.ToInt32(VehicleAttribute.Position)];
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            veh_found += 1;
                        }

                        if (veh_found == 2)
                        {
                            if (npc_pos - ego_pos < 80)
                            {
                                VissimController.Set_VehicleInfo(ego_num, "DesSpeed", 40);

                                action_count = 2;
                            }
                            break;
                        }
                    }

                }

                else if (action_count == 2)
                {
                    double ego_pos = 0;
                    double npc_pos = 0;
                    int npc_num = 0;
                    int veh_found = 0;
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Bicycle"])
                        {
                            npc_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            npc_pos = (double)vehicles[i, Convert.ToInt32(VehicleAttribute.Position)];
                            veh_found += 1;
                        }
                        else if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_pos = (double)vehicles[i, Convert.ToInt32(VehicleAttribute.Position)];
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            veh_found += 1;
                        }

                        if (veh_found == 2)
                        {
                            if (npc_pos - ego_pos < 50)
                            {
                                VissimController.Set_VehicleInfo(ego_num, "DesLane", 2);
                                action_count = 3;
                            }
                            break;
                        }
                    }

                }

                else if (action_count == 3)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            if (Convert.ToInt32(vehicles[i, Convert.ToInt32(VehicleAttribute.Lane)].ToString().Split("-")[1]) == (int)vehicles[i, Convert.ToInt32(VehicleAttribute.DesiredLane)])
                            {
                                VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesSpeed", 60);
                                action_count = 4;
                            }
                            break;
                        }
                    }
                }

                if (action_count == 4)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        { ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)]; break; }
                    }
                }

                if ((double)VissimController.Get_TargetVehicleInfo(ego_num, "DistTravTot") >= 260)
                {
                    return -1;
                }
            }// bicycle

            else if (index == 3)//accident_slow_evasion
            {
                int ego_num = -1;
                if (action_count == 0)
                {
                    int veh_found = 0;
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["NPC1"])
                        {
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "Speed", 60);
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesSpeed", 60);
                            //VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesLane", 1);
                            veh_found += 1;
                            _active = true;
                        }
                        else if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "Speed", 60);
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesSpeed", 60);
                            //VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesLane", 1);
                            veh_found += 1;
                        }

                        else if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["NPC2"])
                        {
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "Speed", 60);
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesSpeed", 60);
                            //VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesLane", 1);
                            veh_found += 1;
                        }

                        if (veh_found == 3)
                        {
                            action_count = 1;
                            break;
                        }
                    }
                }
                else if (action_count == 1)
                {
                    int npc1_num = 0;
                    int veh_found = 0;

                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["NPC1"])
                        {
                            npc1_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            veh_found += 1;
                        }
                        else if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            veh_found += 1;
                        }
                        if (veh_found == 2)
                        {
                            if (VissimController.GetSimulationTime() > 2)
                            {
                                VissimController.Set_VehicleInfo(npc1_num, "DesSpeed", 0);
                                VissimController.Set_VehicleInfo(npc1_num, "Speed", 0);
                                action_count = 2;
                            }
                            break;
                        }
                    }

                }

                else if (action_count == 2)
                {
                    double npc1_pos = 0;
                    double npc2_pos = 0;
                    int npc2_num = 0;
                    int veh_found = 0;
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["NPC2"])
                        {
                            npc2_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            npc2_pos = (double)vehicles[i, Convert.ToInt32(VehicleAttribute.Position)];
                            veh_found += 1;
                        }
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["NPC1"])
                        {
                            npc1_pos = (double)vehicles[i, Convert.ToInt32(VehicleAttribute.Position)];
                            veh_found += 1;
                        }
                        else if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            veh_found += 1;
                        }

                        if (veh_found == 3)
                        {
                            if (npc1_pos - npc2_pos < 15)
                            {
                                VissimController.Set_VehicleInfo(npc2_num, "Speed", 0);
                                VissimController.Set_VehicleInfo(npc2_num, "DesSpeed", 0);
                                action_count = 3;
                            }
                            break;
                        }
                    }

                }

                else if (action_count == 3)
                {
                    double ego_pos = 0;
                    double npc_pos = 0;
                    int npc_num = 0;
                    int veh_found = 0;
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["NPC2"])
                        {
                            npc_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            npc_pos = (double)vehicles[i, Convert.ToInt32(VehicleAttribute.Position)];
                            veh_found += 1;
                        }
                        else if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_pos = (double)vehicles[i, Convert.ToInt32(VehicleAttribute.Position)];
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            veh_found += 1;
                        }

                        if (veh_found == 2)
                        {
                            if (npc_pos - ego_pos < 80)
                            {
                                VissimController.Set_VehicleInfo(ego_num, "DesSpeed", 30);
                                action_count = 4;
                            }
                            break;
                        }
                    }

                }

                else if (action_count == 4)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            if ((double)vehicles[i, Convert.ToInt32(VehicleAttribute.Speed)] <= (double)vehicles[i, Convert.ToInt32(VehicleAttribute.DesiredSpeed)])
                            {
                                VissimController.Set_VehicleInfo(ego_num, "DesLane", 2);
                                action_count = 5;
                            }
                            break;
                        }
                    }
                }

                if (action_count == 5)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        { ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)]; break; }
                    }
                }

                if ((double)VissimController.Get_TargetVehicleInfo(ego_num, "DistTravTot") >= 250)
                {
                    return -1;
                }
            }// accident

            else if (index == 4)//collision_slow_evasion
            {
                int ego_num = -1;
                if (action_count == 0)
                {
                    int veh_found = 0;
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["NPC1"])
                        {
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "Speed", 0);
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesSpeed", 0);
                            //VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesLane", 1);
                            veh_found += 1;
                            _active = true;
                        }
                        else if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "Speed", 50);
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesSpeed", 50);
                            //VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesLane", 1);
                            veh_found += 1;
                        }

                        else if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["NPC2"])
                        {
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "Speed", 0);
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesSpeed", 0);
                            //VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesLane", 1);
                            veh_found += 1;
                        }
                        else if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["NPC3"])
                        {
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "Speed", 0);
                            VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesSpeed", 0);
                            //VissimController.Set_VehicleInfo((int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)], "DesLane", 1);
                            veh_found += 1;
                        }

                        if (veh_found == 4)
                        {
                            action_count = 1;
                            break;
                        }
                    }
                }
                else if (action_count == 1)
                {
                    //int npc3_num = 0;
                    double npc3_pos = 0;
                    double ego_pos = 0;
                    int veh_found = 0;

                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["NPC3"])
                        {
                            //npc3_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            npc3_pos = (double)vehicles[i, Convert.ToInt32(VehicleAttribute.Position)];
                            veh_found += 1;
                        }
                        else if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            ego_pos = (double)vehicles[i, Convert.ToInt32(VehicleAttribute.Position)];
                            veh_found += 1;
                        }
                        if (veh_found == 2)
                        {
                            if (npc3_pos - ego_pos <= 80)
                            {
                                VissimController.Set_VehicleInfo(ego_num, "DesSpeed", 30);
                                action_count = 2;
                            }
                            break;
                        }
                    }

                }

                else if (action_count == 2)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            if ((double)vehicles[i, Convert.ToInt32(VehicleAttribute.Speed)] <= (double)vehicles[i, Convert.ToInt32(VehicleAttribute.DesiredSpeed)])
                            {
                                VissimController.Set_VehicleInfo(ego_num, "DesLane", 2);
                                action_count = 3;
                            }
                            break;
                        }
                    }
                }

                if (action_count == 3)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            break;
                        }
                    }
                }

                if ((double)VissimController.Get_TargetVehicleInfo(ego_num, "DistTravTot") >= 150)
                {
                    return -1;
                }
            }// accident

            else if (index == 5)
            {
                int ego_num = -1;
                if (action_count == 0)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            if (Convert.ToInt32(vehicles[i, Convert.ToInt32(VehicleAttribute.Lane)].ToString().Split("-")[0]) == 217 && (double)vehicles[i, Convert.ToInt32(VehicleAttribute.Position)] >= 5)
                            {
                                VissimController.Set_VehicleInfo(ego_num, "DesSpeed", 0);
                                VissimController.Set_VehicleInfo(ego_num, "Speed", 20);
                                action_count++;
                            };
                            break;
                        }
                    }
                }

                else if (action_count == 1)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            if ((double)vehicles[i, Convert.ToInt32(VehicleAttribute.Speed)] == 0)
                            {
                                for (int stoptime = 0; stoptime < 20; stoptime++)
                                {
                                    if (_simulationCount % 10 == 0 && _simulationCount != 0)
                                    {
                                        _simulationCount = 0;
                                        UpdateChart();
                                    }
                                    SyncronizeSimulation();
                                }
                                VissimController.Set_VehicleInfo(ego_num, "DesSpeed", 50);
                                action_count++;
                            };
                            break;
                        }
                    }
                }
                else if (action_count == 2)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            if (Convert.ToInt32(vehicles[i, Convert.ToInt32(VehicleAttribute.Lane)].ToString().Split("-")[0]) == 519)
                            {
                                VissimController.Set_VehicleInfo(ego_num, "DesSpeed", 30);
                                action_count++;
                            };
                            break;
                        }
                    }
                }

                if (action_count == 3)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            break;
                        }
                    }
                }

                if ((double)VissimController.Get_TargetVehicleInfo(ego_num, "DistTravTot") >= 200)
                {
                    return -1;
                }
            }//intersection_straight

            else if (index == 6)
            {
                int ego_num = -1;
                if (action_count == 0)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            if (Convert.ToInt32(vehicles[i, Convert.ToInt32(VehicleAttribute.Lane)].ToString().Split("-")[0]) == 217 && (double)vehicles[i, Convert.ToInt32(VehicleAttribute.Position)] >= 2)
                            {
                                VissimController.Set_VehicleInfo(ego_num, "DesSpeed", 0);
                                VissimController.Set_VehicleInfo(ego_num, "Speed", 20);
                                action_count++;
                            };
                            break;
                        }
                    }
                }

                else if (action_count == 1)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            if ((double)vehicles[i, Convert.ToInt32(VehicleAttribute.Speed)] == 0)
                            {
                                for (int stoptime = 0; stoptime < 20; stoptime++)
                                {
                                    if (_simulationCount % 10 == 0 && _simulationCount != 0)
                                    {
                                        _simulationCount = 0;
                                        UpdateChart();
                                    }
                                    SyncronizeSimulation();
                                }
                                VissimController.Set_VehicleInfo(ego_num, "DesSpeed", 50);
                                VissimController.Set_VehicleInfo(ego_num, "DesLane", 3);
                                action_count++;
                            };
                            break;
                        }
                    }
                }
                else if (action_count == 2)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            if (Convert.ToInt32(vehicles[i, Convert.ToInt32(VehicleAttribute.Lane)].ToString().Split("-")[0]) == 220 ||
                                Convert.ToInt32(vehicles[i, Convert.ToInt32(VehicleAttribute.Lane)].ToString().Split("-")[0]) == 10029)
                            {
                                VissimController.Set_VehicleInfo(ego_num, "DesLane", 4);
                                action_count++;
                            };
                            break;
                        }
                    }
                }
                else if (action_count == 3)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            if (Convert.ToInt32(vehicles[i, Convert.ToInt32(VehicleAttribute.Lane)].ToString().Split("-")[0]) == 780)
                            {
                                VissimController.Set_VehicleInfo(ego_num, "DesSpeed", 30);
                                action_count++;
                            };
                            break;
                        }
                    }
                }

                if (action_count == 4)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            break;
                        }
                    }
                }

                if ((double)VissimController.Get_TargetVehicleInfo(ego_num, "DistTravTot") >= 200)
                {
                    return -1;
                }
            }//intersection_left

            else if (index == 7)//intersection_right
            {
                int ego_num = -1;
                if (action_count == 0)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            if (Convert.ToInt32(vehicles[i, Convert.ToInt32(VehicleAttribute.Lane)].ToString().Split("-")[0]) == 217 && (double)vehicles[i, Convert.ToInt32(VehicleAttribute.Position)] >= 2)
                            {
                                VissimController.Set_VehicleInfo(ego_num, "DesSpeed", 0);
                                VissimController.Set_VehicleInfo(ego_num, "Speed", 20);
                                VissimController.Set_VehicleInfo(ego_num, "DesLane", 1);
                                action_count++;
                            };
                            break;
                        }
                    }
                }

                else if (action_count == 1)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            if ((double)vehicles[i, Convert.ToInt32(VehicleAttribute.Speed)] == 0)
                            {
                                for (int stoptime = 0; stoptime < 20; stoptime++)
                                {
                                    if (_simulationCount % 10 == 0 && _simulationCount != 0)
                                    {
                                        _simulationCount = 0;
                                        UpdateChart();
                                    }
                                    SyncronizeSimulation();
                                }
                                VissimController.Set_VehicleInfo(ego_num, "DesSpeed", 50);
                                action_count++;
                            };
                            break;
                        }
                    }
                }
                else if (action_count == 2)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            if (Convert.ToInt32(vehicles[i, Convert.ToInt32(VehicleAttribute.Lane)].ToString().Split("-")[0]) == 198)
                            {
                                VissimController.Set_VehicleInfo(ego_num, "DesSpeed", 30);
                                action_count++;
                            };
                            break;
                        }
                    }
                }

                if (action_count == 3)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            break;
                        }
                    }
                }

                if ((double)VissimController.Get_TargetVehicleInfo(ego_num, "DistTravTot") >= 150)
                {
                    return -1;
                }
            }//intersection_right

            else if (index == 8)//roundabout
            {
                int ego_num = -1;
                if (action_count == 0)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            if (Convert.ToInt32(vehicles[i, Convert.ToInt32(VehicleAttribute.Lane)].ToString().Split("-")[0]) == 437)
                            {
                                VissimController.Set_VehicleInfo(ego_num, "DesSpeed", 0);
                                VissimController.Set_VehicleInfo(ego_num, "Speed", 20);
                                action_count++;
                            };
                            break;
                        }
                    }
                }
                else if (action_count == 1)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            if ((double)vehicles[i, Convert.ToInt32(VehicleAttribute.Speed)] == 0)
                            {
                                for (int stoptime = 0; stoptime < 20; stoptime++)
                                {
                                    if (_simulationCount % 10 == 0 && _simulationCount != 0)
                                    {
                                        _simulationCount = 0;
                                        UpdateChart();
                                    }
                                    SyncronizeSimulation();
                                }
                                VissimController.Set_VehicleInfo(ego_num, "DesSpeed", 50);
                                action_count++;
                            };
                            break;
                        }
                    }
                }
                else if (action_count == 2)
                {
                    for (int i = 0; i < vehicles.GetLength(0); i++)
                    {
                        if (Convert.ToInt32(vehicles[i, (int)VehicleAttribute.VehicleType]) == typedict["Ego"])
                        {
                            ego_num = (int)vehicles[i, Convert.ToInt32(VehicleAttribute.Number)];
                            if (Convert.ToInt32(vehicles[i, Convert.ToInt32(VehicleAttribute.Lane)].ToString().Split("-")[0]) == 63 && (double)vehicles[i, Convert.ToInt32(VehicleAttribute.Position)] >= 40)
                            {
                                VissimController.Set_VehicleInfo(ego_num, "DesSpeed", 0);
                                VissimController.Set_VehicleInfo(ego_num, "Speed", 0);
                                for (int stoptime = 0; stoptime < 50; stoptime++)
                                {
                                    if (_simulationCount % 10 == 0 && _simulationCount != 0)
                                    {
                                        _simulationCount = 0;
                                        UpdateChart();
                                    }
                                    SyncronizeSimulation();
                                }
                                return -1;
                            };
                            break;
                        }
                    }
                }
            }//roundabout

            return action_count;
        }

        /// <summary>
        /// 차트 생성, 초기화
        /// </summary>
        private void InitChart()
        {
            // 차트생성, 초기 데이터 입력
            #region Graph Data Init

            #region Ego Vehicle

            _egoVehicleSpeedData = new ObservableCollection<ObservablePoint>();
            _egoVehicleAccelerationData = new ObservableCollection<ObservablePoint>();
            _egoVehicleAheadDistanceData = new ObservableCollection<ObservablePoint>();
            _egoVehicleTTCData = new ObservableCollection<ObservablePoint>();

            #endregion

            #region Around Vehicle

            _aroundVehicleSpeedData = new ObservableCollection<ObservableCollection<ObservableValue>>();
            _aroundVehicleAccelerationData = new ObservableCollection<ObservableCollection<ObservableValue>>();
            _aroundVehicleAheadDistanceData = new ObservableCollection<ObservableCollection<ObservableValue>>();
            _aroundVehicleTTCData = new ObservableCollection<ObservableCollection<ObservableValue>>();

            #endregion

            #region Network Vehicle

            _networkVehicleSpeedData = new ObservableCollection<ObservableCollection<ObservableValue>>();
            _networkVehicleAccelerationData = new ObservableCollection<ObservableCollection<ObservableValue>>();
            _networkVehicleAheadDistanceData = new ObservableCollection<ObservableCollection<ObservableValue>>();
            _networkVehicleTTCData = new ObservableCollection<ObservableCollection<ObservableValue>>();

            #endregion

            for (int i = 0; i < 5; i++)
            {
                _aroundVehicleSpeedData.Add(new ObservableCollection<ObservableValue>());
                _aroundVehicleSpeedData[i] = new ObservableCollection<ObservableValue>();
                _aroundVehicleSpeedData[i].Add(new ObservableValue(0));

                _aroundVehicleAccelerationData.Add(new ObservableCollection<ObservableValue>());
                _aroundVehicleAccelerationData[i] = new ObservableCollection<ObservableValue>();
                _aroundVehicleAccelerationData[i].Add(new ObservableValue(0));

                _aroundVehicleAheadDistanceData.Add(new ObservableCollection<ObservableValue>());
                _aroundVehicleAheadDistanceData[i] = new ObservableCollection<ObservableValue>();
                _aroundVehicleAheadDistanceData[i].Add(new ObservableValue(0));

                _aroundVehicleTTCData.Add(new ObservableCollection<ObservableValue>());
                _aroundVehicleTTCData[i] = new ObservableCollection<ObservableValue>();
                _aroundVehicleTTCData[i].Add(new ObservableValue(0));


                _networkVehicleSpeedData.Add(new ObservableCollection<ObservableValue>());
                _networkVehicleSpeedData[i] = new ObservableCollection<ObservableValue>();
                _networkVehicleSpeedData[i].Add(new ObservableValue(0));

                _networkVehicleAccelerationData.Add(new ObservableCollection<ObservableValue>());
                _networkVehicleAccelerationData[i] = new ObservableCollection<ObservableValue>();
                _networkVehicleAccelerationData[i].Add(new ObservableValue(0));

                _networkVehicleAheadDistanceData.Add(new ObservableCollection<ObservableValue>());
                _networkVehicleAheadDistanceData[i] = new ObservableCollection<ObservableValue>();
                _networkVehicleAheadDistanceData[i].Add(new ObservableValue(0));

                _networkVehicleTTCData.Add(new ObservableCollection<ObservableValue>());
                _networkVehicleTTCData[i] = new ObservableCollection<ObservableValue>();
                _networkVehicleTTCData[i].Add(new ObservableValue(0));
            }

            #endregion

            // 표출할 그래프 형태 설정
            #region Graph Series Init

            #region Ego Vehicle

            EgoVehicleSpeedSeries = new ISeries[]
            {
                new LineSeries<ObservablePoint>
                {
                    Values = _egoVehicleSpeedData,
                    GeometrySize = 0,
                    Name = "Ego Speed",
                    TooltipLabelFormatter =
                        (chartPoint) => $"[{chartPoint.Context.Series.Name}] Time: {chartPoint.SecondaryValue}, Speed: {chartPoint.PrimaryValue:N2}",
                }
            };
            EgoVehicleAccelerationSeries = new ISeries[]
            {
                new LineSeries<ObservablePoint>
                {
                    Values = _egoVehicleAccelerationData,
                    GeometrySize = 0 ,
                    Name = "Ego Acceleration",
                    TooltipLabelFormatter =
                        (chartPoint) => $"[{chartPoint.Context.Series.Name}] Time: {chartPoint.SecondaryValue}, Acceleration: {chartPoint.PrimaryValue:N2}",
                }
            };
            EgoVehicleAheadDistanceSeries = new ISeries[]
            {
                new LineSeries<ObservablePoint>
                {
                    Values = _egoVehicleAheadDistanceData,
                    GeometrySize = 0,
                    Name = "Ego Following Distance",
                    TooltipLabelFormatter =
                        (chartPoint) => $"[{chartPoint.Context.Series.Name}] Time: {chartPoint.SecondaryValue}, Following Distance: {chartPoint.PrimaryValue:N2}",
                }
            };
            EgoVehicleTTCSeries = new ISeries[]
            {
                new LineSeries<ObservablePoint>
                {
                    Values = _egoVehicleTTCData,
                    GeometrySize = 0 ,
                    Name = "Ego TTC",
                    TooltipLabelFormatter =
                        (chartPoint) => $"[{chartPoint.Context.Series.Name}] Time: {chartPoint.SecondaryValue}, TTC: {chartPoint.PrimaryValue:N2}",
                }
            };

            #endregion

            #region Around Vehicle

            AroundVehicleSpeedSeries = new ObservableCollection<ISeries>
            {
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleSpeedData[0],
                    Name = "0 ~10",
                    Fill = new SolidColorPaint(SKColor.Parse("#1E90FF")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50,
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleSpeedData[1],
                    Name = "10~20",
                    Fill = new SolidColorPaint(SKColor.Parse("#008CBA")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleSpeedData[2],
                    Name = "20~30",
                    Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleSpeedData[3],
                    Name = "30~40",
                    Fill = new SolidColorPaint(SKColor.Parse("#FFC107")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleSpeedData[4],
                    Name = "over 40",
                    Fill = new SolidColorPaint(SKColor.Parse("#FF5722")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
            };

            AroundVehicleAccelerationSeries = new ObservableCollection<ISeries>
            {
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleAccelerationData[0],
                    Name = "0 ~0.8",
                    Fill = new SolidColorPaint(SKColor.Parse("#1E90FF")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleAccelerationData[1],
                    Name = "0.8~1.6",
                    Fill = new SolidColorPaint(SKColor.Parse("#008CBA")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleAccelerationData[2],
                    Name = "1.6~3.2",
                    Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleAccelerationData[3],
                    Name = "3.2~4.0",
                    Fill = new SolidColorPaint(SKColor.Parse("#FFC107")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleAccelerationData[4],
                    Name = "over 4.0",
                    Fill = new SolidColorPaint(SKColor.Parse("#FF5722")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
            };
            AroundVehicleAheadDistanceSeries = new ObservableCollection<ISeries>
            {
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleAheadDistanceData[0],
                    Name = "over 80",
                    Fill = new SolidColorPaint(SKColor.Parse("#1E90FF")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleAheadDistanceData[1],
                    Name = "80~60",
                    Fill = new SolidColorPaint(SKColor.Parse("#008CBA")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleAheadDistanceData[2],
                    Name = "60~40",
                    Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleAheadDistanceData[3],
                    Name = "40~20",
                    Fill = new SolidColorPaint(SKColor.Parse("#FFC107")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleAheadDistanceData[4],
                    Name = "20~0",
                    Fill = new SolidColorPaint(SKColor.Parse("#FF5722")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
            };
            AroundVehicleTTCSeries = new ObservableCollection<ISeries>
            {
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleTTCData[0],
                    Name = "over 4",
                    Fill = new SolidColorPaint(SKColor.Parse("#1E90FF")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleTTCData[1],
                    Name = "4~3",
                    Fill = new SolidColorPaint(SKColor.Parse("#008CBA")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleTTCData[2],
                    Name = "3~2",
                    Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleTTCData[3],
                    Name = "2~1",
                    Fill = new SolidColorPaint(SKColor.Parse("#FFC107")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _aroundVehicleTTCData[4],
                    Name = "1~0",
                    Fill = new SolidColorPaint(SKColor.Parse("#FF5722")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
            };

            #endregion

            #region Network Vehicle

            NetworkVehicleSpeedSeries = new ObservableCollection<ISeries>
            {
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleSpeedData[0],
                    Name = "0 ~10",
                    Fill = new SolidColorPaint(SKColor.Parse("#1E90FF")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleSpeedData[1],
                    Name = "10~20",
                    Fill = new SolidColorPaint(SKColor.Parse("#008CBA")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleSpeedData[2],
                    Name = "20~30",
                    Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleSpeedData[3],
                    Name = "30~40",
                    Fill = new SolidColorPaint(SKColor.Parse("#FFC107")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleSpeedData[4],
                    Name = "over 40",
                    Fill = new SolidColorPaint(SKColor.Parse("#FF5722")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
            };
            NetworkVehicleAccelerationSeries = new ObservableCollection<ISeries>
            {
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleAccelerationData[0],
                    Name = "0~0.8",
                    Fill = new SolidColorPaint(SKColor.Parse("#1E90FF")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleAccelerationData[1],
                    Name = "0.8~1.6",
                    Fill = new SolidColorPaint(SKColor.Parse("#008CBA")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleAccelerationData[2],
                    Name = "1.6~2.4",
                    Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleAccelerationData[3],
                    Name = "2.4~3.6",
                    Fill = new SolidColorPaint(SKColor.Parse("#FFC107")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleAccelerationData[4],
                    Name = "over 4",
                    Fill = new SolidColorPaint(SKColor.Parse("#FF5722")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
            };
            NetworkVehicleAheadDistanceSeries = new ObservableCollection<ISeries>
            {
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleAheadDistanceData[0],
                    Name = "over 80",
                    Fill = new SolidColorPaint(SKColor.Parse("#1E90FF")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleAheadDistanceData[1],
                    Name = "80~60",
                    Fill = new SolidColorPaint(SKColor.Parse("#008CBA")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleAheadDistanceData[2],
                    Name = "60~40",
                    Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleAheadDistanceData[3],
                    Name = "40~20",
                    Fill = new SolidColorPaint(SKColor.Parse("#FFC107")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleAheadDistanceData[4],
                    Name = "20~0",
                    Fill = new SolidColorPaint(SKColor.Parse("#FF5722")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
            };
            NetworkVehicleTTCSeries = new ObservableCollection<ISeries>
            {
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleTTCData[0],
                    Name = "over 4",
                    Fill = new SolidColorPaint(SKColor.Parse("#1E90FF")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleTTCData[1],
                    Name = "4~3",
                    Fill = new SolidColorPaint(SKColor.Parse("#008CBA")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleTTCData[2],
                    Name = "3~2",
                    Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleTTCData[3],
                    Name = "2~1",
                    Fill = new SolidColorPaint(SKColor.Parse("#FFC107")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
                new PieSeries<ObservableValue>()
                {
                    Values = _networkVehicleTTCData[4],
                    Name = "1~0",
                    Fill = new SolidColorPaint(SKColor.Parse("#FF5722")),/*
					DataLabelsPaint = new SolidColorPaint(SKColors.DimGray),
					DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
					DataLabelsFormatter = point => point.PrimaryValue.ToString(),*/
					HoverPushout = 0,
                    InnerRadius = 50
                },
            };

            #endregion

            #endregion
        }

        /// <summary>
        /// 차트에 데이터 추가
        /// </summary>
        private void UpdateChart()
        {
            // 자율주행차량이 있을 경우
            if (EgoVehicleExist)
            {
                // 데이터 개수가 400개가 넘어갈 시 최초값 삭제
                if (_egoVehicleSpeedData!.Count >= 400)
                {
                    _egoVehicleSpeedData!.RemoveAt(0);
                    _egoVehicleAccelerationData!.RemoveAt(0);
                    _egoVehicleAheadDistanceData!.RemoveAt(0);
                    _egoVehicleTTCData!.RemoveAt(0);
                }

                // 데이터 추가
                _egoVehicleSpeedData!.Add(new ObservablePoint(_simulationSecond, NetworkInfo!.EgoVehicleSpeed));
                _egoVehicleAccelerationData!.Add(new ObservablePoint(_simulationSecond, NetworkInfo.EgoVehicleAcceleration));
                _egoVehicleAheadDistanceData!.Add(new ObservablePoint(_simulationSecond, NetworkInfo.EgoVehicleAheadDistance));
                _egoVehicleTTCData!.Add(new ObservablePoint(_simulationSecond, NetworkInfo.EgoVehicleTTC));

                // 자율주행차 주변차량 데이터 변경
                for (int i = 0; i < 5; i++)
                {
                    _aroundVehicleSpeedData![i][0].Value = NetworkInfo.AroundVehicleSpeed[i];
                    _aroundVehicleAccelerationData![i][0].Value = NetworkInfo.AroundVehicleAcceleration[i];
                    _aroundVehicleAheadDistanceData![i][0].Value = NetworkInfo.AroundVehicleAheadDistance[i];
                    _aroundVehicleTTCData![i][0].Value = NetworkInfo.AroundVehicleTTC[i];
                }
            }

            // vissim network 그래프 데이터 변경
            for (int i = 0; i < 5; i++)
            {
                _networkVehicleSpeedData![i][0].Value = NetworkInfo!.NetworkVehicleSpeed[i];
                _networkVehicleAccelerationData![i][0].Value = NetworkInfo.NetworkVehicleAcceleration[i];
                _networkVehicleAheadDistanceData![i][0].Value = NetworkInfo.NetworkVehicleAheadDistance[i];
                _networkVehicleTTCData![i][0].Value = NetworkInfo.NetworkVehicleTTC[i];
            }
        }

        /// <summary>
        /// vissim 시뮬레이션 1회 진행 후 vissim 시뮬레이션 정보와 시뮬레이터의 필요 데이터 동기화
        /// </summary>
        private void SyncronizeSimulation()
        {
            VissimController.SimulationRunSingleStep();
            _simulationSecond = VissimController.GetSimulationTime();
            _simulationCount++;
        }

        /// <summary>
        /// 시뮬레이션 실행 결과 저장
        /// </summary>
        /// <param name="simulationModel">시뮬레이션 설정정보</param>
        /// <param name="losName">LOS 이름</param>
        private void SaveResult(SimulationModel simulationModel, string losName)
        {
            // 설정정보 저장개체 추가
            Models.SimulationResult.SettingData settings;

            // vissim파일 사용한 경우
            if (simulationModel.UseVissimNetwork)
            {
                settings = new Models.SimulationResult.SettingData($"{simulationModel.ScenarioFileName}_{simulationModel.NetworkFileName}", losName);
            }
            // xodr파일 사용한 경우
            else
            {
                settings = new Models.SimulationResult.SettingData($"{VissimController.VissimNetworkTempFileName}", losName);
            }

            settings.ScenarioFileName = simulationModel.ScenarioFileName;
            settings.NetworkFileName = simulationModel.NetworkFileName;
            settings.RandomSeed = simulationModel.RandomSeed;
            settings.SimulationResolution = simulationModel.Resolution;
            settings.SimulationBreakAt = simulationModel.BreakAt;
            settings.SimulationPeriod = simulationModel.Period;

            // Junction, 현시 값을 기준으로 AverageData 생성
            // Delay 처리
            object[] delayAttributes = new object[] { "Name", "VehDelay(Current, Avg, All)" };
            object[,] delayValues = VissimController.GetDelayMeasurementValues(delayAttributes);

            int nodeCount = delayValues.GetLength(0);

            // average 데이터 저장
            for (int i = 0; i < nodeCount; i++)
            {
                string[] nameSplit = delayValues[i, 0].ToString()!.Split('_');
                double delayValue = (delayValues[i, 1] is null) ? 0 : (double)delayValues[i, 1];

                (int, int) key = (Convert.ToInt32(nameSplit[0]), Convert.ToInt32(nameSplit[1]));

                if (!NetworkInfo!.SimulationAverageData.ContainsKey(key))
                {
                    NetworkInfo!.SimulationAverageData.Add(key, new AverageData());
                }

                NetworkInfo!.SimulationAverageData[key].Delay = delayValue;
            }

            // TravelTime 처리
            object[] travelTimeAttributes = new object[] { "Name", "TravTm(Current, Avg, All)", "DistTrav(Current, Avg, All)" };
            object[,] travelTimeRowValues = VissimController.GetVehicleTravelTimeMeasurementValues(travelTimeAttributes);

            Dictionary<(int, int), TravelTimeAvg> travelTimeAvgData = new Dictionary<(int, int), TravelTimeAvg>();

            // travel time 데이터 저장
            for (int i = 0; i < travelTimeRowValues.GetLength(0); i++)
            {
                string[] nameSplit = travelTimeRowValues[i, 0].ToString()!.Split('(')[0].Split('_');

                (int, int) key = (Convert.ToInt32(nameSplit[0]), Convert.ToInt32(nameSplit[1]));

                if (!travelTimeAvgData.ContainsKey(key))
                {
                    travelTimeAvgData.Add(key, new TravelTimeAvg());
                }

                travelTimeAvgData[key].AddData(travelTimeRowValues[i, 1], travelTimeRowValues[i, 2]);
            }

            double speedSum = 0;
            double speedCount = 0;

            foreach (var item in travelTimeAvgData)
            {
                // spped m/s
                double speed = (item.Value.TravelTime == 0) ? 0 : item.Value.DistanceTraveled / item.Value.TravelTime;
                speedSum += speed;
                speedCount++;

                NetworkInfo!.SimulationAverageData[item.Key].TravelTime = item.Value.TravelTime;
                NetworkInfo!.SimulationAverageData[item.Key].DistanceTraveled = item.Value.DistanceTraveled;
                // speed m/s -> km/h
                NetworkInfo!.SimulationAverageData[item.Key].Speed = speed * 3.6;
            }

            double speedAvg = speedSum / speedCount * 3.6;

            // Emission 관련 처리
            var emissionResult = VissimController.GetEmissionResults();
            foreach (var item in emissionResult)
            {
                (int, int) key = item.Key;

                NetworkInfo!.SimulationAverageData[key].QueueLength = item.QueueLength;

                NetworkInfo!.SimulationAverageData[key].CO = item.CO;
                NetworkInfo!.SimulationAverageData[key].Nox = item.NOX;
                NetworkInfo!.SimulationAverageData[key].FuelConsumtion = item.FuelConsumption;

                // 속도편차 추가
                NetworkInfo!.SimulationAverageData[key].SpeedDeviation = NetworkInfo!.SimulationAverageData[key].Speed - speedAvg;
            }

            // 생성데이터 엑셀에 저장
            ExportData export = new ExportData(settings, NetworkInfo!.SimulationAverageData, NetworkInfo!.EgoDistance, NetworkInfo!.EgoSpeed, NetworkInfo!.EgoSignal,
                NetworkInfo!.EgoDistanceRaw, NetworkInfo!.EgoSpeedRaw, NetworkInfo!.EgoSignalRaw, simulationModel.WriteRawFiles);
        }
    }
}
