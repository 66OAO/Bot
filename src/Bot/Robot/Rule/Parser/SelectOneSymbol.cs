using System;
using System.Collections.Generic;

namespace Bot.Robot.Rule.Parser
{
	public class SelectOneSymbol : SelectorSymbol
	{
        public SelectOneSymbol(string matchText, int startIndex, Symbol preSymbol, List<Symbol> compoudSymbols)
            : base(matchText, startIndex, preSymbol, compoudSymbols)
		{
		}
	}
}
