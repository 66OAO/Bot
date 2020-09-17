using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using Bot.Common;
using BotLib.Wpf.Extensions;
using DbEntity;

namespace Bot.AssistWindow.Widget.Bottom.BuyerNote
{
	public partial class CtlFavoriteNotes : UserControl
    {
        private WndBuyerNoteMgr _wndMgrDontUse;

		private WndBuyerNoteMgr WndMgr
		{
			get
			{
				if (_wndMgrDontUse == null)
				{
					_wndMgrDontUse = this.xFindAncestor<WndBuyerNoteMgr>();
				}
				return _wndMgrDontUse;
			}
		}

		public CtlFavoriteNotes()
		{
			InitializeComponent();
			if (!DesignerProperties.GetIsInDesignMode(this))
			{
				base.Loaded += CtlFavoriteNotes_Loaded;
			}
		}

		private void CtlFavoriteNotes_Loaded(object sender, RoutedEventArgs e)
		{
			base.Loaded -= CtlFavoriteNotes_Loaded;
			LoadData();
		}

		private void LoadData()
		{
			var favNotes = BuyerNoteHelper.GetFavNotes(WndMgr.SellerMainNick);
			dgMain.ItemsSource = favNotes.Select(k=>new RowData(k));
		}

		private void btnCreate_Click(object sender, RoutedEventArgs e)
		{
            WndFavoriteNoteEditor.Create(WndMgr.SellerMainNick, WndMgr, LoadData);
		}
		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			WndMgr.Close();
		}

        private void hyperlink_Click(object sender, RoutedEventArgs e)
		{
            var rowData = (RowData)dgMain.SelectedItem;
            var hyperlink = e.OriginalSource as Hyperlink;
            if (rowData != null && hyperlink != null)
            {
                string originalString = hyperlink.NavigateUri.OriginalString;
                if (originalString == "编辑")
                {
                    WndFavoriteNoteEditor.Edit(WndMgr.SellerMainNick, rowData.NoteEntity, WndMgr, () => { LoadData(); });
                }
                else
                {
                    if (originalString != "删除")
                    {
                        throw new Exception();
                    }
                    if ( MsgBox.ShowDialog("确定要删除？", "操作确认", "Delete", WndMgr))
                    {
                        BuyerNoteHelper.Delete(rowData.NoteEntity);
                        LoadData();
                    }
                }
            }
		}

		private class RowData
		{
			public string Delete
			{
				get
				{
					return "删除";
				}
			}

			public string Edit
			{
				get
				{
					return "编辑";
				}
			}

			public string Note { get; private set; }

			public FavoriteNoteEntity NoteEntity { get; private set; }

			public RowData(FavoriteNoteEntity favoriteNoteEntity_0)
			{
				NoteEntity = favoriteNoteEntity_0;
				Note = favoriteNoteEntity_0.Note;
			}
		}
	}
}
