using System;

namespace DbEntity
{
	public class GoodsWeightEntity : EntityBase
	{
		public long NumIid { get; set; }

		public double ManualWeightKg { get; set; }

		public long ManualWeightSetTime { get; set; }

		public double TaobaoWeightKg { get; set; }
	}
}
