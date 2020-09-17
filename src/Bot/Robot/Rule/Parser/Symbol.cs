using System;
using System.Collections.Generic;
using BotLib.Extensions;
using Newtonsoft.Json;

namespace Bot.Robot.Rule.Parser
{
	public abstract class Symbol
    {
        public string MatchText;

        private int _nextIndex;

        private string[] _sementic;

        private List<Symbol> _list;

		public string TypeName
		{
			get
			{
				return base.GetType().Name;
			}
		}

		[JsonIgnore]
		public bool IsSolid
		{
			get
			{
				return !(this is LengthWildcardSymbol);
			}
		}

		public int StartIndex { get; set; }

		[JsonIgnore]
		public int NextIndex
		{
			get
			{
				if (this._nextIndex == 0)
				{
					this._nextIndex = this.StartIndex + this.MatchText.Length;
				}
				return this._nextIndex;
			}
		}

		[JsonIgnore]
		public string[] Semantics
		{
			get
			{
				if (this._sementic == null)
				{
                    this._sementic = this.GetSemantics();
				}
				return this._sementic;
			}
		}

        protected abstract string[] GetSemantics();

		[JsonIgnore]
		public int Length
		{
			get
			{
				return this.MatchText.Length;
			}
		}

		[JsonIgnore]
		public Symbol PreSymbol { get; set; }

        public Symbol(string matchText, int startIndex, Symbol preSymbol)
		{
			this._list = null;
            this.MatchText = matchText;
			this.StartIndex = startIndex;
			this.PreSymbol = preSymbol;
		}

        public List<Symbol> SplitToList()
		{
			List<Symbol> list = new List<Symbol>();
			for (Symbol symbol = this; symbol != null; symbol = symbol.PreSymbol)
			{
				Symbol syb = symbol.Clone();
				syb.PreSymbol = null;
				list.Add(syb);
			}
			list.Reverse();
			return list;
		}

		[JsonIgnore]
		public List<Symbol> List
		{
			get
			{
				if (this._list == null)
				{
                    this.ReConstructList();
				}
				return this._list;
			}
		}

        public void ReConstructList()
		{
            this._list = this.SplitToList();
		}

		public override string ToString()
		{
            string text = string.Empty;
			if (this.PreSymbol != null)
			{
				text = this.PreSymbol.ToString() + text;
			}
			return text;
		}


        public Symbol Clone()
		{
			return base.MemberwiseClone() as Symbol;
		}

        public bool IsEqual(Symbol symbol)
		{
			return base.GetType() == symbol.GetType() && this.StartIndex == symbol.StartIndex && this.NextIndex == symbol.NextIndex;
		}

        public void UpdateStartIndex(int startIndexShift)
		{
			this.StartIndex += startIndexShift;
			this._nextIndex = this.StartIndex + this.MatchText.Length;
		}

        public abstract bool AsPatternSymbolSemanticMatchWith(IEnumerable<string> semantics);

	}
}
