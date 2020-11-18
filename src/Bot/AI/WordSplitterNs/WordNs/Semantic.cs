using System;
using Newtonsoft.Json;

namespace Bot.AI.WordSplitterNs.WordNs
{
	public abstract class Semantic
	{
        protected abstract string[] GetSemantics();
		private string[] _semantic;

		[JsonIgnore]
		public string[] Semantics
		{
			get
			{
				if (this._semantic == null)
				{
                    this._semantic = this.GetSemantics();
				}
				return this._semantic;
			}
		}
	}
}
