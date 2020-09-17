using System;

namespace DbEntity
{
	public class SynCheckDownloadEntity
	{
		public string DbAccount { get; set; }

		public long MaxDbAccountServerTimestamp { get; set; }

		public int UnDeleteRecordCount { get; set; }

		public bool IsAllowRedownAll { get; set; }

		public bool IsServerAllowSynCheck { get; set; }
	}
}
