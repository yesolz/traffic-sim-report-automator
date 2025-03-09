using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

using Simulator.Models;

using VISSIMLIB;


namespace Simulator.Common
{
    /// <summary>
    /// Vissim 제어에 관련된 로직
    /// </summary>
    public static class VissimController
    {
        /// <summary>
        /// 임시파일명, Vissim 파일을 사용하지 않고, OpenSCENARIO 파일을 사용시에 해당 파일명으로 저장 후 실행함<br/>
        /// Vissim을 저장하지 않으면 시뮬레이션 실행이 불가능함
        /// </summary>
        public static string VissimNetworkTempFileName = "temp";
        /// <summary>
        /// 임시파일을 저장할 위치
        /// </summary>
        public static string VissimNetworkTempPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Simulator\Resources\vissim\";

        /// <summary>
        /// local vissim, 실행되어있을경우 개체를 다시 사용함
        /// </summary>
        private static Vissim? _vissim { get; set; }

        /// <summary>
        /// VissimController 내부에서 사용하는 vissim 개체
        /// </summary>
        private static Vissim Vissim
        {
            get { return (_vissim != null) ? _vissim : null; }
            set
            {
                if (value != _vissim)
                {
                    _vissim = value;
                }
            }
        }

        /// <summary>
        /// vissim이 실행중인지 확인함
        /// </summary>
        /// <returns>true: 실행중, false: 종료됨</returns>
        public static bool IsVissimRunning()
        {
            // visism이 실행된적이없을경우, false
            if (_vissim is null)
                return false;

            // vissim의 라이센스정보 요청
            // vissim이 종료된 경우 erorr 메시지를 리턴하므로 catch 문에서 false
            // 라이센스정보가 정상 호출되는경우 실행되어있으므로, true
            try
            {
                _vissim.LicenseInfo.ToString();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// vissim 실행함
        /// </summary>
        /// <returns></returns>
        public static async Task ActivateVissimAsync()
        {
            try
            {
                if (false == IsVissimRunning())
                {
                    Vissim = new Vissim();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// vissim 파일을 로드
        /// </summary>
        /// <param name="networkFileName">경로를 포함한 vissim 파일명</param>
        /// <returns></returns>
        public static async Task LoadNetworkAsync(string networkFileName)
        {
            try
            {

                Vissim.LoadNet(networkFileName + ".inpx");
                Vissim.LoadLayout(networkFileName + ".layx");
                Console.WriteLine("NetworkLoaded");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// openDRIVE(*.xodr)파일을 로드
        /// </summary>
        /// <param name="networkFileName">경로를 포함한 openDRIVE(*.xodr) 파일명</param>
        /// <returns></returns>
        public static async Task ImportOpenDriveAsync(string networkFileName)
        {
            try
            {
                Vissim.ImportOpenDrive(networkFileName);
                Console.WriteLine("NetworkLoaded");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 시뮬레이션 결과 저장 설정, evaluation configuration - direct output
        /// </summary>
        /// <param name="fromTime">저장 시작시간</param>
        /// <param name="toTime">저장 종료시간</param>
        /// <returns></returns>
        public static async Task InitSimulationSettingAsync(double fromTime, double toTime)
        {
            try
            {
                await Task.Run(() =>
                {
                    /// Vissim결과 데이터 저장
                    // SSAM
                    Vissim.Net.Evaluation.AttValue["SSAMWriteFile"] = true; // 파일 저장 활성화

                    /* Vehicle Record 
					   More -> Attributes Selection -> 값을 설정하는 방법을 모름. 추후 업데이트
					   Simulation second, Number, Link Number, Lane Index, Speed, Desired speed, Acceleration, Lane change, Coorinates front, Coordinates rear 
					   데이터를 모아서 Excel에 직접 입력 */
                    /* 2023.05.10 Vissim에서 COMInterface로 설정이 불가능함. 
					 * 데이터 읽기, 수정: 가능, 신규로 추가 안됨
					 * AddVehicleRecordAttribute 기능으로 대체함.
					 */
                    Vissim.Net.Evaluation.AttValue["VehRecWriteFile"] = true; // 파일 저장 활성화
                    Vissim.Net.Evaluation.AttValue["VehRecWriteDatabase"] = false; // 파일 데이터베이스 저장 비활성화
                    Vissim.Net.Evaluation.AttValue["VehRecResolution"] = 1; // 저장 간격 (현재설정 : 시뮬레이션 1회당 저장)
                    Vissim.Net.Evaluation.AttValue["VehRecFilterType"] = "All"; // 모든 차량의 데이터 저장
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Vissim - Evaluation - VehicleRecord - Attribute.. 관련 설정을 추가함<br/>
        /// COM Interface에서는 관련 방법을 찾지 못하여 .inpx 파일의 XML 구조체를 수정하는 방식
        /// </summary>
        /// <param name="vissimNetworkFileFullName">경로 + 확장자를 포함한 Vissim파일명</param>
        public static void AddVehicleRecordAttribute(string vissimNetworkFileFullName)
        {
            // XML 형태로 데이터 로드
            XDocument xmlDoc = XDocument.Load(vissimNetworkFileFullName);

            // Vissim - Evaluation - VehicleRecord - Attribute.. 항목에 해당하는 데이터 로드
            XElement targetElement = xmlDoc.Element("network")!.Element("evaluation")!.Element("vehRec")!.Element("attributes")!;

            // 기본으로 설정된 Vehicle Record Attribute 값이 6개임
            // 설정이 안되어 있을 경우 요청받은 항목들을 추가함
            if (targetElement.Descendants().Count() <= 6)
            {
                // 추가할 데이터 항목 List 생성
                List<Models.VissimAttributeSelection> vehicleRecordAttributes = new List<Models.VissimAttributeSelection>();

                // Delay time
                vehicleRecordAttributes.Add(new Models.VissimAttributeSelection("DELAYTM", 2, "SECONDS", false));
                // Driving state
                vehicleRecordAttributes.Add(new Models.VissimAttributeSelection("DRIVSTATE", 0, "DEFAULT", false));
                // Distance traveled (total)
                vehicleRecordAttributes.Add(new Models.VissimAttributeSelection("DISTTRAVTOT", 2, "DEFAULT", false));
                // Dwell time
                vehicleRecordAttributes.Add(new Models.VissimAttributeSelection("DWELLTM", 2, "SECONDS", false));
                // Lane change
                vehicleRecordAttributes.Add(new Models.VissimAttributeSelection("LNCHG", 0, "DEFAULT", false));
                // Headway(2023에서 사라지고, FollowingDistnace (net)으로 대체함)
                vehicleRecordAttributes.Add(new Models.VissimAttributeSelection("FOLLOWDISTNET", 2, "DEFAULT", false));
                // Desired lane
                vehicleRecordAttributes.Add(new Models.VissimAttributeSelection("DESLANE", 0, "DEFAULT", false));
                // Desired speed
                vehicleRecordAttributes.Add(new Models.VissimAttributeSelection("DESSPEED", 2, "DEFAULT", false));
                // Desired speed fractile
                vehicleRecordAttributes.Add(new Models.VissimAttributeSelection("DESSPEEDFRAC", 2, "DEFAULT", false));
                // Destination lane
                vehicleRecordAttributes.Add(new Models.VissimAttributeSelection("DESTLANE", 0, "DEFAULT", false));
                // Lateral deviation (distraction)
                vehicleRecordAttributes.Add(new Models.VissimAttributeSelection("LATDEVDISTRACT", 2, "DEFAULT", false));
                // Lateral deviation (overspeed)
                vehicleRecordAttributes.Add(new Models.VissimAttributeSelection("LATDEVOVERSPEED", 2, "DEFAULT", false));
                // Queue encounters
                vehicleRecordAttributes.Add(new Models.VissimAttributeSelection("QENCOUNT", 0, "DEFAULT", false));
                // Queue time
                vehicleRecordAttributes.Add(new Models.VissimAttributeSelection("QTIME", 2, "SECONDS", false));
                // Safety distance(net)
                vehicleRecordAttributes.Add(new Models.VissimAttributeSelection("SAFDISTNET", 2, "DEFAULT", false));
                // Speed
                vehicleRecordAttributes.Add(new Models.VissimAttributeSelection("SPEED", 2, "DEFAULT", false));
                // Start time
                vehicleRecordAttributes.Add(new Models.VissimAttributeSelection("STARTTM", 2, "SECONDS", false));
                // Time elapsed since start of last lane change
                vehicleRecordAttributes.Add(new Models.VissimAttributeSelection("TMSINCESTARTOFLASTLNCHG", 2, "DEFAULT", false));

                // 위에서 생성한 List 항목을 XML 형태로 입력함
                foreach (var vehicleRecordAttribute in vehicleRecordAttributes)
                {
                    XElement addElement = new XElement("attributeSelection",
                        new XAttribute(nameof(vehicleRecordAttribute.attributeID), vehicleRecordAttribute.attributeID),
                        new XAttribute(nameof(vehicleRecordAttribute.decimals), vehicleRecordAttribute.decimals),
                        new XAttribute(nameof(vehicleRecordAttribute.format), vehicleRecordAttribute.format),
                        new XAttribute(nameof(vehicleRecordAttribute.showUnits), vehicleRecordAttribute.showUnits)
                        );

                    targetElement.Add(addElement);
                }

                // vissim 파일을 저장
                xmlDoc.Save(vissimNetworkFileFullName);
            }
        }

        /// <summary>
        /// .layx 파일을 수정하여 특정 개체의 visibility를 false로 설정<br/>
        /// 대상: conflict area, vehicle route, vehicle traveltime measurement
        /// </summary>
        /// <param name="vissimNetworkFileFullName">경로 + 확장자를 포함한 vissim 파일명</param>
        public static void ModifyVissimLayoutFile(string vissimNetworkFileFullName)
        {
            XDocument xmlDoc = XDocument.Load(vissimNetworkFileFullName);

            foreach (var item in xmlDoc.Element("layout")!.Element("networkEditorLayouts")!.Elements())
            {
                // conflict area
                XAttribute attribute = item.Element("gParSet")!.Element("conflictAreaGPars")!.Attribute("objectVisibility")!;
                attribute.Value = "false";

                // vehicle route
                attribute = item.Element("gParSet")!.Element("vehicleRoutingDecisionGPars")!.Attribute("objectVisibility")!;
                attribute.Value = "false";

                // vehicle travel time measurement
                attribute = item.Element("gParSet")!.Element("vehicleTravelTimeMeasurementsGPars")!.Attribute("objectVisibility")!;
                attribute.Value = "false";
            }

            xmlDoc.Save(vissimNetworkFileFullName);
        }

        /// <summary>
        /// Vissim Simulation, evaluatio configuration - result attribute을 설정
        /// </summary>
        /// <param name="randomSeed">랜덤시드값</param>
        /// <param name="resolution"></param>
        /// <param name="breakAt">워밍업 시간</param>
        /// <param name="period">시뮬레이션 실행 시간</param>
        /// <returns></returns>

        public static void SetRecord()
        {
            if (Vissim.Presentation.AttValue["RecordAVIs"] != 1)
                Vissim.Presentation.AttValue["RecordAVIs"] = true;
            Vissim.Presentation.AttValue["RecordAVIs"] = false;
        }

        public static async Task SetSimulationSettingAsync(int randomSeed, int resolution, double breakAt, double period)
        {
            try
            {
                await Task.Run(() =>
                {
                    /// simulation 설정
                    Vissim.Simulation.AttValue["RandSeed"] = randomSeed; // random seed
                    Vissim.Simulation.AttValue["SimPeriod"] = period + 2; // simulatino period
                    Vissim.Simulation.AttValue["SimRes"] = resolution; // simulation resolution
                    Vissim.Simulation.AttValue["UseAllCores"] = 0;

                    /// evaluation configuration - result attribute
                    // Delays
                    Vissim.Net.Evaluation.AttValue["DelaysCollectData"] = true;
                    Vissim.Net.Evaluation.AttValue["DelaysToTime"] = period;
                    Vissim.Net.Evaluation.AttValue["DelaysFromTime"] = breakAt;
                    // Nodes
                    Vissim.Net.Evaluation.AttValue["NodeResCollectData"] = true;
                    Vissim.Net.Evaluation.AttValue["NodeResToTime"] = period;
                    Vissim.Net.Evaluation.AttValue["NodeResFromTime"] = breakAt;
                    // Vehicle network perfomance
                    Vissim.Net.Evaluation.AttValue["VehNetPerfCollectData"] = true;
                    Vissim.Net.Evaluation.AttValue["VehNetPerfToTime"] = period;
                    Vissim.Net.Evaluation.AttValue["VehNetPerfFromTime"] = breakAt;
                    // Vehicle travel times
                    Vissim.Net.Evaluation.AttValue["VehTravTmsCollectData"] = true;
                    Vissim.Net.Evaluation.AttValue["VehTravTmsToTime"] = period;
                    Vissim.Net.Evaluation.AttValue["VehTravTmsFromTime"] = breakAt;

                    Vissim.Net.Evaluation.AttValue["VehRecToTime"] = period; // 파일 저장 비활성화 시간
                    Vissim.Net.Evaluation.AttValue["VehRecFromTime"] = breakAt; // 파일 저장 활성화 시간


                    Vissim.Net.CameraPositions.ItemByKey[1].AttValue["PitchAngle"] = 25.828727;
                    Vissim.Net.CameraPositions.ItemByKey[1].AttValue["YawAngle"] = 106.570150;
                    Vissim.Net.Storyboards.ItemByKey[1].Keyframes.ItemByKey[1].AttValue["CamPos"] = 1;
                    //Vissim.Net.Storyboards.ItemByKey[1].Keyframes.ItemByKey[1].AttValue["DwellTime"] = period;
                    //Vissim.Net.Storyboards.ItemByKey[1].AttValue["Filename"] = "record";

                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 기본 차량색상을 흰색으로 설정<br/>
        /// vissim 파일 최초 생성 시 다양한 색상의 차량들을 생성하도록 되어있음<br/>
        /// </summary>
        /// <returns></returns>
        public static async Task SetDefaultVehicleColorSettingAsync()
        {
            await Task.Run(() =>
            {
                // ColorDistribution 10번값을 생성, 이전에 미리 생성되어 있는 경우 스킵
                if (!Vissim.Net.ColorDistributions.ItemKeyExists[10])
                {
                    // vissim 그래픽 업데이트 중지 설정
                    Vissim.SuspendUpdateGUI();

                    // color: 색상, share: 색상 지정시에 참고하는 값
                    object attributes = new object[2] { "Color", "Share" };
                    // "FFFFFFFF": 흰색, (double)1: 100% 
                    object[,] values = new object[1, 2] { { "FFFFFFFF", (double)1 } };

                    // ColorDistribution의 key값을 10번으로 지정하여 생성
                    Vissim.Net.ColorDistributions.AddColorDistribution(10, new object[] { 0 });
                    // 생성한 ColorDistribution의 Name 설정
                    Vissim.Net.ColorDistributions.ItemByKey[10].AttValue["Name"] = "DefaultVehicleColor";
                    // 생성한 ColorDistribution의 값을 설정
                    Vissim.Net.ColorDistributions.ItemByKey[10].ColorDistrEl.SetMultipleAttributes(attributes, values);

                    /// vehicleType에서 지정되어있는 color distribution 값을 새로 생성한 값으로 사용하도록 수정
                    // car 
                    Vissim.Net.VehicleTypes.ItemByKey[100].AttValue["ColorDistr1"] = 10;
                    // hgv
                    Vissim.Net.VehicleTypes.ItemByKey[200].AttValue["ColorDistr1"] = 10;
                    // bus
                    Vissim.Net.VehicleTypes.ItemByKey[300].AttValue["ColorDistr1"] = 10;
                    // tram
                    Vissim.Net.VehicleTypes.ItemByKey[400].AttValue["ColorDistr1"] = 10;

                    // vissim 그래픽 업데이트 시작 설정
                    Vissim.ResumeUpdateGUI();
                }
            });
        }

        /// <summary>
        /// vissim에서 모든 link 속성값을 가져옴
        /// </summary>
        /// <param name="attributes">COM HELP - ILink - Attributes 항목 참조</param>
        /// <returns></returns>
        /// <exception cref="Exception">ILink - Attributes 항목에서 없는 데이터를 요청 시 에러 발생</exception>
        public static object[,] GetLinkValues(object[] attributes)
        {
            object[,]? vissimValues = null;

            try
            {
                vissimValues = Vissim.Net.Links.GetMultipleAttributes(attributes);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return (vissimValues is null) ? throw new Exception("GetLinkAttributes Failed") : vissimValues;
        }

        /// <summary>
        /// vissim에서 lane 속성값을 가져옴<br/>
        /// lane은 특정 링크를 지정하지 않으면 데이터를 확인할 수 없음
        /// </summary>
        /// <param name="linkNo">link 번호</param>
        /// <param name="attributes">COM HELP - ILane - Attributes 항목 참조</param>
        /// <returns></returns>
        /// <exception cref="Exception">ILane - Attributes 항목에서 없는 데이터를 요청 시 에러 발생</exception>
        public static object[,] GetLaneValues(int linkNo, object[] attributes)
        {
            object[,]? vissimValues = null;

            try
            {
                vissimValues = Vissim.Net.Links.ItemByKey[linkNo].Lanes.GetMultipleAttributes(attributes);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return (vissimValues is null) ? throw new Exception("GetLaneAttributes Failed") : vissimValues;
        }

        /// <summary>
        /// vissim에서 link.PolyPoints 값을 가져옴<br/>
        /// polypoint는 특정 링크를 지정하지 않으면 데이터를 확인할 수 없음
        /// </summary>
        /// <param name="linkNo">link 번호</param>
        /// <param name="attributes">COM HELP - ILinkPolyPoint - Attribute 항목 참조</param>
        /// <returns></returns>
        /// <exception cref="Exception">ILinkPolyPoint - Attributes 항목에서 없는 데이터를 요청 시 에러 발생</exception>
        public static object[,] GetLinkPolyPointValues(int linkNo, object[] attributes)
        {
            object[,]? vissimValues = null;

            try
            {
                vissimValues = Vissim.Net.Links.ItemByKey[linkNo].LinkPolyPts.GetMultipleAttributes(attributes);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return (vissimValues is null) ? throw new Exception("GetLinkPolyPointsAttributes Failed") : vissimValues;
        }

        /// <summary>
        /// vissim에서 모든 vehicle의 속성값을 가져옴
        /// </summary>
        /// <param name="attributes">COM HELP - IVehicle - Attributes 항목 참조</param>
        /// <returns></returns>
        /// <exception cref="Exception">IVehivle - Attributes 항목에서 없는 데이터를 요청 시 에러 발생</exception>
        public static object[,] GetVehicleValues(object[] attributes)
        {
            object[,]? vissimValues = null;

            try
            {
                vissimValues = Vissim.Net.Vehicles.GetMultipleAttributes(attributes);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return (vissimValues is null) ? throw new Exception("GetVehicleAttributes Failed") : vissimValues;
        }

        /// <summary>
        /// vissim에서 모든 VehicleTravelTimeMeasurement 값을 가져옴
        /// </summary>
        /// <param name="attributes">COM HELP - IVehicleTravelTimeMeasurement - Attributes 참조</param>
        /// <returns></returns>
        /// <exception cref="Exception">IVehicleTravelTimeMeasurement - Attributes 항목에서 없는 데이터를 요청 시 에러 발생</exception>
        public static object[,] GetVehicleTravelTimeMeasurementValues(object[] attributes)
        {
            object[,]? vissimValues = null;

            try
            {
                vissimValues = Vissim.Net.VehicleTravelTimeMeasurements.GetMultipleAttributes(attributes);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return (vissimValues is null) ? throw new Exception("GetTravelTimeMeasurements Failed") : vissimValues;
        }

        /// <summary>
        /// vissim에서 모든 DelayMeasurement 속성값을 가져옴
        /// </summary>
        /// <param name="attributes">COM HELP - IDelayMeasurement - Attributes 참조</param>
        /// <returns></returns>
        /// <exception cref="Exception">IDelayMeasurement - Attributes 항목에 없는 데이터 요청 시 에러 발생</exception>
        public static object[,] GetDelayMeasurementValues(object[] attributes)
        {
            object[,]? vissimValues = null;

            try
            {
                vissimValues = Vissim.Net.DelayMeasurements.GetMultipleAttributes(attributes);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return (vissimValues is null) ? throw new Exception("GetTravelTimeMeasurements Failed") : vissimValues;
        }

        /// <summary>
        /// Node에서 Emission 측정결과를 가져옴<br/>
        /// </summary>
        /// <returns></returns>
        public static List<EmissionResult> GetEmissionResults()
        {
            List<EmissionResult> result = new List<EmissionResult>();

            // Vissim.Net.nodes.GetEnumerator() 요청으로 Node값을 반복해서 사용할 수 있는 형태로 가져옴
            System.Collections.IEnumerator enNodes = Vissim.Net.Nodes.GetEnumerator();
            // 마지막 데이터까지 반복문을 수행
            while (enNodes.MoveNext())
            {
                // INode 형태로 현재값을 지정
                INode node = (INode)enNodes.Current;

                // Node 이름 분리 Junction_신호현시 -> Junction, 신호현시 형태로 분리
                string[] nameSplit = node.AttValue["Name"].ToString()!.Split('_');
                // CO 측정 결과
                object co = node.TotRes.AttValue["EmissionsCO(Current, Avg)"];
                // NOX 측정 결과
                object nox = node.TotRes.AttValue["EmissionsNOx(Current, Avg)"];
                // Queue Length 측정 결과
                object queueLength = node.TotRes.AttValue["QLen(Current, Avg)"];
                // Fuel Consumption 측정 결과
                object fuelConsumption = node.TotRes.AttValue["FuelConsumption(Current, Avg)"];

                // OpenSCENARIO 형태로 사용하면서 (Junction, 신호현시) 형태로 변경함.
                // 시뮬레이션 결과 출력시 다른 데이터들과 매핑하기 위함
                (int, int) key = (Convert.ToInt32(nameSplit[0]), Convert.ToInt32(nameSplit[1]));

                // 측정 결과값 저장
                result.Add(new EmissionResult(key, co, nox, queueLength, fuelConsumption));
            }

            return result;
        }

        /// <summary>
        /// OpenSCENARIO에 저장되어있는 차량의 기초 데이터를 vissim에 설정함<br/>
        /// color distribution, vehicle type, driving behavior, model2d3d, model2d3d distribution, vehicle type, vehicle class 관련 정보
        /// </summary>
        /// <param name="vehicleData"></param>
        /// <returns></returns>
        public static async Task InitScenarioVehicleAsync(Models.VehicleVissimSpawnData vehicleData)
        {

            // vissimObjectKey: ego(1000) or simulstion(1001)
            object vissimObjectKey = vehicleData.VehicleType;
            // 생성할 차량의 이름
            string vissimObjectName = Enum.GetName(typeof(Models.VehicleType), vissimObjectKey)!;
            // vissim에 요청할 데이터 구조체
            object[] attributes = new object[] { };
            // vissim에서 받아온 데이터 구조체
            object[,] values = new object[,] { { } };
            object[,] VisNovValue = new object[,] { { } };
            object[,] VisNameValue = new object[,] { { } };
            VisNovValue = Vissim.Net.ColorDistributions.GetMultiAttValues("No");
            VisNameValue = Vissim.Net.ColorDistributions.GetMultiAttValues("Name");
            Dictionary<object, object> visdict = new Dictionary<object, object>();
            for (int i = 0; i < VisNovValue.GetLength(0); i++)
            {
                visdict[VisNovValue[i, 1]] = VisNameValue[i, 1];
            }
            /// 색상 설정
            // ColorDistributions 확인, 생성할 데이터의 키가 이미 있는경우 추가하지 않음
            if (!visdict.TryGetValue(vehicleData.Name, out object name))
            {
                // ColorDistributions 추가
                while (visdict.ContainsKey(vissimObjectKey))
                {
                    vissimObjectKey = (int)vissimObjectKey + 1;
                }
                vehicleData.VehicleType = (int)vissimObjectKey;
                Vissim.Net.ColorDistributions.AddColorDistribution(vissimObjectKey, new object[] { 0 });
                IColorDistribution color = Vissim.Net.ColorDistributions.ItemByKey[vissimObjectKey];

                // ColorDistributions 속성 수정
                color.AttValue["Name"] = vehicleData.Name;

                // share: 색상 지정시에 참고하는 값, color: 색상
                attributes = new object[2] { "Share", "Color" };
                // (double)1: 100%, vehicleData.Color!: OpenSCENARIO 로드 시 설정한 color 생상
                values = new object[1, 2] { { (double)1, vehicleData.Color! } };
                // vissim에 값 입력
                color.ColorDistrEl.SetMultipleAttributes(attributes, values);

                /* 
                color.ColorDistrEl.SetAllAttValues("Share", (double)1);
                color.ColorDistrEl.SetAllAttValues("Color", vehicleData.Color);
                */
            }


            // Driving Behavior 확인, 생성할 데이터의 키가 이미 있는경우 추가하지 않음
            if (!Vissim.Net.DrivingBehaviors.ItemKeyExists[vissimObjectKey])
            {
                // Ego 차량이면 새로운 Driving Behavior 추가
                if (Models.VehicleType.Ego == (Models.VehicleType)vissimObjectKey)
                {
                    // Driving Behavior AV Normal 복제
                    Vissim.Net.DrivingBehaviors.DuplicateDrivingBehavior(102, vissimObjectKey);
                    Vissim.Net.DrivingBehaviors.ItemByKey[vissimObjectKey].AttValue["Name"] = vehicleData.Name;
                }

                /* Driving Behavior 추가, 기존 값을 복제하지않고, 새로 입력할 경우 사용함
                Vissim.Net.DrivingBehaviors.AddDrivingBehavior(vissimObjectKey);

                IDrivingBehavior drivingBehavior = Vissim.Net.DrivingBehaviors.ItemByKey[vissimObjectKey];
                attributes = new object[4] { "Name", "NumInteractObj", "CarFollowModType", "LatDistStandDef" };
                int numInteractObj = 0;
                double latDistStandDef = 0;

                switch ((Models.VehicleType)vissimObjectKey)
                {
                    case Models.VehicleType.Ego:

                        numInteractObj = 2;
                        latDistStandDef = 0.2;

                        break;

                    case Models.VehicleType.Simulation:

                        numInteractObj = 2;
                        latDistStandDef = 0.2;

                        break;

                    default: throw new Exception("VissimController.InitScenarioVehicleAsync Failed");
                }
                values = new object[1, 4] { { Enum.GetName(typeof(Models.VehicleType), vissimObjectKey)!, numInteractObj, CarFollowingModelType.CarFollowingModelTypeWiedemann99, latDistStandDef } };
                */
            }

            // Model2D3DDistribution 확인, 생성할 데이터의 키가 이미 있는경우 추가하지 않음
            if (!Vissim.Net.Model2D3DDistributions.ItemKeyExists[vissimObjectKey])
            {
                // Model2D3DDistribution 추가
                Vissim.Net.Model2D3DDistributions.AddModel2D3DDistribution(vissimObjectKey, new object[] { "1" });
                IModel2D3DDistribution modelDistribution = Vissim.Net.Model2D3DDistributions.ItemByKey[vissimObjectKey];

                // Model2D3DDistribution 속성 수정
                modelDistribution.AttValue["Name"] = vehicleData.Name;
                modelDistribution.Model2D3DDistrEl.SetAllAttValues("Share", (double)1);
            }

            // Models2D3D 확인, 생성할 데이터의 키가 이미 있는경우 추가하지 않음
            if (!Vissim.Net.Models2D3D.ItemKeyExists[vissimObjectKey])
            {
                // Models2D3D 추가
                attributes = new object[1] { "File3D" };
                values = Vissim.Net.Models2D3D.ItemByKey[2].Model2D3DSegs.GetMultipleAttributes(attributes);
                Vissim.Net.Models2D3D.AddModel2D3D(vissimObjectKey, new object[] { values[0, 0] });
                IModel2D3D model = Vissim.Net.Models2D3D.ItemByKey[vissimObjectKey];

                // Models2D3D 속성 수정
                model.AttValue["Name"] = vehicleData.Name;

                attributes = new object[3] { "width", "length", "height" };
                values = new object[1, 3] { { vehicleData.VehicleBoundingBox.Width, vehicleData.VehicleBoundingBox.Length, vehicleData.VehicleBoundingBox.Height } };
                model.Model2D3DSegs.SetMultipleAttributes(attributes, values);
            }

            // Vehicle Type 확인, 생성할 데이터의 키가 이미 있는경우 추가하지 않음
            if (!Vissim.Net.VehicleTypes.ItemKeyExists[vissimObjectKey])
            {
                // Vehicle Type 추가
                Vissim.Net.VehicleTypes.AddVehicleType(vissimObjectKey);
                IVehicleType vehicleType = Vissim.Net.VehicleTypes.ItemByKey[vissimObjectKey];

                // Vehicle Type 속성 수정
                vehicleType.AttValue["Name"] = vehicleData.Name;
                vehicleType.AttValue["Model2D3DDistr"] = vissimObjectKey;
                vehicleType.AttValue["ColorDistr1"] = vissimObjectKey;
                vehicleType.AttValue["Capacity"] = 5;

                /// dll 파일을 입력한 후 실행 가능한 파일이 없음..
                /// dll파일이 준비 완료되면 2개의 if 조건 중 위의 조건을 사용해야함
                /// 현재는 && false 조건 때문에 import 기능을 수행하지 않게 되어있음
                // Ego 차량인경우, DLL Import
                //if ( Models.VehicleType.Ego == (Models.VehicleType)vissimObjectKey )
                if (Models.VehicleType.Ego == (Models.VehicleType)vissimObjectKey && false)
                {
                    vehicleType.AttValue["ExtDriver"] = true;
                    vehicleType.AttValue["ExtDriverDLLFile"] = Shared.DllFileInfo!.FullName;
                }
            }

            // Vehicle Class 확인, 생성할 데이터의 키가 이미 있는경우 추가하지 않음
            if (!Vissim.Net.VehicleClasses.ItemKeyExists[vissimObjectKey])
            {
                // Vehicle Class 추가
                Vissim.Net.VehicleClasses.AddVehicleClass(vissimObjectKey);
                IVehicleClass vehicleClass = Vissim.Net.VehicleClasses.ItemByKey[vissimObjectKey];

                // Vehicle Class 속성 수정
                vehicleClass.AttValue["Name"] = vehicleData.Name;
                vehicleClass.AttValue["UseVehTypeColor"] = true;
                vehicleClass.AttValue["Color"] = vehicleData.Color;
                vehicleClass.AttValue["VehTypes"] = vissimObjectKey;
            }

        }

        /// <summary>
        /// OpenSCENARIO에 설정되어있는 차량을 지정된 위치에 추가함<br/>
        /// 생성될 차의 위치에서 removeDistance 이내에 있는 vissim 기본 차량은 삭제함
        /// </summary>
        /// <param name="vehicleInitData">OpenSCENARIO(.xosc)파일을 파싱하여 생성한 차량 정보</param>
        /// <param name="removeDistance">차량 생성시에 주변에 존재하는 차량을 제거할 거리</param>
        /// <returns></returns>
        public static async Task AddScenarioVehicleAsync(Models.VehicleVissimSpawnData vehicleInitData, double removeDistance)
        {

            // 차량이 생성될 link, lane에 위치한 차량의 정보를 확인하기위해 데이터를 가져옴
            object[] attributes = new object[3] { "No", "Pos", "VehType" };
            object[,] vehicleDatas = Vissim.Net.Links.ItemByKey[vehicleInitData.LinkNo].Lanes.ItemByKey[vehicleInitData.LaneNo].Vehs.GetMultipleAttributes(attributes);

            for (int i = 0; i < vehicleDatas.GetLength(0); i++)
            {
                // position 값
                double pos = (double)vehicleDatas[i, 1];

                // 생성될 차량의 앞, 뒤로 removeDistance 내에 존재하는 차량일경우
                if (vehicleInitData.Position - removeDistance <= pos && pos <= vehicleInitData.Position + removeDistance)
                {
                    // 차량의 타입이 ego or simulation이 아닌경우
                    int vehType = Convert.ToInt32(vehicleDatas[i, 2].ToString());
                    if (vehType < 1000)
                    {
                        // 차량을 삭제함
                        Vissim.Net.Vehicles.RemoveVehicle((int)vehicleDatas[i, 0]);
                    }
                }
            }
            // 차량 생성
            //Vissim.Net.Vehicles.AddVehicleAtLinkPosition(vehicleInitData.VehicleType, vehicleInitData.LinkNo, vehicleInitData.LaneNo, vehicleInitData.Position, vehicleInitData.DesiredSpeed);
            Vissim.Net.Vehicles.AddVehicleAtLinkPosition(vehicleInitData.VehicleType, vehicleInitData.LinkNo, vehicleInitData.LaneNo, vehicleInitData.Position, 50);

        }

        public static async Task setBoundingBox(Models.VehicleVissimSpawnData vehicleInitData)
        {

            object[] attributes = new object[3] { "No", "Pos", "VehType" };
            object[,] vehicleDatas = Vissim.Net.Links.ItemByKey[vehicleInitData.LinkNo].Lanes.ItemByKey[vehicleInitData.LaneNo].Vehs.GetMultipleAttributes(attributes);
            for (int j = 0; j < vehicleDatas.GetLength(0); j++)
            {
                if ((int.Parse(vehicleDatas[j, 2].ToString()) > 1000 && (int.Parse(vehicleDatas[j, 2].ToString()) != 1002)))
                {
                    Vissim.Net.Vehicles.ItemByKey[vehicleDatas[j, 0]].AttValue["Length"] = vehicleInitData.VehicleBoundingBox.Length;
                }
            }

        }

        /// <summary>
        /// vissim에 차량 생성
        /// </summary>
        /// <param name="vehicleType">차량 타입</param>
        /// <param name="linkNumber">도로 번호</param>
        /// <param name="laneNumber">차선 번호</param>
        /// <param name="position">생성 위치</param>
        /// <param name="desiredSpeed">최고 속력</param>
        /// <returns></returns>
        public static async Task AddVehicle(int vehicleType, int linkNumber, int laneNumber, double position, double desiredSpeed)
        {

            Vissim.Net.Vehicles.AddVehicleAtLinkPosition(vehicleType, linkNumber, laneNumber, position, desiredSpeed);

        }

        public static string GetTypeName(object typenum)
        {
            return Vissim.Net.VehicleTypes.ItemByKey[(int)typenum].AttValue["Name"];
        }

        /// <summary>
        /// 차량 삭제
        /// </summary>
        /// <param name="link">도로 번호</param>
        /// <param name="lane">차선 번호</param>
        /// <param name="positionBegin">삭제 위치(시작)</param>
        /// <param name="positionEnd">삭제 위치(종료)</param>
        /// <returns></returns>
        public static async Task RemoveVehicle(int link, int lane, double positionBegin, double positionEnd)
        {
            await Task.Run(() =>
            {
                object[] attributes = new object[3] { "No", "Pos", "VehType" };
                object[,] vehicleDatas;

                // lane이 0인경우, link의 모든 차선에 존재하는 차량을 대상으로 함
                if (lane == 0)
                {
                    vehicleDatas = Vissim.Net.Links.ItemByKey[link].Vehs.GetMultipleAttributes(attributes);
                }
                else
                {
                    vehicleDatas = Vissim.Net.Links.ItemByKey[link].Lanes.ItemByKey[lane].Vehs.GetMultipleAttributes(attributes);
                }

                // vissim에서 가져온 데이터로 관련여부 조회
                for (int i = 0; i < vehicleDatas.GetLength(0); i++)
                {
                    // 차량 position
                    double pos = (double)vehicleDatas[i, 1];

                    // 삭제 범주에 들어온 경우
                    if (positionBegin <= pos && pos <= positionEnd)
                    {
                        // 차량 타입이 ego or simaultion 타입이 아닐경우
                        int vehType = Convert.ToInt32(vehicleDatas[i, 2].ToString());
                        if ((int)VehicleType.Ego != vehType && (int)VehicleType.Simulation != vehType)
                        {
                            // 차량 삭제
                            Vissim.Net.Vehicles.RemoveVehicle((int)vehicleDatas[i, 0]);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 시뮬레이션 1회 진행<br/>
        /// 진행되는 시간은 (1 / simulatino resolution)<br/>
        /// ex) simulatino resolution: 20 -> 1 / 20 = 0.05초 간격
        /// </summary>
        public static void SimulationRunSingleStep()
        {
            Vissim.Net.Simulation.RunSingleStep();
        }

        /// <summary>
        /// 시뮬레이션 종료
        /// </summary>
        public static void SimulationStop()
        {
            Vissim.Net.Simulation.Stop();
        }

        /// <summary>
        /// vissim 종료
        /// </summary>
        public static void DeactivateVissim()
        {
            Vissim.Exit();
        }

        /// <summary>
        /// 임시 파일 저장, 동일한 이름, 경로에 저장함<br/>
        /// UseVissimNetwork를 사용하지않고, OpenSCENARIO, OpenDRIVE만 사용해서 시뮬레이션 진행할 경우 사용됨<br/>
        /// vissim은 파일이 저장된 이후 simulation 실행이 가능함
        /// </summary>
        public static void SaveVissimFileTemp()
        {
            Vissim.SaveNetAs(VissimNetworkTempPath + VissimNetworkTempFileName);
        }

        /// <summary>
        /// vissim 파일 저장, 파일명, 파일경로에 저장함<br/>
        /// UseVissimNetwork를 사용하는 경우 사용됨<br/>
        /// vissim은 파일이 저장된 이후 simulation 실행이 가능함
        /// </summary>
        /// <param name="path">경로</param>
        /// <param name="fileName">파일명</param>
        public static void SaveVissimFile(string path, string fileName)
        {
            Vissim.SaveNetAs(path + fileName);
        }

        /// <summary>
        /// 시뮬레이션 워밍업 진행
        /// </summary>
        /// <param name="warmUpTime">워밍업 진행 시간</param>
        /// <returns></returns>
        public static async Task SimulationWarmUpAsync(double warmUpTime)
        {

            // 퀵모드 설정
            Vissim.Graphics.CurrentNetworkWindow.AttValue["QuickMode"] = 1;

            // 워밍업 설정
            Vissim.Simulation.AttValue["IsSimBreakAtActive"] = true;
            // 워밍업 시간 설정
            Vissim.Simulation.AttValue["SimBreakAt"] = warmUpTime;

            // 워밍업 진행
            Vissim.Simulation.RunContinuous();

            // 퀵모드 종료
            Vissim.Graphics.CurrentNetworkWindow.AttValue["QuickMode"] = 0;

        }

        /// <summary>
        /// vissim에서 시뮬레이션 시간 수집
        /// </summary>
        /// <returns>시뮬레이션 시간</returns>
        public static double GetSimulationTime()
        {
            return Vissim.Simulation.SimulationSecond;
        }

        /// <summary>
        /// network에서 차량의 평균 속도 수집
        /// </summary>
        /// <returns>현재 시뮬레이션에서 차량의 평균 속도</returns>
        public static object GetAverageSpeed()
        {
            return Vissim.Net.VehicleNetworkPerformanceMeasurement.AttValue["SpeedAvg(Current, Current, All)"];
        }

        /// <summary>
        /// 차량 존재여부 확인
        /// </summary>
        /// <param name="vehicleNumber">확인할 차량의 번호</param>
        /// <returns></returns>
        public static bool IsVehicleExist(int vehicleNumber)
        {
            return Vissim.Net.Vehicles.ItemKeyExists[vehicleNumber];
        }

        /// <summary>
        /// 테스트용 코드, vissim에 차량을 추가함
        /// </summary>
        public static void SpawnTestVehicle()
        {
            Vissim.Net.Vehicles.AddVehicleAtLinkPosition(100, 1, 1, 0, 40);
            Vissim.Net.Vehicles.ItemByKey[3].AttValue["Speed"] = 0;

            Vissim.Net.Vehicles.AddVehicleAtLinkPosition(100, 14, 1, 0, 80);
            Vissim.Net.Vehicles.ItemByKey[3].AttValue["Speed"] = 0;
        }

        /// <summary>
        /// vissim에 signal head 생성
        /// </summary>
        /// <param name="signalHeadNumber">신호등 번호</param>
        /// <param name="linkNumber">생성될 도로 번호</param>
        /// <param name="Pos">생성될 위치</param>
        /// <param name="laneNumber">생성될 차선 번호</param>
        public static void AddSignalHead(int signalHeadNumber, int linkNumber, double Pos, int laneNumber)
        {
            // signalHead 추가시 lane 속성을 요구함
            ILane lane = (ILane)Vissim.Net.Links.ItemByKey[linkNumber].Lanes.ItemByKey[laneNumber];

            // 신호등 추가
            Vissim.Net.SignalHeads.AddSignalHead(signalHeadNumber, lane, Pos);
        }

        /// <summary>
        /// vissim에 signal controller 생성
        /// </summary>
        /// <param name="maxSequence">최대 현시</param>
        /// <param name="signalCycleTime">현시 주기</param>
        public static void AddSignalController(int maxSequence, double signalCycleTime)
        {
            // signal controller 추가
            // signal controller id 값은 최대 현시 * 10의 값, 2현시 -> 20, 3현시 -> 30 등
            Vissim.Net.SignalControllers.AddSignalController(maxSequence * 10);

            // signal controller 개체 지정
            ISignalController controller = Vissim.Net.SignalControllers.ItemByKey[maxSequence * 10];
            // signal controller 작동 방식 설정
            // fixed time simple: 정의된 시간대로 작동
            controller.AttValue["Type"] = SignalControllerType.SignalControllerTypeFixedTime;

            // 1현시가 아닐 경우
            if (maxSequence != 1)
            {
                // 주기 지정
                controller.AttValue["CycTm"] = signalCycleTime;
            }
            // 1현시인경우
            else
            {
                // 황색등을 유지하도록 설정
                controller.AttValue["CycTm"] = 9999;
            }

            // signal controller 추가 후 signal group을 추가해야 사용할 수 있음
            AddSignalGroup(maxSequence, signalCycleTime);
        }

        /// <summary>
        /// signal group 추가
        /// </summary>
        /// <param name="maxSequence">최대 현시</param>
        /// <param name="signalCycleTime">현시 주기</param>
        private static void AddSignalGroup(int maxSequence, double signalCycleTime)
        {
            // 황색등 시간
            double amber = 3;
            // 적색등 시간
            double endRed = 1;
            // 녹색등 시간
            double endGreen = (signalCycleTime / maxSequence) - amber;

            if (maxSequence != 1) // 신호 1현시가 아닌경우
            {
                // 최대 현시 값 만큼, 1부터 signal group 추가
                for (int i = 1; i <= maxSequence; i++)
                {
                    // signal group 추가
                    Vissim.Net.SignalControllers.ItemByKey[maxSequence * 10].SGs.AddSignalGroup(i);
                    // 황색등 시간 지정
                    Vissim.Net.SignalControllers.ItemByKey[maxSequence * 10].SGs.ItemByKey[i].AttValue["Amber"] = amber;
                    // 적색등 시간 지정
                    Vissim.Net.SignalControllers.ItemByKey[maxSequence * 10].SGs.ItemByKey[i].AttValue["EndRed"] = ((i - 1) * (signalCycleTime / maxSequence)) + endRed;
                    // 녹색등 시간 지정
                    Vissim.Net.SignalControllers.ItemByKey[maxSequence * 10].SGs.ItemByKey[i].AttValue["EndGreen"] = ((i - 1) * (signalCycleTime / maxSequence)) + endGreen;
                }
            }
            else // 신호 1현시인경우, 황색등 점멸
            {
                /// EndRed, EndGreen 값을 0으로 설정시 부정확한 설정으로 시뮬레이션 진행이 안됨
                /// 최소 1초는 유지해야함
                // signal group 추가
                Vissim.Net.SignalControllers.ItemByKey[maxSequence * 10].SGs.AddSignalGroup(1);
                // 황색등 시간 지정
                Vissim.Net.SignalControllers.ItemByKey[maxSequence * 10].SGs.ItemByKey[1].AttValue["Amber"] = 9997;
                // 적색등 시간 지정
                Vissim.Net.SignalControllers.ItemByKey[maxSequence * 10].SGs.ItemByKey[1].AttValue["EndRed"] = 1;
                // 녹색등 시간 지정
                Vissim.Net.SignalControllers.ItemByKey[maxSequence * 10].SGs.ItemByKey[1].AttValue["EndGreen"] = 2;
            }
        }

        /// <summary>
        /// Vissim에서 자동 관리하는 signal type인 SignalControllerTypeFixedTimeSimple의 경우<br/>
        /// 점멸등을 표현할 수 없으므로, ControlByCOM 기능을 사용해 점멸하도록 설정함
        /// </summary>
        /// <param name="maxSequence">최대 현시</param>
        /// <param name="signalGroupNumber">signal group 번호</param>
        /// <param name="signalHeadNumbers">signal head 번호</param>
        public static void SetSignalHeadAmberFlashing(int maxSequence, int signalGroupNumber, List<int> signalHeadNumbers)
        {
            // signal controller id 지정
            int signalControllerId = maxSequence * 10;
            // signal group 지정
            int signalGroupId = signalGroupNumber;

            // 1현시로 설정된 signal head 번호의 설정을 변경함
            // **TEST: foreach문 수행 안해도 될거같음..
            foreach (int signalHeadNumber in signalHeadNumbers)
            {
                Vissim.Net.SignalControllers.ItemByKey[signalControllerId].SGs.ItemByKey[signalGroupId].AttValue["ContrByCOM"] = true;
                Vissim.Net.SignalControllers.ItemByKey[signalControllerId].SGs.ItemByKey[signalGroupId].AttValue["SigState"] = SignalizationState.SignalizationStateFlashingAmber;
            }
        }

        /// <summary>
        /// signal head 추가, signal controller, signal group 추가 후<br/>
        /// signal head에 연동되는 signal group 설정
        /// </summary>
        /// <param name="maxSequence">최대 현시</param>
        /// <param name="signalGroupNumber">signal group 번호</param>
        /// <param name="signalHeadNumbers">signal head 번호 목록</param>
        /// <param name="signalHeadName">signal head 이름</param>
        public static void ModifySignalHead(int maxSequence, int signalGroupNumber, List<int> signalHeadNumbers, string signalHeadName)
        {
            // SignalController 번호
            int signalControllerNumber = maxSequence * 10;

            // signal group 지정
            ISignalGroup signalGroup = Vissim.Net.SignalControllers.ItemByKey[signalControllerNumber].SGs.ItemByKey[signalGroupNumber];

            // signal head 목록을 전체 조회하여 
            // SignalController, SignalGroup, name 지정
            // name: junction_signalControllerType_현시 로 구성
            foreach (int signalHeadNumber in signalHeadNumbers)
            {
                ISignalHead signalHead = Vissim.Net.SignalHeads.ItemByKey[signalHeadNumber];

                signalHead.AttValue["SG"] = signalGroup;
                signalHead.AttValue["IsBlockSig"] = false;
                signalHead.AttValue["Name"] = signalHeadName;
            }
        }

        /// <summary>
        /// Vissim에 Vehicle Route를 추가함
        /// </summary>
        /// <param name="decisionLinkNumber">Vehicle Route Decision 번호</param>
        /// <param name="decisionPosition">Vehicle Route Decision Position</param>
        /// <param name="vehicleRoutes">Vehicle Route 생성에 필요한 정보</param>
        public static void AddVehicleRoute(int decisionLinkNumber, double decisionPosition, List<Models.VehicleRouteModel.VehicleRoute> vehicleRoutes)
        {
            // Vehicle Route Decision 추가, Static 방식으로 추가함.
            Vissim.Net.VehicleRoutingDecisionsStatic.AddVehicleRoutingDecisionStatic(0, Vissim.Net.Links.ItemByKey[decisionLinkNumber], decisionPosition);

            // VehicleRoutes에 포함된 차선 수를 합산
            int laneCountSum = 0;
            for (int i = 0; i < vehicleRoutes.Count; i++)
            {
                laneCountSum += vehicleRoutes[i].LaneCount;
            }

            // 위의 Vehicle Route Decision에서 추가한 번호를 가져옴
            // Vissim에서 Index는 1부터 시작하므로 Count 값과 동일함
            int addedNumber = Vissim.Net.VehicleRoutingDecisionsStatic.Count;

            /// Vehicle Route Decision 속성 수정
            /// Scenario에서 추가되는 Ego, Simulation Type Vehicle을 영향받지 않도록 하기 위함
            // 모든 차량 타입 설정을 False 값으로 수정
            Vissim.Net.VehicleRoutingDecisionsStatic.ItemByKey[addedNumber].AttValue["AllVehTypes"] = false;
            // vissim에서 기본 설정된 차량 타입 만 영향 받도록 함
            Vissim.Net.VehicleRoutingDecisionsStatic.ItemByKey[addedNumber].AttValue["VehClasses"] = "10, 20, 30, 40";

            /// link -> ToLink 설정된 개수 만큼 route 추가함
            // Vehicle Route 추가
            for (int i = 1; i <= vehicleRoutes.Count; i++)
            {
                // Vehicle Route 지정할 link 번호
                int routeLinkNumber = vehicleRoutes[i - 1].Number;
                // Vehicle Route 지정할 pos 번호
                double routePosition = vehicleRoutes[i - 1].Position;
                // relative flow 값 생성
                // (Vehicle Route 지정할 link의 차선 수) / (Vehicle Route Decision의 총 차선 수)
                /// ex) [Link Number] [From]: 1, [To]: 2, 3, 4
                ///     [Lane Count] 2: 1, 3: 1, 4: 3
                /// 1 -> 2 이동하는 vehicle route 생성, relative flow: (1 / 5) -> 0.2
                /// 1 -> 3 이동하는 vehicle route 생성, relative flow: (1 / 5) -> 0.2
                /// 1 -> 4 이동하는 vehicle route 생성, relative flow: (3 / 5) -> 0.6
                double relFlowValue = vehicleRoutes[i - 1].LaneCount / (double)laneCountSum;

                // vehicle route에서 link값이 필요함, 관련 데이터 생성
                ILink link = Vissim.Net.Links.ItemByKey[routeLinkNumber];
                // vehicle route 생성
                Vissim.Net.VehicleRoutingDecisionsStatic.ItemByKey[addedNumber].VehRoutSta.AddVehicleRouteStatic(i, link, routePosition);
                // vehicle route의 relative flow값 입력
                Vissim.Net.VehicleRoutingDecisionsStatic.ItemByKey[addedNumber].VehRoutSta.ItemByKey[i].AttValue["RelFlow(1)"] = relFlowValue;
            }
        }

        /// <summary>
        /// Conflict Area 수정
        /// </summary>
        /// <param name="linkList">Vissim Network의 link 관련 데이터</param>
        /// <returns></returns>
        public static async Task ModifyConflictAreaAsync(Dictionary<int, NetworkModel.LinkData> linkList)
        {
            // vissim 그래픽 업데이트 중지
            Vissim.SuspendUpdateGUI();

            await Task.Run(() =>
            {
                /// Vissim Conflict Area 관련정보 수집
                // 수집할 데이터
                object[] attributes = new object[4] { "No", "Status", "Link1\\No", "Link2\\No" };
                // vissim에서 데이터 수집
                object[,] values = Vissim.Net.ConflictAreas.GetMultipleAttributes(attributes);
                // vissim에 업데이트할 데이터 구조체 생성
                object[,] setValues = new object[values.GetLength(0), 2];

                // vissim에서 가져온 데이터의 수 만큼 관련정보를 수정함
                for (int i = 0; i < values.GetLength(0); i++)
                {
                    // conflict area 데이터의 link1 번호
                    int link1 = (int)values[i, 2];
                    // conflict area 데이터의 link2 번호
                    int link2 = (int)values[i, 3];
                    // link1의 lane count
                    int link1Count = linkList[link1].LaneCount;
                    // link2의 lane count
                    int link2Count = linkList[link2].LaneCount;

                    /// polypoint 관련데이터 산출
                    // [link1.X] 마지막 지점의 X 값 - (마지막 지점 - 1)의 X값
                    double dx1 = linkList[link1].PolyPoints[linkList[link1].PolyPoints.Count - 1].X - linkList[link1].PolyPoints[linkList[link1].PolyPoints.Count - 2].X;
                    // [link1.Y] 마지막 지점의 X 값 - (마지막 지점 - 1)의 X값
                    double dy1 = linkList[link1].PolyPoints[linkList[link1].PolyPoints.Count - 1].Y - linkList[link1].PolyPoints[linkList[link1].PolyPoints.Count - 2].Y;
                    // [link2.X] 마지막 지점의 X 값 - (마지막 지점 - 1)의 X값
                    double dx2 = linkList[link2].PolyPoints[linkList[link2].PolyPoints.Count - 1].X - linkList[link2].PolyPoints[linkList[link2].PolyPoints.Count - 2].X;
                    // [link2.Y] 마지막 지점의 X 값 - (마지막 지점 - 1)의 X값
                    double dy2 = linkList[link2].PolyPoints[linkList[link2].PolyPoints.Count - 1].Y - linkList[link2].PolyPoints[linkList[link2].PolyPoints.Count - 2].Y;

                    // link1의 마지막 지점 각도 산출
                    double angle1 = Math.Atan2(dy1, dx1) * 180 / Math.PI;
                    // link2의 마지막 지점 각도 산출
                    double angle2 = Math.Atan2(dy2, dx2) * 180 / Math.PI;

                    /// 위의 angle1, angle2 값이 0 ~ 180, 0 ~ -180 범위이기 때문에
                    /// 0보다 작은경우 +360 더하여 360도 기준 각도로 계산하기 위함
                    // angle1 각도 보정
                    angle1 = angle1 < 0 ? angle1 + 360 : angle1;
                    // angle2 각도 보정
                    angle2 = angle2 < 0 ? angle2 + 360 : angle2;

                    // SetMultiAttValues 관련 키, No임
                    setValues[i, 0] = values[i, 0];

                    // 확인한 link1, link2 데이터 중 한개라도 connector이면 기본값
                    if (link1 >= 10000 || link2 >= 10000)
                    {
                        // passive, (노란색)
                        setValues[i, 1] = ConflictAreaStatus.ConflictAreaStatusPassive;
                    }
                    // link1, link2가 1차선으로 이루어진 경우 or link1, link2의 차선 수가 같을 경우, 둘 다 확인하도록 함
                    else if ((link1Count == 1 && link2Count == 1) || link1Count == link2Count)
                    {
                        // undetermind (빨간색)
                        setValues[i, 1] = ConflictAreaStatus.ConflictAreaStatusUndetermined;
                    }
                    // 그외 경우
                    else
                    {
                        //Console.WriteLine($"Number: {values[i, 0]}, angle1: {angle1}, angle2: {angle2}, abs: {Math.Abs(angle1 - angle2)}");

                        /// link1, link2의 마지막 poly point의 각도를 비교하여 관련값 산출함
                        // link1, link2가 이루고 있는 각도의 차이가 5도 미만일 경우, 합류지점으로 가정함
                        if (Math.Abs(angle1 - angle2) < 5)
                        {
                            // link2의 차선수가 많을 경우, link2 메인도로, link1 합류도로
                            if (link1Count < link2Count)
                            {
                                // link2 우선, link1 양보 설정
                                setValues[i, 1] = ConflictAreaStatus.ConflictAreaStatusOneYieldsTwo;
                            }
                            // link1의 차선수가 많을 경우, link1 메인도로, link2 합류도로
                            else
                            {
                                // link1 우선, link2 양보 설정
                                setValues[i, 1] = ConflictAreaStatus.ConflictAreaStatusTwoYieldsOne;
                            }
                        }
                        // 그 외 경우 분기점으로 , 둘 다 확인하도록 함
                        // link1, link2가 이루고 있는 각도의 차이가 5도 이상일 경우, 분기점으로 가정함
                        else
                        {
                            // undetermind (빨간색)
                            setValues[i, 1] = ConflictAreaStatus.ConflictAreaStatusUndetermined;
                        }
                    }
                }
                // vissim에 관련 데이터 추가함
                Vissim.Net.ConflictAreas.SetMultiAttValues("Status", setValues);
            });

            // vissim 그래픽 업데이트 시작
            Vissim.ResumeUpdateGUI();
        }

        /// <summary>
        /// vissim에 노드를 추가함
        /// </summary>
        /// <param name="nodeNumber">노드 번호</param>
        /// <param name="nodeName">노드명</param>
        /// <param name="wktPolygonString">노드 생성에 필요한 string형태의 데이터</param>
        public static void AddNode(int nodeNumber, string nodeName, string wktPolygonString)
        {
            try
            {
                // 노드 추가함
                Vissim.Net.Nodes.AddNode(nodeNumber, wktPolygonString);
                // 노드명 설정, Junction_현시 구조임
                Vissim.Net.Nodes.ItemByKey[nodeNumber].AttValue["Name"] = nodeName;
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// vissim에 Vehicle Travel Time Measurement 추가
        /// </summary>
        /// <param name="startLinkNumber">측정 시작지점 link 번호</param>
        /// <param name="startLinkPosition">측정 시작지점 position</param>
        /// <param name="endLinkNumber">측정 종료지점 link 번호</param>
        /// <param name="endLinkPosition">측정 종료지점 position</param>
        /// <param name="name">Vehicle Travel Time 명칭</param>
        public static void AddVehicleTravelTime(int startLinkNumber, double startLinkPosition, int endLinkNumber, double endLinkPosition, string name)
        {
            /// startLink, endLink값은 vissim의 AddVehicletravelTimeMeasurement 기능 호출시에 필요하여 생성
            // 시작 지점 link
            ILink startLink = Vissim.Net.Links.ItemByKey[startLinkNumber];
            // 종료 지점 link
            ILink endLink = Vissim.Net.Links.ItemByKey[endLinkNumber];

            // Vehicle Travel Time Measurement 생성
            Vissim.Net.VehicleTravelTimeMeasurements.AddVehicleTravelTimeMeasurement(0, startLink, startLinkPosition, endLink, endLinkPosition);

            // name값을 명명한경우, name값 입력, Junction_현시(방향) 형태임
            if (!name.Equals(""))
            {
                int index = Vissim.Net.VehicleTravelTimeMeasurements.Count;
                Vissim.Net.VehicleTravelTimeMeasurements.ItemByKey[index].AttValue["Name"] = name;
            }
        }

        /// <summary>
        /// vissim에 Delay Measurement 추가
        /// </summary>
        /// <param name="name">Delay Measurement 명칭</param>
        /// <param name="vehicleTravelTimeSequence">vehicle travel time measurement 번호를 string 형태로 입력</param>
        public static void AddDelayMeasurement(string name, string vehicleTravelTimeSequence)
        {
            // Delay measurement 추가
            Vissim.Net.DelayMeasurements.AddDelayMeasurement(0);

            // 현재 추가된 데이터의 생성번호 가져옴
            int index = Vissim.Net.DelayMeasurements.Count;

            /// ex) delay measurement는 vehicle travel time measurement를 묶어서 데이터를 산출하는 형태로 되어있음
            /// 생성 순서: vehicle travel time measurement 생성 -> delay measurement 생성
            ///
            /// vehicle tarvel time measurement 목록 [No, Name] 형태
            /// [1, 148_1(S)], [2, 148_1(R)], [2, 148_1(L)]
            /// 
            /// delay measurement 목록 [no, name] 형태 
            /// [1, 148_1]
            /// 
            /// vehicle dealy measurement의 VehTravTmMeas 입력 시 vehicle travel time measurement의 No를 string 형태로 묶어서 제공함
            /// index: delay measurement의 No 
            /// vehicleTravelTimeSequence: vehicle travel time measurement의 No 목록
            /// Vissim.Net.DelayMeasurements.ItemByKey[index].AttValue["VehTravTmMeas"] = vehicleTravelTimeSequence 형태로 사용될 때 실제 값은
            /// Vissim.Net.DelayMeasurements.ItemByKey[1].AttValue["VehTravTmMeas"] = "1, 2, 3" 이러한 형태로 사용됨

            // name 설정, Junction_현시 형태임
            Vissim.Net.DelayMeasurements.ItemByKey[index].AttValue["Name"] = name;
            // Vehile Travel Time Measurement의 번호 List를 string 형태로 입력
            Vissim.Net.DelayMeasurements.ItemByKey[index].AttValue["VehTravTmMeas"] = vehicleTravelTimeSequence;
        }

        /// <summary>
        /// Vissim의 신호 데이터 수집
        /// </summary>
        /// <returns></returns>
        public static List<(int, string, double)> GetSignalData()
        {
            // 결과물 반환 형태
            // int: signal head link
            // string: signal head name
            // double: signal head position
            List<(int, string, double)> result = new List<(int, string, double)>();

            // vissim에서 가져올 signal head 데이터 설정
            object[] attributes = new object[3] { "Lane", "Name", "Pos" };
            // vissim에서 데이터 가져옴
            object[,] values = Vissim.Net.SignalHeads.GetMultipleAttributes(attributes);

            // 가져온 데이터들을 변환
            for (int i = 0; i < values.GetLength(0); i++)
            {
                // signalhead 값은 link-lane 값으로 되어있음.. link를 따로 제공하지않아 link값만 호출
                string linkLane = values[i, 0].ToString()!;
                // link값 생성
                int link = Convert.ToInt32(linkLane.Split('-')[0]);
                // name값 생성
                string name = values[i, 1].ToString()!;
                // position값 생성
                double pos = Convert.ToDouble(values[i, 2]);
                // 결과값에 해당 데이터가 없으면
                if (!result.Contains((link, name, pos)))
                {
                    // 관련 데이터 추가함
                    result.Add((link, name, pos));
                }
            }

            // 결과값 반환
            return result;
        }

        public static Dictionary<string, int> GetVehicleTypeValues()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            object[] attributes = new object[2] { "No", "Name" };
            object[,] values = Vissim.Net.VehicleTypes.GetMultipleAttributes(attributes);

            for (int i = 0; i < values.GetLength(0); i++)
            {
                result.Add(values[i, 1].ToString(), Convert.ToInt32(values[i, 0]));

            }

            return result;
        }

        public static (int linkNo, int laneNo) Get_egolink(int key, int lanekey)
        {
            //if (Vissim.Net.Links.ItemKeyExists[key]) return key;
            object[,] names = Vissim.Net.Links.GetMultiAttValues("name");
            object[,] lanes = Vissim.Net.Links.GetMultiAttValues("NumLanes");
            object[,] laneno = Vissim.Net.Links.GetMultiAttValues("No");
            for (int i = 0; i < names.GetLength(0); i++)
            {
                if (names[i, 1].ToString().Length > 0)
                {
                    string[] lane = names[i, 1].ToString().Split(" ");
                    lane = lane[1].Split("-");
                    if (int.Parse(lane[0]) == key && int.Parse(lane[1]) == 0)
                    {
                        if (lanekey < 0 && lane[2] == "Right")
                            return ((int)laneno[i, 1], (int)lanes[i, 1] + lanekey + 1);
                        else if (lanekey > 0 && lane[2] == "Left")
                            return ((int)laneno[i, 1], lanekey);
                    }
                }
            }

            return (-1, -1);
        }

        public static object Get_TargetVehicleInfo(int CarNo, string key)
        {

            return Vissim.Net.Vehicles.ItemByKey[CarNo].AttValue[key];
        }

        public static void Set_VehicleInfo(int CarNo, string key, object value)
        {
            if (key == "DesLane")
                Vissim.Net.Vehicles.ItemByKey[CarNo].AttValue[key] = Convert.ToInt32(value);
            else if (key == "Speed")
                Vissim.Net.Vehicles.ItemByKey[CarNo].AttValue[key] = Convert.ToDouble(value);
            else if (key == "DesSpeed")
                Vissim.Net.Vehicles.ItemByKey[CarNo].AttValue[key] = Convert.ToDouble(value);
        }

        /// <summary>
        /// vissim에서 시뮬레이션 실행 횟수 가져옴
        /// </summary>
        /// <returns></returns>
        public static int GetSimulationRunCount()
        {
            return Convert.ToInt32(Vissim.Net.SimulationRuns.Count);
        }

        /// <summary>
        /// Driving Behavior - CarFollowModType Enumeration
        /// </summary>
        enum CarFollowModType
        {
            CarFollowingModelTypeNoInteraction = 1,
            CarFollowingModelTypeWiedemann74 = 2,
            CarFollowingModelTypeWiedemann99 = 3,
        }

        /// <summary>
        /// Vehicle Type - Category Enumeration
        /// </summary>
        enum Category
        {
            VehicleCategoryCar = 1,
            VehicleCategoryHGV = 2,
            VehicleCategoryBus = 3,
            VehicleCategoryTram = 4,
            VehicleCategoryPedestrian = 5,
            VehicleCategoryBike = 6,
        }

        /// <summary>
        /// Vissim에 그래픽 갱신을 하지 않도록 요청함
        /// </summary>
        public static void SuspendUpdateGUI()
        {
            Vissim.SuspendUpdateGUI();
        }

        /// <summary>
        /// Vissim에 그래픽을 갱신하도록 요청함
        /// </summary>
        public static void ResumeUpdateGUI()
        {
            Vissim.ResumeUpdateGUI();
        }

        public static bool CalcEndOfRoadCondition(int vehNo)
        {
            int curlink = (int)Vissim.Net.Vehicles.ItemByKey[vehNo].AttValue["Link"];
            double curpos = (double)Vissim.Net.Vehicles.ItemByKey[vehNo].AttValue["Pos"];
            int maxlength = Vissim.Net.Links.ItemByKey[curlink].AttValue["Length2D"];
            return curpos > maxlength - 10 ? true : false;
        }

        /// <summary>
        /// Emission 결과값 저장하기위한 데이터 구조체
        /// </summary>
        public class EmissionResult
        {
            // junction, 현시값을 키값으로 사용함
            public (int, int) Key { get; init; }
            // CO 측정 결과
            public double CO { get; init; }
            // NOX 측정 결과
            public double NOX { get; init; }
            // 대기열 측정 결과
            public double QueueLength { get; init; }
            // 연료소비량 측정 결과
            public double FuelConsumption { get; init; }

            /// <summary>
            /// 생성자
            /// </summary>
            /// <param name="key">(int junctionId, int sequence)</param>
            /// <param name="co">CO 측정값</param>
            /// <param name="nox">NOX 측정값</param>
            /// <param name="queueLength">대기열 측정값</param>
            /// <param name="fuelConsumption">연료소비량 측정값</param>
            public EmissionResult((int, int) key, object co, object nox, object queueLength, object fuelConsumption)
            {
                // 키값 입력
                Key = key;
                // CO값 입력
                CO = (co is null) ? 0 : (double)co;
                // NOX값 입력
                NOX = (nox is null) ? 0 : (double)nox;
                // 대기열값 입력
                QueueLength = (queueLength is null) ? 0 : (double)queueLength;
                // 연료소비량 입력
                FuelConsumption = (fuelConsumption is null) ? 0 : (double)fuelConsumption;
            }
        }
    }
}
