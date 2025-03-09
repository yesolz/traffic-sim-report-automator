using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.WPF;

namespace Simulator.Views
{
	/// <summary>
	/// MsChartView.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class ChartView : UserControl
	{
		public ChartView()
		{
			InitializeComponent();

			Loaded += View_Loaded;
		}

		private void View_Loaded(object sender, RoutedEventArgs e)
		{
			CreateImageFromCartesianControl();
			CreateImageFromPieControl();
			CreateImageFromGeoControl();
		}

		private void CreateImageFromCartesianControl()
		{
			/*var chartControl = (CartesianChart)FindName("cartesianChart");
			var skChart = new SKCartesianChart(chartControl) { Width = 900, Height = 600, };
			skChart.SaveImage("CartesianImageFromControl.png");*/
		}

		private void CreateImageFromPieControl()
		{
			/*var chartControl = (PieChart)FindName("pieChart");
			var skChart = new SKPieChart(chartControl) { Width = 900, Height = 600, };
			skChart.SaveImage("PieImageFromControl.png");*/
		}

		private void CreateImageFromGeoControl()
		{
			/*var chartControl = (GeoMap)FindName("geoChart");
			var skChart = new SKGeoMap(chartControl) { Width = 900, Height = 600, };
			skChart.SaveImage("MapImageFromControl.png");*/
		}
	}
}
