using System;
using System.Collections.Generic;
using Bot.AI.WordSplitterNs.WordNs;
using BotLib.Extensions;
using BotLib.Misc;

namespace Bot.AI.WordSplitterNs.ElementParser
{
	public class ChineseWordParser : WordParser
	{
        protected override TextWord Create(string s)
		{
			return new ChineseWord(s);
		}

        protected override List<IndexRange> GetIndexRanges(string s)
		{
			var ranges = s.xGetChineseStringRanges();
			var wordRanges = new List<IndexRange>();
			foreach (IndexRange r in ranges)
			{
				wordRanges.AddRange(this.GetSplitIndexRanges(s, r));
			}
			return wordRanges;
		}

		private List<IndexRange> GetSplitIndexRanges(string s, IndexRange r)
		{
			var granges = new List<IndexRange>();
			var text = s.Substring(r.Start, r.Length);
            var wordRanges = ChineseWordSplitter.SplitToAllConbinedWords(text, false);
			foreach (var indexRange in wordRanges)
			{
				granges.Add(new IndexRange(indexRange.Start + r.Start, indexRange.Length));
			}
			return granges;
		}
	}
}
