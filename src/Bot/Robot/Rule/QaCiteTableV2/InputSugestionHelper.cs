using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BotLib;
using BotLib.Extensions;
using Bot.AI.WordSplitterNs;
using Bot.Common.Trivial;
using DbEntity;

namespace Bot.Robot.Rule.QaCiteTableV2
{
    public class InputSugestionHelper
    {
        private CiteTableV2 _citeTable;
        private ConcurrentDictionary<string, InputPromptString> _prompDict;
        private double _logPrompDictCountDontUse;

        private int _countForLogPrompDictCount;
        private double LogPrompDictCount
        {
            get
            {
                if (_countForLogPrompDictCount != _prompDict.Count)
                {
                    _countForLogPrompDictCount = _prompDict.Count;
                    _logPrompDictCountDontUse = Math.Log((double)((_countForLogPrompDictCount <= 0) ? 1 : _countForLogPrompDictCount));
                }
                return _logPrompDictCountDontUse;
            }
        }

        public InputSugestionHelper(CiteTableV2 citeTable)
        {
            _prompDict = new ConcurrentDictionary<string, InputPromptString>();
            _countForLogPrompDictCount = -1;
            _citeTable = citeTable;
        }

        public void AddInputPromptWordCite(ShortcutEntity shortcut)
        {
            if (shortcut == null || (string.IsNullOrEmpty(shortcut.Title) && string.IsNullOrEmpty(shortcut.Text))) return;
            var text = string.Concat(shortcut.Code, " ", shortcut.Title, " ", shortcut.Text);
            text = text.Trim().ToLower();
            var key = shortcut.Text.xToBanJiaoAndRemoveCharThatAsciiValueLessThan32AndToLower();
            if (_prompDict.ContainsKey(key))
            {
                var prompt = _prompDict.xTryGetValue(key, null);
                if (prompt != null)
                {
                    prompt.AddUseCount(1);
                }
            }
            else
            {
                var wdDict = WordSpliter.Split(text, true);
                int wordsCount = wdDict.Values.Sum(k => k.Count);
                if (wordsCount > 0)
                {
                    var prompt = InputPromptString.Create(text, wordsCount, wdDict, shortcut);
                    if (_prompDict.TryAdd(key, prompt))
                    {
                        foreach (var w in wdDict)
                        {
                            GetInputPromptWordCiteData(w.Key).Add(prompt);
                        }
                    }
                }
            }
        }

        private InputPromptWordCiteData GetInputPromptWordCiteData(string wd)
        {
            var wordCiteData = _citeTable.GetWordCiteData(wd);
            if (wordCiteData.Prompt == null)
            {
                wordCiteData.Prompt = new InputPromptWordCiteData();
            }
            return wordCiteData.Prompt;
        }

        public void AddInputPromptWordCite(string txt)
        {
            if (txt.xIsNullOrEmptyOrSpace()) return;

            if (_prompDict.ContainsKey(txt))
            {
                var prompt = _prompDict.xTryGetValue(txt, null);
                if (prompt != null)
                {
                    prompt.UpdateLatestUse(txt);
                }
            }
            else
            {
               var wdDict = WordSpliter.Split(txt, true);
               int wordsCount = wdDict.Values.Sum(k => k.Count);
               if (wordsCount > 0)
                {
                    var prompt = InputPromptString.Create(txt, wordsCount, wdDict, null);
                    if (_prompDict.TryAdd(txt, prompt))
                    {
                        foreach (var w in wdDict)
                        {
                            GetInputPromptWordCite(w.Key).Add(prompt);
                        }
                    }
                }
            }

        }

        public List<InputPromptString> GetInputSugestion(string input, Dictionary<long, double> contextNumiid = null, int maxCount = 5)
        {
            if (input.xIsNullOrEmptyOrSpace()) return null;
            input = input.Trim().ToLower();
            var wdDict = WordSpliter.Split(input, true);
            var promptDict = new Dictionary<InputPromptString, double>();
            foreach (var wd in wdDict)
            {
                var word = wd.Key;
                var wordCiteData = GetInputPromptWordCite(word);
                var items = wordCiteData.Items;
                foreach (var prompt in items)
                {
                    if (promptDict.ContainsKey(prompt))
                    {
                        promptDict[prompt] += 1.0;
                    }
                    else
                    {
                        promptDict.Add(prompt, 1.0);
                    }
                }
            }
            var scorePrompts = new List<ItemScore<InputPromptString>>();
            if (promptDict.Count > 0)
            {
                var inputWords = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var kv in promptDict)
                {
                    var score = kv.Value;
                    var text = kv.Key.Text;
                    if (inputWords.Length == 1)
                    {
                        if (text.StartsWith(inputWords[0]))
                        {
                            score *= (4 + input.Length / 10);
                        }
                        else if (text.Contains(inputWords[0]))
                        {
                            score *= (2 + input.Length / 10);
                        }
                    }
                    else if (ContainsAllWords(text, inputWords))
                    {
                        score *= (2 + input.Length / 10);
                    }
                    if (kv.Key.IsShortcutOrRuleAnswer)
                    {
                        score *= 3.0;
                    }
                    scorePrompts.Add(new ItemScore<InputPromptString>(kv.Key, score));
                }
            }
            scorePrompts = scorePrompts.OrderByDescending(k => k.Score).Take(maxCount * 50).ToList();
            if (!scorePrompts.xIsNullOrEmpty())
            {
                double scoreWeight = 0.4;
                double useCountWeight = 0.15;
                double latestUseWeight = 0.35;
                var maxScore = scorePrompts.Max(k => k.Score);
                var minScore = scorePrompts.Min(k => k.Score);
                var maxLatestUse = scorePrompts.Max(k => k.Item.LatestUse);
                var minLatestUse = scorePrompts.Min(k => k.Item.LatestUse);
                var maxUseCount = scorePrompts.Max(k => k.Item.UseCount);
                var minUseCount = scorePrompts.Min(k => k.Item.UseCount);
                double scoreSeed = (maxScore - minScore == 0.0) ? 0.0 : (scoreWeight / (maxScore - minScore));
                double useCountSeed = (maxUseCount - minUseCount == 0) ? 0.0 : (useCountWeight / (maxUseCount - minUseCount));
                double latestUseSeed = ((maxLatestUse - minLatestUse).TotalSeconds == 0.0) ? 0.0 : (latestUseWeight / ((maxLatestUse - minLatestUse).TotalSeconds));
                foreach (var item in scorePrompts)
                {
                    double scoreVal = (item.Score - minScore) * scoreSeed;
                    double useCountVal = 0.0;
                    double lastestUseVal = 0.0;
                    if (item.Item != null)
                    {
                        useCountVal = (double)(item.Item.UseCount - minUseCount) * useCountSeed;
                        lastestUseVal = (item.Item.LatestUse - minLatestUse).TotalSeconds * latestUseSeed;
                    }
                    item.Score = scoreVal + useCountVal + lastestUseVal;
                }
            }
            return scorePrompts.OrderByDescending(k => k.Score).Select(k => k.Item).Take(maxCount).ToList();
        }


        private static bool ContainsAllWords(string txt, string[] words)
        {
            if (words == null || words.Length < 1) return false;            
            return !words.Any(k => !txt.Contains(k));
        }

        private InputPromptWordCiteData GetInputPromptWordCite(string text)
        {
            WordCiteData wordCiteData = _citeTable.GetWordCite(text);
            if (wordCiteData.Prompt == null)
            {
                wordCiteData.Prompt = new InputPromptWordCiteData();
            }
            return wordCiteData.Prompt;
        }

    }
}
