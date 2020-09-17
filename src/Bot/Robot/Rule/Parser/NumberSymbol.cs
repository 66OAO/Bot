using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BotLib.Extensions;
using BotLib.Misc;
using Newtonsoft.Json;

namespace Bot.Robot.Rule.Parser
{
	public class NumberSymbol : WildcardSymbol
	{
        public NumberSymbol(string matchText, int startIndex, Symbol pre)
            : base(matchText, startIndex, pre)
		{
			this._isNumberInit = false;
		}

		[JsonIgnore]
		public double Number
		{
			get
			{
				if (!this._isNumberInit)
				{
					string input = this.MatchText.xConvertChineseNumberStringToArabNumberString();
					this._number = Convert.ToDouble(input);
					this._isNumberInit = true;
				}
				return this._number;
			}
		}

        public override string GetSymbolText()
		{
			return "{*:数字}";
		}

        protected override string[] GetSemantics()
		{
			string matchText = this.MatchText;
			string text = this.MatchText.xConvertChineseNumberStringToArabNumberString();
			string[] result;
			if (matchText == text)
			{
				result = new string[]
				{
					"W_数字",
					matchText
				};
			}
			else
			{
				result = new string[]
				{
					"W_数字",
					matchText,
					text
				};
			}
			return result;
		}


        public override bool AsPatternSymbolSemanticMatchWith(IEnumerable<string> semantics)
		{
            foreach (string value in semantics)
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
			return Regex.IsMatch(txt, "[0-9壹一二贰貳三参參四肆五伍六陆陸七柒八捌九玖零]");
		}

		private bool _isNumberInit;

		private double _number;

		public const string SymbolText = "{*:数字}";

		public const string SymbolTextWithoutBrace = "*:数字";
	}
}
