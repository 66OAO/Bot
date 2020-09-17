using System;
using System.Collections.Generic;
using System.Linq;
using BotLib.Extensions;

namespace Bot.Robot.Rule.Parser
{
	public abstract class SelectorSymbol : Symbol
	{
        public SelectorSymbol(string matchText, int startIndex, Symbol preSymbol, List<Symbol> compoudSymbols)
            : base(matchText, startIndex, preSymbol)
		{
			this.CompoudSymbols = compoudSymbols;
		}

        public override bool AsPatternSymbolSemanticMatchWith(IEnumerable<string> semantics)
		{
			foreach (string value in base.Semantics)
			{
                if (semantics.Contains(value))
				{
					return true;
				}
			}
			return false;
		}

		public List<Symbol> CompoudSymbols { get; private set; }

        protected override string[] GetSemantics()
		{
			HashSet<string> hashSet = new HashSet<string>();
			foreach (Symbol symbol in this.CompoudSymbols)
			{
				hashSet.xAddRange(symbol.Semantics);
			}
			return hashSet.ToArray<string>();
		}
	}
}
