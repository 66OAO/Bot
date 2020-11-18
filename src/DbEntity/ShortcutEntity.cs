using System;
using BotLib.Extensions;

namespace DbEntity
{
	public class ShortcutEntity : TreeNode
    {
        private string _uniformCode;
		public string Code { get; set; }
        public string Text { get; set; }
		public string ImageName { get; set; }
		public string Title { get; set; }

		public string GetUniformCode()
		{
			if (_uniformCode == null)
			{
				if (string.IsNullOrEmpty(this.Code))
				{
					this._uniformCode = "";
				}
				else
				{
					this._uniformCode = this.Code.ToLower().xToBanJiao();
				}
			}
			return this._uniformCode;
		}


		public string GetShowTitle()
		{
			string text;
            if (string.IsNullOrEmpty(this.Title))
			{
                if (Text.Length > 20)
				{
					text = this.Text.Substring(0, 20) + "...";
				}
				else
				{
					text = this.Text;
				}
			}
			else
			{
				text = this.Title;
			}
			if (text == null)
			{
				text = "???";
			}
			return text.xRemoveLineBreak(" ");
		}
	}
}
