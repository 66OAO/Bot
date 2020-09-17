using System;
using BotLib.Extensions;
using Newtonsoft.Json;

namespace Bot.AI.WordSplitterNs.WordNs
{
	public class GoodsUrl : UrlWord
	{
		[JsonConstructor]
		public GoodsUrl(string text): base(text)
		{
		}

		[JsonIgnore]
		public string Gid
		{
			get
			{
				if (this._gid == null)
				{
					this._gid = RegexEx.Match(base.Text.ToLower(), "(?<=(item.taobao|detail.tmall).com/item.htm\\?[a-zA-Z0-9_+-./%&=]*?id=)\\d+");
				}
				return this._gid;
			}
		}

        protected override string[] GetSemantics()
		{
			return new string[]
			{
				"W_GoodsUrl",
				"W_URL",
				"W_GoodsUrl_" + this.Gid
			};
		}

		private string _gid;
	}
}
