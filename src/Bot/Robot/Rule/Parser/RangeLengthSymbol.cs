using System;

namespace Bot.Robot.Rule.Parser
{
	public class RangeLengthSymbol : LengthWildcardSymbol
	{
		public int MaxLength { get; private set; }

		public int MinLength { get; private set; }

        public RangeLengthSymbol(string matchText, int startIndex, Symbol preSymbol, int maxLength, int minLength)
            : base(matchText, startIndex, preSymbol)
		{
			
			this.MaxLength = maxLength;
			this.MinLength = minLength;
		}

        public override string GetSymbolText()
		{
			return "{*:m,n}";
		}

        public override bool IsLengthMatch(int len)
		{
			return len <= this.MaxLength && len >= this.MinLength;
		}

		public const string SymbolText = "{*:m,n}";
	}
}
