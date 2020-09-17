using System;
using Newtonsoft.Json;

namespace Bot.AI.WordSplitterNs.WordNs
{
	public class ChineseNumberWord : NumberWord
	{
		[JsonConstructor]
		public ChineseNumberWord(string text) : base(text)
		{
		}
	}
}
