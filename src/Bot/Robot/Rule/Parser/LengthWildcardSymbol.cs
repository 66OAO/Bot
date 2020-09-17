using System;
using System.Collections.Generic;

namespace Bot.Robot.Rule.Parser
{
	public abstract class LengthWildcardSymbol : WildcardSymbol
	{
        public LengthWildcardSymbol(string matchText, int startIndex, Symbol pre)
            : base(matchText, startIndex, pre)
		{
		}

        public abstract bool IsLengthMatch(int len);

        public override bool AsPatternSymbolSemanticMatchWith(IEnumerable<string> semantics)
		{
			return true;
		}

        protected override string[] GetSemantics()
		{
			return new string[0];
		}

        public override bool IsTextContainsMyKindOfSymbol(string txt)
		{
			return false;
		}
    }
}
