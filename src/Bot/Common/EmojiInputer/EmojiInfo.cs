﻿using System;

namespace Bot.Common.EmojiInputer
{
	public class EmojiInfo
	{
		public string Desc;
		public string Text;
		public EmojiInfo(string desc, string text)
		{
			this.Desc = desc;
			this.Text = text;
		}
	}
}
