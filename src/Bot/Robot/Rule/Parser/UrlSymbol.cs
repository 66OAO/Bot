using System;
using System.Collections.Generic;
using System.Linq;
using BotLib.Extensions;

namespace Bot.Robot.Rule.Parser
{
	public class UrlSymbol : WildcardSymbol
	{
        public UrlSymbol(string matchText, int startIndex, Symbol pre)
            : base(matchText, startIndex, pre)
		{
		}

        public override string GetSymbolText()
		{
			return "{*:链接}";
		}

        protected override string[] GetSemantics()
		{
			Uri uri = new Uri(this.MatchText);
			return new HashSet<string>
			{
				"W_URL",
				uri.Host,
				uri.Host + uri.LocalPath,
				uri.Host + uri.PathAndQuery
			}.ToArray<string>();
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

        public override bool IsTextContainsMyKindOfSymbol(string txt)
		{
			return txt.xIsUrl();
		}

		public const string SymbolText = "{*:链接}";

		public const string SymbolTextWithoutBrace = "*:链接";
	}
}
