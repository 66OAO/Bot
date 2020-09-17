using System;

namespace DbEntity
{
	public class TransferFileParamsEntity
	{
		public long Timestamp;

		public string ClientId;

		public int Version;

		public TransferFileTypeEnum FileType;

		public string FileName;

		public TransferFileOpTypeEnum OpType;
	}
}
