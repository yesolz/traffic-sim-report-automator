using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Models.VissimModels
{
	/// <summary>
	/// 자율주행차량 정보
	/// </summary>
	public class EgoVehicleModel
	{
		#region Properties

		/// <summary>
		/// 자율주행차량 번호
		/// </summary>
		public int Number { get; set; }
		/// <summary>
		/// 자율주행차량 존재 유무
		/// </summary>
		public bool Exist { get; set; }
		/// <summary>
		/// 자율주행차량 상태
		/// </summary>
		public States State { get; set; }

		#endregion

		/// <summary>
		/// 자율주행 차량 네트워크 존재여부
		/// </summary>
		public enum States
		{
			/// <summary>
			/// 아직 생성되지 않음
			/// </summary>
			NotAppear,
			/// <summary>
			/// 생성됨
			/// </summary>
			Appear,
			/// <summary>
			/// 생성된 후 사라짐
			/// </summary>
			Disappear
		}
	}
}
