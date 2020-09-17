using System;

namespace Bot.Robot.Rule.Parser
{
	public abstract class WildcardSymbol : Symbol
	{
        public WildcardSymbol(string matchText, int startIndex, Symbol pre)
            : base(matchText, startIndex, pre)
		{
		}
        public abstract string GetSymbolText();
        public static bool IsSymbolTextWithoutBrace(string meaning)
        {
            return meaning != "*:数字" && meaning != "*:链接" && meaning != "*:地址";
        }
        public abstract bool IsTextContainsMyKindOfSymbol(string txt);
	}
}
