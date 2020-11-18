using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Automation.ChatDeskNs.Automators.DeskAutomators
{
    public class DeskAutomatorV7_20_00N : DeskAutomatorV6_07_00N
    {
        public DeskAutomatorV7_20_00N(HwndInfo hwndInfo, string seller)
            :            base(hwndInfo, seller)
		{
		}

        protected override List<WinApi.WindowClue> ChatRecordChromeHwndClueUnCache()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 3),
				new WinApi.WindowClue(WinApi.ClsNameEnum.SplitterBar, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.PrivateWebCtrl, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.CefBrowserWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.Chrome_WidgetWin_0, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.Chrome_RenderWidgetHostHWND, null, -1)
			};
        }
    }
}
