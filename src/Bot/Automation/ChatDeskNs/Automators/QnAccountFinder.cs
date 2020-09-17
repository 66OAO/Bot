using BotLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BotLib.Extensions;

namespace Bot.Automation.ChatDeskNs.Automators
{
    public class QnAccountFinder
    {
        public virtual string ChatWindowTitlePattern
        {
            get
            {
                return ".*(?= - 接待中心)";
            }
        }

        private static Dictionary<int, LoginedSeller> _pidSellerDict;

        static QnAccountFinder()
        {
            _pidSellerDict = new Dictionary<int, LoginedSeller>();
        }
        public virtual Dictionary<string, int> GetAllChatDeskSellerNameAndHwndInner(bool noFind)
        {
            var rtdict = new Dictionary<string, int>();
            WinApi.FindAllDesktopWindowByClassNameAndTitlePattern("StandardFrame", this.ChatWindowTitlePattern, (int hwnd, string title) =>
            {
                if (!noFind || WinApi.IsVisible(hwnd))
                {
                    string winTitle = Regex.Match(title, this.ChatWindowTitlePattern).ToString();
                    if (string.IsNullOrEmpty(winTitle))
                    {
                        Log.Error("GetSellersNameFromChatDeskTitle：nick为空,title=" + title);
                    }
                    else
                    {
                        if (rtdict.ContainsKey(winTitle) && rtdict[winTitle] != hwnd)
                        {
                            Log.Error(string.Format("GetAllChatDeskSellerNameAndHwndInner,重复发现窗口,name={0},hwnd1={1},hwnd2={2}", winTitle, rtdict[winTitle], hwnd));
                        }
                        rtdict[winTitle] = hwnd;
                    }
                }
            });
            return rtdict;
        }

        private static HashSet<int> GetAliWorkbenchPids()
        {
            var pids = new HashSet<int>();
            var aliWorkbenchPs = Process.GetProcessesByName("AliWorkbench");
            foreach (var p in aliWorkbenchPs.xSafeForEach())
            {
                pids.Add(p.Id);
            }
            return pids;
        }

        public virtual Dictionary<string, LoginedSeller> GetLoginedSellers()
        {
            var rtdict = new Dictionary<string, LoginedSeller>();
            var cacheDict = new Dictionary<int, LoginedSeller>();
            var pids = GetAliWorkbenchPids();
            foreach (var pid in pids.xSafeForEach())
            {
                if (_pidSellerDict.ContainsKey(pid))
                {
                    var loginedSeller = _pidSellerDict[pid];
                    if (WinApi.IsHwndAlive(loginedSeller.SellerHwnd))
                    {
                        cacheDict[pid] = loginedSeller;
                        rtdict[loginedSeller.Name] = loginedSeller;
                    }
                }
                else
                {
                    int hWnd;
                    string sellerName = this.GetSellerName(pid, out hWnd);
                    if (!string.IsNullOrEmpty(sellerName) && hWnd != 0)
                    {
                        var value = new LoginedSeller(sellerName, hWnd, 0);
                        rtdict[sellerName] = value;
                        cacheDict[pid] = value;
                    }
                }
            }
            _pidSellerDict = cacheDict;
            return rtdict;
        }

        private string GetSellerName(int pid, out int hwnd)
		{
			var seller = string.Empty;
			var htmp = 0;
			try
			{
			    WinApi.FindAllDesktopWindowByClassNameAndTitlePattern("StandardFrame", this.ChatWindowTitlePattern, (qnHwnd, title) =>
                {
                    if (WinApi.IsVisible(qnHwnd))
                    {
                        string winTitle = Regex.Match(title, this.ChatWindowTitlePattern).ToString();
                        seller = winTitle;
                        htmp = qnHwnd;
                    }
                }, pid);
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
			hwnd = htmp;
			return seller;
		}
    }
}
