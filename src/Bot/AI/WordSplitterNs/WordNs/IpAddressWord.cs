using System;
using Newtonsoft.Json;

namespace Bot.AI.WordSplitterNs.WordNs
{
	public class IpAddressWord : UrlWord
	{
		[JsonConstructor]
		public IpAddressWord(string text): base(text)
		{
		}

        protected override string[] GetSemantics()
		{
			return new string[]
			{
				"W_IP",
				this.Text
			};
		}
	}
}
