using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Microsoft.Xaml.Behaviors.Core;
using Simulator.Common;
using Simulator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Simulator.ViewModels
{
	/// <summary>
	/// 최초 실행시 실행되는 View Model
	/// </summary>
	public partial class MainViewModel : ObservableObject
	{
		/// <summary>
		/// timer, sub view에서 조작을 하면 안되는 이벤트가 발생한 경우 주기적으로 확인해 화면이동버튼을 사용하지 못하게 하기 위함
		/// </summary>
		private Timer _timer;
		/// <summary>
		/// 표출해야할 view model
		/// </summary>
		private BaseViewModel _displayViewModel { get; set; }

		/// <summary>
		/// button 색상 설정값
		/// </summary>
		[ObservableProperty]
		private Brush _stateColorChangeViewButton = (Shared.IsButtonEnable == true) ? (Brush)Application.Current.FindResource("HighlightAmber") : (Brush)Application.Current.FindResource("Label");

		/// <summary>
		/// 뷰 전환
		/// </summary>
		public BaseViewModel DisplayViewModel
		{
			get { return _displayViewModel; }
			set
			{
				if (value != _displayViewModel)
				{
					_displayViewModel = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// 툴팁 설정, 설정화면, 실행화면이 나뉘어져 있음
		/// </summary>
		private string _changeViewToolTip { get; set; }
		public string ChangeViewToolTip
		{
			get { return _changeViewToolTip; }
			set
			{
				if (value != _changeViewToolTip)
				{
					_changeViewToolTip = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// 시나리오 선택 화면이 표출되고있는지 여부
		/// </summary>
		private bool _isSelectViewDisplayed { get; set; }

		/// <summary>
		/// 생성자
		/// </summary>
		public MainViewModel()
		{
			#region Set default view
			
			_displayViewModel = new SelectViewModel();
			_isSelectViewDisplayed = true;
			_changeViewToolTip = UpdateToolTip();

			_timer = new Timer();
			_timer.Interval = 1000;
			_timer.Elapsed += TimerElapsed;
			_timer.Start();

			#endregion
		}

		/// <summary>
		/// 화면 전환버튼의 툴팁 설정
		/// </summary>
		/// <returns></returns>
		private string UpdateToolTip()
		{
			string toolTip = "";
			if (_isSelectViewDisplayed)
			{
				toolTip = "시뮬레이션 화면으로 이동";
			}
			else
			{
				toolTip = "시나리오 선택 화면으로 이동";
			}

			return toolTip;
		}

		/// <summary>
		/// 화면 전환 버튼 클릭 시 행위
		/// </summary>
		[RelayCommand]
		private void ChangeView()
		{
			if ( !Shared.IsButtonEnable )
			{
				ChangeViewToolTip = "시뮬레이션이 진행중입니다. 화면을 변경할 수 없습니다.";
				return;
			}

			// 현재화면: 시나리오 선택
			if (_isSelectViewDisplayed)
			{
				_isSelectViewDisplayed = false;
				ChangeViewToolTip = UpdateToolTip();

				DisplayViewModel = new SimulatorViewModel();
			}
			// 현재화면: 시뮬레이션
			else
			{
				_isSelectViewDisplayed = true;
				ChangeViewToolTip = UpdateToolTip();

				DisplayViewModel = new SelectViewModel();
			}
		}

		/// <summary>
		/// 타이머가 실행할 이벤트<br/>
		/// 버튼 사용가능 유무를 확인해 main view에 전달
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TimerElapsed(object? sender, ElapsedEventArgs e)
		{
			IsButtonEnable = Shared.IsButtonEnable;
		}

		/// <summary>
		/// 버튼 사용가능 유무
		/// </summary>
		private bool _isButtonEnable { get; set; } = true;
		/// <summary>
		/// 버튼 사용가능 유무
		/// </summary>
		public bool IsButtonEnable
		{
			get { return _isButtonEnable; }
			set
			{
				if ( value != _isButtonEnable )
				{
					if (value == true)
					{
						StateColorChangeViewButton = (Brush)Application.Current.FindResource("HighlightAmber");
					}
					else
					{
						StateColorChangeViewButton = (Brush)Application.Current.FindResource("Label");
					}
					_isButtonEnable = value;
					OnPropertyChanged();
				}
			}
		}
	}
}
