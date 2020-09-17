using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Bot.Common.Windows;
using Bot.Common;
using DbEntity;

namespace Bot.AssistWindow.Widget.Bottom.BuyerNote
{
	public partial class WndBuyerNoteMgr : EtWindow
	{
		public WndBuyerNoteMgr()
		{
			this.InitializeComponent();
		}

		public static void MyShow(string seller)
		{
            EtWindow.ShowSameNickOneInstance<WndBuyerNoteMgr>(seller, () => {
                var wnd = new WndBuyerNoteMgr();
                wnd.Seller = seller;
                return wnd;
            }, null, false);
		}

		public string SellerMainNick
		{
			get
			{
				return TbNickHelper.GetMainPart(base.Seller);
			}
		}
	}
}
