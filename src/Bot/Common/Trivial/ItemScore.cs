using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Common.Trivial
{
    public class ItemScore<T>
    {
        public ItemScore(T item, double score)
		{
			this.Score = score;
			this.Item = item;
		}

        public double Score;

        public T Item;
    }
}
