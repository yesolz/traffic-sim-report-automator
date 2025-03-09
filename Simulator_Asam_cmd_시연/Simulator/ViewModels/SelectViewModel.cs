using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

using Simulator.Common;
using Simulator.Models;

namespace Simulator.ViewModels
{
    /// <summary>
    /// dll, xosc 파일 선택에 사용되는 select view model
    /// </summary>
    public partial class SelectViewModel : BaseViewModel
    {
        /// <summary>
        /// esmini 실행 시 관리할 process 개체
        /// </summary>
        System.Diagnostics.Process? _miniProcess;

        /// <summary>
        /// esmini Argument 옵션
        /// </summary>
        string _esminiArguments = "";

        /// <summary>
        /// esmini 파일 경로
        /// </summary>
        string _miniFilePath = "";
        /// <summary>
        /// xodr 폴더 경로
        /// </summary>
        string _xodrDirectory = "";
        /// <summary>
        /// xosc 폴더 경로
        /// </summary>
        string _xoscDirectory = "";
        /// <summary>
        /// vissim 폴더 경로
        /// </summary>
        string _vissimDirectory = "";
        /// <summary>
        /// dll 폴더 경로
        /// </summary>
        string _dllDirectory = "";
        /// <summary>
        /// vissim 실행 파일
        /// </summary>
        string _vissimExeFile = "";
        /// <summary>
        /// vissim 파일 존재 유무
        /// </summary>
        bool _vissimexeFileFound = false;

        /// <summary>
        /// 테스트용 실행 유무
        /// </summary>
        bool _isDebug { get; set; } = false;

        /// <summary>
        /// 화면 내부에 scenario, network 화면 전환을위함
        /// </summary>
        [ObservableProperty]
        private BaseViewModel _contentViewModel;

        /// <summary>
        /// scenario 화면 표출 여부
        /// </summary>
        [ObservableProperty]
        private bool _isScenarioActivated;

        /// <summary>
        /// network 화면 표출 여부
        /// </summary>
        [ObservableProperty]
        private bool _isNetworkActivated;

        /// <summary>
        /// vissim 편집 버튼 표출 여부<br/>
        /// vissim 편집 버튼 누를 시 scenario 파일이 삭제 가능하면 안되니.. 관련설정 같이 사용
        /// </summary>
        [ObservableProperty]
        private Visibility _editButtonVisibility;

        /// <summary>
        /// 화면 밑에 현재 상태를 표출
        /// </summary>
        [ObservableProperty]
        private string _selectorStatus;

