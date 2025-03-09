using Microsoft.Office.Interop.Excel;
using Simulator.Models.AsamModels;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Xml.Linq;

namespace Simulator.Models
{
    /// <summary>
    /// 시뮬레이션 설정정보 및 Vissim에서 생성해야 하는 정보
    /// </summary>
    public class SimulationModel
    {
        /// <summary>
        /// xosc 파일 정보
        /// </summary>
        private FileInfo? _scenarioFileInfo { get; set; }
        /// <summary>
        /// xodr 파일 정보
        /// </summary>
        private FileInfo? _networkFileInfo { get; set; }

        public StoryboardModel Storyboard { get; set; }

        /// <summary>
        /// 파일명 요청 시 _scenarioFileInfo.Name 형태의 데이터를 string 형태로 반환함
        /// </summary>
        public string ScenarioFileName
        {
            get
            {
                if (_scenarioFileInfo is null)
                {
                    return "";
                }
                return _scenarioFileInfo.Name.Replace(_scenarioFileInfo.Extension, "");
            }
            private set
            {
                if (_scenarioFileInfo is null || value != _scenarioFileInfo.Name)
                {
                    _scenarioFileInfo = new FileInfo(value);
                }
            }
        }

        /// <summary>
        /// 파일명 요청 시 _networkFileInfo.Name 형태의 데이터를 string 형태로 반환함
        /// </summary>
        public string NetworkFileName
        {
            get
            {
                if (_networkFileInfo is null)
                {
                    return "";
                }
                return _networkFileInfo.Name.Replace(_networkFileInfo.Extension, "");
            }
            private set
            {
                if (_networkFileInfo is null || value != _networkFileInfo.Name)
                {
                    _networkFileInfo = new FileInfo(value);
                }
            }
        }

        /// <summary>
        /// 파일명 요청 시 _networkFileInfo.FullName 형태의 데이터를 string 형태로 반환함
        /// </summary>
        public string NetworkFileFullName
        {
            get
            {
                if (_networkFileInfo is null)
                    return "";

                return _networkFileInfo.FullName;
            }
        }

        /// <summary>
        /// 시뮬레이션 실행 번호
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 랜덤시드 값, 초기값: 42
        /// </summary>
        public int RandomSeed { get; set; } = 42;
        /// <summary>
        /// 시뮬레이션 해상도 값, 초기값: 10
        /// </summary>
        public int Resolution { get; set; } = 10;

        /// <summary>
        /// 워밍업시간 초기값: 0
        /// </summary>
        public double BreakAt { get; set; }

        /// <summary>
        /// 시뮬레이션 실행 시간, 초기값: 600
        /// </summary>
        public double Period { get; set; } = 600;

        /// <summary>
        /// Raw 파일 저장 여부, 초기값: false
        /// </summary>
        public bool WriteRawFiles { get; set; } = true;

        /// <summary>
        /// LOS A 사용여부
        /// </summary>
        private bool _losA { get; set; } = true;

        /// <summary>
        /// LOS A 사용여부
        /// </summary>
        public bool LosA
        {
            get { return _losA; }
            set
            {
                if (value != _losA)
                {
                    _losA = value;
                }
            }
        }

        /// <summary>
        /// LOS B 사용여부
        /// </summary>
        private bool _losB { get; set; } = true;
        /// <summary>
        /// LOS B 사용여부
        /// </summary>
        public bool LosB
        {
            get { return _losB; }
            set
            {
                if (value != _losB)
                {
                    _losB = value;
                }
            }
        }

        /// <summary>
        /// LOS C 사용여부
        /// </summary>
        private bool _losC { get; set; } = true;
        /// <summary>
        /// LOS C 사용여부
        /// </summary>
        public bool LosC
        {
            get { return _losC; }
            set
            {
                if (value != _losC)
                {
                    _losC = value;
                }
            }
        }

        /// <summary>
        /// LOS D 사용여부
        /// </summary>
        private bool _losD { get; set; } = true;
        /// <summary>
        /// LOS D 사용여부
        /// </summary>
        public bool LosD
        {
            get { return _losD; }
            set
            {
                if (value != _losD)
                {
                    _losD = value;
                }
            }
        }

