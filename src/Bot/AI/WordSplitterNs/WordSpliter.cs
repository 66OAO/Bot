using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BotLib;
using BotLib.Misc;
using Bot.AI.WordSplitterNs.ElementParser;
using Bot.Robot.Rule.Parser;
using Bot.AI.WordSplitterNs.WordNs;

namespace Bot.AI.WordSplitterNs
{
	public class WordSpliter
    {
        private static IpParser _ipParser;
        private static ArabNumberParser _arabParser;
        private static ChineseNumberParser _chNumberParser;
        private static EmojiParser _emojiParser;
        private static EmailParser _emailParser;
        private static EnglishWordParser _englishParser;
        private static GoodsUrlParser _goodsParser;
        private static UrlParser _urlParser;
        private static ChineseWordParser _chineseParser;

        static WordSpliter()
        {
            _ipParser = new IpParser();
            _arabParser = new ArabNumberParser();
            _chNumberParser = new ChineseNumberParser();
            _emojiParser = EmojiParser.Parser;
            _emailParser = new EmailParser();
            _englishParser = new EnglishWordParser();
            _goodsParser = new GoodsUrlParser();
            _urlParser = new UrlParser();
            _chineseParser = new ChineseWordParser();
        }

        private static string ConvertToString(string text, List<PlainTextSymbol> ptslist)
		{
            var sb = new StringBuilder();
			int idx = 0;
			foreach (PlainTextSymbol plainTextSymbol in ptslist)
			{
				Util.Assert(plainTextSymbol.StartIndex >= idx);
				if (plainTextSymbol.StartIndex > idx)
				{
					sb.Append(string.Format(" [{0}] ", text.Substring(idx, plainTextSymbol.StartIndex - idx)));
				}
				sb.Append(string.Format(" <{0} ({1})> ", text.Substring(plainTextSymbol.StartIndex, plainTextSymbol.Length), plainTextSymbol.GetType().Name));
				idx = plainTextSymbol.NextIndex;
			}
			if (idx < text.Length)
			{
				sb.Append(string.Format(" [{0}] ", text.Substring(idx)));
			}
			return sb.ToString();
		}

		public static Dictionary<string, List<int>> Split(string input, bool splitSingleChar)
		{
			var dictPts = new Dictionary<string, List<int>>();
            var ptslist = ParsePlainTextSymbols(input, splitSingleChar);
			foreach (var plainTextSymbol in ptslist)
			{
				string matchText = plainTextSymbol.MatchText;
				if (!dictPts.ContainsKey(matchText))
				{
					dictPts[matchText] = new List<int>();
				}
				dictPts[matchText].Add(plainTextSymbol.StartIndex);
			}
			return dictPts;
		}

        public static List<PlainTextSymbol> ParsePlainTextSymbols(string text, bool distinct = false)
		{
            var ptSyms = new List<PlainTextSymbol>();
			List<IndexRange> idxRangs;
            ptSyms.AddRange(_emojiParser.Parse(text, out idxRangs));
			ptSyms.AddRange(_goodsParser.Parse(text, idxRangs, out idxRangs));
			ptSyms.AddRange(_emailParser.Parse(text, idxRangs, out idxRangs));
			ptSyms.AddRange(_urlParser.Parse(text, idxRangs, out idxRangs));
			ptSyms.AddRange(_ipParser.Parse(text, idxRangs, out idxRangs));
			ptSyms.AddRange(_arabParser.Parse(text, idxRangs, out idxRangs));
			ptSyms.AddRange(_chNumberParser.Parse(text, idxRangs, out idxRangs));
			ptSyms.AddRange(_englishParser.Parse(text, idxRangs, out idxRangs));
			ptSyms.AddRange(_chineseParser.Parse(text, idxRangs, out idxRangs));
			if (distinct)
			{
				for (int i = 0; i < text.Length; i++)
				{
					string ptext = text.Substring(i, 1).Trim();
					if (ptext.Length == 1)
					{
						ptSyms.Add(new PlainTextSymbol(ptext, i, null));
					}
				}
			}
			ptSyms.Sort((l,r)=>
			{
				int diffVal = l.StartIndex.CompareTo(r.StartIndex);
				if (diffVal == 0)
				{
					diffVal = l.Length.CompareTo(r.Length);
				}
				return diffVal;
			});
			if (distinct)
			{
				for (int j = 1; j < ptSyms.Count; j++)
				{
                    var pts1 = ptSyms[j - 1];
                    var pts2 = ptSyms[j];
					if (pts1.StartIndex == pts2.StartIndex && pts1.Length == pts2.Length)
					{
						ptSyms.RemoveAt(j);
						j--;
					}
				}
			}
			return ptSyms;
		}

        private static List<IndexRange> GetNonAtomicLogicalCharRanges(string input, int startIdx, int nextIdx, List<PlainTextSymbol> ptslist)
		{
            var list = ptslist.Where(k=>k.StartIndex >= startIdx && k.NextIndex <= nextIdx)
                .Select(k=> new IndexRange(k.StartIndex, k.Length)).ToList();
            var exceptRanges = IndexRange.GetExceptRanges(list, startIdx, nextIdx);
            foreach (var range in exceptRanges)
			{
				for (int i = range.Start; i < range.NextStart; i++)
				{
					list.Add(new IndexRange(i, 1));
				}
			}
			return list;
		}

        public static List<IndexRange> GetAtomicRanges(List<PlainTextSymbol> ptslist, string text)
		{
			var idxRngs = new List<IndexRange>();
			int nextIdx = 0;
			foreach (var plainTextSymbol in ptslist)
			{
				for (int i = nextIdx; i < plainTextSymbol.StartIndex; i++)
				{
					idxRngs.Add(new IndexRange(i, 1));
				}
				idxRngs.Add(new IndexRange(plainTextSymbol.StartIndex, plainTextSymbol.Length));
				nextIdx = plainTextSymbol.NextIndex;
			}
			for (int j = nextIdx; j < text.Length; j++)
			{
				idxRngs.Add(new IndexRange(j, 1));
			}
			idxRngs.Sort((l,r) => l.Start.CompareTo(r.Start));
			return idxRngs;
		}

        public static List<IndexRange> GetAtomicRanges(string text)
		{
            var source = ParsePlainTextSymbols(text, false);
            var atptslist = source.Where(k => k.Word is IAtomicTextWord).ToList<PlainTextSymbol>();
			atptslist.Sort((l, r) => l.StartIndex.CompareTo(r.StartIndex));
            var noatptslist = source.Where(k => !(k.Word is IAtomicTextWord)).ToList<PlainTextSymbol>();
			noatptslist.Sort((l,r) => l.StartIndex.CompareTo(r.StartIndex));
            return GetAtomicRanges(atptslist, text);
		}

	}
}
