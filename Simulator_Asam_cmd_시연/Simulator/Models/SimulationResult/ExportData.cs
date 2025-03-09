using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Media.Media3D;

using Microsoft.Office.Interop.Excel;

using Simulator.Common;

namespace Simulator.Models.SimulationResult
{
	/// <summary>
	/// 시뮬레이션에서 산출된 데이터를 EXCEL로 출력
	/// </summary>
	internal class ExportData
	{
		/// <summary>
		/// Excel Application
		/// </summary>
		private Application application { get; set; }
		/// <summary>
		/// Excel Workbook
		/// </summary>
		private Workbook workbook { get; set; }
		/// <summary>
		/// Excel Worksheet
		/// </summary>
		private Worksheet worksheet { get; set; }
		/// <summary>
		/// 입력 범위
		/// </summary>
		private Microsoft.Office.Interop.Excel.Range range { get; set; }

		/// <summary>
		/// 엑셀 저장 경로
		/// </summary>
		private string excelPath { get; init; }

		/// <summary>
		/// 생성자
		/// </summary>
		/// <param name="settingData">시뮬레이션 세팅 데이터</param>
		/// <param name="averageData">시뮬레이션 평균값 데이터</param>
		/// <param name="egoDistance">자율주행차량 안전거리 통계 데이터</param>
		/// <param name="egoSpeed">자율주행차량 제한속도준수 통계 데이터</param>
		/// <param name="egoSignal">자율주행차량 신호준수율 통계 데이터</param>
		/// <param name="egoDistanceRaw">자율주행차량 안전거리 수집 데이터</param>
		/// <param name="egoSpeedRaw">자율주행차량 제한속도준수 수집 데이터</param>
		/// <param name="egoSignalRaw">자율주행차량 신호준수율 수집 데이터</param>
		/// <param name="writeRawFiles">수집 데이터 출력 여부</param>
		public ExportData(SettingData settingData, Dictionary<(int, int), AverageData> averageData,
			Dictionary<int, EgoSafety.Distance> egoDistance, Dictionary<int, EgoSafety.Speed> egoSpeed, Dictionary<int, EgoSafety.Signal> egoSignal,
			Dictionary<int, List<EgoSafety.DistanceRaw>> egoDistanceRaw, Dictionary<int, List<EgoSafety.SpeedRaw>> egoSpeedRaw, Dictionary<int, List<EgoSafety.SignalRaw>> egoSignalRaw,
			bool writeRawFiles)
		{
			// 엑셀 파일 저장경로 설정
			excelPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Simulator\Resources\vissim\report_psi\chart-server\server\data\";

			// 엑셀파일 실행
			application = new Application();
			// 워크북 생성
			workbook = application.Workbooks.Add();
			// 워크시트 생성
			worksheet = application.Worksheets.Item[1] as Worksheet;
			// 워크시트 이름 설정
			worksheet.Name = "Result";

			// 기본 디자인 생성
			SetBasicDesigne();
			// 시뮬레이션 기본 설정값 입력
			WriteSettingData(settingData);
			// 시뮬레이션 평균값 입력
			WriteAverageData(averageData, settingData.SimulationPeriod, settingData.SimulationBreakAt, settingData.SimulationResolution);
			// 자율주행차 제한속도준수 통계 데이터 입력
			WriteSafetySpeed(egoSpeed, settingData.SimulationResolution);
			// 자율주행차 안전거리준수율 통계 데이터 입력
			WriteSafetyDistance(egoDistance, settingData.SimulationResolution);
			// 자율주행차 신호준수율 통계 데이터 입력
			WriteSafetySignal(egoSignal, settingData.SimulationResolution);

			// 엑셀 데이터 저장
			SaveExcel($"{settingData.VissimNetworkFileName}", settingData.LosName, false);

			// 수집 데이터출력하도록 설정했을 경우
			if (writeRawFiles)
			{
				// 엑셀파일 실행
				application = new Application();
				// 워크북 생성
				workbook = application.Workbooks.Add();
				// 워크시트 생성
				worksheet = application.Worksheets.Item[1] as Worksheet;
				// 워크시트 이름 설정
				worksheet.Name = "Speed";

				// 자율주행차 제한속도준수 수집 데이터 입력
				WriteSafetySpeedRaw(egoSpeedRaw);
				// 자율주행차 안전거리준수율 수집 데이터 입력
				WriteSafetyDistanceRaw(egoDistanceRaw);
				// 자율주행차 신호준수율 수집 데이터 입력
				WriteSafetySignalRaw(egoSignalRaw);

				// 엑셀 데이터 저장
				SaveExcel($"{settingData.VissimNetworkFileName}", settingData.LosName, true);
			}
		}

