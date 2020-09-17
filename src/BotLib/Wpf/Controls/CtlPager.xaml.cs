using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using BotLib.Misc;

namespace BotLib.Wpf.Controls
{
	public partial class CtlPager : UserControl
	{
		public event EventHandler<PageChangedEventArgs> EvPageNumberChanged;

        private DelayCaller _dcaller;
        private bool _collapseOnOnePage = true;
        private int _pageCount;
        private int _pageNo;

		public void Init(int pageCount, bool collapseOnOnePage = true, int pageNo = 1)
		{
			base.IsEnabled = true;
			Util.Assert(pageNo > 0 && pageNo <= pageCount);
			this.tblkPageCount.Visibility = Visibility.Visible;
			this.tboxPageNo.IsEnabled = true;
			this._collapseOnOnePage = collapseOnOnePage;
			this.PageCount = pageCount;
			this.PageNo = pageNo;
			if (PageNo == pageNo)
			{
				this.RaisePageNoChangedEvent(pageNo, -1);
			}
		}

		public void Init(int recCount, int recPerPage, bool collapseOnOnePage = true, int pageNo = 1)
		{
			Util.Assert(recCount >= 0 && recPerPage > 0);
			int pageCount = (int)(1.0 + (double)recCount * 1.0 / (double)recPerPage);
			this.Init(pageCount, collapseOnOnePage, pageNo);
		}

		public void InitInfinitePageCount(bool disableOnPageChanged = true)
		{
			this.PageCount = int.MaxValue;
			this.PageNo = 1;
			this.tblkPageCount.Visibility = Visibility.Collapsed;
			this.tboxPageNo.IsEnabled = false;
		}

		public int PageCount
		{
			get
			{
				return this._pageCount;
			}
			private set
			{
				Util.Assert(value > 0);
				this._pageCount = value;
				this.tblkPageCount.Text = "/" + value;
				if (_collapseOnOnePage && value == 1)
				{
					base.Visibility = Visibility.Collapsed;
				}
				else
				{
					base.Visibility = Visibility.Visible;
				}
			}
		}

		public int PageNo
		{
			get
			{
				return this._pageNo;
			}
			set
			{
				Util.Assert(value >= 0 && value <= this.PageCount);
				this.tboxPageNo.Text = value.ToString();
				if (value != this._pageNo)
				{
					int pageNo = this._pageNo;
					this._pageNo = value;
					this.RaisePageNoChangedEvent(value, pageNo);
				}
			}
		}

		public CtlPager()
		{
			this.InitializeComponent();
			this._dcaller = new DelayCaller(()=>
			{
				this.OnInputPageNumber();
			}, 500, true);
		}

		public void Reset()
		{
			this.EvPageNumberChanged = null;
			this._pageNo = 0;
			this._pageCount = 0;
			this._collapseOnOnePage = true;
		}

		public void ShowFinished(int showRecCount, int recordPerPage)
		{
			if (PageCount >= int.MaxValue)
			{
				if (showRecCount == 0 && this.PageNo > 1)
				{
					this.Init(this.PageNo - 1, true, this.PageNo - 1);
				}
				else
				{
					if (showRecCount < recordPerPage)
					{
						this.Init(this.PageNo, true, this.PageNo);
					}
				}
			}
		}

		private void OnInputPageNumber()
		{
			if (!string.IsNullOrEmpty(tboxPageNo.Text.Trim()))
			{
				try
				{
					int pageNo = -1;
					try
					{
						pageNo = Convert.ToInt32(this.tboxPageNo.Text.Trim());
					}
					catch
					{
						pageNo = this.PageNo;
						Util.Beep();
					}
					if (pageNo < 1)
					{
						pageNo = 1;
						Util.Beep();
					}
					if (pageNo > this.PageCount)
					{
						pageNo = this.PageCount;
						Util.Beep();
					}
					this.PageNo = pageNo;
				}
				catch (Exception e)
				{
					Log.Exception(e);
				}
			}
		}

		private void RaisePageNoChangedEvent(int newPageNo, int oldPageNo)
		{
			IsEnabled = false;
			if (EvPageNumberChanged != null)
			{
				EvPageNumberChanged(this, new PageChangedEventArgs(newPageNo, oldPageNo));
			}
			IsEnabled = true;
		}

		private void Button_Prev_Click(object sender, RoutedEventArgs e)
		{
			if (PageNo > 1)
			{
				int pageNo = this.PageNo;
				this.PageNo = pageNo - 1;
			}
			else
			{
				Util.Beep();
			}
		}

		private void Button_Next_Click(object sender, RoutedEventArgs e)
		{
			if (PageNo < this.PageCount)
			{
				int pageNo = this.PageNo;
				this.PageNo = pageNo + 1;
			}
			else
			{
				Util.Beep();
			}
		}

		private void tboxPageNo_TextChanged(object sender, TextChangedEventArgs e)
		{
			this._dcaller.CallAfterDelay();
		}

		private void Button_Last_Click(object sender, RoutedEventArgs e)
		{
			if (this.PageNo == this.PageCount)
			{
				Util.Beep();
			}
			else
			{
				this.PageNo = this.PageCount;
			}
		}

		private void Button_First_Click(object sender, RoutedEventArgs e)
		{
			if (PageNo == 1)
			{
				Util.Beep();
			}
			else
			{
				this.PageNo = 1;
			}
		}

		private void tboxPageNo_LostFocus(object sender, RoutedEventArgs e)
		{
			this.tboxPageNo.Text = this.PageNo.ToString();
		}

	}
}
