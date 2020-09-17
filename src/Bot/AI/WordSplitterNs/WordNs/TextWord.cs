using System;
using Newtonsoft.Json;

namespace Bot.AI.WordSplitterNs.WordNs
{
	public class TextWord : Semantic
	{
		[JsonConstructor]
		public TextWord(string text)
		{
			this.Text = text;
		}

		public string Text { get; set; }

        protected override string[] GetSemantics()
		{
			return new string[]
			{
				this.Text
			};
		}
	}
}
