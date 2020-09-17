using System;
using Newtonsoft.Json;

namespace DbEntity
{
	public class TbNickEntity
	{
		[JsonIgnore]
		public string SubPart { get; private set; }

		[JsonIgnore]
		public string MainPart { get; private set; }

		[JsonIgnore]
		public bool IsMainAccount { get; private set; }

		public readonly string Nick;

		[JsonConstructor]
		public TbNickEntity(string nick)
		{
			this.Nick = nick.Trim();
			this.IsMainAccount = TbNickHelper.IsMainAccount(this.Nick);
			this.MainPart = TbNickHelper.GetMainPart(this.Nick);
			this.SubPart = TbNickHelper.GetSubPart(this.Nick);
		}

	}
}
