using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using Bot.Common;
using BotLib;
using BotLib.Extensions;
using BotLib.Misc;
using BotLib.Wpf.Controls;
using BotLib.Wpf.Extensions;
using DbEntity;

namespace Bot.AssistWindow.Widget.Bottom.BuyerNote
{
	public partial class CtlBuyerNoteViewer : UserControl
    {
        private DelayCaller _dcaller;
        private List<CtlBuyerNoteViewer.RowData> _pageData;
        private WndBuyerNoteMgr _wndMgrDontUse;
        private int _recPerPage;
        private List<BuyerNoteEntity> _searchedNotes;
        
        public CtlBuyerNoteViewer()
		{
			_recPerPage = 15;
			InitializeComponent();
			if (!DesignerProperties.GetIsInDesignMode(this))
			{
				base.Loaded += CtlBuyerNoteViewer_Loaded;
			}
		}

		private void CtlBuyerNoteViewer_Loaded(object sender, RoutedEventArgs e)
		{
			base.Loaded -= CtlBuyerNoteViewer_Loaded;
            _dcaller = new DelayCaller(LoadBuyerNotes, 500, true);
            pager.EvPageNumberChanged += pager_EvPageNumberChanged;
			btnReset.xPerformClick();
		}

        private void pager_EvPageNumberChanged(object sender, PageChangedEventArgs e)
		{
			ShowPage(e.NewPageNo);
		}

		private void ShowPage(int newPageNo)
		{
			_searchedNotes = (_searchedNotes ?? new List<BuyerNoteEntity>());
			_pageData = _searchedNotes.Skip(_recPerPage * (newPageNo - 1)).Take(_recPerPage).Select(k=>new RowData(k)).ToList();
			dgMain.ItemsSource = _pageData;
		}

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

        private SearchCondition GetSearchCondition()
		{
			var searchCond = new SearchCondition();
			searchCond.DateFrom = dpFrom.SelectedDate;
			var selectedDate = dpTo.SelectedDate;
			if (selectedDate != null)
			{
				selectedDate = new DateTime?(selectedDate.Value.Date.AddDays(1.0).AddMilliseconds(-1.0));
			}
			searchCond.DateTo = selectedDate;
			searchCond.Seller = tboxSeller.Text.Trim();
			searchCond.Buyer = tboxBuyer.Text.Trim();
			return searchCond;
		}

        private void ClearSearchCondition()
		{
			ResetSearchDate();
			tboxBuyer.Text = "";
			tboxSeller.Text = "";
			cboxLastDays.IsChecked = false;
		}

        private void ResetSearchDate()
		{
			dpFrom.SelectedDate = new DateTime(2010, 1, 1);
			dpTo.SelectedDate = BatTime.Now.Date;
		}

		private void hyperLink_Click(object sender, RoutedEventArgs e)
		{
		    var rowData = (RowData)dgMain.SelectedItem;
			var link = e.OriginalSource as Hyperlink;
			if (rowData != null && link != null)
			{
				string originalString = link.NavigateUri.OriginalString;
				if (originalString == "详情")
				{
					WndDetail.MyShow(rowData.NoteEntity.BuyerMainNick, rowData.NoteEntity.Recorder);
				}
				else
				{
					if (!(originalString == "删除"))
					{
						throw new Exception();
					}
                    if (MsgBox.ShowDialog("确定要删除？", "操作确认", "CtlBuyerNoteViewer.Delete", WndMgr))
                    {
                        BuyerNoteHelper.Delete(rowData.NoteEntity);
                        _searchedNotes.Remove(rowData.NoteEntity);
                        var newPageNo = (_pageData.Count > 1) ? pager.PageNo : (pager.PageNo - 1);
                        ShowPage(newPageNo);
                    }
				}
			}
		}

		private void btnReset_Click(object sender, RoutedEventArgs e)
		{
            ClearSearchCondition();
			LoadBuyerNotes();
		}

        private void LoadBuyerNotes()
        {
            var searchCondition = GetSearchCondition();
            _searchedNotes = BuyerNoteHelper.SearchBuyerNotes(searchCondition.DateFrom, searchCondition.DateTo, searchCondition.Seller, searchCondition.Buyer, WndMgr.Seller);
            _searchedNotes = (_searchedNotes ?? new List<BuyerNoteEntity>());
            pager.Init(_searchedNotes.Count, _recPerPage, true, 1);
        }

		private void btnHelp_Click(object sender, RoutedEventArgs e)
		{
		}

		private async void btnDeleteAll_Click(object sender, RoutedEventArgs e)
		{
            if (MsgBox.ShowDialog("确定要删除？", "操作确认", WndMgr))
            {
                await Task.Factory.StartNew(() => {
                    BuyerNoteHelper.BatchDelete(_searchedNotes);
                }, TaskCreationOptions.LongRunning);
                btnReset.xPerformClick();
            }
		}
		private string SellerMainNick
		{
			get
			{
				return TbNickHelper.GetMainPart(WndMgr.Seller);
			}
		}

		private async void BtnImport_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btnExport_Click(object sender, RoutedEventArgs e)
		{

		}

        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			if (base.IsLoaded)
			{
				LoadBuyerNotes();
			}
		}

		private void tboxSeller_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (base.IsLoaded)
			{
				_dcaller.CallAfterDelay();
			}
		}

		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			WndMgr.Close();
		}

		private void cboxLastDays_Click(object sender, RoutedEventArgs e)
		{
			var isChecked = cboxLastDays.IsChecked;
			if (isChecked.HasValue && isChecked.Value)
			{
				dpFrom.SelectedDate = BatTime.Now.AddDays(-3.0).Date;
				dpTo.SelectedDate = BatTime.Now.Date;
			}
			else
			{
                ResetSearchDate();
			}
			LoadBuyerNotes();
		}

		private class RowData
		{
			public BuyerNoteEntity NoteEntity { get; private set; }

			public string BuyerMainNick { get; private set; }

			public string Recorder { get; private set; }

			public string Note { get; private set; }

			public string Modified { get; private set; }

			public string Delete
			{
				get
				{
					return "删除";
				}
			}

			public string Detail
			{
				get
				{
					return "详情";
				}
			}

			public RowData(BuyerNoteEntity buyerNoteEntity_0)
			{
				NoteEntity = buyerNoteEntity_0;
				Modified = buyerNoteEntity_0.RecordTime.xToString();
				Note = buyerNoteEntity_0.Note;
				Recorder = buyerNoteEntity_0.Recorder;
				BuyerMainNick = buyerNoteEntity_0.BuyerMainNick;
			}
		}

		public class SearchCondition
		{
			public SearchCondition()
			{
				DateFrom = null;
				DateTo = null;
			}

			public DateTime? DateFrom;

			public DateTime? DateTo;

			public string Buyer;

			public string Seller;
		}
	}
}
