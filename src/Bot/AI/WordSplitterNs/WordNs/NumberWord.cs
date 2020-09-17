using System;
using BotLib.Extensions;
using Newtonsoft.Json;

namespace Bot.AI.WordSplitterNs.WordNs
{
	public class NumberWord : TextWord
	{
		[JsonConstructor]
		public NumberWord(string text): base(text)
		{
		}

        protected override string[] GetSemantics()
		{
			var arabText = this.Text.xConvertChineseNumberStringToArabNumberString();
			string[] rt;
            if (Text == arabText)
			{
				rt = new string[]
				{
					"W_数字",
					Text
				};
			}
			else
			{
				rt = new string[]
				{
					"W_数字",
					Text,
					arabText
				};
			}
			return rt;
		}
	}
}
