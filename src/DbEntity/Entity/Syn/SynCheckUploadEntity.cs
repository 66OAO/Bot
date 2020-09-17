using System;

namespace DbEntity
{
	public class SynCheckUploadEntity
	{
		public string DbAccount { get; set; }

		public CheckOccasionEnum Occasion { get; set; }

		public long MaxDbAccountServerSynTime { get; set; }

		public int ClientUnDeleteCount { get; set; }
	}
}
