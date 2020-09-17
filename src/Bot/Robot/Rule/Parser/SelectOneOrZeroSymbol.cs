using System;
using System.Collections.Generic;

namespace Bot.Robot.Rule.Parser
{
	public class SelectOneOrZeroSymbol : SelectorSymbol
	{
        public SelectOneOrZeroSymbol(string matchText, int startIndex, Symbol preSymbol, List<Symbol> compoudSymbols)
            : base(matchText, startIndex, preSymbol, compoudSymbols)
		{
		}
	}
}