        /// <summary>
        /// LOS E 사용여부
        /// </summary>
        private bool _losE { get; set; } = true;
        /// <summary>
        /// LOS E 사용여부
        /// </summary>
        public bool LosE
        {
            get { return _losE; }
            set
            {
                if (value != _losE)
                {
                    _losE = value;
                }
            }
        }

        /// <summary>
        /// LOS F 사용여부
        /// </summary>
        private bool _losF { get; set; } = true;
        /// <summary>
        /// LOS F 사용여부
        /// </summary>
        public bool LosF
        {
            get { return _losF; }
            set
            {
                if (value != _losF)
                {
                    _losF = value;
                }
            }
        }

        /// <summary>
        /// 생성한 vissim network 사용 여부, 초기값: false
        /// </summary>
        private bool _useVissimNetwork { get; set; } = false;

        /// <summary>
        /// 생성한 vissim network 사용 여부, 초기값: false
        /// </summary>
        public bool UseVissimNetwork
        {
            get { return _useVissimNetwork; }
            set
            {
                if (value != _useVissimNetwork)
                {
                    _useVissimNetwork = value;

                    if (value)
                    {
                        NetworkType = "Vissim";
                    }
                    else
                    {
                        NetworkType = "OpenDRIVE";
                    }
                }
            }
        }

        /// <summary>
        /// Simulator View에서 설정된 LOS 값을 ListView에 표출하기위해 사용
        /// </summary>
        public string UseLos
        {
            get
            {
                string returnValue = "";

                if (LosA)
                    returnValue += "A, ";

                if (LosB)
                    returnValue += "B, ";

                if (LosC)
                    returnValue += "C, ";

                if (LosD)
                    returnValue += "D, ";

                if (LosE)
                    returnValue += "E, ";

                if (LosF)
                    returnValue += "F, ";

                return returnValue.Substring(0, returnValue.Length - 2);
            }
        }

        /// <summary>
        /// Simulator View에서 설정된 Network Type(vissim, openDRIVE)을 표출하기위해 사용
        /// </summary>
        public string NetworkType { get; set; } = "OpenDRIVE";

        /// <summary>
        /// Vissim에서 생성해야하는 차량 데이터<br/>
        /// Key: entityRef.Name
        /// </summary>
        public Dictionary<string, VehicleOpenSCENARIOSpawnData> VehicleList { get; init; }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="openScenario">Serialized soxc data</param>
        /// <param name="major">메인 버전</param>
        /// <param name="minor">서브 버전</param>
        /// <param name="scenarioFile">시나리오 파일</param>
        /// <param name="networkFile">네트워크 파일</param>
        /// <param name="cnt">생성된 실행 목록 수</param>
        public SimulationModel(object openScenario, object catalog, int major, int minor, string scenarioFile, string networkFile, int cnt)
        {
            Index = cnt + 1;

            ScenarioFileName = scenarioFile;
            NetworkFileName = networkFile;
            VehicleList = InitVehicleData(openScenario, catalog, major, minor);
        }

