using System;

namespace Bot.Robot.Rule.Parser
{
	public class FixLengthSymbol : LengthWildcardSymbol
	{
		public new int Length { get; private set; }

        public FixLengthSymbol(string matchText, int startIndex, Symbol preSymbol, int len)
            : base(matchText, startIndex, preSymbol)
		{
			this.Length = len;
		}

        public override string GetSymbolText()
		{
			return "{*:n}";
		}

        public override bool IsLengthMatch(int len)
		{
			return len == this.Length;
		}

		public const string SymbolText = "{*:n}";
	}
}
