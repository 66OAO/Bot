using Bot.Automation.ChatDeskNs;
using BotLib;
using BotLib.Extensions;
using BotLib.Wpf.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Automation.ChromeDebugger
{
    public class Injector
    {
        private static bool HideChromeDebugWindow;
        public static int ContinuousFailCount;
        static Injector()
        {
            HideChromeDebugWindow = true;
            ContinuousFailCount = 0;
        }

        public static bool Inject(ChatDesk desk, string url, Func<bool> isConnectOk)
        {
            bool rt = false;
            try
            {
                Log.Info("ocr s2," + desk.Seller);
                var injectJs = GetInjectJs(url, desk.Seller);
                int chromeDevHwnd = GetExistChromeDevTools(desk);
                Util.Assert(chromeDevHwnd != 0);
                DoInject(injectJs, chromeDevHwnd, desk, isConnectOk);
                if (HideChromeDebugWindow)
                {
                    WinApi.CloseWindow(chromeDevHwnd, 2000);
                }
                Thread.Sleep(100);
                if (isConnectOk())
                {
                    ContinuousFailCount = 0;
                    rt = true;
                }
                else
                {
                    ContinuousFailCount++;
                }
            }
            catch (Exception ex)
            {
                Log.Error("ocr," + ex.Message);
                ContinuousFailCount++;
            }
            return rt;
        }

        private static int GetExistChromeDevTools(ChatDesk desk)
        {
            int chromeDevHwnd = GetExistChromeDevTools(desk.AliappProcessId, 5);
            if (chromeDevHwnd == 0)
            {
                chromeDevHwnd = OpenNewChromeDevTools(desk);
            }
            return chromeDevHwnd;
        }

        private static string GetInjectJs(string svrUrl, string seller)
        {
            return string.Concat(new string[]
			{
				"window.qnIntervalErrCount = 0;\r\nwindow.qnajax = function (name, value,onReady) {\r\nif(window.qnIntervalErrCount>5) return;\r\n    var url = '",
				svrUrl,
				"';\r\n    var xmlhttp = new XMLHttpRequest();\r\n    xmlhttp.open('POST', url, true);\r\n    xmlhttp.setRequestHeader('Content-type', 'application/x-www-form-urlencoded');\r\n    var content = 's=encodeURIComponent(",
				seller,
				")&n=' + name;\r\n    if (typeof (value) != undefined) {\r\n        content += '&v=' + value;\r\n    }\r\n    xmlhttp.send(content);\r\n\r\n    if (typeof (onReady) != 'undefined') {\r\n        xmlhttp.onreadystatechange = function () {\r\n            if (xmlhttp.readyState == 4) {\r\n                onReady(xmlhttp);\r\n            }\r\n        }\r\n    }\r\n}\r\n\r\nwindow.qnajax('connected');\r\n\r\nif (typeof (window.qnInterval) != 'undefined') {\r\n    clearInterval(window.qnInterval);\r\n}\r\n\r\nwindow.qnInterval = setInterval(function () {\r\n    window.qnajax('tick', '',function (xmlhttp) {\r\n        if (xmlhttp.status != 200) {\r\n            window.qnIntervalErrCount++;\r\n        } else {\r\n            window.qnIntervalErrCount = 0;\r\n        }\r\n        if (window.qnIntervalErrCount > 5) {\r\n            clearInterval(window.qnInterval);\r\n            window.qnInterval = undefined;\r\n            console.clear();\r\n            ////window.onEventNotify=window.___qnww2;\r\n            ////window.___qnww2=undefined;\r\n        }\r\n    })\r\n}, 800);\r\n\r\nwindow.qnPostHtml = function (delay) {\r\n    if (typeof(delay) !='undefined') {\r\n        setTimeout(function() {\r\n            window.qnajax('html', encodeURIComponent(document.body.outerHTML));\r\n        }, delay);\r\n    } else {\r\n        window.qnajax('html', encodeURIComponent(document.body.outerHTML));\r\n    }\r\n}\r\n\r\nif (typeof (window.___qnww2) == 'undefined') {\r\n    window.___qnww2=typeof(window.___qnww)=='undefined'?window. onEventNotify:window.___qnww;\r\n}\r\n\r\nwindow.onEventNotify = function (sid, name, a, data) {\r\n    window.___qnww2(sid, name, a, data);\r\nif(typeof(window.qnInterval)!='undefined'){\r\n    name = JSON.parse(name);\r\n    if (sid.indexOf('onConversationChange') >= 0) {\r\n        window.qnCurrentBuyer = name.cid.nick;\r\n        window.qnajax('onConversationChange', window.qnCurrentBuyer);\r\n        window.qnPostHtml(500);\r\n        window.qnPostHtml(1500);\r\n    } else if (sid.indexOf('onSendNewMsg') >= 0) {\r\n        var nick = name[0].nick;\r\n        window.qnajax('onSendNewMsg', nick);\r\n        if (window.qnCurrentBuyer == nick) {\r\n            window.qnPostHtml(100);\r\n        }\r\n    } else if (sid.indexOf('onReceiveNewMsg') >= 0) {\r\n        var nick = name[0].nick;\r\n        window.qnajax('onReceiveNewMsg',nick);\r\n        if (window.qnCurrentBuyer == nick) {\r\n            window.qnPostHtml(100);\r\n        }\r\n    } else if (sid.indexOf('onConversationAdd') >= 0) {\r\n        window.qnajax('onConversationAdd', name.cid.nick);\r\n    } else if (sid.indexOf('onConversationClose') >= 0) {\r\n        window.qnajax('onConversationClose', name.cid.nick);\r\n    } else if (sid.indexOf('onNetDisConnect') >= 0) {\r\n        window.qnajax('onNetDisConnect', name.nick);\r\n    } else if (sid.indexOf('onNetReConnectOK') >= 0) {\r\n        window.qnajax('onNetReConnectOK', name.nick);\r\n    }\r\n}\r\n}\r\n\r\nwindow.onunload = function () {\r\n    window.qnajax('onUnload');\r\n}\r\n\r\nwindow.qnPostHtml();\r\nconsole.clear();\r\n"
			});
        }

        private static int OpenNewChromeDevTools(ChatDesk desk)
        {
            var debugHwnd = 0;
            AssertSingleChatChromeVisible(desk);
            if (!WinApi.BringTopAndDoAction(desk.Hwnd.Handle, () =>
            {
                var chatRecordChromeHwnd = desk.Automator.ChatRecordChromeHwnd;
                WinApi.ClickPointBySendMessage(chatRecordChromeHwnd, 5, 5);
                WinApi.PressF12();
                Thread.Sleep(100);
                OpenChromeDevTools(desk.AliappProcessId);
                debugHwnd = GetExistChromeDevTools(desk.AliappProcessId, 5);
                AssertSingleChatChrome(debugHwnd);
                HideChromeDebugWindow = debugHwnd > 0;
                HideChromeDevDebugWindow(debugHwnd);
            }, 5))
            {
                throw new Exception("Can't bring chat desk to top");
            }
            return debugHwnd;
        }

        private static void AssertSingleChatChromeVisible(ChatDesk desk)
        {
            int chatRecordChromeHwnd = desk.Automator.ChatRecordChromeHwnd;
            Util.Assert(WinApi.IsVisible(chatRecordChromeHwnd));
        }

        private static void HideChromeDevDebugWindow(int hwnd)
        {
            WinApi.SetTransparentWindow(hwnd);
            WinApi.BringTopAndSetWindowSize(hwnd, 0, 3000, 1200, 600);
        }

        internal static void ActiveChromeDev(ChatDesk desk)
        {
            try
            {
                bool isForeground = desk.IsForeground;
                int chromeDevHwnd = GetExistChromeDevTools(desk);
                Util.Assert(chromeDevHwnd != 0);
                isForeground = (isForeground || desk.IsForeground);
                for (int i = 0; i < 5; i++)
                {
                    if (i > 0)
                    {
                        Thread.Sleep(30);
                    }
                    WinApi.BringTopAndDoAction(chromeDevHwnd, () =>
                    {
                        WinApi.PressEsc();
                    }, 2);
                }
                if (isForeground)
                {
                    desk.BringTop();
                }
            }
            catch (Exception ex)
            {
                Log.Error("ocr2," + ex.Message);
            }
        }

        private static void AssertSingleChatChrome(int dbgHwnd)
        {
            if (dbgHwnd == 0)
            {
                throw new Exception("找不到窗口");
            }
            string text = WinApi.GetText(dbgHwnd);
        }

        public static void OpenChromeDevTools(int processId, bool hasOkButton = true)
        {
            int i = 0;
            while (i < 10)
            {
                int devWarnDialogHwnd = GetOpenDevToolsWarning(processId);
                if (devWarnDialogHwnd == 0)
                {
                    Thread.Sleep(50);
                    i++;
                    continue;
                }
                int okButonHwnd = hasOkButton ? FindOkButtonHwnd(devWarnDialogHwnd) : FindNoButtonHwnd(devWarnDialogHwnd);
                if (okButonHwnd == 0)
                {
                    throw new Exception("Can't Find Ok Button in warning dialog.");
                }
                for (int j = 0; j < 10; j++)
                {
                    WinApi.ClickPointBySendMessage(okButonHwnd, 10, 10);
                    Thread.Sleep(50);
                    if (!WinApi.IsHwndAlive(okButonHwnd))
                    {
                        goto DONE;
                    }
                }
            }
        DONE: ;
        }

        private static int FindNoButtonHwnd(int hwnd)
        {
            return WinApi.FindChildHwnd(hwnd, new WinApi.WindowClue("Button", "否(&N)", -1));
        }

        private static void DoInject(string injectJs, int chromeDevHwnd, ChatDesk desk, Func<bool> isConnectOk)
        {
            for (int i = 0; i < 4; i++)
            {
                WinApi.BringTopAndDoAction(chromeDevHwnd, () =>
                {
                    DispatcherEx.xInvoke(() =>
                    {
                        WinApi.PressEsc();
                    });
                }, 2);
                if (i == 0)
                {
                    Thread.Sleep(4000);
                }
                else
                {
                    Thread.Sleep(4000);
                }
                ExcuteJsInner(injectJs, chromeDevHwnd);
                Thread.Sleep(100);
                if (isConnectOk())
                {
                    break;
                }
            }
            if (desk.IsForeground)
            {
                desk.BringTop();
            }
        }

        private static void ExcuteJsInner(string injectJs, int chromeDevHwnd)
        {
            ClipboardEx.UseClipboardWithAutoRestore(() =>
            {
                DispatcherEx.xInvoke(() =>
                {
                    ClipboardEx.SetTextSafe(injectJs);
                    WinApi.BringTopAndDoAction(chromeDevHwnd, () =>
                    {
                        WinApi.PressCtrlV();
                        Thread.Sleep(500);
                        WinApi.PressEnterKey();
                    }, 4);
                });
            });
        }

        private static int GetExistChromeDevTools(int processId, int tryCnt = 20)
        {
            int debugHwnd = 0;
            int i = 0;
            while (i < tryCnt && debugHwnd == 0)
            {
                if (i > 0)
                {
                    Thread.Sleep(50);
                }
                var debugHwnds = WinApi.FindAllDesktopWindowByClassNameAndTitlePattern("Aef_WidgetWin_0", "^alires:.+", null);
                debugHwnds = debugHwnds.Where(k =>
                {
                    int pid = 0;
                    WinApi.Api.GetWindowThreadProcessId(k, ref pid);
                    return pid == processId;
                }).ToList();
                debugHwnd = ((debugHwnds != null) ? debugHwnds.FirstOrDefault() : 0);
                i++;
            }
            return debugHwnd;
        }

        private static int FindOkButtonHwnd(int hwnd)
        {
            return WinApi.FindChildHwnd(hwnd, new WinApi.WindowClue("Button", "是(&Y)", -1));
        }

        private static int GetOpenDevToolsWarning(int processId)
        {
            int devWarnDialogHwnd = 0;
            for (int i = 0; i < 5; i++)
            {
                var warningDialogs = WinApi.FindAllDesktopWindowByClassNameAndTitlePattern("#32770", "Warning", null);
                warningDialogs = warningDialogs.Where(k =>
                {
                    int pid = 0;
                    WinApi.Api.GetWindowThreadProcessId(k, ref pid);
                    return pid == processId;
                }).ToList();
                devWarnDialogHwnd = ((warningDialogs != null) ? warningDialogs.FirstOrDefault() : 0);
                if (devWarnDialogHwnd != 0)
                {
                    break;
                }
                Thread.Sleep(50);
            }
            return devWarnDialogHwnd;
        }

    }

}
