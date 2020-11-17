using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Bot.Common;
using Bot.Help;
using BotLib.Extensions;
using Xceed.Wpf.Toolkit;
using DbEntity;

namespace Bot.Options
{
	public partial class CtlSuperAccountOptions : UserControl, IOptions
    {
        private string _seller;
        private string _sellerMain;
		public CtlSuperAccountOptions(string seller)
		{
			InitializeComponent();
			InitUI(seller);
		}

		public OptionEnum OptionType
		{
			get
			{
				return OptionEnum.SuperAccount;
			}
		}

		public HelpData GetHelpData()
		{
			throw new NotImplementedException();
		}

		public void InitUI(string seller)
		{
			_seller = seller;
			_sellerMain = TbNickHelper.GetMainPart(seller);
			tbkTip.Text = string.Format("请输入【{0}】店铺的【特权子账号(权限与主账号一样)】：(输入子账号部分即可，多个账号用逗号分隔)", _sellerMain);
            var superAccounts = Params.Auth.GetSuperAccounts(_sellerMain);
            tboxNicks.Text = superAccounts.xToString(" ，");
			if (Params.Auth.GetIsAllAccountEditKnowledge(seller))
			{
				rbtKnAll.IsChecked = true;
				rbtKnSuper.IsChecked = false;
			}
			else
			{
				rbtKnAll.IsChecked = false;
				rbtKnSuper.IsChecked = true;
			}
			if (Params.Auth.GetIsAllAccountEditRobot(seller))
			{
				rbtRobotAll.IsChecked = true;
				rbtRobotSuper.IsChecked = false;
			}
			else
			{
				rbtRobotAll.IsChecked = false;
				rbtRobotSuper.IsChecked = true;
			}
			if (Params.Auth.GetIsAllAccountEditShortCut(seller))
			{
				rbtScAll.IsChecked = true;
				rbtScSuper.IsChecked = false;
			}
			else
			{
				rbtScAll.IsChecked = false;
				rbtScSuper.IsChecked = true;
			}
		}

		public void NavHelp()
		{
			throw new NotImplementedException();
		}

		public void RestoreDefault()
		{
		}

		public void Save(string seller)
		{
            var superAccounts = tboxNicks.Text;
            superAccounts = superAccounts.Replace(' ', ',').Replace('\u3000', ',');
            Params.Auth.SetSuperAccounts(_sellerMain, superAccounts);
			bool? isChecked = rbtKnAll.IsChecked;
			Params.Auth.SetIsAllAccountEditKnowledge(seller, isChecked.GetValueOrDefault() & isChecked != null);
			isChecked = rbtRobotAll.IsChecked;
			Params.Auth.SetIsAllAccountEditRobot(seller, isChecked.GetValueOrDefault() & isChecked != null);
			isChecked = rbtScAll.IsChecked;
			Params.Auth.SetIsAllAccountEditShortCut(seller, isChecked.GetValueOrDefault() & isChecked != null);
		}
	}
}
