using System;
using System.Collections.Generic;
using Bot.AI.WordSplitterNs.WordNs;
using BotLib;
using BotLib.Extensions;
using BotLib.Misc;
using System.Linq;

namespace Bot.AI.WordSplitterNs.ElementParser
{
	public class EmojiParser : WordParser
	{
        private const string _emojiCvs = "/:^_^,/:^$^,/:q,/:815,/:809,/:^o^,/:081,/:087,/:086,/:h,/:012,/:806,/:b,/:^x^,/:814,/:^w^,/:080,/:066,/:807,/:805,/:071,/:072,/:065,/:804,/:813,/:818,/:015,/:084,/:801,/:811,/:?,/:077,/:083,/:817,/:!,/:068,/:079,/:028,/:026,/:007,/:816,/:'\"\",/:802,/:027,/:(zz...),/:*&*,/:810,/:>_<,/:018,/:>o<,/:020,/:044,/:819,/:085,/:812,/:\",/:>m<,/:>@<,/:076,/:069,/:o,/:067,/:043,/:p,/:808,/:>w<,/:073,/:008,/:803,/:074,/:o=o,/:036,/:039,/:045,/:046,/:048,/:047,/:girl,/:man,/:052,/:(ok),/:8*8 ,/:)-(,/:lip,/:-f,/:-w,/:y,/:qp,/:$,/:%,/:(&),/:@,/:~b,/:u* u,/:clock,/:r,/:c,/:plane,/:075";
        private static readonly HashSet<string> _emojiSet;
        private static readonly CharMatcher _matcher;
        public static EmojiParser Parser;
		public static int MaxPhizLength { get; private set; }
		public static int MinPhizLength { get; private set; }
        
		static EmojiParser()
		{
			Parser = new EmojiParser();
            var emojiTexts = "/:^_^,/:^$^,/:q,/:815,/:809,/:^o^,/:081,/:087,/:086,/:h,/:012,/:806,/:b,/:^x^,/:814,/:^w^,/:080,/:066,/:807,/:805,/:071,/:072,/:065,/:804,/:813,/:818,/:015,/:084,/:801,/:811,/:?,/:077,/:083,/:817,/:!,/:068,/:079,/:028,/:026,/:007,/:816,/:'\"\",/:802,/:027,/:(zz...),/:*&*,/:810,/:>_<,/:018,/:>o<,/:020,/:044,/:819,/:085,/:812,/:\",/:>m<,/:>@<,/:076,/:069,/:o,/:067,/:043,/:p,/:808,/:>w<,/:073,/:008,/:803,/:074,/:o=o,/:036,/:039,/:045,/:046,/:048,/:047,/:girl,/:man,/:052,/:(ok),/:8*8 ,/:)-(,/:lip,/:-f,/:-w,/:y,/:qp,/:$,/:%,/:(&),/:@,/:~b,/:u* u,/:clock,/:r,/:c,/:plane,/:075".xSplitByComma(StringSplitOptions.RemoveEmptyEntries);
			int maxLen = 0;
			int minLen = int.MaxValue;
            foreach (var emt in emojiTexts)
			{
                if (emt.Length > maxLen)
				{
                    maxLen = emt.Length;
				}
                if (emt.Length < minLen)
				{
                    minLen = emt.Length;
				}
			}
			MaxPhizLength = maxLen;
			MinPhizLength = minLen;
            _emojiSet = new HashSet<string>(emojiTexts);
            for (int j = 0; j < emojiTexts.Length; j++)
			{
                emojiTexts[j] = emojiTexts[j].Substring(2);
			}
            _matcher = CharMatcher.Create(emojiTexts.ToList());
		}


        public List<IndexRange> GetExceptRanges(string s)
		{
            var ranges = this.GetIndexRanges(s);
			return IndexRange.GetExceptRanges(ranges, s);
		}

        protected override List<IndexRange> GetIndexRanges(string s)
		{
			var rngs = new List<IndexRange>();
			int idx = s.Length - MinPhizLength + 1;
			for (int i = 0; i < idx; i++)
			{
				if (s[i] == '/' && s[i + 1] == ':')
				{
					int len = _matcher.GetEmojiLenth(s, i + 2);
					if (len > 0)
					{
						rngs.Add(new IndexRange(i, len + 2));
						i += len + 1;
					}
				}
			}
			return rngs;
		}

        protected override TextWord Create(string s)
		{
			return new EmojiWord(s);
		}

		private class CharMatcher
		{
            private readonly Dictionary<char, CharMatcher> Dictionary;

            private const char EmptyChar = '\0';
            public CharMatcher()
            {
                this.Dictionary = new Dictionary<char, CharMatcher>();

            }

			public int GetEmojiLenth(string s, int startIdx)
			{
				int idx = startIdx;
				int len = 0;
				var matcher = this;
				while (idx < s.Length)
				{
                    matcher = matcher.GetMatcher(s[idx++]);
                    if (matcher == null)
					{
						break;
					}
                    if (this.HasEmptyCharKey())
					{
						len = idx - startIdx;
					}
				}
				return len;
			}

            public static CharMatcher Create(List<string> emojiTexts)
			{
				Util.Assert(emojiTexts.xCount<string>() > 0);
				var matcher = new CharMatcher();
				emojiTexts.xSortStringByAsciiAsc();
				while (emojiTexts.Count > 0)
				{
					if (emojiTexts.First() == "")
					{
						matcher.Dictionary['\0'] = null;
						int idx = emojiTexts.FindIndex(k =>!string.IsNullOrEmpty(k));
						if (idx == -1)
						{
							idx = emojiTexts.Count;
						}
						emojiTexts.RemoveRange(0, idx);
					}
					else
					{
						char ch = emojiTexts[0][0];
						int idx = emojiTexts.FindIndex(k => k[0] != ch);
						if (idx == -1)
						{
							idx = emojiTexts.Count;
						}
                        var emojits = emojiTexts.xCopy(0, idx);
                        for (int i = 0; i < emojits.Count; i++)
                        {
                            emojits[i] = emojits[i].Substring(1);
                        }
                        matcher.Dictionary[ch] = CharMatcher.Create(emojits);
						emojiTexts.RemoveRange(0, idx);
					}
				}
				return matcher;
			}

            private bool HasEmptyCharKey()
			{
				return this.Dictionary.ContainsKey('\0');
			}

			private CharMatcher GetMatcher(char ky)
			{
				CharMatcher matcher =null;
                this.Dictionary.TryGetValue(char.ToLower(ky), out matcher);
				return matcher;
			}

		}
	}
}
