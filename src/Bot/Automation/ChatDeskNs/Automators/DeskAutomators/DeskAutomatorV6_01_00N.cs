using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Automation.ChatDeskNs.Automators.DeskAutomators
{
    public class DeskAutomatorV6_01_00N : DeskAutomator
    {
        public DeskAutomatorV6_01_00N(HwndInfo hwndInfo, string seller)
            : base(hwndInfo, seller)
		{
		}

        protected override List<WinApi.WindowClue> GetCloseBuyerButtonHwndClueUnCache()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 2),
				new WinApi.WindowClue(WinApi.ClsNameEnum.SplitterBar, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardButton, "关闭", -1)
			};
        }
        protected override List<WinApi.WindowClue> GetSendMessageButtonHwndClueUnCache()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 2),
				new WinApi.WindowClue(WinApi.ClsNameEnum.SplitterBar, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardButton, "发送", -1)
			};
        }
        protected override List<WinApi.WindowClue> GetEditorHwndClueUnCache()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.SplitterBar, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.RichEditComponent, null, -1)
			};
        }
    }
}
