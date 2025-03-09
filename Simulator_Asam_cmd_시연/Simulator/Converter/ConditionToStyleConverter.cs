using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Simulator.Converter
{
	/// <summary>
	/// View에서 조건에 따라서 특정 뷰를 보여주기 위해 사용함
	/// </summary>
	public class ConditionToStyleConverter : IValueConverter
	{
		public Style? ActivateStyle { get; set; }
		public Style? DeactivateStyle { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool)value ? ActivateStyle! : DeactivateStyle!;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new InvalidOperationException();
		}
	}
}
