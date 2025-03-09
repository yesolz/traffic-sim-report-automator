using System;
using System.IO;
using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Simulator.ViewModels
{
	public partial class ScenarioViewModel : BaseViewModel
	{
		/// <summary>
		/// Visibility 설정
		/// </summary>
		[ObservableProperty]
		private Visibility _visible;

		/// <summary>
		/// 선택된 시나리오 번호<br/>
		/// Shared에 저장된 SimulationList[_index] 항목으로 사용
		/// </summary>
		private int _index { get; set; }

		/// <summary>
		/// LOS A 설정값 반환
		/// </summary>
		public bool LosA
		{
			get
			{
				if ( Shared.SimulationList.Count > 0 && Shared.SimulationList[_index] is not null )
					return Shared.SimulationList[_index].LosA;
				else
					return false;
			}
			set
			{
				if ( value != Shared.SimulationList[_index].LosA )
				{
					Shared.SimulationList[_index].LosA = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// LOS B 설정값 반환
		/// </summary>
		public bool LosB
		{
			get
			{
				if ( Shared.SimulationList.Count > 0 && Shared.SimulationList[_index] is not null )
					return Shared.SimulationList[_index].LosB;
				else
					return false;
			}
			set
			{
				if ( value != Shared.SimulationList[_index].LosB )
				{
					Shared.SimulationList[_index].LosB = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// LOS C 설정값 반환
		/// </summary>
		public bool LosC
		{
			get
			{
				if ( Shared.SimulationList.Count > 0 && Shared.SimulationList[_index] is not null )
					return Shared.SimulationList[_index].LosC;
				else
					return false;
			}
			set
			{
				if ( value != Shared.SimulationList[_index].LosC )
				{
					Shared.SimulationList[_index].LosC = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// LOS D 설정값 반환
		/// </summary>
		public bool LosD
		{
			get
			{
				if ( Shared.SimulationList.Count > 0 && Shared.SimulationList[_index] is not null )
					return Shared.SimulationList[_index].LosD;
				else
					return false;
			}
			set
			{
				if ( value != Shared.SimulationList[_index].LosD )
				{
					Shared.SimulationList[_index].LosD = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// LOS E 설정값 반환
		/// </summary>
		public bool LosE
		{
			get
			{
				if ( Shared.SimulationList.Count > 0 && Shared.SimulationList[_index] is not null )
					return Shared.SimulationList[_index].LosE;
				else
					return false;
			}
			set
			{
				if ( value != Shared.SimulationList[_index].LosE )
				{
					Shared.SimulationList[_index].LosE = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// LOS F 설정값 반환
		/// </summary>
		public bool LosF
		{
			get
			{
				if ( Shared.SimulationList.Count > 0 && Shared.SimulationList[_index] is not null )
					return Shared.SimulationList[_index].LosF;
				else
					return false;
			}
			set
			{
				if ( value != Shared.SimulationList[_index].LosF )
				{
					Shared.SimulationList[_index].LosF = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// 랜덤시드 설정값 반환
		/// </summary>
		public int RandomSeed
		{
			get
			{
				if ( Shared.SimulationList.Count > 0 && Shared.SimulationList[_index] is not null )
					return Shared.SimulationList[_index].RandomSeed;
				else
					return 1;
			}
			set
			{
				if ( 0 < value )
				{
					if ( value != Shared.SimulationList[_index].RandomSeed )
					{
						Shared.SimulationList[_index].RandomSeed = (int)Math.Truncate((double)value);
						OnPropertyChanged();
					}
				}
				else
				{
					MessageBox.Show("Random Seed 값은 1 이상의 자연수만 설정할 수 있습니다.");
				}
			}
		}

		/// <summary>
		/// 시뮬레이션 해상도 설정값 반환
		/// </summary>
		public int Resolution
		{
			get
			{
				if ( Shared.SimulationList.Count > 0 && Shared.SimulationList[_index] is not null )
					return Shared.SimulationList[_index].Resolution;
				else
					return 1;
			}
			set
			{
				if ( 0 < value && value < 21 )
				{
					if ( value != Shared.SimulationList[_index].Resolution )
					{
						Shared.SimulationList[_index].Resolution = value;
						OnPropertyChanged();
					}
				}
				else
				{
					MessageBox.Show("Resolution 값은 1 ~ 20 사이의 자연수만 설정할 수 있습니다.");
				}
			}
		}

		/// <summary>
		/// 웜이업시간 설정값 반환
		/// </summary>
		public double BreakAt
		{
			get
			{
				if ( Shared.SimulationList.Count > 0 && Shared.SimulationList[_index] is not null )
					return Shared.SimulationList[_index].BreakAt;
				else
					return 0;
			}
			set
			{
				if ( 0 <= value )
				{
					if ( value != Shared.SimulationList[_index].BreakAt )
					{
						Shared.SimulationList[_index].BreakAt = value;
						OnPropertyChanged();
					}
				}
				else
				{
					MessageBox.Show("Break At 값은 0 이상의 실수만 설정할 수 있습니다.");
				}
			}
		}

		/// <summary>
		/// 시뮬레이션 실행시간 설정값 반환
		/// </summary>
		public double Period
		{
			get
			{
				if ( Shared.SimulationList.Count > 0 && Shared.SimulationList[_index] is not null )
					return Shared.SimulationList[_index].Period;
				else
					return 0;
			}
			set
			{
				if ( 0 <= value )
				{
					if ( value != Shared.SimulationList[_index].Period )
					{
						Shared.SimulationList[_index].Period = value;
						OnPropertyChanged();
					}
				}
				else
				{
					MessageBox.Show("Period 값은 0 이상의 실수만 설정할 수 있습니다.");
				}
			}
		}

		/// <summary>
		/// Vissim network 사용 유무 설정값 반환
		/// </summary>
		public bool UseVissimNetwork
		{
			get
			{
				if ( Shared.SimulationList.Count > 0 && Shared.SimulationList[_index] is not null )
					return Shared.SimulationList[_index].UseVissimNetwork;
				else
					return false;
			}
			set
			{
				if ( value != Shared.SimulationList[_index].UseVissimNetwork )
				{
					// false -> true 변경 시 파일이 존재하는지 확인
					if (value)
					{
						// 조회할 폴더 설정
						DirectoryInfo dirInfo = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Simulator\Resources\vissim");
						// 파일정보 
						FileInfo simulationFile = new FileInfo($"{Shared.SimulationList[_index].ScenarioFileName}_{Shared.SimulationList[_index].NetworkFileName}");

						// 파일이 있는지 확인함
						bool isExist = false;
						foreach (var file in dirInfo.GetFiles())
						{
							if ( file.Name.Replace(file.Extension, "").Equals(simulationFile.Name))
							{
								isExist = true;
								break;
							}
						}

						// 존재하지 않을경우 메시지 표현
						if (!isExist)
						{
							MessageBox.Show("Vissim 파일이 존재하지 않습니다.\n상단의 [Vissim 편집] 버튼을 눌러 파일을 생성해주세요.");
							return;
						}
					}

					// 파일이 존재할 경우 true 값으로 설정
					Shared.SimulationList[_index].UseVissimNetwork = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// Raw 파일 출력 설정값 반환
		/// </summary>
		public bool WriteRawFiles
		{
			get
			{
				if ( Shared.SimulationList.Count > 0 && Shared.SimulationList[_index] is not null )
					return Shared.SimulationList[_index].WriteRawFiles;
				else
					return false;
			}
			set
			{
				if ( value != Shared.SimulationList[_index].WriteRawFiles )
				{
					Shared.SimulationList[_index].WriteRawFiles = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// 생성자, 인자 없이 호출될 경우, select 화면 표출 X
		/// </summary>
		public ScenarioViewModel()
		{
			_visible = Visibility.Hidden;
		}

		/// <summary>
		/// 생성자, 인자 입력되어 호출될 경우, select 화면 표출 O
		/// </summary>
		/// <param name="index">파일 번호</param>
		public ScenarioViewModel(int index)
		{
			_index = index;
			_visible = Visibility.Visible;
		}
	}
}
