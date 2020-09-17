using System;

namespace DbEntity
{
	public class BuyerNoteEntity : EntityBase
	{
		public string Note { get; set; }

		public string BuyerMainNick { get; set; }

		public string Recorder { get; set; }

		public DateTime RecordTime
		{
			get
			{
                return _recordTime.Year < 2013 ? new DateTime(base.ModifyTick).AddHours(8.0) : _recordTime;
			}
			set
			{
				this._recordTime = value;
			}
		}

		private DateTime _recordTime;
	}
}
