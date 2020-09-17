using System;
using System.Collections.Generic;
using Bot.AI.WordSplitterNs.WordNs;
using BotLib.Extensions;
using BotLib.Misc;

namespace Bot.AI.WordSplitterNs.ElementParser
{
	public class ChineseNumberParser : WordParser
	{
        protected override TextWord Create(string s)
		{
			return new ChineseNumberWord(s);
		}

        protected override List<IndexRange> GetIndexRanges(string s)
		{
			s = s.xConvertChineseNumberStringToArabNumberString();
			return s.xGetArabNumberIndexRanges();
		}
	}
}
