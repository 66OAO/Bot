using BotLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BotLib.Extensions;
using BotLib.Wpf.Extensions;
using BotLib.Misc;
using System.ComponentModel;
using Bot.Options.HotKey;
using Bot.Automation;
using Bot.Options;

namespace Bot.AssistWindow.Widget
{
    /// <summary>
    /// BottomPanel.xaml 的交互逻辑
    /// </summary>
    public partial class BottomPanel : UserControl, IWakable
    {
        public BottomPanel.Tipper TheTipper;
        private WndAssist _wndDontUse;
        private string _buyerBeforeDetach;
        private string _preBuyer;
        private bool _isHighden;
        public class Tipper
        {
            private BottomPanel _bottomPanel;
            public Tipper(BottomPanel bottomPanel)
            {
                this._bottomPanel = bottomPanel;
            }

            public void ShowTip(string msg)
            {
                this.ShowTip(msg, -1);
            }

            public void ShowTip(string msg, int showSeconds = 5)
            {
                if (_bottomPanel.tblkTip.Visibility == Visibility.Collapsed)
                {
                    _bottomPanel.tblkTip.Visibility = Visibility.Visible;
                }
                _bottomPanel.tblkTip.Text = msg;
                DelayCaller.CallAfterDelayInUIThread(() =>
                {
                    _bottomPanel.tblkTip.Visibility = Visibility.Collapsed;
                }, showSeconds * 1000);
            }

        }

        private WndAssist Wnd
        {
            get
            {
                if (this._wndDontUse == null)
                {
                    this._wndDontUse = (this.xFindParentWindow() as WndAssist);
                    this.Init(this._wndDontUse);
                    Util.Assert(this._wndDontUse != null);
                }
                return this._wndDontUse;
            }
            set
            {
                this._wndDontUse = value;
            }
        }

        public BottomPanel()
        {
            this._buyerBeforeDetach = null;
            this._isHighden = false;
            this.InitializeComponent();
            this.gridMain.ColumnDefinitions[0].Width = WpfUtil.ConvertPixelWidthToGridLength(275);
            this.gridMain.ColumnDefinitions[2].Width = WpfUtil.ConvertPixelWidthToGridLength(150);
            this.TheTipper = new BottomPanel.Tipper(this);
            base.SizeChanged += this.BottomPanel_SizeChanged;
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                base.Loaded += this.BottomPanel_Loaded;
            }
        }

        private void BottomPanel_Loaded(object sender, RoutedEventArgs e)
        {
            base.Loaded -= this.BottomPanel_Loaded;
            if (Params.IsDevoloperClient)
            {
                this.btnOption.ContextMenu = (base.FindResource("menuSuper") as ContextMenu);
            }
        }