		/// <summary>
		/// 종료자, 엑셀파일 저장 후 종료할 떄 관련 Process가 생성되어 있을 경우 종료함
		/// </summary>
		~ExportData()
		{
			// 워크북 닫기
			workbook.Close();
			// 엑셀 어플리케이션 종료
			application.Quit();

			/// 관련 자원 회수
			ReleaseExcelObject(workbook);
			ReleaseExcelObject(application);

			/// excel 프로세스가 생성되어있는지 확인
			/// 사용자가 excel을 실행한 경우 MainWindowTitle 값이 "" 형태와 다름,
			/// "" 값인 형태에만 종료함
			foreach (var process in Process.GetProcesses())
			{
				// excel 프로세스가 생성되어 있을 떄
				if (process.ProcessName.ToLower().Contains("excel"))
				{
					// excel 명칭이 지정되어있지 않을 때
					if (process.MainWindowTitle == "")
					{
						// process 종료
						process.Kill();
					}
				}
			}
		}

		/// <summary>
		/// 엑셀 디자인 기본 세팅
		/// </summary>
		private void SetBasicDesigne()
		{
			#region Simulation Info

			// Simulation Setting
			range = worksheet.Range[$"A{1}:B{2}"];
			SetStyle(range, true, true);
			range.Value = "Simulation Setting";

			// XOSC File Name
			range = worksheet.Range[$"A{3}"];
			SetStyle(range, false, true);
			range.Value = "XOSC File";

			// XODR File Name
			range = worksheet.Range[$"A{4}"];
			SetStyle(range, false, true);
			range.Value = "XODR File";

			// LOS 값
			range = worksheet.Range[$"A{5}"];
			SetStyle(range, false, true);
			range.Value = "LOS";

			// Random Seed
			range = worksheet.Range[$"A{6}"];
			SetStyle(range, false, true);
			range.Value = "Random Seed";

			// Resolution
			range = worksheet.Range[$"A{7}"];
			SetStyle(range, false, true);
			range.Value = "Resolution";

			// Break At
			range = worksheet.Range[$"A{8}"];
			SetStyle(range, false, true);
			range.Value = "Break At";

			// Period
			range = worksheet.Range[$"A{9}"];
			SetStyle(range, false, true);
			range.Value = "Period";

			#endregion
			#region Average Values
			// Average Values

			// Section
			range = worksheet.Range[$"C{1}:C{2}"];
			SetStyle(range, true, true);
			range.Value = "Junction";

			#region traffic Operation Efficiency

			// Traffic Operation Efficiency
			range = worksheet.Range[$"D{1}:H{1}"];
			SetStyle(range, true, true);
			range.Value = "Traffic Operation Efficiency";

			// Delay(avg)
			range = worksheet.Range[$"D{2}"];
			SetStyle(range, true, true);
			range.Value = "Delay(avg)  [s]";

			// Speed(avg)
			range = worksheet.Range[$"E{2}"];
			SetStyle(range, true, true);
			range.Value = "Speed(avg)  [km/h]";

			// Queue Length(avg)
			range = worksheet.Range[$"F{2}"];
			SetStyle(range, true, true);
			range.Value = "Queue Length(avg)  [m]";

			// Travel Time(avg)
			range = worksheet.Range[$"G{2}"];
			SetStyle(range, true, true);
			range.Value = "Travel Time(Avg)  [s]";

			// Distance Traveled(avg)
			range = worksheet.Range[$"H{2}"];
			SetStyle(range, true, true);
			range.Value = "Distance Traveled(avg)  [m]";


			#endregion
			#region Traffic Safety

			// Traffic Safety
			range = worksheet.Range[$"I{1}:M{1}"];
			SetStyle(range, true, true);
			range.Value = "Traffic Safety";

			// Speed Deviation(avg)
			range = worksheet.Range[$"I{2}"];
			SetStyle(range, true, true);
			range.Value = "Speed Deviation(Avg)  [km/h]";

			// Conflict(Section)
			range = worksheet.Range[$"J{2}"];
			SetStyle(range, true, true);
			range.Value = "Conflict(Section)  [TTC ≤ 0.8]";

			// Time To Collision(avg)
			range = worksheet.Range[$"K{2}"];
			SetStyle(range, true, true);
			range.Value = "TTC(avg)  [s]";

			// Rral-time Safety TTC Index
			range = worksheet.Range[$"L{2}"];
			SetStyle(range, true, true);
			range.Value = "RSTI(Section)";

			// Real-time Stopping distance Index
			range = worksheet.Range[$"M{2}"];
			SetStyle(range, true, true);
			range.Value = "RSI(Section)";


			#endregion
			#region Traffic Environment

			// Traffic Environment
			range = worksheet.Range[$"N{1}:P{1}"];
			SetStyle(range, true, true);
			range.Value = "Traffic Environment";

			// Nox(avg)
			range = worksheet.Range[$"N{2}"];
			SetStyle(range, true, true);
			range.Value = "Nox(avg)  [g]";

			// CO(avg)
			range = worksheet.Range[$"O{2}"];
			SetStyle(range, true, true);
			range.Value = "CO(avg)  [g]";

			// Fuel Consumtion(Avg)
			range = worksheet.Range[$"P{2}"];
			SetStyle(range, true, true);
			range.Value = "Fuel Consumtion(avg)  [L]";


			#endregion
			#endregion

			// 입력된 셀 테두리 
			/*
			range = worksheet.Range[$"A{1}:P{14}"];
			SetStyle(range, false, false, true);
			*/
			range = worksheet.Range[$"A{1}:B{9}"];
			SetStyle(range, false, false, true);

			range = worksheet.Range[$"C{1}:P{2}"];
			SetStyle(range, false, false, true);

			/*
			// Simulation Log 추가 전 빈공간 추가
			string startCell = "A" + (15).ToString();
			string endCell = "P" + (17).ToString();
			range = worksheet.Range[$"{startCell}:{endCell}"];
			SetStyle(range, true);
			*/

			// 엑셀파일 행 고정
			range = worksheet.Range["D3"];
			range.Select();
			worksheet.Application.ActiveWindow.FreezePanes = true;
		}

