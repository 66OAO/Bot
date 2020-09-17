using System;

namespace Bot.Robot.Rule.Parser
{
	public class NotMoreThanLengthSymbol : LengthWildcardSymbol
	{
		public int MaxLength { get; private set; }

        public NotMoreThanLengthSymbol(string matchText, int startIndex, Symbol preSymbol, int maxLength)
            : base(matchText, startIndex, preSymbol)
		{
			this.MaxLength = maxLength;
		}

        public override string GetSymbolText()
		{
			return "{*:,n}";
		}

        public override bool IsLengthMatch(int len)
		{
			return len <= this.MaxLength;
		}

		public const string SymbolText = "{*:,n}";
	}
}
