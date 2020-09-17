using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Common;
using BotLib;
using BotLib.Cypto;
using BotLib.Extensions;
using BotLib.FileSerializer;
using BotLib.Misc;

namespace Bot.AI.WordSplitterNs
{
    public class ChineseWordSplitter
    {
        public const int MaxWordLength = 7;
        public const int MinWordLength = 1;
        private static StringBuilder sb;
        private static int _wcount = 0;
        private static HashSet<string> _allWordSet;
        private static SeriableObject<HashSet<string>> _s;

        static ChineseWordSplitter()
        {
            sb = new StringBuilder();
            _wcount = 0;
            try
            {
                _s = new SeriableObject<HashSet<string>>("ChineseWordSet.LoginWordSet", () => new HashSet<string>(), true, false, true);
                SetWordDatas(_s.Data);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public static bool IsWord(string s)
        {
            return !string.IsNullOrEmpty(s) && _allWordSet.Contains(s);
        }

        public static List<List<SplitInfo>> SplitToMaxLengthWords(string s, bool toSimplifiedChinese = false)
        {
            if (toSimplifiedChinese)
            {
                s = s.xToSimplifiedChinese(false);
            }
            return SplitOverlappedWord(s, 0);
        }

        public static List<IndexRange> SplitToAllConbinedWords(string s, bool toSimplifiedChinese = false)
        {
            var splitRanges = new List<IndexRange>();
            var splitInfos = SplitToMaxLengthWords(s, toSimplifiedChinese);
            var splitDict = new Dictionary<string, SplitInfo>();
            foreach (var infos in splitInfos)
            {
                foreach (var si in infos)
                {
                    var key = si.StartIndex + "," + si.Length;
                    if (!splitDict.ContainsKey(key))
                    {
                        splitDict[key] = si;
                        var spis = GetInnerSplitInfos(si);
                        foreach (var innerSInfo in spis)
                        {
                            var ik = si.StartIndex + "," + si.Length;
                            if (!splitDict.ContainsKey(ik))
                            {
                                splitDict[ik] = innerSInfo;
                            }
                        }
                    }
                }
            }
            foreach (var kv in splitDict)
            {
                splitRanges.Add(new IndexRange(kv.Value.StartIndex, kv.Value.Length));
            }
            splitRanges.Sort((l,r) => l.Start.CompareTo(r.Start));
            return splitRanges;
        }

        private static List<SplitInfo> GetInnerSplitInfos(SplitInfo sinfo)
        {
            List<SplitInfo> innerSplitInfos = new List<SplitInfo>();
            for (int i = sinfo.StartIndex; i < sinfo.NextIndex; i++)
            {
                int length = Math.Min(7, sinfo.NextIndex - i - 1);
                for (int j = 1; j <= length; j++)
                {
                    string text = sinfo.Context.Substring(i, j);
                    if (IsWord(text))
                    {
                        SplitInfo item = new SplitInfo
                        {
                            Context = sinfo.Context,
                            IsWord = true,
                            Length = j,
                            StartIndex = i
                        };
                        innerSplitInfos.Add(item);
                    }
                }
            }
            return innerSplitInfos;
        }

        private static List<List<SplitInfo>> SplitOverlappedWord(string s, int fromidx)
        {
            List<List<SplitInfo>> overlappedSplitInfos = new List<List<SplitInfo>>();
            if (fromidx >= s.Length) return overlappedSplitInfos;
            List<SplitInfo> sInfos = SplitWord(s, fromidx);
            if (sInfos.Count == 0)
            {
                List<SplitInfo> item = new List<SplitInfo>{
                    new SplitInfo
                    {
                        IsWord = false,
                        Context = s,
                        StartIndex = fromidx,
                        Length = s.Length - fromidx
                    } 
                };
                overlappedSplitInfos.Add(item);
            }
            else
            {
                foreach (var si in sInfos)
                {
                    var splitInfos = new List<SplitInfo>();
                    var length = si.Length;
                    var startIndex = si.StartIndex;
                    if (startIndex > fromidx)
                    {
                        int len = startIndex - fromidx;
                        splitInfos.Add(new SplitInfo
                        {
                            IsWord = IsWord(s.Substring(fromidx, len)),
                            Context = s,
                            StartIndex = fromidx,
                            Length = len
                        });
                    }
                    splitInfos.Add(new SplitInfo
                    {
                        IsWord = true,
                        Context = s,
                        StartIndex = startIndex,
                        Length = length
                    });
                    List<List<SplitInfo>> ovlpdSplitInfos = SplitOverlappedWord(s, si.NextIndex);
                    overlappedSplitInfos.AddRange(MergerSplitInfos(splitInfos, ovlpdSplitInfos));
                }
            }
            return overlappedSplitInfos;
        }

        private static List<List<SplitInfo>> SplitOverlappedWord(string s, int startIndex, int fromidx, int toidx)
        {
            List<List<SplitInfo>> list = new List<List<SplitInfo>>();
            int idx = 0;
            for (int i = fromidx; i <= toidx; i = idx + 1)
            {
                int wlen;
                idx = GetWordStartIndex(s, i, out wlen);
                Util.Assert(idx < 0 || idx >= fromidx);
                if (idx < fromidx || idx > toidx || idx + wlen <= toidx + 1)
                {
                    break;
                }
                var splitInfos = new List<SplitInfo>();
                splitInfos.Add(new SplitInfo
                {
                    IsWord = IsWord(s.Substring(startIndex, idx - startIndex)),
                    Context = s,
                    StartIndex = startIndex,
                    Length = idx - startIndex
                });
                splitInfos.Add(new SplitInfo
                {
                    IsWord = true,
                    Context = s,
                    StartIndex = idx,
                    Length = wlen
                });
                List<List<SplitInfo>> list_ = SplitOverlappedWord(s, idx + wlen);
                list.AddRange(MergerSplitInfos(splitInfos, list_));
            }
            return list;
        }

        private static List<List<SplitInfo>> MergerSplitInfos(List<SplitInfo> splitInfos, List<List<SplitInfo>> overlappedSplitInfos)
        {
            List<List<SplitInfo>> mergerSplitInfos = new List<List<SplitInfo>>();
            if (splitInfos.xIsNullOrEmpty())
            {
                mergerSplitInfos.AddRange(overlappedSplitInfos);
            }
            else if (overlappedSplitInfos.xIsNullOrEmpty())
            {
                mergerSplitInfos.Add(splitInfos);
            }
            else
            {
                foreach (List<SplitInfo> infos in overlappedSplitInfos)
                {
                    List<SplitInfo> mgInfos = new List<SplitInfo>();
                    mgInfos.AddRange(splitInfos);
                    mgInfos.AddRange(infos);
                    mergerSplitInfos.Add(mgInfos);
                }
            }
            return mergerSplitInfos;
        }

        private static int GetWordStartIndex(string s, int idx, out int wlen)
        {
            wlen = -1;
            for (int i = idx; i < s.Length; i++)
            {
                int len = Math.Min(s.Length - i, 7);
                for (int j = len; j > 0; j--)
                {
                    var w = s.Substring(i, j);
                    if (IsWord(w))
                    {
                        wlen = j;
                        return i;
                    }
                }
            }
            return -1;
        }

        private static List<SplitInfo> SplitWord(string s, int fromidx)
        {
            var sInfos = new List<SplitInfo>();
            int wlen;
            int idx = GetWordStartIndex(s, fromidx, out wlen);
            if (idx >= fromidx)
            {
                sInfos.Add(new SplitInfo
                {
                    Context = s,
                    StartIndex = idx,
                    IsWord = true,
                    Length = wlen
                });
                int nextIdx = idx + wlen;
                for (fromidx++; fromidx < nextIdx; fromidx++)
                {
                    int nlen;
                    int nidx = GetWordStartIndex(s, fromidx, out nlen);
                    if (nidx < fromidx || nidx >= nextIdx)
                    {
                        break;
                    }
                    if (nidx + nlen > nextIdx)
                    {
                        sInfos.Add(new SplitInfo
                        {
                            Context = s,
                            StartIndex = nidx,
                            IsWord = true,
                            Length = nlen
                        });
                    }
                }
            }
            return sInfos;
        }

        private static void SetWordDatas(HashSet<string> data)
        {
            _allWordSet = new HashSet<string>();
            _allWordSet.xAddRange(data);
            LoadPreDefinedChineseWords();
        }

        private static void LoadPreDefinedChineseWords()
        {
            try
            {
                var fn = PathEx.StartUpPathOfExe + "w.dic";
                var text = FileEx.TryReadFile(fn);
                if (string.IsNullOrEmpty(text))
                {
                    throw new Exception("字典为空");
                }
                var items = text.xSplitByLine(StringSplitOptions.RemoveEmptyEntries);
                _allWordSet.xAddRange(items);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                var errMsg = "默认字典损坏，请重新安装软件";
                Log.Error(errMsg);
                MsgBox.ShowErrTip(errMsg,null);
            }
        }

        public class SplitInfo
        {
            public string Text
            {
                get
                {
                    return this.Context.Substring(this.StartIndex, this.Length);
                }
            }

            public int NextIndex
            {
                get
                {
                    return this.StartIndex + this.Length;
                }
            }

            public string Context;

            public int StartIndex;

            public int Length;

            public bool IsWord;
        }
    }
}