		/// <summary>
		/// 시뮬레이션 기본설정값 입력
		/// </summary>
		private void WriteSettingData(SettingData settingData)
		{
			// XOSC 파일명
			range = worksheet.Range[$"B{3}"];
			range.Value = settingData.ScenarioFileName;

			// XODR 파일명
			range = worksheet.Range[$"B{4}"];
			range.Value = settingData.NetworkFileName;

			// LOS 이름
			range = worksheet.Range[$"B{5}"];
			range.Value = settingData.LosName.Substring(settingData.LosName.Length - 1).ToUpper();

			// Random Seed
			range = worksheet.Range[$"B{6}"];
			range.Value = settingData.RandomSeed;

			// Simulation Resolution
			range = worksheet.Range[$"B{7}"];
			range.Value = settingData.SimulationResolution;

			// Simulation Break At
			range = worksheet.Range[$"B{8}"];
			range.Value = settingData.SimulationBreakAt;

			// Simulation Period
			range = worksheet.Range[$"B{9}"];
			range.Value = settingData.SimulationPeriod;
		}

		/// <summary>
		/// 평균값 데이터 입력
		/// </summary>
		private void WriteAverageData(Dictionary<(int, int), AverageData> simulationAverageData, double simulationPeriod, double simulationBreakAt, int simulationResolution)
		{
			// 시뮬레이션 평균 데이터 구조체 생성
			object[,] data = new object[simulationAverageData.Count, 14];

			int index = 0;
			// 입력받은 데이터를 데이터 구조체에 입력
			foreach (var item in simulationAverageData)
			{
				// Junction_현시 구조로 관련값을 산출하므로, 관련 키값 생성
				string junction = $"{item.Key.Item1}_{item.Key.Item2}";
				// 데이터 추출할 개체
				AverageData averageData = item.Value;

				// RSTI 산출
				double RSTI = averageData.TTCI / ((simulationPeriod - simulationBreakAt) * 60 * simulationResolution);
				// RSI 산출
				double RSI = averageData.SDI / ((simulationPeriod - simulationBreakAt) * 60 * simulationResolution);

				// 측정 구역 이름 
				data[index, 0] = junction;
				data[index, 1] = Math.Round(averageData.Delay, 3);
				data[index, 2] = Math.Round(averageData.Speed, 3);
				data[index, 3] = Math.Round(averageData.QueueLength, 3);
				data[index, 4] = Math.Round(averageData.TravelTime, 3);
				data[index, 5] = Math.Round(averageData.DistanceTraveled, 3);
				data[index, 6] = Math.Round(averageData.SpeedDeviation, 3);
				data[index, 7] = averageData.NumberOfConflict;
				data[index, 8] = Math.Round(averageData.TTC / averageData.TTCCounter, 3);
				data[index, 9] = Math.Round(RSTI, 3);
				data[index, 10] = Math.Round(RSI, 3);
				data[index, 11] = Math.Round(averageData.Nox, 3);
				data[index, 12] = Math.Round(averageData.CO, 3);
				data[index, 13] = Math.Round(averageData.FuelConsumtion, 3);

				index++;
			}

			// 데이터 저장위치 설정
			range = worksheet.Range[$"C{3}", $"P{3 + simulationAverageData.Count - 1}"];
			// 데이터 저장
			range.Value = data;
			// 스타일 설정
			SetStyle(range, false, false, false, true);
		}

