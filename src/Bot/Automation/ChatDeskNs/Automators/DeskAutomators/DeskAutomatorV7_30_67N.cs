using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Automation.ChatDeskNs.Automators.DeskAutomators
{
    public class DeskAutomatorV7_30_67N : DeskAutomatorV7_21_00N
    {
        public DeskAutomatorV7_30_67N(HwndInfo hwndInfo, string seller)
            : base(hwndInfo, seller)
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
				new WinApi.WindowClue(WinApi.ClsNameEnum.StackPanel, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.PrivateWebCtrl, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.Aef_WidgetWin_0, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.Aef_RenderWidgetHostHWND, null, -1)
			};
        }

    }
}
