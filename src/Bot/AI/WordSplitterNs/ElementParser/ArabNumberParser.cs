using System;
using System.Collections.Generic;
using Bot.AI.WordSplitterNs.WordNs;
using BotLib.Extensions;
using BotLib.Misc;

namespace Bot.AI.WordSplitterNs.ElementParser
{
	public class ArabNumberParser : WordParser
	{
        protected override TextWord Create(string s)
		{
			return new ArabNumberWord(s);
		}

        protected override List<IndexRange> GetIndexRanges(string s)
		{
			return s.xGetArabNumberIndexRanges();
		}
	}
}