		/// <summary>
		/// 자율주행차 제한속도준수율 통계 데이터 저장
		/// </summary>
		/// <param name="egoSpeed">자율주행차 통계 데이터</param>
		/// <param name="simulationResolution">시뮬레이션 해상도</param>
		private void WriteSafetySpeed(Dictionary<int, EgoSafety.Speed> egoSpeed, int simulationResolution)
		{
			// worksheet가 없을 경우, 시트 생성
			if ( application.Worksheets.Count < 2 )
			{
				worksheet = application.Worksheets.Add(After: application.Worksheets.Item[1]);
				worksheet.Name = "Speed";
			}

			// 저장할 worksheet 설정
			worksheet = application.Worksheets.Item[2] as Worksheet;

			/// 엑셀 시트에 column name 설정
			range = worksheet.Range[$"A{1}"];
			range.Value = "차량 번호";

			range = worksheet.Range[$"B{1}"];
			range.Value = "제한속도 준수 여부";

			range = worksheet.Range[$"C{1}"];
			range.Value = "제한속도 준수 비율";

			range = worksheet.Range[$"D{1}"];
			range.Value = "제한속도 비준수 강도";

			range = worksheet.Range[$"E{1}"];
			range.Value = "주행 시간 [s]";

			range = worksheet.Range[$"F{1}"];
			range.Value = "제한속도 비준수 시간  [s]";

			// column name 스타일 지정
			range = worksheet.Range[$"A{1}:F{1}"];
			SetStyle(range, false, true, true);

			/// 자율주행차량 제한속도준수율 관련 데이터 저장
			// 제한속도준수율 저장할 개체 생성
			object[,] data = new object[egoSpeed.Count, 6];

			int index = 0;
			// 받은 데이터를 모두 파싱
			foreach ( var pair in egoSpeed )
			{
				// 자율주행차량 번호
				data[index, 0] = pair.Key;
				data[index, 1] = (pair.Value.OverSpeedCount > 0) ? false : true;
				data[index, 2] = Math.Round(((pair.Value.CheckCount - pair.Value.OverSpeedCount) / (double)pair.Value.CheckCount) * 100, 2) + "%";
				data[index, 3] = Math.Round(pair.Value.Penalty, 2);
				data[index, 4] = Math.Round((double)pair.Value.CheckCount / simulationResolution, 2);
				data[index, 5] = Math.Round((double)pair.Value.OverSpeedCount / simulationResolution, 2);

				index++;
			}

			/// 입력될 셀 범위 지정
			string startCell = "A" + (2).ToString();
			string endCell = "F" + (2 + index - 1).ToString();

			range = worksheet.Range[startCell, endCell];
			VissimDataEdgeBorderLine(range, true);
			// 셀 범위에 측정값 입력
			range.Value = data;

			// 엑셀파일 행 고정
			range = worksheet.Range["A2"];
			range.Select();
			worksheet.Application.ActiveWindow.FreezePanes = true;
		}

		/// <summary>
		/// 자율주행차 제한속도준수율 수집 데이터 저장
		/// </summary>
		/// <param name="egoSpeedRaw">자율주행차 제한속도준수율 수집 데이터</param>
		private void WriteSafetySpeedRaw(Dictionary<int, List<EgoSafety.SpeedRaw>> egoSpeedRaw)
		{
			/// 워크시트 column name 지정
			range = worksheet.Range[$"A{1}"];
			range.Value = "차량 번호";

			range = worksheet.Range[$"B{1}"];
			range.Value = "시뮬레이션 시간";

			range = worksheet.Range[$"C{1}"];
			range.Value = "도로";

			range = worksheet.Range[$"D{1}"];
			range.Value = "차선";

			range = worksheet.Range[$"E{1}"];
			range.Value = "과속 유무";

			range = worksheet.Range[$"F{1}"];
			range.Value = "속도  [km/h]";

			range = worksheet.Range[$"G{1}"];
			range.Value = "가속도  [m/s^2]";

			// 워크시트 column 스타일 지정
			range = worksheet.Range[$"A{1}:G{1}"];
			SetStyle(range, false, true, true);

			int count = 0;

			// 수집 데이터 개수 산출
			foreach (var pair in egoSpeedRaw)
			{
				count += pair.Value.Count;
			}

			// 수집 데이터 저장 구조체 생성
			object[,] data = new object[count, 7];

			/// 수집 데이터 파싱
			int index = 0;
			foreach ( var pair in egoSpeedRaw )
			{
				foreach (var rawData in pair.Value)
				{
					// 자율주행차량 번호
					data[index, 0] = pair.Key;
					data[index, 1] = rawData.SimulationSecond;
					data[index, 2] = rawData.EgoLinkNumber;
					data[index, 3] = rawData.EgoLaneNumber;
					data[index, 4] = rawData.OverSpeed;
					data[index, 5] = rawData.EgoSpeed;
					data[index, 6] = rawData.EgoAcceleration;

					index++;
				}
			}

			/// 데이터 입력될 셀 지정
			string startCell = "A" + (2).ToString();
			string endCell = "G" + (2 + index - 1).ToString();

			range = worksheet.Range[startCell, endCell];
			VissimDataEdgeBorderLine(range, true);
			// 입력 데이터 저장
			range.Value = data;

			// 엑셀파일 행 고정
			range = worksheet.Range["A2"];
			range.Select();
			worksheet.Application.ActiveWindow.FreezePanes = true;
		}

