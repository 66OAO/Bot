using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Bot.AssistWindow.Widget.Right.ShortCut;
using Bot.Common;
using Bot.Automation.ChatDeskNs;
using BotLib;
using BotLib.Wpf.Extensions;
using Bot.AssistWindow.Widget.Robot;
using Bot.Common.Db;

namespace Bot.AssistWindow.Widget
{
	public partial class RightPanel : UserControl, IWakable
    {
        private WndAssist _wndDontUse;
        private bool _isWiden;
        private bool _isHighden;
        private bool _isSouthEast;
        private enum TabTypeEnum
        {
            Unknown,
            Logis,
            ShortCut,
            Robot,
            Goods,
            Order,
            Coupon,
            Test
        }

        private WndAssist Wnd
        {
            get
            {
                if (_wndDontUse == null)
                {
                    WndAssist wnd = this.xFindParentWindow() as WndAssist;
                    Init(wnd);
                    Util.Assert(_wndDontUse != null);
                }
                return _wndDontUse;
            }
            set
            {
                _wndDontUse = value;
            }
        }

		public RightPanel()
		{
			_isWiden = false;
			_isHighden = false;
			_isSouthEast = false;
			InitializeComponent();
		}

		public void Init(WndAssist wnd)
		{
			if (_wndDontUse == null)
			{
				Wnd = wnd;
				lblSeller.Content = Wnd.Desk.Seller;
				string seller = Wnd.Desk.Seller;
                string[] tabs = new string[] { "话术"/*, "机器人"*/ };
                foreach (string tabName in tabs)
                {
                    TabTypeEnum tabType = GetTabType(tabName);
                    TabItem tabItem = CreateTabItem(tabType);
                    if (tabItem != null)
                    {
                        AddTabItem(tabItem, tabType);
                    }
                }
				tabControl.SelectionChanged -= tabControl_SelectionChanged;
				tabControl.SelectionChanged += tabControl_SelectionChanged;
			}
		}

		public void UpdateRobotTab()
		{
            DispatcherEx.xInvoke(() => {
            });
		}

		public void UpdateShortcutUI()
		{
			DispatcherEx.xInvoke(()=>{
                try
			    {
				    TabItem tabItem = GetTabItem(TabTypeEnum.ShortCut);
				    CtlShortcut ctlShortcut = ((tabItem != null) ? tabItem.Content : null) as CtlShortcut;
				    if (ctlShortcut != null)
				    {
                        ctlShortcut.SetTitleButtonsVisible();
					    ctlShortcut.SetContentVisible();
                        ctlShortcut.LoadDatas(null, null);
				    }
			    }
			    catch (Exception e)
			    {
                    Log.Exception(e);
			    }
            });
		}

		public void ReShowAfterChangePanelOption()
		{
            Util.Assert(_wndDontUse != null);
			tabControl.Items.Clear();
			var seller = Wnd.Desk.Seller;
            var tabs = new string[] {"话术","机器人" };
			foreach (string tabName in tabs)
			{
                TabTypeEnum tabType = GetTabType(tabName);
                TabItem tabItem = CreateTabItem(tabType);
                if (tabItem != null)
                {
                    AddTabItem(tabItem, tabType);
                }
			}
		}

		private TabItem CreateTabItem(TabTypeEnum tabType)
		{
            TabItem tabItem = null ;
			switch (tabType)
			{
			    case TabTypeEnum.ShortCut:
                    tabItem = TabShortcut();
				    break;
			    default:
                    tabItem = TabRobot();
				    break;
			}
            return tabItem;
		}

		private TabTypeEnum GetTabType(string typeName)
		{
            TabTypeEnum tabType = TabTypeEnum.Unknown;
            switch (typeName)
            {
                case "话术":
                    tabType = TabTypeEnum.ShortCut;
                    break;
                case "机器人":
                    tabType = TabTypeEnum.Robot;
                    break;
            }
			return tabType;
		}


