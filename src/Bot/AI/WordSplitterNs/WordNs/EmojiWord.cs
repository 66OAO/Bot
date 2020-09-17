using System;
using Newtonsoft.Json;

namespace Bot.AI.WordSplitterNs.WordNs
{
	public class EmojiWord : TextWord, IAtomicTextWord
	{
		[JsonConstructor]
		public EmojiWord(string text): base(text)
		{
		}

        protected override string[] GetSemantics()
		{
			return new string[]
			{
				"W_Emoji",
				this.Text
			};
		}
	}
}
