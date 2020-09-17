using System;
using Newtonsoft.Json;

namespace Bot.AI.WordSplitterNs.WordNs
{
	public class ChineseWord : TextWord
	{
		[JsonConstructor]
		public ChineseWord(string text): base(text)
		{
		}
	}
}
