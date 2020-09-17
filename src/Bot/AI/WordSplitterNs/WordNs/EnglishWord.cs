using System;
using Newtonsoft.Json;

namespace Bot.AI.WordSplitterNs.WordNs
{
	public class EnglishWord : TextWord, IAtomicTextWord
	{
		[JsonConstructor]
		public EnglishWord(string text): base(text)
		{
		}
	}
}
