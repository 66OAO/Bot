using System;

namespace DbEntity
{
	public class TimestampEntity
	{
		public TimestampEntity(long tick)
		{
			this.Timestamp = tick;
		}

		public long Timestamp;
	}
}