		/// <summary>
		/// 자율주행차 안전거리준수율 통계 데이터 입력
		/// </summary>
		/// <param name="egoDistance">자율주행차량 안전거리준수율 통계 데이터</param>
		/// <param name="simulationResolution">시뮬레이션 해상도</param>
		private void WriteSafetyDistance(Dictionary<int, EgoSafety.Distance> egoDistance, int simulationResolution)
		{
			// worksheet 생성 안되어있을 경우 새로 생성함
			if ( application.Worksheets.Count < 3 )
			{
				worksheet = application.Worksheets.Add(After: application.Worksheets.Item[2]);
				worksheet.Name = "Distnace";
			}

			// 데이터 입력할 worksheet 지정
			worksheet = application.Worksheets.Item[3] as Worksheet;

			/// column name 지정
			range = worksheet.Range[$"A{1}"];
			range.Value = "차량 번호";

			range = worksheet.Range[$"B{1}"];
			range.Value = "질량에 따른 추가 안전거리 확보율";

			range = worksheet.Range[$"C{1}"];
			range.Value = "안전거리 확보율";

			range = worksheet.Range[$"D{1}"];
			range.Value = "주행 시간  [s]";

			// column name 스타일 지정
			range = worksheet.Range[$"A{1}:D{1}"];
			SetStyle(range, false, true, true);

			// 안전거리준수율 저장할 구조체 생성
			object[,] data = new object[egoDistance.Count, 4];

			// 관련 데이터 입력
			int index = 0;
			foreach ( var pair in egoDistance )
			{
				// 자율주행차량 번호
				data[index, 0] = pair.Key;
				data[index, 1] = Math.Round(((pair.Value.CheckCount - pair.Value.NotConsiderWeight) / (double)pair.Value.CheckCount) * 100, 2) + "%";
				data[index, 2] = Math.Round(((pair.Value.CheckCount - pair.Value.NotConsiderSafeDistance) / (double)pair.Value.CheckCount) * 100, 2) + "%";
				data[index, 3] = Math.Round((double)pair.Value.CheckCount / simulationResolution, 2);

				index++;
			}

			// 데이터 입력 범위 지정
			string startCell = "A" + (2).ToString();
			string endCell = "D" + (2 + index - 1).ToString();

			/* 통신지연 제외 
			range = worksheet.Range[$"A{1}"];
			range.Value = "차량 번호";

			range = worksheet.Range[$"B{1}"];
			range.Value = "질량에 따른 추가 안전거리 확보율";

			range = worksheet.Range[$"C{1}"];
			range.Value = "통신지연을 고려한 안전거리 확보율";

			range = worksheet.Range[$"D{1}"];
			range.Value = "안전거리 확보율";

			range = worksheet.Range[$"E{1}"];
			range.Value = "주행 시간 [s]";

			range = worksheet.Range[$"A{1}:E{1}"];
			SetStyle(range, false, true, true);

			object[,] data = new object[egoDistance.Count, 5];

			int index = 0;
			foreach ( var pair in egoDistance )
			{
				data[index, 0] = pair.Key;
				data[index, 1] = Math.Round(((pair.Value.CheckCount - pair.Value.NotConsiderWeight) / (double)pair.Value.CheckCount) * 100, 2) + "%";
				data[index, 2] = Math.Round(((pair.Value.CheckCount - pair.Value.NotConsiderLatency) / (double)pair.Value.CheckCount) * 100, 2) + "%";
				data[index, 3] = Math.Round(((pair.Value.CheckCount - pair.Value.NotConsiderSafeDistance) / (double)pair.Value.CheckCount) * 100, 2) + "%";
				data[index, 4] = Math.Round((double)pair.Value.CheckCount / simulationResolution, 2);

				index++;
			}

			string startCell = "A" + (2).ToString();
			string endCell = "E" + (2 + index - 1).ToString();

			*/

			range = worksheet.Range[startCell, endCell];
			VissimDataEdgeBorderLine(range, true);
			// 데이터 입력
			range.Value = data;

			// 엑셀파일 행 고정
			range = worksheet.Range["A2"];
			range.Select();
			worksheet.Application.ActiveWindow.FreezePanes = true;
		}

