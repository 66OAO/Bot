using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Bot.Robot.Rule.QaCiteTableV2
{
	public class WordCiteData
	{
		public InputPromptWordCiteData Prompt;
	}

    public class WordCiteData<T>
    {
        public ConcurrentBag<T> Items;
        private const int MaxCiteCount = 1000;
        private int _realItemCount;
        private double _logItemCount;
        private int _countForLogItemCount;
        public double LogItemCount
        {
            get
            {
                if (_countForLogItemCount != _realItemCount)
                {
                    _countForLogItemCount = _realItemCount;
                    _logItemCount = Math.Log((double)((_realItemCount <= 0) ? 1 : _realItemCount));
                }
                return _logItemCount;
            }
        }
        public WordCiteData()
        {
            Items = new ConcurrentBag<T>();
            _realItemCount = 0;
            _logItemCount = 0.0;
            _countForLogItemCount = -1;
        }

        public void Add(T t)
        {
            if (!Items.Contains(t))
            {
                if (Items.Count < 1000)
                {
                    Items.Add(t);
                }
                _realItemCount++;
            }
        }

    }
}