		private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.OriginalSource == tabControl)
			{
                SetTabStyle(e);
				ActivateTab(e);
			}
		}

        private void SetTabStyle(SelectionChangedEventArgs e)
		{
            if (e.AddedItems.Count > 0)
            {
                var tabIt = e.AddedItems[0] as TabItem;
                if (tabIt != null)
                {
                    var tb = tabIt.Header as TextBlock;
                    tb.Foreground = Brushes.White;
                }
            }
            if (e.RemovedItems.Count > 0)
            {
                var tabIt = e.RemovedItems[0] as TabItem;
                if (tabIt != null)
                {
                    var tb = tabIt.Header as TextBlock;
                    tb.Foreground = Brushes.Black;
                }
            }
		}


        private TabItem TabShortcut()
		{
			return new TabItem
			{
				Header = "话术",
				Content = new CtlShortcut(Wnd.Desk, this)
			};
		}

        private TabItem TabRobot()
        {
            return new TabItem
            {
                Header = "机器人",
                Content = new CtlRobotQA(Wnd)
            };
        }

		private void AddTabItem(TabItem tabItem, TabTypeEnum tabType)
		{
			tabItem.Tag = tabType;
			if (!(tabItem.Header is TextBlock))
			{
				tabItem.Header = TextBlockEx.Create(tabItem.Header.ToString(), new object[0]);
			}
			tabItem.Style = (Style)FindResource("tabRightPanel");
			tabControl.Items.Add(tabItem);
		}

		private TabItem GetTabItem(TabTypeEnum tabType)
		{
			TabItem tabItem = null;
            foreach (TabItem item in tabControl.Items)
			{
                TabTypeEnum ty = (TabTypeEnum)item.Tag;
				if (ty == tabType)
				{
                    tabItem = item;
					break;
				}
			}
			return tabItem;
		}

        private void rectWiden_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_isWiden = true;
		}

        private void rectWiden_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_isWiden)
			{
				_isWiden = false;
				Rectangle rectangle = sender as Rectangle;
				rectangle.ReleaseMouseCapture();
                int width = (int)e.GetPosition(this).X + 5;
                SetRightPanelWidth(width, 0);
			}
		}

        private void rectWiden_MouseMove(object sender, MouseEventArgs e)
		{
			if (_isWiden)
			{
				if (e.LeftButton == MouseButtonState.Released)
				{
					_isWiden = false;
					var captured = Mouse.Captured;
					if (captured != null)
					{
						captured.ReleaseMouseCapture();
					}
				}
				else
				{
					var rectangle = sender as Rectangle;
					rectangle.CaptureMouse();
					int width = (int)e.GetPosition(this).X + 5;
                    SetRightPanelWidth(width, 5);
				}
			}
		}

        private void SetRightPanelWidth(int width, int minVal = 5)
		{
            if (width < 365)
			{
                width = 365;
			}
            if (Math.Abs(ActualWidth - (double)width) > (double)minVal)
			{
                this.xSetWidth(width);
                WndAssist.WaParams.SetRightPanelWidth(Wnd.Desk.Seller, width);         
			}
		}

        private void rectHighden_MouseMove(object sender, MouseEventArgs e)
		{
			if (_isHighden)
			{
				if (e.LeftButton == MouseButtonState.Released)
				{
					_isHighden = false;
					var captured = Mouse.Captured;
					if (captured != null)
					{
						captured.ReleaseMouseCapture();
					}
				}
				else
				{
					var rectangle = sender as Rectangle;
					rectangle.CaptureMouse();
					int height = (int)e.GetPosition(this).Y + 5;
					SetBottomPanelHeight(height, 5);
				}
			}
		}


        private void SetBottomPanelHeight(int height, int minVal = 5)
		{
            int miniVal = (int)Wnd.ToLogicalHeight() + 100 + 6;
            if (height < miniVal)
            {
                height = miniVal;
            }
            if (Math.Abs(ActualHeight - (double)height) > (double)minVal)
            {
                this.xSetHeight((double)height);
                Wnd.ctlBottomPanel.xSetHeight((double)height - Wnd.ToLogicalHeight() - 6.0);
                WndAssist.WaParams.SetBottomPanelHeight(Wnd.Desk.Seller, height - (int)Wnd.ToLogicalHeight() - 6);
            }
		}

        private void rectHighden_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			_isHighden = false;
			Rectangle rectangle = sender as Rectangle;
			rectangle.ReleaseMouseCapture();
			int height = (int)e.GetPosition(this).Y + 5;
			SetBottomPanelHeight(height, 0);
		}

        private void rectHighden_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_isHighden = true;
		}

        private void rectCorner_MouseMove(object sender, MouseEventArgs e)
		{
			if (_isSouthEast)
			{
				if (e.LeftButton == MouseButtonState.Released)
				{
					_isHighden = false;
					var captured = Mouse.Captured;
					if (captured != null)
					{
						captured.ReleaseMouseCapture();
					}
				}
				else
				{
					Rectangle rectangle = sender as Rectangle;
					rectangle.CaptureMouse();
					int width = (int)e.GetPosition(this).X + 5;
					int height = (int)e.GetPosition(this).Y + 5;
					SetRightPanelWidth(width, 5);
					SetBottomPanelHeight(height, 5);
				}
			}
		}

        private void rectCorner_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			_isSouthEast = false;
			var rectangle = sender as Rectangle;
			rectangle.ReleaseMouseCapture();
			int width = (int)e.GetPosition(this).X + 5;
			int height = (int)e.GetPosition(this).Y + 5;
			SetRightPanelWidth(width, 0);
			SetBottomPanelHeight(height, 0);
		}

        private void rectCorner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_isSouthEast = true;
		}

		public void WakeUp()
		{
			IWakable wakable = tabControl.SelectedContent as IWakable;
			if (wakable != null)
			{
				wakable.WakeUp();
			}
		}

		public void Sleep()
		{
            IWakable wakable = tabControl.SelectedContent as IWakable;
            if (wakable != null)
            {
                wakable.Sleep();
            }
		}

        private void ActivateTab(SelectionChangedEventArgs e)
		{
			if (e.AddedItems != null)
			{
                foreach (TabItem tabItem in e.AddedItems)
				{
					if (tabItem != null)
					{
						IWakable wakable = tabItem.Content as IWakable;
						if (wakable != null)
						{
							wakable.WakeUp();
						}
					}
				}
			}
			if (e.RemovedItems != null)
			{
                foreach (TabItem tabItem in e.RemovedItems)
				{
					if (tabItem != null)
					{
						IWakable wakable = tabItem.Content as IWakable;
						if (wakable != null)
						{
							wakable.Sleep();
						}
					}
				}
			}
		}

        private void btnHelp_Click(object sender, RoutedEventArgs e)
		{
			TabItem tabItem = tabControl.SelectedItem as TabItem;
			string text = "Help";
			if (tabItem != null)
			{
				switch ((TabTypeEnum)tabItem.Tag)
				{
				case TabTypeEnum.Logis:
					text = "Logis";
					break;
				case TabTypeEnum.ShortCut:
					text = "Shortcut";
					break;
				case TabTypeEnum.Robot:
					text = "Robot";
					break;
				case TabTypeEnum.Goods:
					text = "Knowledge";
					break;
				case TabTypeEnum.Order:
					text = "Trade";
					break;
				}
			}
		}

		private void btnSyn_Click(object sender, RoutedEventArgs e)
		{
            DbSyner.EvSynFinished += DbSyner_EvSynFinished;
            this.btnSyn.IsEnabled = false;
            DbSyner.SynData(false);
		}

        private void DbSyner_EvSynFinished(object sender, DbSyner.SynFinishedEventArgs e)
        {
            DbSyner.EvSynFinished -= this.DbSyner_EvSynFinished;
            DispatcherEx.xInvoke(()=>this.btnSyn.IsEnabled = true);
            if (e.IsOk)
            {
                MsgBox.ShowTip("数据同步完成", "提示");
            }
            else
            {
                MsgBox.ShowErrDialog("数据同步失败，原因是：" + e.Error + "\r\n\r\n可试着再同步一次");
            }
        }

        private void btnHideBottromPanel_Click(object sender, RoutedEventArgs e)
        {
            Wnd.IsShowRightPanel = false;
            Visibility = Visibility.Hidden;
            Wnd.ctlBottomPanel.btnShowPanelRight.Visibility = Visibility.Visible;
        }
	}
}
