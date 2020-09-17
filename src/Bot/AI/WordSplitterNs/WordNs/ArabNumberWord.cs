using System;
using Newtonsoft.Json;

namespace Bot.AI.WordSplitterNs.WordNs
{
	public class ArabNumberWord : NumberWord, IAtomicTextWord
	{
		[JsonConstructor]
        public ArabNumberWord(string text)
            : base(text)
		{
		}
	}
}
