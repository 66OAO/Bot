using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Automation.ChatDeskNs.Automators.DeskAutomators
{
    public class DeskAutomatorV6_02_00N : DeskAutomatorV6_01_00N
    {
        public DeskAutomatorV6_02_00N(HwndInfo hwndInfo, string seller)
            : base(hwndInfo, seller)
		{
		}
        protected override List<WinApi.WindowClue> GetCloseBuyerButtonHwndClueUnCache()
        {
            var clst = this.GetDeskEditorHwndClue();
            clst.Add(new WinApi.WindowClue(WinApi.ClsNameEnum.StandardButton, "关闭", -1));
            return clst;
        }
        protected override List<WinApi.WindowClue> GetEditorHwndClueUnCache()
        {
            var clst = this.GetDeskEditorHwndClue();
            clst.Add(new WinApi.WindowClue(WinApi.ClsNameEnum.RichEditComponent, null, -1));
            return clst;
        }
        protected override List<WinApi.WindowClue> GetSendMessageButtonHwndClueUnCache()
        {
            var clst = this.GetDeskEditorHwndClue();
            clst.Add(new WinApi.WindowClue(WinApi.ClsNameEnum.StandardButton, "发送", -1));
            return clst;
        }
        protected virtual List<WinApi.WindowClue> GetDeskEditorHwndClue()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.SplitterBar, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 1)
			};
        }
    }
}
