using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BotLib.Extensions;
using Bot.AssistWindow.Widget.Right.ShortCut;
using Bot.AssistWindow.Widget.Bottom;
using System.Linq;
using DbEntity;

namespace Bot.Robot.Rule.QaCiteTableV2
{
	public class CiteTableV2
	{
        public ConcurrentDictionary<string, WordCiteData> CiteDict;
        private string _dbAccount;
        private InputSugestionHelper _inputSugestHelper;
        private bool _isInited;
        private bool _isIniting;
        private object _initSynObj;

		public CiteTableV2(string dbAccount)
		{
			_isInited = false;
			_isIniting = false;
			_initSynObj = new object();
			_dbAccount = dbAccount;
			InitData();
		}

		private void InitData()
		{
			CiteDict = new ConcurrentDictionary<string, WordCiteData>();
			_inputSugestHelper = new InputSugestionHelper(this);
		}

        public void AddInputPromptWordCite(string txt)
		{
			_inputSugestHelper.AddInputPromptWordCite(txt);
		}

        public WordCiteData GetWordCite(string text)
		{
			var wordCiteData = CiteDict.xTryGetValue(text, null);
			if (wordCiteData == null)
			{
				wordCiteData = new WordCiteData();
				CiteDict.TryAdd(text, wordCiteData);
			}
			return wordCiteData;
		}

        public void LoadFromDb(bool force = false)
		{
			lock (_initSynObj)
			{
                if (force || (!_isInited && !_isIniting))
				{
					_isIniting = true;
                    Task.Factory.StartNew(Init, TaskCreationOptions.LongRunning);
				}
			}
		}


		private void Init()
		{
			InitData();
            InitShortcuts();
			_isIniting = false;
			_isInited = true;
		}

        public WordCiteData GetWordCiteData(string wdKey)
        {
            var wordCiteData = CiteDict.xTryGetValue(wdKey, null);
            if (wordCiteData == null)
            {
                wordCiteData = new WordCiteData();
                CiteDict.TryAdd(wdKey, wordCiteData);
            }
            return wordCiteData;
        }

        private void InitShortcuts()
        {
            string mainNick = TbNickHelper.GetWwMainNickFromPubOrPrvDbAccount(_dbAccount);
            var ses = ShortcutHelper.GetShopShortcuts(mainNick);
            foreach (var et in ses)
            {
                AddInputPromptWordCite(et);
            }
        }

        public void AddInputPromptWordCite(ShortcutEntity et)
        {
            _inputSugestHelper.AddInputPromptWordCite(et);
        }

        public List<CtlAnswer.Item4Show> GetInputSugestion(string input, Dictionary<long, double> contextNumiid = null, int maxCount = 5)
		{
            var prompts=  _inputSugestHelper.GetInputSugestion(input, contextNumiid, maxCount);
            prompts = prompts ?? new List<InputPromptString>();
            return prompts.Select(k =>
            {
                CtlAnswer.Item4Show rt;
                if (k.Tag is ShortcutEntity)
                {
                    rt = new CtlAnswer.Item4Show(k.Tag as ShortcutEntity);
                }
                else
                {
                    string text = (k.Text != null) ? k.Text.Replace("{u:图片}", "") : null;
                    rt = new CtlAnswer.Item4Show(text, text);
                }
                return rt;
            }).ToList();
		}
	}
}