        private void BottomPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double actualHeight = this.row1.ActualHeight;
            if (actualHeight > 0.0)
            {
                this.column2.Width = new GridLength(this.row1.ActualHeight, GridUnitType.Pixel);
            }
        }

        public void Init(WndAssist wnd)
        {
            if (this._wndDontUse == null)
            {
                this.Wnd = wnd;
                this.ctlAnswer.Init(wnd);
                this.InitUI();
            }
        }

        private void InitUI()
        {

        }

        public void WakeUp()
        {
            //this.Wnd.Desk.EvChromeConnected -= Desk_EvChromeConnected;
            //this.Wnd.Desk.EvChromeDetached -= Desk_EvChromeDetached;
            //this.Wnd.Desk.ChatRecord.EvChatRecordChanged -= ChatRecord_EvChatRecordChanged;
            //this.Wnd.Desk.EvChromeConnected += Desk_EvChromeConnected;
            //this.Wnd.Desk.EvChromeDetached += Desk_EvChromeDetached;
            //this.Wnd.Desk.ChatRecord.EvChatRecordChanged += ChatRecord_EvChatRecordChanged;
            this.Wnd.Desk.EvBuyerChanged -= Desk_EvBuyerChanged;
            this.Wnd.Desk.EvBuyerChanged += Desk_EvBuyerChanged;

            if (this.Wnd.Desk.IsChatRecordChromeOk)
            {
                DispatcherEx.xInvoke(() =>
                {
                    this.ctlBuyer.ShowBuyer(this.Wnd.Desk.Buyer);
                });
            }

            //if (this._chatRecordBeforeDetach != this.Wnd.Desk.ChatRecord.CachedHtml)
            //{
            //    this.OnChatRecordChanged();
            //}

            if (this._buyerBeforeDetach != this.Wnd.Desk.Buyer)
            {
                DispatcherEx.xInvoke(() =>
                {

                    this.ctlBuyer.ShowBuyer(this.Wnd.Desk.Buyer);
                    this.ctlMemo.LoadBuyerNote(this.Wnd.Desk.BuyerMainNick, this.Wnd.Desk.Seller);
                    this._preBuyer = this.Wnd.Desk.Buyer;
                });
            }
        }

        public void Sleep()
        {

        }


        void Desk_EvBuyerChanged(object sender, Automation.ChatDeskNs.BuyerChangedEventArgs e)
        {
            DispatcherEx.xInvoke(() =>
            {
                this.ctlBuyer.ShowBuyer(this.Wnd.Desk.Buyer);
                this.ctlMemo.LoadBuyerNote(this.Wnd.Desk.BuyerMainNick, this.Wnd.Desk.Seller);
                this._preBuyer = this.Wnd.Desk.Buyer;
            });
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.Wnd.Desk.Editor.ClearPlainText();
        }

        public void MonitorHotKey(HotKeyHelper.HotOp op)
        {
            switch (op)
            {
                case HotKeyHelper.HotOp.QinKong:
                    this.btnClear.xPerformClick();
                    break;
                case HotKeyHelper.HotOp.ZhiSi:
                    this.btnKnowledge.xPerformClick();
                    break;
                case HotKeyHelper.HotOp.ArrowDown:
                    this.FocusAnswerItem(false);
                    break;
                case HotKeyHelper.HotOp.ArrowDown2:
                    this.FocusAnswerItem(true);
                    break;
            }
        }
        private bool FocusAnswerIfNeed(int hWnd, bool forceFocusAnswer)
        {
            bool rt;
            if (forceFocusAnswer)
            {
                rt = (Params.InputSuggestion.IsUseDownKey && this.ctlAnswer.HasItems && this.Wnd.Desk.Editor.GetChatEditorHwndInfo().Handle == hWnd);
            }
            else
            {
                rt = (Params.InputSuggestion.IsUseDownKey && this.ctlAnswer.HasItems && this.Wnd.Desk.Editor.GetChatEditorHwndInfo().Handle == hWnd && !this.Wnd.Desk.Editor.IsEmptyForPlainCachedText(false) && !this.Wnd.Desk.Editor.HasEmoji());
            }
            return rt;
        }

        private void FocusAnswerItem(bool focusAnswer)
        {
            int hWnd = WinApi.GetFocusHwnd();
            if (FocusAnswerIfNeed(hWnd, focusAnswer))
            {
                this.ctlAnswer.FocusItem();
                string text = this.Wnd.Desk.Editor.GetPlainCachedText(false).Trim();
                if (!text.xIsNullOrEmptyOrSpace())
                {
                    this.ctlAnswer.AppendInpuTextForEndItem(text);
                }
            }
            else
            {
                WinApi.FocusWnd(hWnd);
            }
        }

        private void btnShowPanelRight_Click(object sender, RoutedEventArgs e)
        {
            this.btnShowPanelRight.Visibility = Visibility.Collapsed;
            this.Wnd.ShowPanelRight();
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
            int miniVal = 100;
            if (height < miniVal)
            {
                height = miniVal;
            }
            if (Math.Abs(ActualHeight - (double)height) > (double)minVal)
            {
                this.xSetHeight((double)height);
                Wnd.ctlRightPanel.xSetHeight((double)height + Wnd.ToLogicalHeight() + 6.0);
                WndAssist.WaParams.SetBottomPanelHeight(Wnd.Desk.Seller, height);
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

        private void btnOption_Click(object sender, RoutedEventArgs e)
        {
            WndOption.MyShow(this.Wnd.Desk.Seller, this.Wnd, OptionEnum.Unknown, null);
        }

    }
}
