using System;
using System.Collections.Generic;

namespace Bot.Robot.Rule.Parser
{
	public class AddressSymbol : WildcardSymbol
	{
        public AddressSymbol(string matchText, int startIndex, Symbol pre)
            : base(matchText, startIndex, pre)
		{
		}

        public override string GetSymbolText()
		{
			return "{*:地址}";
		}

        protected override string[] GetSemantics()
		{
			return new string[]
			{
				"W_地址",
				this.MatchText
			};
		}

        public override bool IsTextContainsMyKindOfSymbol(string txt)
		{
			return false;
		}

        public override bool AsPatternSymbolSemanticMatchWith(IEnumerable<string> semantics)
		{
            return true;
		}

		public const string SymbolText = "{*:地址}";

		public const string SymbolTextWithoutBrace = "*:地址";
	}
}
