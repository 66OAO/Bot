using System;
using BotLib;

namespace Bot.Robot.Rule.Parser
{
	public class AnyLengthSymbol : LengthWildcardSymbol
	{
        public AnyLengthSymbol(string matchText, int startIndex, Symbol pre)
            : base(matchText, startIndex, pre)
		{
		}

        public override string GetSymbolText()
		{
			return "{*}";
		}

        public override bool IsLengthMatch(int len)
		{
			Util.Assert(len >= 0);
			return true;
		}

		public const string SymbolText = "{*}";

		public const string SymbolTextWithoutBrace = "*";
	}
}
