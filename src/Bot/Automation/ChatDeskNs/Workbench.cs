using BotLib;
using BotLib.Wpf.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Automation.ChatDeskNs
{
    public class Workbench
    {
        private int _hwnd;
        private string _seller;
        private const int WaitChromeInitTimeMs = 10000;
        private int _chatDeskButtonHwnd;
        private List<WinApi.WindowClue> _chatDeskButtonClue;
        private int _addrTextBoxHwnd;
        private List<WinApi.WindowClue> _AddrTextBoxClue;

        public Workbench(int hwnd, string seller)
		{
			this._chatDeskButtonHwnd = 0;
			this._addrTextBoxHwnd = 0;
			this._hwnd = hwnd;
			this._seller = seller;
		}

        public void BringTop()
        {
            WinApi.TopMost(this._hwnd);
            DispatcherEx.DoEvents();
            WinApi.CancelTopMost(this._hwnd);
        }

        public bool IsAlreadyExist { get; set; }

        public bool IsAlive
        {
            get
            {
                return WinApi.IsHwndAlive(this._hwnd);
            }
        }
        public bool Nav(string text, ChatDesk chatDesk)
        {
            bool result = false;
            try
            {
                chatDesk.Automator.OpenWorkbench();
                Util.WaitFor(() => false, 300, 300, false);
                HwndInfo hwndInfo = new HwndInfo(this.AddrTextBoxHwnd, "AddrTextBoxHwnd");
                WinApi.ClickPointBySendMessage(hwndInfo.Handle, 30, 5);
                Util.WaitFor(() => false, 100, 100, false);
                WinApi.Editor.SetText(hwndInfo, text, true);
                for (int i = 0; i < 2; i++)
                {
                    Util.WaitFor(() => false, 100, 100, false);
                    WinApi.ClickHwndBySendMessage(hwndInfo.Handle, 1);
                    WinApi.PressEnterKey();
                }
                result = true;
            }
            catch (Exception e)
            {
                Log.Exception(e);
                result = false;
            }
            return result;
        }

        public void HideWorkbench()
        {
            WinApi.HideDeskWindow(this._hwnd);
        }

        public void OpenChatDesk()
        {
            WinApi.ClickHwndBySendMessage(this.ChatDeskButtonHwnd, 2);
        }

        private int ChatDeskButtonHwnd
        {
            get
            {
                if (this._chatDeskButtonHwnd == 0)
                {
                    this._chatDeskButtonHwnd = WinApi.FindDescendantHwnd(this._hwnd, this.ChatDeskButtonClue, "ChatDeskButtonHwnd");
                }
                return this._chatDeskButtonHwnd;
            }
        }

        private List<WinApi.WindowClue> ChatDeskButtonClue
        {
            get
            {
                if (this._chatDeskButtonClue == null)
                {
                    this._chatDeskButtonClue = this.GetChatDeskButtonClue();
                }
                return this._chatDeskButtonClue;
            }
        }

        private List<WinApi.WindowClue> GetChatDeskButtonClue()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StackPanel, null, 1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StackPanel, null, 2),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardButton, null, 1)
			};
        }

        private int AddrTextBoxHwnd
        {
            get
            {
                if (this._addrTextBoxHwnd == 0)
                {
                    this._addrTextBoxHwnd = WinApi.FindDescendantHwnd(this._hwnd, this.AddrTextBoxClue, "AddrTextBoxHwnd");
                }
                return this._addrTextBoxHwnd;
            }
        }

        private List<WinApi.WindowClue> AddrTextBoxClue
        {
            get
            {
                if (this._AddrTextBoxClue == null)
                {
                    this._AddrTextBoxClue = this.GetAddrTextBoxClue();
                }
                return this._AddrTextBoxClue;
            }
        }

        private List<WinApi.WindowClue> GetAddrTextBoxClue()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StackPanel, null, 1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StackPanel, null, 1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StackPanel, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.EditComponent, null, -1)
			};
        }

        public void CloseWorkbench()
        {
            WinApi.CloseWindow(this._hwnd, 2000);
        }

    }
}