        /// <summary>
        /// Edit button 사용 가능 유무
        /// </summary>
        public bool IsEditButtonEnable
        {
            get { return Shared.IsSelectViewEditButtonEnable; }
            set
            {
                if (value != Shared.IsSelectViewEditButtonEnable)
                {
                    // 사용 가능 시 색상 변경
                    EditButtonColor = (value) ? (Brush)Application.Current.FindResource("LightGreen") : (Brush)Application.Current.FindResource("Label");

                    // 공용 설정값 변경
                    Shared.IsButtonEnable = value;
                    Shared.IsSelectViewEditButtonEnable = value;

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 편집 버튼 색상
        /// </summary>
        [ObservableProperty]
        private Brush _editButtonColor;

        /// <summary>
        /// 제거 버튼 색상
        /// </summary>
        [ObservableProperty]
        private Brush _removeButtonColor;

        /// <summary>
        /// simulation 버튼에 마우스 오버시 표출될 툴팁 설정
        /// </summary>
        public string SimulationButtonToolTip
        {
            get { return Shared.SimulationToolTip; }
            set
            {
                if (value != Shared.SimulationToolTip)
                {
                    Shared.SimulationToolTip = value;
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 기본 색상, 실행 불가능, 설정 필요시 사용할 색상
        /// </summary>
        private Brush _stateColorWarning;
        /// <summary>
        /// 기본 색상, 실행 가능시 사용할 색상
        /// </summary>
        private Brush _stateColorNormal;

        /// <summary>
        /// dll 관련 설정여부 표출에 사용할 색상
        /// </summary>
        [ObservableProperty]
        private Brush _stateColorDllImport;

        /// <summary>
        /// 시나리오 유무에 따른 설정여부 표출에 사용할 색상
        /// </summary>
        [ObservableProperty]
        private Brush _stateColorScenarioImport;

        /// <summary>
        /// dll 파일 정보
        /// </summary>
        public string DllFileName
        {
            get
            {
                if (Shared.DllFileInfo is null)
                {
                    return "DLL 파일을 선택해 주세요.";
                }

                return Shared.DllFileInfo.Name.Replace(Shared.DllFileInfo.Extension, "");
            }
            set
            {
                if (Shared.DllFileInfo is null || value != Shared.DllFileInfo.FullName)
                {
                    if (File.Exists(value))
                    {
                        Shared.DllFileInfo = new FileInfo(value);
                    }

                    SetStateColor(nameof(DllFileName));
                    CheckSimulationState();

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 시나리오 파일 목록
        /// </summary>
        public ScenarioFileList ScenarioItems
        {
            get { return Shared.ScenarioFileList; }
            set { Shared.ScenarioFileList = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 선택된 시나리오 파일 정보
        /// </summary>
        [ObservableProperty]
        private ScenarioFileInfo? _selectedScenarioFileInfo;

        /// <summary>
        /// 선택된 시나리오 번호
        /// </summary>
        private int _selectedScenarioIndex;

        /// <summary>
        /// 선택된 시나리오 번호
        /// </summary>
        public int SelectedScenarioIndex
        {
            get { return _selectedScenarioIndex; }
            set
            {
                if (value == _selectedScenarioIndex)
                    return;

                _selectedScenarioIndex = value;
                OnPropertyChanged();
                EditButtonVisibility = Visibility.Hidden;

                if (0 <= value)
                {
                    ContentViewModel = new ScenarioViewModel(value);
                    EditButtonVisibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public SelectViewModel()
        {
            _selectorStatus = "";
            #region Set default view

            _isScenarioActivated = true;
            _isNetworkActivated = false;
            _contentViewModel = new ScenarioViewModel();

            #endregion

            Shared.SimulationToolTip = "";

            ScenarioItems.CollectionChanged += ScenarioItems_CollectionChanged;


            UpdateSimulationToolTip();

            OnLoaded();
        }

        /// <summary>
        /// view 화면 로드시완료 시 실행될 행위
        /// </summary>
        public void OnLoaded()
        {
            //SearchVissimExeFile();
            // 필요 디렉토리 확인 및 생성
            CheckDirectory();

            // 테스트 설정 시 테스트 데이터 입력
            if (_isDebug)
            {
                InitTestData();
            }
        }

        #region Command

        /// <summary>
        /// DLL 파일 조회, 추가
        /// </summary>
        public void SearchDllFile(string dllFileName)
        {

            // dll 기본 경로 설정
            string dllPath = _dllDirectory;

            // ShowDialog(): 파일 선택창 생성
            // GetValueOrDefault(): 사용자가 선택 버튼을 누른경우

            // 선택한 파일의 정보 가져옴
            FileInfo file = new FileInfo(dllFileName);
            // 기본설정된 dll 폴더 정보 가져옴
            DirectoryInfo dirInfo = new DirectoryInfo(_dllDirectory);

            // 기본설정된 dll 폴더에 file이 있는지 확인함
            bool fileExist = false;
            foreach (var item in dirInfo.EnumerateFiles())
            {
                if (item.Name == file.Name)
                {
                    fileExist = true;
                    break;
                }
            }

            // 기본설정된 dll 폴더에 dll 파일이 없을 경우 복사함
            if (!fileExist)
            {
                Console.WriteLine("no existing " + dllFileName);
            }

            // dll 파일 정보 입력
            DllFileName = _dllDirectory + file.Name;

        }

        /// <summary>
        /// 시나리오 파일 선택
        /// </summary>

        public void SearchScenarioFile(string xosc)
        {

            // 초기에 읽어서 확인할 데이터가 있으므로, 관련개체 생성
            XmlParser parser = new XmlParser();
            // multiSelect 가능하므로, List 형태로 생성
            ScenarioFileList items = new ScenarioFileList();
            string fileinfofullname = "";
            #region OpenSCENARIO, OpenDRIVE 파일 탐색
            DirectoryInfo di = new DirectoryInfo(_xoscDirectory);
            foreach (FileInfo files in di.GetFiles())
            {
                //전체 경로 출력
                if (files.Name.Contains(xosc))
                {
                    fileinfofullname = files.FullName;
                    //폴더이름만 출력
                    Console.WriteLine(files.Name);
                }
            }
            // 선택한 시나리오 파일 정보
            FileInfo scenarioFileInfo = new FileInfo(fileinfofullname);

            // 네트워크 파일명, 버전 확인
            string networkFileName =  "sangam-0801.xodr";
            (int major, int minor) = parser.GetScenarioVersion(scenarioFileInfo.FullName);

            // 네트워크 파일 탐색
            (bool isNetworkFileFound, string networkFileFullName) = (true, _xodrDirectory + networkFileName);
            FileInfo networkFileInfo;


            // 네트워크 파일 탐색 실패 시 사용자에게 직접 선택하도록 요구
            if (!isNetworkFileFound)
            {
                MessageBoxResult networkFildNotFound = MessageBox.Show($"LogicFile에 설정된 OpenDRIVE 파일명을 찾을 수 없습니다.\n : {networkFileName} \n\n직접 파일을 추가하시겠습니까?", "Not Found", MessageBoxButton.YesNo);

                networkFileInfo = null;
            }
            else // network 파일을 찾은 경우 네트워크 파일정보 지정
            {
                networkFileInfo = new FileInfo(networkFileFullName);
            }

            #endregion
            
            AddSimulationItem(scenarioFileInfo, networkFileInfo, major, minor);

            CheckSimulationState();
        }

        /// <summary>
        /// 사용자가 추가한 scenario 데이터를 삭제
        /// </summary>
        /// <param name="selectedItems">view에서 선택되어있는 아이템</param>
        [RelayCommand]
        private void RemoveScenarioFile(object selectedItems)
        {
            // 선택한 목록 명시적 형변환
            IList items = (IList)selectedItems;
            // 선택한 개수 확인
            int count = items.Count;

            // 삭제할 인덱스 확인용 개체 생성
            List<int> indexes = new List<int>();

            // 삭제할 아이템의 인덱스 복사
            for (int i = 0; i < count; i++)
            {
                ScenarioFileInfo selectedItem = (ScenarioFileInfo)items[i];
                indexes.Add(selectedItem.Index);
            }

            // 새로 할당할 데이터개체 생성
            ScenarioFileList newScenarioFileList = new ScenarioFileList();
            int newIndex = 1;
            for (int i = 0; i < Shared.ScenarioFileList.Count; i++)
            {
                // 표출중인 파일목록의 index와 삭제할 인덱스와 다른 경우
                if (!indexes.Contains(Shared.ScenarioFileList[i].Index))
                {
                    // 새로 운개체 생성
                    Shared.ScenarioFileList[i].Index = newIndex++;
                    newScenarioFileList.Add(Shared.ScenarioFileList[i]);
                }
            }

            // view에서 표출하고있는 파일목록 갱신
            ScenarioItems = newScenarioFileList;

            // 새로 할당할 데이터 개체 생성
            SimulationList newSimulationList = new SimulationList();
            newIndex = 1;
            for (int i = 0; i < Shared.SimulationList.Count; i++)
            {
                // 공유하고있는 Simulation 목록의 index와 삭제할 인덱스가 다른 경우
                if (!indexes.Contains(Shared.SimulationList[i].Index))
                {
                    // 새로운 개체 생성
                    Shared.SimulationList[i].Index = newIndex++;
                    newSimulationList.Add(Shared.SimulationList[i]);
                }
            }

            // 공유하고있던 파일 정보 갱신
            Shared.SimulationList = newSimulationList;
            ContentViewModel = new ScenarioViewModel();
            SelectedScenarioIndex = -1;

            CheckSimulationState();
        }

        /// <summary>
        /// Scenario View 생성, 표출
        /// </summary>
        [RelayCommand]
        private void DisplayScenarioView()
        {
            IsScenarioActivated = true;
            IsNetworkActivated = false;

            if (SelectedScenarioFileInfo is null)
                ContentViewModel = new ScenarioViewModel();
            else
                ContentViewModel = new ScenarioViewModel(SelectedScenarioIndex);
        }

        /// <summary>
        /// Network View 생성, 표출
        /// </summary>
        [RelayCommand]
        private void DisplayNetworkView()
        {
            IsNetworkActivated = true;
            IsScenarioActivated = false;

            if (SelectedScenarioFileInfo is null)
                ContentViewModel = new NetworkViewModel();
            else
                ContentViewModel = new NetworkViewModel(SelectedScenarioFileInfo.NetworkFileName);
        }

        /// <summary>
        /// esmini를 활용한 차량 생성 위치 확인
        /// </summary>
        [RelayCommand]
        private void ConfirmVehicleLocation()
        {
            // null 확인 및 개체 생성
            if (_miniProcess is null)
            {
                _miniProcess = new System.Diagnostics.Process();
            }
            // 관련 정보가 생성되었을 경우
            else
            {
                // 프로세스 종료
                _miniProcess.Close();

                // 종료할 프로세스의 window title 조회용
                string replace = "esmini " + _esminiArguments.Replace(_xoscDirectory, "");

                // 현재 실행하고있는 process 목록 조회
                System.Diagnostics.Process[] executedProcess = Process.GetProcesses();
                foreach (var process in executedProcess)
                {
                    // 종료할 프로세스의 이름과 일치할 경우, 종료
                    if (process.MainWindowTitle.Contains(replace))
                    {
                        process.Kill();
                    }
                }
            }

            // esmini 실행 정보 설정 및 실행
            _miniProcess.StartInfo.FileName = _miniFilePath;

            _esminiArguments = $"--osc {_xoscDirectory}{SelectedScenarioFileInfo!.ScenarioFileName}.xosc";
            _miniProcess.StartInfo.Arguments = $"--window 60 60 1024 576 {_esminiArguments}";
            _miniProcess.StartInfo.CreateNoWindow = true;

            _miniProcess.Start();
        }

        /// <summary>
        /// Vissim 편집 버튼
        /// </summary>
        [RelayCommand]
        private async void EditVissimNetwork()
        {
            // 버튼 클릭 시 연관있는 행위 못하도록 함
            IsEditButtonEnable = false;

            // vissim이 실행되지 않은 경우, 실행
            if (!VissimController.IsVissimRunning())
            {
                SelectorStatus = "Vissim을 실행중입니다.. 잠시 기다려 주세요.";
                await VissimController.ActivateVissimAsync();
            }

            //await VissimController.ActivateVissimAsync();
            //await VissimController.LoadNetworkAsync(SelectedScenarioFileInfo.NetworkFullFileName);

            // vissim파일 폴더 정보
            DirectoryInfo dir = new DirectoryInfo(_vissimDirectory);
            // vissim 파일 존재 여부
            bool found = false;

            // vissim파일 폴더에서 현재 데이터 확인
            foreach (var file in dir.EnumerateFiles())
            {
                string fileName = file.Name.Split('.')[0];

                // 일치하는 파일이 있을 경우
                // vissim을 실행, 기존에 생성된 network 로드
                if (fileName.Equals($"{SelectedScenarioFileInfo!.ScenarioFileName}_{SelectedScenarioFileInfo!.NetworkFileName}") && (file.Extension.ToLower().Equals(".inpx") || file.Extension.ToLower().Equals(".layx")))
                {
                    found = true;
                    SelectorStatus = "Vissim Network 파일을 로드중입니다.";

                    await VissimController.LoadNetworkAsync(_vissimDirectory + fileName);

                    SelectorStatus = "Vissim 실행을 완료 했습니다.";
                    break;
                }
            }

            // vissim파일 폴더에서 일치하는 파일이 없을 경우 새로 생성
            if (!found)
            {
                SelectorStatus = "Vissim Network 파일을 새로 생성합니다.";
                await VissimController.ImportOpenDriveAsync(SelectedScenarioFileInfo!.NetworkFileFullName);
                await VissimController.SetDefaultVehicleColorSettingAsync();

                SelectorStatus = "네트워크 초기 정보를 분석중입니다. 시간이 오래 걸릴 수 있습니다.";
                NetworkModel network = new NetworkModel(SelectedScenarioFileInfo!.NetworkFileFullName);
                await network.InitLinkList();
                await network.InitPolyPoints();
                await network.InitRoadData();

                SelectorStatus = "신호를 데이터를 분석중입니다.";
                SignalModel signal = new SignalModel(SelectedScenarioFileInfo!.NetworkFileFullName);
                await signal.InitSignalList(network.LinkList, network.RoadList);
                SelectorStatus = "신호를 생성중입니다.";
                await signal.AddVissimSignalHeads(40);

                SelectorStatus = "경로 데이터를 분석중입니다.";
                await network.InitVissimFromToLinkData();
                VehicleRouteModel routeModel = new VehicleRouteModel();
                await routeModel.InitRouteList(network.LinkList, network.vissimFromToLinkList);
                SelectorStatus = "경로 데이터를 생성중입니다.";
                await routeModel.AddVissimVehicleRoutes();

                SelectorStatus = "Conflict Area를 생성중입니다.";
                await VissimController.ModifyConflictAreaAsync(network.LinkList);

                SelectorStatus = "교차로 데이터를 분석중입니다.";
                NodeModel node = new NodeModel();
                await node.InitNodeList(network.LinkList, network.vissimFromToLinkList);
                SelectorStatus = "Node를 생성중입니다.";
                await node.AddVissimNodes();
                SelectorStatus = "Travel Time 측정지역을 생성중입니다.";
                await node.AddVissimVehicleTravelTimes(network.LinkList);
                SelectorStatus = "Delay 측정지역을 생성중입니다.";
                await node.AddVissimDelayMesaurements();

                SelectorStatus = "Vissim 파일을 저장중입니다.";
                // 생성한 파일 저장
                VissimController.SaveVissimFile(_vissimDirectory, $"{SelectedScenarioFileInfo.ScenarioFileName}_{SelectedScenarioFileInfo.NetworkFileName}");

                // vissim record 수정
                VissimController.AddVehicleRecordAttribute($"{_vissimDirectory}{SelectedScenarioFileInfo.ScenarioFileName}_{SelectedScenarioFileInfo.NetworkFileName}.inpx");

                // Conflict Area, Vehicle Route 안보이게 수정
                VissimController.ModifyVissimLayoutFile($"{_vissimDirectory}{SelectedScenarioFileInfo.ScenarioFileName}_{SelectedScenarioFileInfo.NetworkFileName}.layx");

                await VissimController.LoadNetworkAsync($"{_vissimDirectory}{SelectedScenarioFileInfo.ScenarioFileName}_{SelectedScenarioFileInfo.NetworkFileName}");

                SelectorStatus = "Vissim 파일 생성을 완료 했습니다.";
            }

            await ResetSelectorStatus();

            IsEditButtonEnable = true;
        }

        /// <summary>
        /// 5초 뒤 설정화면의 안내 문구를 초기화함
        /// </summary>
        /// <returns></returns>
        private async Task ResetSelectorStatus()
        {
            await Task.Run(() =>
            {
                System.Threading.Thread.Sleep(5000);
                SelectorStatus = "";
            });
        }

        #endregion

        #region Simulation Condition Check

        /// <summary>
        /// 시뮬레이션 실행가능유무 확인
        /// </summary>
        private void CheckSimulationState()
        {
            if (Shared.DllFileInfo is null || ScenarioItems.Count == 0)
                Shared.IsSimulationDisable = true;
            else
                Shared.IsSimulationDisable = false;

            //UpdateSimulationToolTip();
        }

        /// <summary>
        /// 시뮬레이션 툴팁 업데이트
        /// </summary>
        private void UpdateSimulationToolTip()
        {
            string toolTip = "";

            if (Shared.IsSimulationDisable)
            {
                if (Shared.DllFileInfo is null)
                {
                    toolTip += "Dll 파일이 선택되지 않았습니다.\n";
                }

                if (ScenarioItems.Count == 0)
                {
                    toolTip += "Scenario 파일이 선택되지 않았습니다.\n";
                }

                toolTip = toolTip.Substring(0, toolTip.Length - 1);
            }
            else
            {
                toolTip = "실행 준비가 완료되었습니다.";
            }

            SimulationButtonToolTip = toolTip;
        }

        /// <summary>
        /// 확인 필요시: 빨강, 실행 가능시: 녹색<br/>
        /// 확인 및 설정이 필요한 항목에 색상을 지정
        /// </summary>
        /// <param name="propertyName"></param>
        /// <exception cref="ArgumentException"></exception>
        private void SetStateColor(string propertyName)
        {
            switch (propertyName)
            {
                case "DllFileName":

                    if (Shared.DllFileInfo is null)
                        StateColorDllImport = _stateColorWarning;
                    else
                        StateColorDllImport = _stateColorNormal;

                    break;

                case "ScenarioItems":

                    if (ScenarioItems.Count == 0)
                        StateColorScenarioImport = _stateColorWarning;
                    else
                        StateColorScenarioImport = _stateColorNormal;

                    break;

                default: throw new ArgumentException("GetStateColor");
            }
        }

        #endregion

        #region Directory Check

        /// <summary>
        /// 디렉토리 확인
        /// </summary>
        private void CheckDirectory()
        {
            // 기본 설정 폴더
            string defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Simulator";
            // resource 폴더
            string resourcePath = defaultPath + @"\Resources";

            // 최상위 폴더가 없을 경우
            if (!Directory.Exists(defaultPath))
            {
                CreateDirectoryAll(defaultPath, resourcePath);
                CheckDirectory();
            }
            else
            {
                // esmini 경로 확인
                if (Directory.Exists(defaultPath + @"\esmini-demo"))
                {
                    // 파일이 있을경우, 경로지정
                    if (File.Exists(defaultPath + @"\esmini-demo\bin\esmini.exe"))
                    {
                        _miniFilePath = defaultPath + @"\esmini-demo\bin\esmini";
                    }
                    // 파일이 없을 경우 esmini 설치 및 재확인
                    else
                    {
                        UnzipEsmini(defaultPath, true);
                        CheckDirectory();
                        return;
                    }
                }
                // esmini 경로 없을 경우 esmini 설치 및 재확인
                else
                {
                    UnzipEsmini(defaultPath);
                    CheckDirectory();
                    return;
                }

                // Resources check
                if (!Directory.Exists(resourcePath))
                {
                    CreateDirectoryResource(resourcePath);
                    CreateDirectoryDll(resourcePath);
                    CreateDirectoryVissim(resourcePath);
                    CreateDirectoryXodr(resourcePath);
                    CreateDirectoryXosc(resourcePath);
                }

                // dll check
                if (!Directory.Exists(resourcePath + @"\dll"))
                {
                    CreateDirectoryDll(resourcePath);
                }
                _dllDirectory = resourcePath + @"\dll\";

                // vissim check
                if (!Directory.Exists(resourcePath + @"\vissim"))
                {
                    CreateDirectoryVissim(resourcePath);
                }
                _vissimDirectory = resourcePath + @"\vissim\";
                Shared.VissimFileDirectory = _vissimDirectory;

                // xodr check
                if (!Directory.Exists(resourcePath + @"\xodr\"))
                {
                    CreateDirectoryXodr(resourcePath);
                }
                _xodrDirectory = resourcePath + @"\xodr\";

                // xosc check
                if (!Directory.Exists(resourcePath + @"\xosc"))
                {
                    CreateDirectoryXosc(resourcePath);
                }
                _xoscDirectory = resourcePath + @"\xosc\";
             
            }
        }

        /// <summary>
        /// 최상위 Simulation 폴더가 없을경우 하위 경로를 모두 생성함
        /// </summary>
        /// <param name="defaultPath">기본 경로</param>
        /// <param name="resourcePath">resource 경로</param>
        private void CreateDirectoryAll(string defaultPath, string resourcePath)
        {
            CreateDirectorySimulator(defaultPath);
            CreateDirectoryResource(resourcePath);
            CreateDirectoryDll(resourcePath);
            CreateDirectoryVissim(resourcePath);
            CreateDirectoryXodr(resourcePath);
            CreateDirectoryXosc(resourcePath);

            UnzipEsmini(defaultPath);
        }

        /// <summary>
        /// Simulator 폴더 생성
        /// </summary>
        /// <param name="defaultPath"></param>
        private void CreateDirectorySimulator(string defaultPath)
        {
            Directory.CreateDirectory(defaultPath);
        }

        /// <summary>
        /// Resource 폴더 생성
        /// </summary>
        /// <param name="resourcePath"></param>
        private void CreateDirectoryResource(string resourcePath)
        {
            Directory.CreateDirectory(resourcePath);
        }

        /// <summary>
        /// dll 폴더 생성
        /// </summary>
        /// <param name="resourcePath"></param>
        private void CreateDirectoryDll(string resourcePath)
        {
            Directory.CreateDirectory(resourcePath + @"\dll");
        }

        /// <summary>
        /// vissim 폴더 생성
        /// </summary>
        /// <param name="resourcePath"></param>
        private void CreateDirectoryVissim(string resourcePath)
        {
            Directory.CreateDirectory(resourcePath + @"\vissim");
        }

        /// <summary>
        /// xodr 폴더 생성
        /// </summary>
        /// <param name="resourcePath"></param>
        private void CreateDirectoryXodr(string resourcePath)
        {
            Directory.CreateDirectory(resourcePath + @"\xodr");
        }

        /// <summary>
        /// xosc 폴더 생성
        /// </summary>
        /// <param name="resourcePath"></param>
        private void CreateDirectoryXosc(string resourcePath)
        {
            Directory.CreateDirectory(resourcePath + @"\xosc");
        }

        /// <summary>
        /// esmini 설치
        /// </summary>
        /// <param name="defaultPath">설치 경로</param>
        /// <param name="force">덮어쓰기 유무</param>
        private void UnzipEsmini(string defaultPath, bool force = false)
        {
            using (Stream stream = new MemoryStream(Properties.Resources.esmini_demo))
            {
                using (ZipArchive archive = new ZipArchive(stream))
                {
                    archive.ExtractToDirectory(defaultPath, force);
                }
            }
        }

        #endregion

        #region esmini

        /// <summary>
        /// xosc 폴더에 파일이 존재하는지 확인
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool IsXoscDirectoryFileExist(string fileName)
        {
            if (File.Exists(_xoscDirectory + fileName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Target: dll, xodr, xosc<br/>
        /// 내문서/Simulation 폴더에 사용자가 선택한 파일을 복사함
        /// </summary>
        /// <param name="selectedFile"></param>
        private void CopyFileToSimulationDirectory(FileInfo selectedFile)
        {
            string targetDirectory = "";

            switch (selectedFile.Name.Split('.')[1].ToLower())
            {
                case "dll": targetDirectory = _dllDirectory; break;
                case "xosc": targetDirectory = _xoscDirectory; break;
                case "xodr": targetDirectory = _xodrDirectory; break;

                default: throw new Exception("CopyFileToSimulationDirectory Error");
            }

            File.Copy(selectedFile.FullName, targetDirectory + selectedFile.Name);
        }

        /// <summary>
        /// xodr 폴더에 파일이 존재하는지 확인
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool IsXodrDirectoryFileExist(string fileName)
        {
            if (File.Exists(_xodrDirectory + fileName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// esmini 사용을 위해 xosc 파일의 xodr 경로 설정을 수정함
        /// </summary>
        /// <param name="path">파일 경로</param>
        /// <param name="fileName">파일명</param>
        private void EditXoscLogicFile(string path, string fileName)
        {
            List<string> editData = File.ReadAllLines(_xoscDirectory + fileName).ToList();

            for (int i = 0; i < editData.Count; i++)
            {
                // xodr 파일 관련설정을 찾은 경우 경로 수정
                if (editData[i].Contains("LogicFile"))
                {
                    string[] split = editData[i].Split('=');
                    split[1] = split[1].Trim().Remove(0, 1);
                    split[1] = split[1].Replace("\"/>", "");

                    split[1] = $"\"{_xodrDirectory}{split[1]}" + ".xodr\"/>";

                    editData[i] = split[0] + $"={split[1]}";

                    break;
                }
            }

            // 파일 저장
            File.WriteAllLines(_xoscDirectory + fileName, editData);
        }

        /// <summary>
        /// xosc 파일과 선택한 파일이 동일한지 확인함
        /// </summary>
        /// <param name="targetDirectoryFileFullName">내문서/Simulation/Resource/xosc/파일명</param>
        /// <param name="userSelectedFileFullName">사용자가 선택한 파일명</param>
        /// <returns></returns>
        private (bool, string) IsSameFile(string targetDirectoryFileFullName, string userSelectedFileFullName)
        {
            List<string> target = File.ReadAllLines(targetDirectoryFileFullName).ToList();
            List<string> selected = File.ReadAllLines(userSelectedFileFullName).ToList();

            // 데이터의 줄 개수가 다른 경우
            if (target.Count != selected.Count)
            {
                return (false, "Row");
            }

            // 데이터가 동일하지 않을 경우
            for (int i = 0; i < target.Count; i++)
            {
                // 데이터가 다를경우
                if (target[i] != selected[i])
                {
                    // LogicFile은 복사하면서 관련 설정을 변경하므로 확인하지 않음
                    if (target[i].Contains("LogicFile"))
                        continue;

                    return (false, "Data");
                }
            }

            return (true, "");
        }

        /// <summary>
        /// xodr 파일 조회
        /// </summary>
        /// <param name="path">경로</param>
        /// <param name="networkFileName">파일명</param>
        /// <returns></returns>
        private (bool, string) SearchNetworkFile(string path, string networkFileName)
        {
            // 파일정보 설정
            FileInfo networkFileInfo = new FileInfo(networkFileName);
            string fileName = networkFileInfo.Name;

            // networkFileName에 절대경로가 있을 경우
            if (networkFileInfo.DirectoryName is not null && networkFileInfo.Exists)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(networkFileInfo.DirectoryName);
                foreach (var file in dirInfo.GetFiles())
                {
                    if (file.Name.Replace(file.Extension, "").Equals(fileName) || file.Name.Equals(fileName))
                    {
                        return (true, file.FullName);
                    }
                }
            }
            else // 그 외 경우
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                // 입력된 경로에서 파일을 탐색
                foreach (var file in dirInfo.GetFiles())
                {
                    if (file.Name.Replace(file.Extension, "").Equals(fileName) || file.Name.Equals(fileName))
                    {
                        return (true, file.FullName);
                    }
                }

                // 입력된 경로에서 찾지 못했을 경우
                // 상위 폴더로 이동해 해당 폴더에서 관련 파일을 모두 확인함
                foreach (var dir in dirInfo.Parent!.EnumerateDirectories())
                {
                    try
                    {
                        dir.GetAccessControl();

                        foreach (var file in dir.GetFiles())
                        {
                            if (file.Name.Replace(file.Extension, "").Equals(fileName) || file.Name.Equals(fileName))
                            {
                                return (true, file.FullName);
                            }
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        continue;
                    }
                }
            }

            return (false, "");
        }

        /// <summary>
        /// 시뮬레이션 파일 추가
        /// </summary>
        /// <param name="scenarioFileInfo">xosc 파일 정보</param>
        /// <param name="networkFileInfo">xodr 파일 정보</param>
        /// <param name="major">메인 버전</param>
        /// <param name="minor">서브 버전</param>
        private void AddSimulationItem(FileInfo scenarioFileInfo, FileInfo networkFileInfo, int major, int minor)
        {
            int cnt = ScenarioItems.Count;

            ScenarioItems.Add(new ScenarioFileInfo(scenarioFileInfo, networkFileInfo, major, minor, Shared.SimulationList.Count + 1));

            ScenarioFileInfo info = ScenarioItems[cnt++];
            Shared.SimulationList.Add(new SimulationModel(info.Scenario.OpenSCENARIO, info.Scenario.Catalog, major, minor, info.ScenarioFileFullName, info.NetworkFileFullName, Shared.SimulationList.Count));
        }

        private void SearchVissimExeFile()
        {
            string[] searchDirectory = new string[2] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) };

            foreach (string dir in searchDirectory)
            {
                TraverseDirectory(dir);

                if (_vissimexeFileFound)
                    break;
            }
        }

        private void TraverseDirectory(string path)
        {
            try
            {
                foreach (string dir in Directory.GetDirectories(path))
                {
                    // 재귀적으로 하위 디렉토리를 검색합니다.
                    TraverseDirectory(dir);

                    if (_vissimexeFileFound)
                        return;
                }
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }

            // 현재 디렉토리에서 수행할 작업
            if (_vissimexeFileFound)
                return;

            string[] files = Directory.GetFiles(path);

            foreach (var file in files)
            {
                if (Path.GetFileName(file).ToLower().StartsWith("vissim") && Path.GetExtension(file).ToLower() == ".exe")
                {
                    _vissimExeFile = file;
                    _vissimexeFileFound = true;
                    break;
                }
            }
        }

        #endregion

        /// <summary>
        /// 시나리오파일이 추가되거나, 삭제되었을 때 관련설정 반영
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScenarioItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SetStateColor(nameof(ScenarioItems));
            CheckSimulationState();

            if (Shared.ScenarioFileList.Count == 1)
            {
                SelectedScenarioIndex = -1;
                SelectedScenarioFileInfo = null;
            }
        }

        /// <summary>
        /// Debug용 테스트 데이터 입력
        /// </summary>
        private void InitTestData()
        {
            if (Shared.SimulationList.Count > 0)
                return;

            DllFileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\DrivingSimulatorProxy.dll";
            string scenarioFile = $"{_xoscDirectory}FollowLeadingVehicle.xosc";
            string networkFile = $"{_xodrDirectory}Town01.xodr";

            if (!File.Exists(scenarioFile))
                return;

            if (!File.Exists(networkFile))
                return;

            XmlParser parser = new XmlParser();
            FileInfo scenarioFileInfo = new FileInfo(scenarioFile);
            FileInfo networkfileinfo = new FileInfo(networkFile);
            (int major, int minor) versions = parser.GetScenarioVersion(scenarioFile);

            if (networkfileinfo.Exists)
            {
                ScenarioItems.Add(new ScenarioFileInfo(scenarioFileInfo, networkfileinfo, versions.major, versions.minor, Shared.SimulationList.Count + 1));

                ScenarioFileInfo info = ScenarioItems[0];

                Shared.SimulationList.Add(new SimulationModel(info.Scenario.OpenSCENARIO, info.Scenario.Catalog, versions.major, versions.minor, info.ScenarioFileFullName, info.NetworkFileFullName, Shared.SimulationList.Count));
            }
        }
    }
}
