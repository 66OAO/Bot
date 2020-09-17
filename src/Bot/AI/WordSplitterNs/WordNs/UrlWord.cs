using System;
using System.Collections.Generic;
using System.Linq;
using BotLib;
using Newtonsoft.Json;

namespace Bot.AI.WordSplitterNs.WordNs
{
	public class UrlWord : TextWord, IAtomicTextWord
	{
		[JsonConstructor]
		public UrlWord(string text): base(text)
		{
		}

        protected override string[] GetSemantics()
		{
			try
			{
				Uri uri = null;
				try
				{
					uri = new Uri(Text);
				}
				catch
				{
					uri = new Uri("http://" + Text);
				}
				return new HashSet<string>
				{
					"W_URL",
					uri.Host,
					uri.Host + uri.LocalPath,
					uri.Host + uri.PathAndQuery
				}.ToArray();
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
			return new string[0];
		}
	}
}
