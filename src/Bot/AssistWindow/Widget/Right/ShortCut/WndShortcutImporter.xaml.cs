using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Bot.Common;
using Bot.Common.Account;
using Bot.Common.Windows;
using Bot.Help;
using DbEntity;
using BotLib.Db.Sqlite;
using BotLib.Wpf.Extensions;
using Xceed.Wpf.Toolkit;

namespace Bot.AssistWindow.Widget.Right.ShortCut
{
	public partial class WndShortcutImporter : EtWindow
	{
		private string _sellerMain;
		private Action _onImportFinished;
		private Action _onStartImport;
		private class LocalParams
		{
			public static bool IsReplace
			{
				get
				{
					return PersistentParams.GetParam("IsReplace", false);
				}
				set
				{
					PersistentParams.TrySaveParam("IsReplace", value);
				}
			}

			public static bool IsPub
			{
				get
				{
					return PersistentParams.GetParam("IsPub", true);
				}
				set
				{
					PersistentParams.TrySaveParam("IsPub", value);
				}
			}
		}

		public WndShortcutImporter(string seller)
		{
			this.InitializeComponent();
			this.Seller = seller;
			this._sellerMain = TbNickHelper.GetMainPart(seller);
			if (LocalParams.IsReplace)
			{
				this.rbtReplace.IsChecked = new bool?(true);
				this.rbtAppend.IsChecked = new bool?(false);
			}
			else
			{
				this.rbtReplace.IsChecked = new bool?(false);
				this.rbtAppend.IsChecked = new bool?(true);
			}
			if (LocalParams.IsPub)
			{
				this.rbtPub.IsChecked = new bool?(true);
			}
			else
			{
				LocalParams.IsPub = false;
				this.rbtPub.Content = "导入成：全店通用短语(没有权限，需要主号或特权子号)";
				this.rbtPub.IsEnabled = false;
				
				this.rbtPrv.IsChecked = new bool?(true);
			}
			this.SelectFile(null);
		}

		public static void MyShow(string seller, Window owner, Action onStartImport, Action onImportFinished)
		{
			new WndShortcutImporter(seller)
			{
				_onImportFinished = onImportFinished,
				_onStartImport = onStartImport
			}.FirstShow(seller, owner, null, false);
		}

		private void wtboxFilename_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			var fileName = this.wtboxFilename.Text.Trim();
			this.SelectFile(fileName);
		}

		private void SelectFile(string oldFileName = null)
		{
			var fileName = OpenFileDialogEx.GetOpenFileName("选择【快捷短语】文件", "CSV格式话术文件(*.csv)|*.csv", oldFileName);
			if (!string.IsNullOrEmpty(fileName) && fileName != oldFileName)
			{
				this.wtboxFilename.Text = fileName;
                var dbAccount = this.GetDbAccount(fileName);
                if (!string.IsNullOrEmpty(dbAccount))
				{
                    this.SetImportDataType(dbAccount);
                    this.ImportOtherNickDataTip(dbAccount);
				}
			}
		}

		private void ImportOtherNickDataTip(string dbAccount)
		{
			var mainNick = TbNickHelper.GetWwMainNickFromPubOrPrvDbAccount(dbAccount);
			if (mainNick != this._sellerMain)
			{
				var msg = string.Format("注意：确定要导入其它店铺的短语？打开的文件是【{0}】店铺的短语", mainNick);
				MsgBox.ShowDialog(msg, null, "提示");
			}
		}

        private void SetImportDataType(string dbAccount)
		{
            if (TbNickHelper.IsPubDbAccount(dbAccount))
			{
				this.rbtPub.IsChecked = true;
			}
			else
			{
				this.rbtPrv.IsChecked = true;
			}
		}

        private string GetDbAccount(string fileName)
		{
            return ShortcutImporter.GetDbAccount(fileName);
		}

		private void btnHelp_Click(object sender, RoutedEventArgs e)
		{
		}

		private void rbtReplace_Click(object sender, RoutedEventArgs e)
		{
			bool? isChecked = this.rbtReplace.IsChecked;
			LocalParams.IsReplace = (isChecked.GetValueOrDefault() & isChecked != null);
		}

        private void btnImport_Click(object sender, RoutedEventArgs e)
		{
			this.Import();
		}

		private void rbtPrv_Click(object sender, RoutedEventArgs e)
		{
			bool? isChecked = this.rbtPub.IsChecked;
			LocalParams.IsPub = (isChecked.GetValueOrDefault() & isChecked != null);
		}

		private void Import()
		{
			var fn = this.wtboxFilename.Text.Trim();
			if (string.IsNullOrEmpty(fn))
			{
				MsgBox.ShowErrTip("请选择需要导入的文件", null);
			}
			else
			{
				var isAllReplace = LocalParams.IsReplace;
				var dbaccount = (LocalParams.IsPub ? AccountHelper.GetPubDbAccount(_sellerMain) : AccountHelper.GetPrvDbAccount(Seller));
				SaveShortcutShowType();
				Close();
				if (_onStartImport != null)
				{
					_onStartImport();
				}
				Task.Factory.StartNew(()=>{
                    
                }, TaskCreationOptions.LongRunning);
			}
		}

		private void SaveShortcutShowType()
		{
			if (!LocalParams.IsPub)
			{
				var showType = Params.Shortcut.GetShowType(Seller);
				if (showType == Params.Shortcut.ShowType.ShopOnly)
				{
					Params.Shortcut.SetShowType(Seller, Params.Shortcut.ShowType.ShopAndSelf);
				}
			}
		}

	}
}
