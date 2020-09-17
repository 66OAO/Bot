using System;
using Newtonsoft.Json;

namespace Bot.AI.WordSplitterNs.WordNs
{
	public class EMailWord : UrlWord, IAtomicTextWord
	{
		[JsonConstructor]
		public EMailWord(string text): base(text)
		{
		}

        protected override string[] GetSemantics()
		{
			return new string[]
			{
				"W_Email",
				base.Text
			};
		}
	}
}
