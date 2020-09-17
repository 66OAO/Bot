using System;
using System.Collections.Generic;
using System.Linq;
using BotLib.Extensions;

namespace Bot.Robot.Rule.QaCiteTableV2
{
	public class InputPromptString
	{
        public string Text { get; private set; }
        private int _wordsCount;
        private Dictionary<string, int> WordCountDict;
        private double _reciprocalOfCount;

		public bool IsShortcutOrRuleAnswer { get; private set; }

		public int UseCount { get; private set; }

		public DateTime LatestUse { get; private set; }

		public long[] ContextNumiids { get; private set; }

        public object Tag
        {
            get;
            private set;
        }
        public int WordsCount
        {
            get
            {
                return _wordsCount;
            }
            private set
            {
                _wordsCount = value;
                _reciprocalOfCount = 1.0 / (double)value;
            }
        }

		private InputPromptString()
		{
			_reciprocalOfCount = 0.0;
		}

        private void AddOrUpdateWordCount(Dictionary<string, List<int>> dict)
        {
            foreach (var wd in dict)
            {
                int count = wd.Value.Count;
                if (count > 1)
                {
                    if (WordCountDict == null)
                    {
                        WordCountDict = new Dictionary<string, int>();
                    }
                    WordCountDict[wd.Key] = count;
                }
            }
        }

        public static InputPromptString Create(string text, int wordsCount, Dictionary<string, List<int>> wordDict, object tag)
        {
            var prompt = new InputPromptString
            {
                Text = text,
                Tag = tag,
                IsShortcutOrRuleAnswer = true,
                UseCount = 2,
                LatestUse = BatTime.Now.Date.AddDays(-1.0),
                WordsCount = wordsCount
            };
            prompt.AddOrUpdateWordCount(wordDict);
            return prompt;
        }

        public void UpdateLatestUse(string txt)
		{
			UseCount += 3;
			if (LatestUse < BatTime.Now.Date)
			{
				LatestUse = BatTime.Now.Date;
			}
			IsShortcutOrRuleAnswer = true;
		}

        public void AddUseCount(int cnt = 1)
        {
            UseCount += cnt;
        }

        public double GetWordCount(string text)
		{
            int wordCount = (WordCountDict != null) ? WordCountDict.xTryGetValue(text, 1) : 1;
            return wordCount * _reciprocalOfCount;
		}

	}
}
