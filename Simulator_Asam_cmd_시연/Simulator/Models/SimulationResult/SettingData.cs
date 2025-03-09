using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Models.SimulationResult
{
	/// <summary>
	/// Vissim Simulation 설정 데이터
	/// </summary>
	internal class SettingData
	{
		/// <summary>
		/// Vissim 파일명
		/// </summary>
		internal string VissimNetworkFileName { get; set; }
		/// <summary>
		/// .xosc 파일명
		/// </summary>
		internal string ScenarioFileName { get; set; }
		/// <summary>
		/// .xodr 파일명
		/// </summary>
		internal string NetworkFileName { get; set; }
		/// <summary>
		/// Random Seed 설정값
		/// </summary>
		internal int RandomSeed { get; set; }
		/// <summary>
		/// Simulatino Resolution 설정값
		/// </summary>
		internal int SimulationResolution { get; set; }
		/// <summary>
		/// Simulation Break At 설정값
		/// </summary>
		internal double SimulationBreakAt { get; set; }
		/// <summary>
		/// Simulation Period 설정값
		/// </summary>
		internal double SimulationPeriod { get; set; }
		/// <summary>
		/// LOS 이름
		/// </summary>
		internal string LosName { get; set; }

		/// <summary>
		/// 생성자
		/// </summary>
		/// <param name="vissimNetworkFileName">Vissim 파일명</param>
		/// <param name="losName">LOS 이름</param>
        public SettingData(string vissimNetworkFileName, string losName)
        {
			VissimNetworkFileName = vissimNetworkFileName;
			LosName = losName;
        }
    }
}
