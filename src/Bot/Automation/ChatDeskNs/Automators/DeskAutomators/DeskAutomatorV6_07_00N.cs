using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Automation.ChatDeskNs.Automators.DeskAutomators
{
    public class DeskAutomatorV6_07_00N : DeskAutomatorV6_01_00N
    {
        public DeskAutomatorV6_07_00N(HwndInfo hwndInfo, string seller)
            : base(hwndInfo, seller)
		{
		}
        protected override List<WinApi.WindowClue> GetBuyerPicClueUnCache()
        {
            return new List<WinApi.WindowClue>
			{
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 2),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 1),
				new WinApi.WindowClue(WinApi.ClsNameEnum.StandardButton, null, -1)
			};
        }
    }
}
