using BotLib.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Automation.ChatDeskNs.Automators
{
    public class DeskAutomator
    {
        private HwndInfo _deskHwnd;
        private string _seller;
        private int _firstControlHwnd;
        private int _singleChatEditorHwnd;
        private int _groupChatEditorHwnd;
        private DateTime dateTime_0;
        private int _groupChatCloseButtonHwnd;
        private int _groupChatSendButtonHwnd;
        private int _toolbarPlusHwnd;
        private int _recentContactButtonHwnd;
        private int _buyerPicHwnd;
        private List<WinApi.WindowClue> _buyerPicClue;
        private int _ChatRecordChromeHwnd;
        private DateTime _chatRecordChromeHwndCacheTime;
        private List<WinApi.WindowClue> _ChatRecordChromeHwndClue;

        private List<WinApi.WindowClue> _groupChatEditorHwndClue;
        private List<WinApi.WindowClue> _groupChatCloseButtonHwndClue;
        private List<WinApi.WindowClue> _groupChatSendButtonHwndClue;
        private List<WinApi.WindowClue> _editorHwndClue;
        private List<WinApi.WindowClue> _sendMessageButtonHwndClue;
        private List<WinApi.WindowClue> _toolbarPlusClueDontUse;
        private List<WinApi.WindowClue> _recentContactButtonClueDontUse;
        private List<WinApi.WindowClue> _closeBuyerButtonHwndClue;
        private List<WinApi.WindowClue> _openWorkbenchButtonHwndClue;
        protected int _sendMessageButtonHwnd;
        protected int _closeBuyerButtonHwnd;
        protected int _openWorkbenchButtonHwnd;
        public int SingleChatEditorHwnd
        {
            get
            {
                if (this._singleChatEditorHwnd == 0)
                {
                    this._singleChatEditorHwnd = WinApi.FindDescendantHwnd(this._deskHwnd.Handle, this.GetEditorHwndClue(), "SingleChatEditorHwnd");
                }
                return this._singleChatEditorHwnd;
            }
        }
        public int GroupChatEditorHwnd
        {
            get
            {
                if (this._groupChatEditorHwnd == 0)
                {
                    this._groupChatEditorHwnd = WinApi.FindDescendantHwnd(this._deskHwnd.Handle, this.GetGroupChatEditorHwndClue(), "GroupChatEditorHwnd");
                }
                return this._groupChatEditorHwnd;
            }
        }
        public int GroupChatCloseButtonHwnd
        {
            get
            {
                if (this._groupChatCloseButtonHwnd == 0 && (DateTime.Now - this.dateTime_0).TotalSeconds > 5.0)
                {
                    this.dateTime_0 = DateTime.Now;
                    this._groupChatCloseButtonHwnd = WinApi.FindDescendantHwnd(this._deskHwnd.Handle, this.GetGroupChatCloseButtonHwndClue(), "GroupChatCloseButtonHwnd");
                }
                return this._groupChatCloseButtonHwnd;
            }
        }
        public int GroupChatSendButtonHwnd
        {
            get
            {
                if (this._groupChatSendButtonHwnd == 0)
                {
                    this._groupChatSendButtonHwnd = WinApi.FindDescendantHwnd(this._deskHwnd.Handle, this.GetGroupChatSendButtonHwndClue(), "GroupChatSendButtonHwnd");
                }
                return this._groupChatSendButtonHwnd;
            }
        }
        public int ToolbarPlusHwnd
        {
            get
            {
                if (this._toolbarPlusHwnd == 0)
                {
                    this._toolbarPlusHwnd = WinApi.FindDescendantHwnd(this._deskHwnd.Handle, this.GetToolbarPlusClueDontUse(), "ToolbarPlusHwnd");
                }
                return this._toolbarPlusHwnd;
            }
        }
        public int RecentContactButtonHwnd
        {
            get
            {
                if (this._recentContactButtonHwnd == 0)
                {
                    this._recentContactButtonHwnd = WinApi.FindDescendantHwnd(this._deskHwnd.Handle, this.GetRecentContactButtonClue(), "RecentContactButtonHwnd");
                }
                return this._recentContactButtonHwnd;
            }
        }
        public int BuyerPicHwnd
        {
            get
            {
                if (this._buyerPicHwnd == 0)
                {
                    this._buyerPicHwnd = WinApi.FindDescendantHwnd(this._deskHwnd.Handle, this.GetBuyerPicClue(), "BuyerPicHwnd");
                }
                return this._buyerPicHwnd;
            }
        }
        protected int SingleChatSendMessageButtonHwnd
        {
            get
            {
                if (this._sendMessageButtonHwnd == 0)
                {
                    this._sendMessageButtonHwnd = WinApi.FindDescendantHwnd(this._deskHwnd.Handle, this.GetSendMessageButtonHwndClue(), "SingleChatSendMessageButtonHwnd");
                }
                return this._sendMessageButtonHwnd;
            }
        }
        protected int SingleChatCloseButtonHwnd
        {
            get
            {
                if (this._closeBuyerButtonHwnd == 0)
                {
                    this._closeBuyerButtonHwnd = WinApi.FindDescendantHwnd(this._deskHwnd.Handle, this.GetCloseBuyerButtonHwndClue(), "SingleChatCloseButtonHwnd");
                }
                return this._closeBuyerButtonHwnd;
            }
        }
        protected int OpenWorkbenchButtonHwnd
        {
            get
            {
                if (this._openWorkbenchButtonHwnd == 0)
                {
                    this._openWorkbenchButtonHwnd = WinApi.FindDescendantHwnd(this._deskHwnd.Handle, this.GetOpenWorkbenchButtonHwnd(), "OpenWorkbenchButtonHwnd");
                }
                return this._openWorkbenchButtonHwnd;
            }
        }
        protected virtual string WidgetWindowTitlePattern
        {
            get
            {
                return ".*(?= - 工作台)";
            }
        }
        protected virtual string QnWindowProcessName
        {
            get
            {
                return "AliWorkbench";
            }
        }
        public DeskAutomator(HwndInfo deskhwnd, string seller)
        {
            this._firstControlHwnd = 0;
            this._singleChatEditorHwnd = 0;
            this._groupChatEditorHwnd = 0;
            this.dateTime_0 = DateTime.MinValue;
            this._groupChatCloseButtonHwnd = 0;
            this._groupChatSendButtonHwnd = 0;
            this._toolbarPlusHwnd = 0;
            this._recentContactButtonHwnd = 0;
            this._buyerPicHwnd = 0;
            this._sendMessageButtonHwnd = 0;
            this._closeBuyerButtonHwnd = 0;
            this._openWorkbenchButtonHwnd = 0;
            this._deskHwnd = deskhwnd;
            this._seller = seller;
        }

        public virtual bool GetIsDeskVisible()
        {
            return WinApi.IsVisible(this._deskHwnd.Handle);
        }

        public bool IsSingleChatCloseButtonEnable()
        {
            return this.SingleChatCloseButtonHwnd != 0 && WinApi.IsWindowEnabled(this.SingleChatCloseButtonHwnd);
        }

        public bool? IsSingleChatEditorVisible
        {
            get
            {
                return WinApi.IsVisible(this.SingleChatEditorHwnd);
            }
        }
        public bool IsSingleChatCloseButtonVisible()
        {
            return this.SingleChatCloseButtonHwnd != 0 && WinApi.IsVisible(this.SingleChatCloseButtonHwnd);
        }
        public bool IsGroupChatCloseButtonEnable()
        {
            return this.GroupChatCloseButtonHwnd != 0 && WinApi.IsWindowEnabled(this.GroupChatCloseButtonHwnd);
        }
        public bool IsAlive(bool useCache = true)
        {
            bool rt;
            if (!WinApi.IsHwndAlive(this._deskHwnd.Handle))
            {
                rt = false;
            }
            else
            {
                bool hasTitle;
                string seller = this.GetSellerOfDesk(this._deskHwnd, out hasTitle);
                rt = (!hasTitle || seller == this._seller);
            }
            return rt;
        }
        public bool IsControlVisible()
        {
            return WinApi.IsVisible(this.GetDeskFirstControlHwnd());
        }
        private int GetDeskFirstControlHwnd()
        {
            if (this._firstControlHwnd == 0)
            {
                WinApi.WindowClue clue = new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1);
                this._firstControlHwnd = WinApi.FindChildHwnd(this._deskHwnd.Handle, clue);
            }
            return this._firstControlHwnd;
        }
        public void ClickSingleChatSendMessageButton()
        {
            WinApi.ClickHwndBySendMessage(this.SingleChatSendMessageButtonHwnd, 1);
        }
        public void ClickGroupChatSendMessageButton()
        {
            WinApi.ClickHwndBySendMessage(this.GroupChatSendButtonHwnd, 1);
        }
        public void ClickSingleChatCloseBuyerButton()
        {
            WinApi.ClickHwndBySendMessage(this.SingleChatCloseButtonHwnd, 1);
        }
        public void ClickGroupChatCloseBuyerButton()
        {
            WinApi.ClickHwndBySendMessage(this.GroupChatCloseButtonHwnd, 1);
        }
        public void OpenWorkbench()
        {
            WinApi.ClickHwndBySendMessage(this.OpenWorkbenchButtonHwnd, 1);
        }
        public Rectangle GetBuyerNameRegion()
        {
            int buyerPicHwnd = this.BuyerPicHwnd;
            Rectangle windowRectangle = WinApi.GetWindowRectangle(buyerPicHwnd);
            return new Rectangle(windowRectangle.Right + 8, windowRectangle.Top + 10, 200, 3);
        }
        private string IsChatWnd(HwndInfo hwndInfo)
        {
            string isChatWindow = "";
            string txt;
            if (WinApi.GetText(hwndInfo, out txt))
            {
                isChatWindow = RegexEx.Match(txt, QnAccountFinderFactory.Finder.ChatWindowTitlePattern);
            }
            return isChatWindow;
        }
        public void ClickRecentContactButton()
        {
            WinApi.ClickHwndBySendMessage(this.RecentContactButtonHwnd, 1);
        }
        private string GetSellerOfDesk(HwndInfo deskHwnd, out bool hasTitle)
        {
            string matchChatWindowTitle = "";
            string title;
            hasTitle = WinApi.GetText(deskHwnd, out title);
            if (hasTitle)
            {
                matchChatWindowTitle = RegexEx.Match(title, QnAccountFinderFactory.Finder.ChatWindowTitlePattern);
            }
            return matchChatWindowTitle;
        }
        private List<WinApi.WindowClue> GetBuyerPicClue()
        {
            if (this._buyerPicClue == null)
            {
                this._buyerPicClue = this.GetBuyerPicClueUnCache();
            }
            return this._buyerPicClue;
        }
        protected virtual List<WinApi.WindowClue> GetBuyerPicClueUnCache()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardButton, null, -1)
			};
        }
        private List<WinApi.WindowClue> GetGroupChatEditorHwndClue()
        {
            if (this._groupChatEditorHwndClue == null)
            {
                this._groupChatEditorHwndClue = this.GetGroupChatEditorHwndClueUnCache();
            }
            return this._groupChatEditorHwndClue;
        }
        protected virtual List<WinApi.WindowClue> GetGroupChatEditorHwndClueUnCache()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 5),
				new WinApi.WindowClue(WinApi.ClsNameEnum.SplitterBar, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.RichEditComponent, null, -1)
			};
        }
        private List<WinApi.WindowClue> GetGroupChatCloseButtonHwndClue()
        {
            if (this._groupChatCloseButtonHwndClue == null)
            {
                this._groupChatCloseButtonHwndClue = this.GetGroupChatCloseButtonHwndClueUnCache();
            }
            return this._groupChatCloseButtonHwndClue;
        }
        protected virtual List<WinApi.WindowClue> GetGroupChatCloseButtonHwndClueUnCache()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 5),
				new WinApi.WindowClue(WinApi.ClsNameEnum.SplitterBar, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardButton, "关闭", -1)
			};
        }
        private List<WinApi.WindowClue> GetGroupChatSendButtonHwndClue()
        {
            if (this._groupChatSendButtonHwndClue == null)
            {
                this._groupChatSendButtonHwndClue = this.GetGroupChatSendButtonHwndClueUnCache();
            }
            return this._groupChatSendButtonHwndClue;
        }
        protected virtual List<WinApi.WindowClue> GetGroupChatSendButtonHwndClueUnCache()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 5),
				new WinApi.WindowClue(WinApi.ClsNameEnum.SplitterBar, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardButton, "发送", -1)
			};
        }
        private List<WinApi.WindowClue> GetEditorHwndClue()
        {
            if (this._editorHwndClue == null)
            {
                this._editorHwndClue = this.GetEditorHwndClueUnCache();
            }
            return this._editorHwndClue;
        }
        protected virtual List<WinApi.WindowClue> GetEditorHwndClueUnCache()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.SplitterBar, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.RichEditComponent, null, -1)
			};
        }
        private List<WinApi.WindowClue> GetSendMessageButtonHwndClue()
        {
            if (this._sendMessageButtonHwndClue == null)
            {
                this._sendMessageButtonHwndClue = this.GetSendMessageButtonHwndClueUnCache();
            }
            return this._sendMessageButtonHwndClue;
        }
        protected virtual List<WinApi.WindowClue> GetSendMessageButtonHwndClueUnCache()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 2),
				new WinApi.WindowClue(WinApi.ClsNameEnum.SplitterBar, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardButton, "发送", -1)
			};
        }
        private List<WinApi.WindowClue> GetToolbarPlusClueDontUse()
        {
            if (this._toolbarPlusClueDontUse == null)
            {
                this._toolbarPlusClueDontUse = this.GetToolbarPlusClueUnCache();
            }
            return this._toolbarPlusClueDontUse;
        }
        protected virtual List<WinApi.WindowClue> GetToolbarPlusClueUnCache()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.ToolBarPlus, null, -1)
			};
        }
        private List<WinApi.WindowClue> GetRecentContactButtonClue()
        {
            if (this._recentContactButtonClueDontUse == null)
            {
                this._recentContactButtonClueDontUse = this.GetRecentContactButtonClueUnCache();
            }
            return this._recentContactButtonClueDontUse;
        }
        protected virtual List<WinApi.WindowClue> GetRecentContactButtonClueUnCache()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, "ID_ESERVICE_CONTROLPAGE", -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardButton, null, 1)
			};
        }
        private List<WinApi.WindowClue> GetCloseBuyerButtonHwndClue()
        {
            if (this._closeBuyerButtonHwndClue == null)
            {
                this._closeBuyerButtonHwndClue = this.GetCloseBuyerButtonHwndClueUnCache();
            }
            return this._closeBuyerButtonHwndClue;
        }
        private List<WinApi.WindowClue> GetOpenWorkbenchButtonHwnd()
        {
            if (this._openWorkbenchButtonHwndClue == null)
            {
                this._openWorkbenchButtonHwndClue = this.GetOpenWorkbenchButtonHwndClueUnCache();
            }
            return this._openWorkbenchButtonHwndClue;
        }
        protected virtual List<WinApi.WindowClue> GetCloseBuyerButtonHwndClueUnCache()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 2),
				new WinApi.WindowClue(WinApi.ClsNameEnum.SplitterBar, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardButton, "关闭", -1)
			};
        }
        protected virtual List<WinApi.WindowClue> GetOpenWorkbenchButtonHwndClueUnCache()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, "ID_ESERVICE_CONTROLPAGE", -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardButton, null, 1)
			};
        }
        protected virtual List<string> GetUsersFromProcessMainWindowTitle()
        {
            List<string> titles = new List<string>();
            Process[] processesByName = Process.GetProcessesByName(this.QnWindowProcessName);
            if (processesByName != null)
            {
                titles = processesByName.Select(new Func<Process, string>(this.GetChatTitleFromProcessMainWindowTitle)).ToList<string>();
            }
            return titles;
        }
        private string GetChatTitle(string title)
        {
            string text = this.MatchChatTitle(title);
            if (string.IsNullOrEmpty(text))
            {
                text = this.MatchWidgetWindowTitle(title);
            }
            return text;
        }
        private string MatchChatTitle(string title)
        {
            return RegexEx.Match(title.Trim(), QnAccountFinderFactory.Finder.ChatWindowTitlePattern).Trim();
        }
        private string MatchWidgetWindowTitle(string title)
        {
            return RegexEx.Match(title.Trim(), this.WidgetWindowTitlePattern).Trim();
        }
        private string GetChatTitleFromProcessMainWindowTitle(Process ps)
        {
            return this.GetChatTitle(ps.MainWindowTitle);
        }

        public int ChatRecordChromeHwnd
        {
            get
            {
                if (this._ChatRecordChromeHwnd == 0 || this._chatRecordChromeHwndCacheTime.xElapse().TotalSeconds > 5.0)
                {
                    this._ChatRecordChromeHwnd = WinApi.FindDescendantHwnd(this._deskHwnd.Handle, this.ChatRecordChromeHwndClue, "ChatRecordChromeHwnd");
                    this._chatRecordChromeHwndCacheTime = DateTime.Now;
                }
                return this._ChatRecordChromeHwnd;
            }
        }

        private List<WinApi.WindowClue> ChatRecordChromeHwndClue
        {
            get
            {
                if (this._ChatRecordChromeHwndClue == null)
                {
                    this._ChatRecordChromeHwndClue = this.ChatRecordChromeHwndClueUnCache();
                }
                return this._ChatRecordChromeHwndClue;
            }
        }

        protected virtual List<WinApi.WindowClue> ChatRecordChromeHwndClueUnCache()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 2),
				new WinApi.WindowClue(WinApi.ClsNameEnum.SplitterBar, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.PrivateWebCtrl, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.Aef_WidgetWin_0, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.Aef_RenderWidgetHostHWND, null, -1)
			};
        }


        private bool? _isGroupEditorVisible;
        public bool? IsGroupEditorVisible
        {
            get
            {
                if (this._isGroupEditorVisible == null)
                {
                    this._isGroupEditorVisible = WinApi.IsVisible(this.GroupChatEditorHwnd);
                }
                return this._isGroupEditorVisible;
            }
        }
    }
}