        /// <summary>
        /// vissim에서 생성할 차량 정보 변환
        /// </summary>
        /// <param name="openScenario">Serialized soxc data</param>
        /// <param name="major">메인 버전</param>
        /// <param name="minor">서브 버전</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private Dictionary<string, VehicleOpenSCENARIOSpawnData> InitVehicleData(object openScenario, object catalog, int major, int minor)
        {
            // 데이터 저장, 반환할 개체 생성
            Dictionary<string, VehicleOpenSCENARIOSpawnData> vehicleList = new Dictionary<string, VehicleOpenSCENARIOSpawnData>();
            // 1.0 버전으로 명시적 형변환, 1.0 scenario만 사용
            DTO.OpenSCENARIO_1_0.OpenScenario scenario = (DTO.OpenSCENARIO_1_0.OpenScenario)openScenario;
            Storyboard =new StoryboardModel(scenario);
            DTO.Catalog.OpenSCENARIO catalog_ref = (DTO.Catalog.OpenSCENARIO)catalog;
            DTO.Catalog.OpenSCENARIOCatalogVehicle vehicle_ref = new DTO.Catalog.OpenSCENARIOCatalogVehicle();

            // 변환해야할 개체 수 확인
            int cnt = scenario.Entities.ScenarioObject.Count();

            // 차량 번호
            List<int> vehicleIndex = new List<int>();

            // vehicleCategory -> 분류 
            for (int i = 0; i < cnt; i++)
            {
                // 차량 정보가 아닌 ped, miscObject 관련값은 사용하지 않음
                if (scenario.Entities.ScenarioObject[i].Vehicle is null)
                {
                    if (scenario.Entities.ScenarioObject[i].CatalogReference.entryName == null)
                        continue;
                    else
                    {
                        string key = scenario.Entities.ScenarioObject[i].CatalogReference.entryName;
                        vehicle_ref = Array.Find(catalog_ref.Catalog.Vehicle, element => element.name == key);
                        if (vehicle_ref == null)
                            continue;

                    }
                }


                // 차량 번호 추가
                vehicleIndex.Add(i);

                // vehicle type, color 변환
                string vehicleType = "";
                string color = "";

                foreach (var item in vehicle_ref.Properties)
                {
                    switch (item.name)
                    {
                        case "type": vehicleType = Array.Find(vehicle_ref.ParameterDeclarations, element => element.name == item.value.ToString().Replace("$", "")).value; break;
                        case "color": color = Array.Find(vehicle_ref.ParameterDeclarations, element => element.name == item.value.ToString().Replace("$", "")).value; break;
                        default: break;//throw new Exception("Entities.ScenarioObject[].Vehicle.Properties.Property Not Identified object Found");
                    }
                }

                // 차량정보 변환 및 정보 추가
                vehicleList.Add(scenario.Entities.ScenarioObject[i].name, new VehicleOpenSCENARIOSpawnData()
                {
                    // Vehicle name, max speed
                    Name = vehicle_ref.name,
                    MaxSpeed = Convert.ToDouble(vehicle_ref.Performance.maxSpeed),
                    BoundingBox = new VehicleBoundingBox()
                    {
                        // vehicle attribute
                        X = Convert.ToDouble(vehicle_ref.BoundingBox.Center.x),
                        Y = Convert.ToDouble(vehicle_ref.BoundingBox.Center.y),
                        Z = Convert.ToDouble(vehicle_ref.BoundingBox.Center.z),
                        Height = Convert.ToDouble(vehicle_ref.BoundingBox.Dimensions.height),
                        Width = Convert.ToDouble(vehicle_ref.BoundingBox.Dimensions.width),
                        Length = Convert.ToDouble(vehicle_ref.BoundingBox.Dimensions.length),
                    },
                    Category = vehicle_ref.vehicleCategory,
                    VehicleType = ParseVehicleType(vehicleType),
                    Color = color,
                    ScenarioObjectName = scenario.Entities.ScenarioObject[i].name
                });
            }

            // 생성된 차량 번호의 생성 위치 변환
            foreach (var index in vehicleIndex)
            {
                // Vehicle location
                DTO.OpenSCENARIO_1_0.TeleportAction teleportAction = (DTO.OpenSCENARIO_1_0.TeleportAction)scenario.Storyboard.Init.Actions.Private[index].PrivateAction[0].Item;

                // 차량의 생성 위치가 없을 경우 에러메시지
                if (teleportAction.Position.Item is null)
                {
                    MessageBox.Show("teleportAction.Position.Item is null");
                }

                //teleportAction.Position.Item?.ToString()? = DTO.OpenSCENARIO_1_0.WorldPosition 형식으로 되어있음.
                // '.' 기준으로 자른 뒤 마지막 이름 형식을 확인해서 해당 포지션 값을 파싱함
                string positionItemName = teleportAction.Position.Item!.ToString()!.Split('.')[2];

                //VehicleCreateLocation createLocation = vehicleList[scenario.Storyboard.Init.Actions.Private[index].entityRef].CreateLocation;
                VehicleCreateLocation createLocation = new();
                createLocation.LocationType = ParsePositionItem(positionItemName);

                // world position, lane position, road position 생성
                switch (createLocation.LocationType)
                {
                    case LocationType.WorldPosition:

                        DTO.OpenSCENARIO_1_0.WorldPosition worldPosition = (DTO.OpenSCENARIO_1_0.WorldPosition)teleportAction.Position.Item;

                        createLocation.X = Convert.ToDouble(worldPosition.x);
                        createLocation.Y = Convert.ToDouble(worldPosition.y);

                        break;

                    case LocationType.LanePosition:

                        DTO.OpenSCENARIO_1_0.LanePosition lanePosition = (DTO.OpenSCENARIO_1_0.LanePosition)teleportAction.Position.Item;

                        createLocation.RoadId = Convert.ToInt32(lanePosition.roadId);
                        createLocation.LaneNo = Convert.ToInt32(lanePosition.laneId);
                        createLocation.Pos = Convert.ToDouble(lanePosition.s);

                        break;

                    case LocationType.RoadPosition:

                        DTO.OpenSCENARIO_1_0.RoadPosition roadPosition = (DTO.OpenSCENARIO_1_0.RoadPosition)teleportAction.Position.Item;

                        createLocation.RoadId = Convert.ToInt32(roadPosition.roadId);
                        createLocation.Pos = Convert.ToDouble(roadPosition.s);
                        createLocation.T = Convert.ToDouble(roadPosition.t);

                        break;

                    // RElativeRoadPosition의 경우 특정 entityRef를 참조하는 정보와, ds, dt 값으로 구성되어 있음.
                    // 파싱된 entityRef의 값(roadId, Pos(s), t)을 불러와서 ds, dt 값을 추가하여 계산함.
                    // 예제 문서에서는 선순위로 eneityRef 값이 지정되어있고, 후순위로 RelativeRoadPosition이 호출되지만
                    // 순서가 바뀌어 있는 경우에는 마지막에 확인해서 entityRef값을 복사해와야 하는 문제가 발생할 수 있음
                    case LocationType.RelativeRoadPosition:

                        DTO.OpenSCENARIO_1_0.RelativeRoadPosition relativeRoadPosition = (DTO.OpenSCENARIO_1_0.RelativeRoadPosition)teleportAction.Position.Item;

                        createLocation.RoadId = vehicleList[relativeRoadPosition.entityRef].CreateLocation.RoadId;
                        createLocation.Pos = vehicleList[relativeRoadPosition.entityRef].CreateLocation.Pos + Convert.ToDouble(relativeRoadPosition.ds);
                        createLocation.T = vehicleList[relativeRoadPosition.entityRef].CreateLocation.T + Convert.ToDouble(relativeRoadPosition.dt);

                        break;

                    default: throw new Exception("Not Identifyed item input, SimulationModel.InitVehicleData");
                }

                // 파싱한 생성정보 저장
                vehicleList[scenario.Storyboard.Init.Actions.Private[index].entityRef].CreateLocation = createLocation;
            }

            return vehicleList;
        }