		/// <summary>
		/// 자율주행차량 안전거리준수율 수집 데이터 저장
		/// </summary>
		/// <param name="egoDistanceRaw">자율주행차량 안전거리준수율 수집 데이터</param>
		private void WriteSafetyDistanceRaw(Dictionary<int, List<EgoSafety.DistanceRaw>> egoDistanceRaw)
		{
			// worksheet에 sheet가 없을 경우 새로 생성
			if ( application.Worksheets.Count < 2 )
			{
				worksheet = application.Worksheets.Add(After: application.Worksheets.Item[1]);
				worksheet.Name = "Distnace";
			}

			// 데이터 입력할 worksheet 생성
			worksheet = application.Worksheets.Item[2] as Worksheet;

			/// column name 생성
			range = worksheet.Range[$"A{1}"];
			range.Value = "차량 번호";

			range = worksheet.Range[$"B{1}"];
			range.Value = "시뮬레이션 시간";

			range = worksheet.Range[$"C{1}"];
			range.Value = "속도  [km/h]";

			range = worksheet.Range[$"D{1}"];
			range.Value = "가속도  [m/s^2]";

			range = worksheet.Range[$"E{1}"];
			range.Value = "무게  [kg]";

			range = worksheet.Range[$"F{1}"];
			range.Value = "센서 검지 유형";

			range = worksheet.Range[$"G{1}"];
			range.Value = "앞 차량 속도  [km/h]";

			range = worksheet.Range[$"H{1}"];
			range.Value = "앞 차량 무게  [kg]";

			range = worksheet.Range[$"I{1}"];
			range.Value = "앞 차량과의 속도 차이  [km/h]";

			range = worksheet.Range[$"J{1}"];
			range.Value = "앞 차량과의 거리  [m]";

			range = worksheet.Range[$"K{1}"];
			range.Value = "안전거리  [m]";

			range = worksheet.Range[$"L{1}"];
			range.Value = "질량차 추가 안전거리  [m]";

			// column name 스타일 지정
			range = worksheet.Range[$"A{1}:L{1}"];
			SetStyle(range, false, true, true);

			// rawData 개수 산출
			int count = 0;
			foreach (var pair in egoDistanceRaw)
			{
				count += pair.Value.Count;
			}

			// 안전거리지표 저장할 데이터 구조체 생성
			object[,] data = new object[count, 12];

			// 수집 데이터 입력
			int index = 0;
			foreach ( var pair in egoDistanceRaw )
			{
				foreach (var rawData in pair.Value)
				{
					// 자율주행차 번호
					data[index,  0] = pair.Key;
					data[index,  1] = rawData.SimulationSecond;
					data[index,  2] = rawData.EgoSpeed;
					data[index,  3] = rawData.EgoAcceleration;
					data[index,  4] = rawData.EgoWeight;
					data[index,  5] = rawData.EgoLeadTargetType;

					// 검지된 물체가 차량일 경우, 차량 관련데이터도 입력함
					if (rawData.EgoLeadTargetType == "VEHICLE")
					{
						data[index, 6] = rawData.AheadSpeed;
						data[index, 7] = rawData.AheadWeight;
						data[index, 8] = rawData.EgoSpeedDifference;
						data[index, 9] = rawData.EgoFollowDistance;
						data[index, 10] = rawData.EgoSpeed == 0 ? "-" : rawData.SafetyDistance;
						data[index, 11] = 
							(rawData.EgoSpeed == 0) ? "-" : 
							(rawData.EgoWeight == 0 || rawData.AheadWeight == 0) ? "-" :
							rawData.ConsiderWeightSafetyDistance;
					}

					index++;
				}
			}

			// 데이터 저장할 범위 지정
			string startCell = "A" + (2).ToString();
			string endCell = "L" + (2 + index - 1).ToString();

			range = worksheet.Range[startCell, endCell];
			VissimDataEdgeBorderLine(range, true);
			// 데이터 저장
			range.Value = data;

			// 엑셀 행고정
			range = worksheet.Range["A2"];
			range.Select();
			worksheet.Application.ActiveWindow.FreezePanes = true;
		}

		/// <summary>
		/// 자율주행차량 신호준수율 통계 데이터 저장
		/// </summary>
		/// <param name="egoSignal">자율주행차량 신호준수율 통계 데이터</param>
		/// <param name="simulationResolution">시뮬레이션 해상도</param>
		private void WriteSafetySignal(Dictionary<int, EgoSafety.Signal> egoSignal, int simulationResolution)
		{
			// worksheet에 관련 항목이 없을 경우 추가함
			if ( application.Worksheets.Count < 4 )
			{
				worksheet = application.Worksheets.Add(After: application.Worksheets.Item[3]);
				worksheet.Name = "Signal";

				/// Panelty값 출력
				range = worksheet.Range["K1"];
				range.Value = "녹색 가중치";

				range = worksheet.Range["L1"];
				range.Value = (int)EgoSafety.SignalPenalty.Green;

				range = worksheet.Range["K2"];
				range.Value = "황색 전반 가중치";

				range = worksheet.Range["L2"];
				range.Value = (int)EgoSafety.SignalPenalty.AmberSafe;

				range = worksheet.Range["K3"];
				range.Value = "황색 후반 가중치";

				range = worksheet.Range["L3"];
				range.Value = (int)EgoSafety.SignalPenalty.AmberUnsafe;

				range = worksheet.Range["K4"];
				range.Value = "적색 가중치";

				range = worksheet.Range["L4"];
				range.Value = (int)EgoSafety.SignalPenalty.Red;
			}

			// worksheet 지정
			worksheet = application.Worksheets.Item[4] as Worksheet;

			/// column name 생성
			range = worksheet.Range[$"A{1}"];
			range.Value = "차량 번호";

			range = worksheet.Range[$"B{1}"];
			range.Value = "교차로 진입 시점 황색등 여부";

			range = worksheet.Range[$"C{1}"];
			range.Value = "패널티 합";

			range = worksheet.Range[$"D{1}"];
			range.Value = $"녹색 [s]";

			range = worksheet.Range[$"E{1}"];
			range.Value = $"황색 전반 [s]";

			range = worksheet.Range[$"F{1}"];
			range.Value = $"황색 후반 [s]";

			range = worksheet.Range[$"G{1}"];
			range.Value = $"적색 [s]";

			range = worksheet.Range[$"H{1}"];
			range.Value = "제한속도 비준수 강도";

			range = worksheet.Range[$"I{1}"];
			range.Value = "측정 시간 [s]";

			// column name 스타일 생성
			range = worksheet.Range[$"A{1}:I{1}"];
			SetStyle(range, false, true, true);

			// 신호준수율 통계 데이터 저장 구조체 생성
			object[,] data = new object[egoSignal.Count, 9];

			// 관련 데이터 입력
			int index = 0;
			foreach ( var pair in egoSignal )
			{
				// 자율주행차량 번호
				data[index, 0] = pair.Key;
				data[index, 1] = pair.Value.PassedInAmber;
				data[index, 2] = Math.Round(pair.Value.SigPenalty, 2);
				data[index, 3] = Math.Round((double)pair.Value.InGreen / simulationResolution, 2);
				data[index, 4] = Math.Round((double)pair.Value.InAmberAhead / simulationResolution, 2);
				data[index, 5] = Math.Round((double)pair.Value.InAmberLast / simulationResolution, 2);
				data[index, 6] = Math.Round((double)pair.Value.InRed / simulationResolution, 2);
				data[index, 7] = Math.Round(pair.Value.Penalty, 2);
				data[index, 8] = Math.Round((double)pair.Value.CheckCount / simulationResolution, 2);

				index++;
			}

			/// 데이터 저장할 범위 지정
			string startCell = "A" + (2).ToString();
			string endCell = "I" + (2 + index - 1).ToString();

			range = worksheet.Range[startCell, endCell];
			VissimDataEdgeBorderLine(range, true);
			// 데이터 저장
			range.Value = data;

			// 엑셀 행 고정ㄴ
			range = worksheet.Range["A2"];
			range.Select();
			worksheet.Application.ActiveWindow.FreezePanes = true;
		}

