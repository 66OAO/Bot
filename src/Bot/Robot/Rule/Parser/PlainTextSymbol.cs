using System;
using System.Collections.Generic;
using System.Linq;
using Bot.AI.WordSplitterNs.WordNs;
using Newtonsoft.Json;

namespace Bot.Robot.Rule.Parser
{
	public class PlainTextSymbol : Symbol
	{
		public TextWord Word { get; private set; }

		[JsonConstructor]
        public PlainTextSymbol(TextWord textWord, int startIndex)
            : base(textWord.Text, startIndex, null)
		{
			this.Word = textWord;
		}

        public PlainTextSymbol(TextWord textWord, int startIndex, Symbol preSymbol)
            : base(textWord.Text, startIndex, preSymbol)
		{
			
			this.Word = textWord;
		}

        public PlainTextSymbol(string text, int startIndex, Symbol preSymbol)
            : base(text, startIndex, preSymbol)
		{
			this.Word = new TextWord(text);
		}

        protected override string[] GetSemantics()
		{
			return this.Word.Semantics;
		}

        public override bool AsPatternSymbolSemanticMatchWith(IEnumerable<string> semantics)
		{
            return semantics.Contains(this.Word.Text);
		}
	}
}
