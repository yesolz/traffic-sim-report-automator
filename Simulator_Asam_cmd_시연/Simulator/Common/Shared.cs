using Simulator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Simulator
{
    /// <summary>
    /// 데이터 공유 목적 구조체
    /// Scenario Selector에서 생성된 데이터를 Simulator에서 같이 사용하기위한 목적으로 사용
    /// </summary>
    public static class Shared
    {
        /// <summary>
        /// Scenario Selector에서 데이터가 변경될 시 해당 툴팁 업데이트<br/>
        /// 시뮬레이터 실행 가능 상태에 대해 버튼 마우스 오버시 해당 문구 표출
        /// </summary>
        public static string SimulationToolTip { get; set; } = "";
		/// <summary>
		/// 시뮬레이션 실행 가능 유무<br/>
		/// 최초 실행시 dll파일, scenario 파일이 없기 때문에 true<br/>
		/// dll 입력, scenario 입력시 체크하여 실행유무 변경함
		/// </summary>
		public static bool IsSimulationDisable { get; set; } = true;

		/// <summary>
		/// 시뮬레이터가 실행시 참고하는 파일 목록<br/>
        /// .xodr, .xosc 파일 실행위치
		/// </summary>
		public static ScenarioFileList ScenarioFileList { get; set; } = new ScenarioFileList();
		/// <summary>
		/// 시뮬레이터가 자율주행차량에 입력할 DLL 파일 정보<br/>
		/// </summary>
		public static FileInfo? DllFileInfo { get; set; }

		/// <summary>
		/// 시뮬레이터가 실행 시 필요한 데이터 목록<br/>
		/// LOS 설정, 차량위치 등을 가지고 있음
		/// </summary>
		public static SimulationList SimulationList { get; set; } = new SimulationList();

		/// <summary>
		/// 내문서\\Simulator\\Resources\\vissim\\<br/>
		/// vissim에서 결과 파일을 출력할 디렉토리
		/// </summary>
		public static string VissimFileDirectory { get; set; }

		/// <summary>
		/// Simulator: SimulationStartButton, Listview 사용가능 유무
		/// MainWindow: 화면 전환 버튼 사용가능 유무
		/// </summary>
        public static bool IsButtonEnable { get; set; } = true;

		/// <summary>
		/// Scenario Select 화면에서 Vissim 편집기능 사용 시
		/// 제거 버튼, Listview Enable 등에 바인딩하여 사용
		/// </summary>
        public static bool IsSelectViewEditButtonEnable { get; set; } = true;

		public static int ScenarioIndex { get; set; }

		public static string ResultFileName { get; set; } = "";

    }
}