		/// <summary>
		/// 자율주행차량 신호준수율 수집 데이터 저장
		/// </summary>
		/// <param name="egoSignalRaw">자율주행차량 신호준수율 수집 데이터</param>
		private void WriteSafetySignalRaw(Dictionary<int, List<EgoSafety.SignalRaw>> egoSignalRaw)
		{
			// worksheet 없을 경우 새로 생성
			if ( application.Worksheets.Count < 3 )
			{
				worksheet = application.Worksheets.Add(After: application.Worksheets.Item[2]);
				worksheet.Name = "Signal";
			}

			// worksheet 지정
			worksheet = application.Worksheets.Item[3] as Worksheet;

			/// column name 생성
			range = worksheet.Range[$"A{1}"];
			range.Value = "차량 번호";

			range = worksheet.Range[$"B{1}"];
			range.Value = "시뮬레이션 시간";

			range = worksheet.Range[$"C{1}"];
			range.Value = "도로";

			range = worksheet.Range[$"D{1}"];
			range.Value = "차선";

			range = worksheet.Range[$"E{1}"];
			range.Value = $"차량 위치";

			range = worksheet.Range[$"F{1}"];
			range.Value = $"신호 존재 유무";

			range = worksheet.Range[$"G{1}"];
			range.Value = $"신호 위치";

			range = worksheet.Range[$"H{1}"];
			range.Value = $"신호 통과 여부";

			range = worksheet.Range[$"I{1}"];
			range.Value = $"녹색 현시 시작시간  [s]";

			range = worksheet.Range[$"J{1}"];
			range.Value = "황색 현시 시작시간  [s]";

			range = worksheet.Range[$"K{1}"];
			range.Value = "황색 현시 종료시간  [s]";

			range = worksheet.Range[$"L{1}"];
			range.Value = "패널티";

			range = worksheet.Range[$"M{1}"];
			range.Value = "녹색 통과 여부";

			range = worksheet.Range[$"N{1}"];
			range.Value = "황색 전반 통과 여부";

			range = worksheet.Range[$"O{1}"];
			range.Value = "황색 후반 통과 여부";

			range = worksheet.Range[$"P{1}"];
			range.Value = "적색 통과 여부";

			// column name 스타일 지정
			range = worksheet.Range[$"A{1}:P{1}"];
			SetStyle(range, false, true, true);

			// 데이터 수 확인
			int count = 0;
			foreach (var pair in egoSignalRaw)
			{
				count += pair.Value.Count;
			}

			// 자율주행차량 신호준수율 저장할 데이터 개체 생성
			object[,] data = new object[count, 16];

			// 데이터 저장
			int index = 0;
			foreach ( var pair in egoSignalRaw )
			{
				foreach (var rawData in pair.Value)
				{
					// 자율주행차 번호
					data[index,  0] = pair.Key;
					data[index,  1] = rawData.SimulationSecond;
					data[index,  2] = rawData.EgoLinkNumber;
					data[index,  3] = rawData.EgoLaneNumber;
					data[index,  4] = rawData.EgoPosition;
					data[index,  5] = rawData.SignalHeadExist;

					// 신호가 있는 경우 신호 데이터 추가 출력
					if (rawData.SignalHeadExist)
					{
						data[index, 6] = rawData.SignalHeadPosition;
						data[index, 7] = rawData.IsSignalHeadPassed;
						data[index, 8] = rawData.GreenStartTime;
						data[index, 9] = rawData.AmberStartTime;
						data[index, 10] = rawData.AmberEndTime;
						data[index, 11] = rawData.Panelty;
						data[index, 12] = rawData.IsPassedInGreen;
						data[index, 13] = rawData.IsPassedInAmberSafe;
						data[index, 14] = rawData.IsPassedInAmberUnsafe;
						data[index, 15] = rawData.IsPassedInRed;
					}
					/*else
					{
						data[index,  6] = "";
						data[index,  7] = "";
						data[index,  8] = "";
						data[index,  9] = "";
						data[index, 10] = "";
						data[index, 11] = "";
						data[index, 12] = "";
						data[index, 13] = "";
						data[index, 14] = "";
						data[index, 15] = "";
					}*/

					index++;
				}
			}

			/// 데이터 저장 범위 설정
			string startCell = "A" + (2).ToString();
			string endCell = "P" + (2 + index - 1).ToString();

			range = worksheet.Range[startCell, endCell];
			VissimDataEdgeBorderLine(range, true);
			// 데이터 저장
			range.Value = data;

			// 엑셀 행 고정ㄴ
			range = worksheet.Range["A2"];
			range.Select();
			worksheet.Application.ActiveWindow.FreezePanes = true;
		}


