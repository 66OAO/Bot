using System;
using Newtonsoft.Json;

namespace Bot.AI.WordSplitterNs.WordNs
{
	public class TextWord : Semantic
	{
		public string Text { get; set; }

		[JsonConstructor]
		public TextWord(string text)
		{
			this.Text = text;
		}

        protected override string[] GetSemantics()
		{
			return new string[]
			{
				this.Text
			};
		}
	}
}