        /// <summary>
        /// 이벤트 관련된 Story 정보 파싱
        /// </summary>
        private void InitStoryData()
        {

        }
        /// <summary>
        /// Position Item 값을 LocationType 형태로 반환
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private LocationType ParsePositionItem(string itemName)
        {
            LocationType locationType;

            if (LocationType.TryParse(itemName, out locationType))
            {
                return locationType;
            }
            else
            {
                throw new Exception("PositionItemName Parse Fail.. SimulationModel.ParsePositionItem()");
            }
        }

        /// <summary>
        /// vehicleType 변환
        /// </summary>
        /// <param name="vehicleTypeString"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private VehicleType ParseVehicleType(string vehicleTypeString)
        {
            switch (vehicleTypeString)
            {
                case "ego_vehicle": return VehicleType.Ego;
                case "simulation": return VehicleType.Simulation;
                case "target_vehicle": return VehicleType.Target;
                case "drop_vehicle": return VehicleType.Drop;

                default:
                    return VehicleType.Target; //throw new Exception("vehicleTypeString Parse Fail.. SimulationModel.ParseVehicleType()");

            }
        }
    }

    /// <summary>
    /// 시뮬레이션 실행 시 진행할 데이터 목록
    /// </summary>
    public class SimulationList : ObservableCollection<SimulationModel>
    {

    }
}