		/// <summary>
		/// range에 입력된 셀에 형식 부여
		/// </summary>
		/// <param name="range">셀 범위</param>
		/// <param name="merge">병합</param>
		/// <param name="bold">글자 굵게</param>
		/// <param name="border">테두리(범위 내 모든 셀)</param>
		/// <param name="edgeBorder">테두리(외곽)</param>
		private void SetStyle(Microsoft.Office.Interop.Excel.Range range, bool merge = false, bool bold = false, bool border = false, bool edgeBorder = false)
		{
			if ( merge )
			{
				MergeCell(range);
			}

			if ( bold )
			{
				BoldCharacter(range);
			}

			if ( border )
			{
				DrawBorderLine(range);
			}

			if ( edgeBorder )
			{
				DrawEdgeBorderLine(range);
			}
		}

		/// <summary>
		/// 지정 범위의 셀 병합
		/// </summary>
		/// <param name="range">셀 범위</param>
		private void MergeCell(Microsoft.Office.Interop.Excel.Range range)
		{
			range.Merge();
			range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
		}

		/// <summary>
		/// 지정 범위의 셀 가장자리에 테두리 생성
		/// </summary>
		/// <param name="range">셀 범위</param>
		private void DrawEdgeBorderLine(Microsoft.Office.Interop.Excel.Range range)
		{
			range.Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
			range.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
			range.Borders[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
			range.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
		}

		/// <summary>
		/// 지정 범위의 셀 모두 테두리 생성
		/// </summary>
		/// <param name="range">셀 범위</param>
		private void DrawBorderLine(Microsoft.Office.Interop.Excel.Range range)
		{
			range.Borders.LineStyle = XlLineStyle.xlContinuous;
		}

		/// <summary>
		/// 지정 범위의 셀 문자 굵게 변경
		/// </summary>
		/// <param name="range">셀 범위</param>
		private void BoldCharacter(Microsoft.Office.Interop.Excel.Range range)
		{
			range.Font.Bold = true;
		}

		/// <summary>
		/// Vissim에서 수집한 데이터의 테두리 관련 설정
		/// </summary>
		/// <param name="range">지정 점위</param>
		/// <param name="underLine">밑줄 사용 여부</param>
		private void VissimDataEdgeBorderLine(Microsoft.Office.Interop.Excel.Range range, bool underLine = false)
		{
			if ( underLine == true )
			{
				range.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
			}

			range.Borders[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
			range.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
		}

		/// <summary>
		/// 엑셀 파일 저장
		/// </summary>
		/// <param name="fileName">저장할 파일 명</param>
		/// <param name="losName">LOS 값</param>
		/// <param name="isRowFile">수집데이텅 유문</param>
		private void SaveExcel(string fileName, string losName, bool isRowFile)
		{
			string excelName;

			// 각 worksheet의 column width를 자동 설정
			for ( int i = 0; i < application.Worksheets.Count; i++ )
			{
				worksheet = (Worksheet)application.Worksheets[i + 1];
				worksheet.Columns.AutoFit();
			}

			// 엑셀 파일 이름
			excelName = $"{fileName}_{VissimController.GetSimulationRunCount():D3}_{losName}";

            // 수집데이터 저장시 엑셀파일이름에 _Raw 추가
            if (isRowFile)
			{
				excelName += "_Raw";
            }
			else
			{
                Shared.ResultFileName = excelPath + excelName;
            }

			// 엑셀파일 저장
			workbook.SaveAs(excelPath + excelName);
		}

		/// <summary>
		/// Excel파일을 사용할 경우 메모리가 자동 해제되지 않아서 Process가 계속해서 생성되는 경우가 있음<br/>
		/// 관련사항을 해결하기위해 수동으로 메모리 해제를 요청함
		/// </summary>
		/// <param name="obj">메모리 해제할 object</param>
		private void ReleaseExcelObject(object obj)
		{
			try
			{
				if ( obj != null )
				{
					// 메모리 해제
					Marshal.ReleaseComObject(obj);
					obj = null;
				}
			}
			catch ( Exception ex )
			{
				obj = null;
				throw ex;
			}
			finally
			{
				// 데이터 메모리 정리 요청
				GC.Collect();
			}
		}
	}
}
