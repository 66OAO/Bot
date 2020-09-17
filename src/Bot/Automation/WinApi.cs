using BotLib;
using BotLib.Collection;
using BotLib.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Bot.Automation
{
    public static class WinApi
    {
        public enum ClsNameEnum
        {
            Unknown,
            StandardWindow,
            StandardButton,
            SplitterBar,
            RichEditComponent,
            PrivateWebCtrl,
            Aef_WidgetWin_0,
            ToolBarPlus,
            Aef_RenderWidgetHostHWND,
            SuperTabCtrl,
            StackPanel,
            EditComponent
        }
        public class WindowClue
        {
            public string ClsName
            {
                get;
                private set;
            }
            public string Text
            {
                get;
                private set;
            }
            public int SkipCount
            {
                get;
                private set;
            }
            public WindowClue(WinApi.ClsNameEnum clsname, string text = null, int skipCount = -1)
            {
                this.ClsName = clsname.ToString();
                if (text == "")
                {
                    text = null;
                }
                this.Text = text;
                this.SkipCount = skipCount;
            }
            public WindowClue(string clsname, string text = null, int skipCount = -1)
            {
                this.ClsName = clsname;
                this.Text = ((text == "") ? null : text);
                this.SkipCount = skipCount;
            }
        }
        public class WindowPlacement
        {
            public Rectangle RestoreWindow;
            public WindowState WinState;
            public override string ToString()
            {
                return string.Format("rect:left={0},top={1},width={2},height={3}.state={4}", new object[]
				{
					this.RestoreWindow.Left,
					this.RestoreWindow.Top,
					this.RestoreWindow.Width,
					this.RestoreWindow.Height,
					this.WinState
				});
            }
            public WindowPlacement()
            {
            }
        }
        public static class Editor
        {
            private static object _pasteRichTextSynObj;
            public static bool GetText(HwndInfo hwnd, out string txt)
            {
                bool rlt = WinApi.HwndTextReaderFromTalkerNoAi.GetText(hwnd.Handle, out txt);
                txt = (txt ?? "");
                return rlt;
            }
            public static bool GetText(HwndInfo hwnd, out string txt, out string err, bool isvip = false)
            {
                bool text = WinApi.HwndTextReaderFromTalkerNoAi.GetText(hwnd.Handle, out txt, out err, isvip);
                txt = (txt ?? "");
                return text;
            }
            public static bool SetText(HwndInfo hwnd, string txt, bool moveCaretToEnd = false)
            {
                var sb = new StringBuilder();
                sb.Append(txt);
                bool result = WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.SendForSetText(hwnd, sb, 2000);
                if (moveCaretToEnd)
                {
                    WinApi.Editor.MoveCaretToEnding(hwnd);
                }
                return result;
            }
            public static bool SelectTextAll(HwndInfo hwnd)
            {
                return WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwnd, "SelectTextAll", 177, 0, -1, 2000);
            }
            public static bool MoveCaretToEnding(HwndInfo hwnd)
            {
                return WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwnd, "MoveCaretToEnding", 177, -1, -1, 2000);
            }
            public static void PasteRichText(HwndInfo hwnd, string rtf)
            {
                object obj = WinApi.Editor._pasteRichTextSynObj;
                lock (obj)
                {
                    Dictionary<string, object> dict = ClipboardEx.Backup();
                    ClipboardEx.SetRichText(rtf);
                    WinApi.PasteText(hwnd);
                    ClipboardEx.Restore(dict);
                }
            }
            public static void Test()
            {
            }
            static Editor()
            {
                WinApi.Editor._pasteRichTextSynObj = new object();
            }
        }
        private class HwndTextReaderFromTalkerNoAi
        {
            private static Cache<int, bool> _hwndAccessedCache;
            private static List<int> _hwndAccessedList;
            private static Dictionary<int, int> _badHwndDict;
            public static bool GetText(int hWnd, out string txt)
            {
                bool result = false;
                txt = null;
                if (hWnd == 0)
                {
                    return result;
                }
                if (WinApi.HwndTextReaderFromTalkerNoAi.IsBadHwnd(hWnd))
                {
                    txt = "";
                    return result;
                }
                if (!WinApi.HwndTextReaderFromTalkerNoAi.HasHwndOrAdd(hWnd))
                {
                    result = WinApi.HwndTextReaderFromTalkerNoAi.GetHwndText(hWnd, out txt);
                }
                else
                {
                    var sb = new StringBuilder(2048);
                    result = WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.SendForGetText(hWnd, sb, 2000);
                    txt = sb.ToString();
                }
                return result;
            }
            public static bool GetHwndText(int hWnd, out string txt)
            {
                txt = null;
                bool result = false;
                if (hWnd == 0)
                {
                    return result;
                }
                if (WinApi.HwndTextReaderFromTalkerNoAi.IsBadHwnd(hWnd))
                {
                    txt = "";
                    return result;
                }
                var sb = new StringBuilder(2048);
                if (result = WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.SendForGetText(hWnd, sb, 2000))
                {
                    txt = sb.ToString();
                }
                return result;
            }
            private static bool HasHwndOrAdd(int hWnd)
            {
                bool rt = false;
                if (WinApi.HwndTextReaderFromTalkerNoAi._hwndAccessedList.Contains(hWnd))
                {
                    rt = true;
                }
                else
                {
                    WinApi.HwndTextReaderFromTalkerNoAi._hwndAccessedList.Add(hWnd);
                }
                return rt;
            }
            private static bool IsBadHwnd(int hWnd)
            {
                return WinApi.HwndTextReaderFromTalkerNoAi._badHwndDict.ContainsKey(hWnd) && WinApi.HwndTextReaderFromTalkerNoAi._badHwndDict[hWnd] > 5;
            }
            static HwndTextReaderFromTalkerNoAi()
            {
                WinApi.HwndTextReaderFromTalkerNoAi._hwndAccessedCache = new Cache<int, bool>(1000, 60000, null);
                WinApi.HwndTextReaderFromTalkerNoAi._hwndAccessedList = new List<int>();
                WinApi.HwndTextReaderFromTalkerNoAi._badHwndDict = new Dictionary<int, int>();
            }
            public static bool GetText(int hwnd, out string txt, out string err, bool isvip)
            {
                bool rt = false;
                txt = null;
                err = null;
                if (hwnd == 0)
                {
                    txt = null;
                    err = "hwnd==0";
                }
                else if (!WinApi.HwndTextReaderFromTalkerNoAi.AddOrUpdateHwndAccessed(hwnd))
                {
                    if (!(rt = WinApi.HwndTextReaderFromTalkerNoAi.GetTextNoBlock(hwnd, out txt)))
                    {
                        err = "GetTextNoBlock";
                    }
                }
                else
                {
                    StringBuilder stringBuilder = new StringBuilder(2048);
                    if (!(rt = WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.SendForGetText(hwnd, stringBuilder, 2000, isvip)))
                    {
                        err = "SendForGetText";
                    }
                    txt = stringBuilder.ToString();
                }
                return rt;
            }

            public static bool GetTextNoBlock(int hwnd, out string txt)
            {
                txt = null;
                bool result = false;
                if (hwnd == 0)
                {
                    txt = null;
                }
                else
                {
                    StringBuilder stringBuilder = new StringBuilder(2048);
                    if (result = WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.SendForGetText(hwnd, stringBuilder, 2000, false))
                    {
                        txt = stringBuilder.ToString();
                    }
                }
                return result;
            }

            private static bool AddOrUpdateHwndAccessed(int hwnd)
            {
                bool rt = false;
                if (WinApi.HwndTextReaderFromTalkerNoAi._hwndAccessedCache.ContainsKey(hwnd))
                {
                    rt = true;
                }
                else
                {
                    WinApi.HwndTextReaderFromTalkerNoAi._hwndAccessedCache.AddOrUpdate(hwnd, true);
                }
                return rt;
            }


            public static class MessageSender
            {
                private class SendMsgSafe
                {
                    private int _hwnd;
                    private int _msg;
                    private int _wp;
                    private StringBuilder _sblp;
                    private int _ilp;
                    private ManualResetEvent _mre;
                    public bool IsFinished;
                    public SendMsgSafe(int hWnd, int msg, int wp, StringBuilder sblp, int msTimeout = 2000)
                    {
                        this._mre = new ManualResetEvent(false);
                        this.IsFinished = false;
                        this._hwnd = hWnd;
                        this._msg = msg;
                        this._wp = wp;
                        this._sblp = sblp;
                        new Thread(new ThreadStart(this.SendForGetText))
                        {
                            IsBackground = true
                        }.Start();
                        if (this._mre.WaitOne(msTimeout))
                        {
                            this.IsFinished = true;
                        }
                    }
                    public SendMsgSafe(int hWnd, int msg, int wp, int ilp, int msTimeout = 2000)
                    {
                        this._mre = new ManualResetEvent(false);
                        this.IsFinished = false;
                        this._hwnd = hWnd;
                        this._msg = msg;
                        this._wp = wp;
                        this._ilp = ilp;
                        new Thread(new ThreadStart(this.SendForSetText))
                        {
                            IsBackground = true
                        }.Start();
                        if (this._mre.WaitOne(msTimeout))
                        {
                            this.IsFinished = true;
                        }
                    }
                    private void SendForGetText()
                    {
                        WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.SendMsgSafe.SendMessage(this._hwnd, this._msg, this._wp, this._sblp);
                        this._mre.Set();
                    }
                    private void SendForSetText()
                    {
                        WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.SendMsgSafe.SendMessage(this._hwnd, this._msg, this._wp, this._ilp);
                        this._mre.Set();
                    }
                    [DllImport("user32.dll")]
                    public static extern int SendMessage(int hWnd, int Msg, int wParam, StringBuilder lParam);
                    [DllImport("user32.dll", EntryPoint = "SendMessage")]
                    public static extern int SendMessage(int hWnd, int Msg, int wParam, int lParam);
                }
                private class HwndSendMessageStatInfo
                {
                    static HwndSendMessageStatInfo()
                    {
                        WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo._dict = new Cache<int, WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo>(1000, 0, null);
                    }
                    public HwndSendMessageStatInfo(int hwnd, string desc)
                    {
                        this._isInvalid = false;
                        this._hwnd = hwnd;
                        this._desc = desc;
                        this._lastOkTime = DateTime.MaxValue;
                        this._lastFailTime = DateTime.MaxValue;
                        this._lastStartFailTime = DateTime.MaxValue;
                    }
                    private static Cache<int, WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo> _dict;
                    private int _hwnd;
                    private string _desc;
                    private ulong _okCount;
                    private ulong _totalFailCount;
                    private ulong _lastFailCount;
                    private DateTime _lastOkTime;
                    private DateTime _lastFailTime;
                    private DateTime _lastStartFailTime;
                    private bool _isInvalid;
                    public static bool HwndCanAccess(int hWnd)
                    {
                        bool canAccess = true;
                        if (hWnd == 0)
                        {
                            canAccess = false;
                            return canAccess;
                        }
                        WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo hwndSendMessageStatInfo;
                        if (WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo._dict.TryGetValue(hWnd, out hwndSendMessageStatInfo, null))
                        {
                            canAccess = hwndSendMessageStatInfo.CanAccess;
                        }
                        return canAccess;
                    }
                    public static void Report(HwndInfo hwndInfo, string desc, bool isOk)
                    {
                        try
                        {
                            if (!WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo._dict.ContainsKey(hwndInfo.Handle))
                            {
                                WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo._dict[hwndInfo.Handle] = new WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo(hwndInfo.Handle, desc + "," + hwndInfo.Description);
                            }
                            if (isOk)
                            {
                                WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo._dict[hwndInfo.Handle].UpdateOkCount();
                            }
                            else
                            {
                                bool isInvalid;
                                string lastError = WinApi.GetLastError(out isInvalid);
                                Log.Error(string.Format("MessageSender.HwndSendMessageStatInfo,LastError={0},adesc={1},hwndDesc={2},hwnd={3}", new object[]
							{
								lastError,
								desc,
								hwndInfo.Description,
								hwndInfo.Handle
							}));
                                WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo._dict[hwndInfo.Handle].UpdateFailCount(isInvalid);
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Exception(e);
                        }
                    }
                    public static void Report(int hWnd, string desc, bool isOk)
                    {
                        try
                        {
                            if (!WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo._dict.ContainsKey(hWnd))
                            {
                                WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo._dict[hWnd] = new WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo(hWnd, desc);
                            }
                            if (isOk)
                            {
                                WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo._dict[hWnd].UpdateOkCount();
                            }
                            else
                            {
                                bool isInvalid;
                                string lastError = WinApi.GetLastError(out isInvalid);
                                WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo._dict[hWnd].UpdateFailCount(isInvalid);
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Exception(e);
                        }
                    }

                    private bool CanAccess
                    {
                        get
                        {
                            return (!this._isInvalid && this._lastFailCount < 2) || (DateTime.Now - this._lastFailTime).TotalMinutes > 2.0;
                        }
                    }
                    public void UpdateOkCount()
                    {
                        this._okCount += 1;
                        this._lastOkTime = DateTime.Now;
                        this._lastStartFailTime = DateTime.MaxValue;
                        this._lastFailCount = 0;
                        this._isInvalid = false;
                    }
                    public void UpdateFailCount(bool isInvalid)
                    {
                        this._totalFailCount += 1uL;
                        this._lastFailCount += 1uL;
                        this._lastFailTime = DateTime.Now;
                        this._isInvalid = isInvalid;
                        if (this._lastStartFailTime == DateTime.MaxValue)
                        {
                            this._lastStartFailTime = this._lastFailTime;
                        }
                        this.LogFailIfNeed();
                    }

                    public static bool IsHwndOk(int hwnd)
                    {
                        bool rt = true;
                        try
                        {
                            if (WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo._dict == null)
                            {
                                Log.Info("IsHwndOk, dict==null");
                            }
                            WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo hwndSendMessageStatInfo;
                            if (WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo._dict != null && WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo._dict.TryGetValue(hwnd, out hwndSendMessageStatInfo, null))
                            {
                                if (hwndSendMessageStatInfo == null)
                                {
                                    Log.Info("IsHwndOk,x==null");
                                }
                                if (hwndSendMessageStatInfo != null && (hwndSendMessageStatInfo._lastFailCount > 20uL || (hwndSendMessageStatInfo._lastFailTime != DateTime.MaxValue && (DateTime.Now - hwndSendMessageStatInfo._lastFailTime).TotalSeconds < 5.0 && hwndSendMessageStatInfo._lastFailCount > 2uL)))
                                {
                                    rt = false;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Exception(e);
                        }
                        return rt;
                    }
                    private void LogFailIfNeed()
                    {
                        if (this._totalFailCount == 10uL || this._totalFailCount % 1000uL == 0uL)
                        {
                            string msg = string.Format("send message fail.hwnd={0},desc={1},total fail count={2},\r\n                            last continue fail count={3},last start fail time={4},last fail time={5},\r\n                            ok count={6},last ok time={7}", new object[]
						{
							this._hwnd,
							this._desc,
							this._totalFailCount,
							this._lastFailCount,
							this._lastStartFailTime,
							this._lastFailTime,
							this._okCount,
							this._lastOkTime
						});
                            Log.Error(msg);
                        }
                    }
                }

                public static bool SendForGetText(int hwnd, StringBuilder sb, int timeoutMs = 2000, bool isvip = false)
                {
                    bool rt = false;
                    if (hwnd != 0 && (isvip || WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo.IsHwndOk(hwnd)))
                    {
                        int lpdwResult;
                        rt = WinApi.Api.SendMessageTimeout(hwnd, 13, sb.Capacity, sb, WinApi.Api.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG | WinApi.Api.SendMessageTimeoutFlags.SMTO_ERRORONEXIT, timeoutMs, out lpdwResult);
                        WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo.Report(hwnd, "SendForGetText", rt);
                    }
                    return rt;
                }
                public static bool SendForGetText(int hWnd, StringBuilder sbText, int uTimeout = 2000)
                {
                    bool result = false;
                    if (WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo.HwndCanAccess(hWnd))
                    {
                        int lpdwResult;
                        result = WinApi.Api.SendMessageTimeout(hWnd, 13, sbText.Capacity, sbText, WinApi.Api.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG | WinApi.Api.SendMessageTimeoutFlags.SMTO_ERRORONEXIT, uTimeout, out lpdwResult);
                        WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo.Report(hWnd, "SendForGetText", result);
                    }
                    return result;
                }
                public static bool SendForSetText(int hWnd, StringBuilder sbText, int uTimeout = 2000)
                {
                    bool result = false;
                    if (WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo.HwndCanAccess(hWnd))
                    {
                        int lpdwResult;
                        result = WinApi.Api.SendMessageTimeout(hWnd, 12, 0, sbText, WinApi.Api.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG | WinApi.Api.SendMessageTimeoutFlags.SMTO_ERRORONEXIT, uTimeout, out lpdwResult);
                        WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo.Report(hWnd, "SendForSetText", result);
                    }
                    return result;
                }
                public static bool SendForGetText(HwndInfo hwndInfo, StringBuilder sbText, int uTimeout = 2000)
                {
                    bool result = false;
                    if (WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo.HwndCanAccess(hwndInfo.Handle))
                    {
                        int lpdwResult;
                        result = WinApi.Api.SendMessageTimeout(hwndInfo.Handle, 13, sbText.Capacity, sbText, WinApi.Api.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG | WinApi.Api.SendMessageTimeoutFlags.SMTO_ERRORONEXIT, uTimeout, out lpdwResult);
                        WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo.Report(hwndInfo, "SendForGetText", result);
                    }
                    return result;
                }
                public static bool SendForSetText(HwndInfo hwndInfo, StringBuilder sbText, int uTimeout = 2000)
                {
                    bool result = false;
                    if (WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo.HwndCanAccess(hwndInfo.Handle))
                    {
                        int lpdwResult;
                        result = WinApi.Api.SendMessageTimeout(hwndInfo.Handle, 12, 0, sbText, WinApi.Api.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG | WinApi.Api.SendMessageTimeoutFlags.SMTO_ERRORONEXIT, uTimeout, out lpdwResult);
                        WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo.Report(hwndInfo, "SendForSetText", result);
                    }
                    return result;
                }
                public static bool SendMessage(int hWnd, int msg, int wParam, int lParam, int uTimeout = 2000, [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
                {
                    HwndInfo hwndInfo = new HwndInfo(hWnd, "unknown");
                    int lpdwResult;
                    return WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwndInfo, caller, msg, wParam, lParam, out lpdwResult, uTimeout);
                }
                public static bool SendMessage(HwndInfo hwndInfo, string caller, int msg, int wParam, int lParam, out int lpdwResult, int uTimeout = 2000)
                {
                    bool result = false;
                    lpdwResult = 0;
                    if (WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo.HwndCanAccess(hwndInfo.Handle))
                    {
                        result = WinApi.Api.SendMessageTimeout(hwndInfo.Handle, msg, wParam, lParam, WinApi.Api.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG | WinApi.Api.SendMessageTimeoutFlags.SMTO_ERRORONEXIT, uTimeout, out lpdwResult);
                        WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo.Report(hwndInfo, caller, result);
                    }
                    return result;
                }
                public static bool SendMessage(HwndInfo hWnd, string caller, int msg, int wParam, int lParam, int uTimeout = 2000)
                {
                    bool result = false;
                    if (WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo.HwndCanAccess(hWnd.Handle))
                    {
                        int lpdwResult;
                        result = WinApi.Api.SendMessageTimeout(hWnd.Handle, msg, wParam, lParam, WinApi.Api.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG | WinApi.Api.SendMessageTimeoutFlags.SMTO_ERRORONEXIT, uTimeout, out lpdwResult);
                        WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.HwndSendMessageStatInfo.Report(hWnd, caller, result);
                    }
                    return result;
                }
            }
        }

        private static class ChildHwndManager
        {
            private class ChildHwndCollection
            {
                private int ParentHwnd;
                private List<int> ChildHwnds = new List<int>();
                private DateTime objNewTime;
                public ChildHwndCollection(int parentHwnd, List<int> childHwnds)
                {
                    this.ParentHwnd = parentHwnd;
                    this.ChildHwnds = childHwnds;
                    this.objNewTime = DateTime.Now;
                }
                public bool IsGreaterThenTwoSecd()
                {
                    return (DateTime.Now - this.objNewTime).TotalSeconds > (double)WinApi.ChildHwndManager.TwoSecd;
                }
                public bool ContainsHwnd(int childHwnd)
                {
                    return this.ChildHwnds.Contains(childHwnd);
                }
            }
            private static readonly int TwoSecd = 2;
            private static Dictionary<int, ChildHwndCollection> _childHwndCache = new Dictionary<int, WinApi.ChildHwndManager.ChildHwndCollection>(10, null);
            public static bool IsChild(int parentHwnd, int childHwnd)
            {
                if (!WinApi.ChildHwndManager._childHwndCache.ContainsKey(parentHwnd) || WinApi.ChildHwndManager._childHwndCache[parentHwnd].IsGreaterThenTwoSecd())
                {
                    List<int> childHwnds = WinApi.ChildHwndManager.GetChildHwnds(parentHwnd);
                    WinApi.ChildHwndManager.ChildHwndCollection value = new WinApi.ChildHwndManager.ChildHwndCollection(parentHwnd, childHwnds);
                    WinApi.ChildHwndManager._childHwndCache[parentHwnd] = value;
                }
                return WinApi.ChildHwndManager._childHwndCache[parentHwnd].ContainsHwnd(childHwnd);
            }
            private static List<int> GetChildHwnds(int pHwnd)
            {
                List<int> childHwnds = new List<int>();
                int hwnd = 0;
                while (true)
                {
                    hwnd = WinApi.Api.FindWindowEx(pHwnd, hwnd, null, null);
                    if (hwnd == 0)
                    {
                        break;
                    }
                    childHwnds.Add(hwnd);
                    childHwnds.AddRange(WinApi.ChildHwndManager.GetChildHwnds(hwnd));
                }
                return childHwnds;
            }
        }
        public static class Api
        {
            public enum ShowWindowCommands
            {
                Hide,
                Normal,
                ShowMinimized,
                Maximize,
                ShowMaximized = 3,
                ShowNoActivate,
                Show,
                Minimize,
                ShowMinNoActive,
                ShowNA,
                Restore,
                ShowDefault,
                ForceMinimize
            }
            private enum GetWindow_Cmd : uint
            {
                GW_HWNDFIRST,
                GW_HWNDLAST,
                GW_HWNDNEXT,
                GW_HWNDPREV,
                GW_OWNER,
                GW_CHILD,
                GW_ENABLEDPOPUP
            }
            public static class SWP
            {
                public static readonly int NOSIZE;
                public static readonly int NOMOVE;
                public static readonly int NOZORDER;
                public static readonly int NOREDRAW;
                public static readonly int NOACTIVATE;
                public static readonly int DRAWFRAME;
                public static readonly int FRAMECHANGED;
                public static readonly int SHOWWINDOW;
                public static readonly int HIDEWINDOW;
                public static readonly int NOCOPYBITS;
                public static readonly int NOOWNERZORDER;
                public static readonly int NOREPOSITION;
                public static readonly int NOSENDCHANGING;
                public static readonly int DEFERERASE;
                public static readonly int ASYNCWINDOWPOS;
                static SWP()
                {
                    WinApi.Api.SWP.NOSIZE = 1;
                    WinApi.Api.SWP.NOMOVE = 2;
                    WinApi.Api.SWP.NOZORDER = 4;
                    WinApi.Api.SWP.NOREDRAW = 8;
                    WinApi.Api.SWP.NOACTIVATE = 16;
                    WinApi.Api.SWP.DRAWFRAME = 32;
                    WinApi.Api.SWP.FRAMECHANGED = 32;
                    WinApi.Api.SWP.SHOWWINDOW = 64;
                    WinApi.Api.SWP.HIDEWINDOW = 128;
                    WinApi.Api.SWP.NOCOPYBITS = 256;
                    WinApi.Api.SWP.NOOWNERZORDER = 512;
                    WinApi.Api.SWP.NOREPOSITION = 512;
                    WinApi.Api.SWP.NOSENDCHANGING = 1024;
                    WinApi.Api.SWP.DEFERERASE = 8192;
                    WinApi.Api.SWP.ASYNCWINDOWPOS = 16384;
                }
            }
            public enum GetWindowType : uint
            {
                GW_HWNDFIRST,
                GW_HWNDLAST,
                GW_HWNDNEXT,
                GW_HWNDPREV,
                GW_OWNER,
                GW_CHILD,
                GW_ENABLEDPOPUP
            }
            public enum ShowCmdEnum
            {
                HIDE,
                SHOWNORMAL,
                NORMAL = 1,
                SHOWMINIMIZED,
                SHOWMAXIMIZED,
                MAXIMIZE = 3,
                SHOWNOACTIVATE,
                SHOW,
                MINIMIZE,
                SHOWMINNOACTIVE,
                SHOWNA,
                RESTORE,
                SHOWDEFAULT,
                FORCEMINIMIZE
            }
            public struct Point
            {
                public int x;
                public int y;
                public Point(int x, int y)
                {
                    this.x = x;
                    this.y = y;
                }
            }
            public struct Rect
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
            public struct WindowPlacement
            {
                public int length;
                public uint flags;
                public WinApi.Api.ShowCmdEnum showCmd;
                public WinApi.Api.Point ptMinPosition;
                public WinApi.Api.Point ptMaxPosition;
                public WinApi.Api.Rect rcNormalPosition;
            }
            [Flags]
            public enum SendMessageTimeoutFlags : uint
            {
                SMTO_NORMAL = 0u,
                SMTO_BLOCK = 1u,
                SMTO_ABORTIFHUNG = 2u,
                SMTO_NOTIMEOUTIFNOTHUNG = 8u,
                SMTO_ERRORONEXIT = 32u
            }
            public static readonly int HWND_TOPMOST;
            public static readonly int HWND_NOTOPMOST;
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool ShowWindow(int hWnd, WinApi.Api.ShowWindowCommands cmd);
            [DllImport("user32.dll")]
            public static extern bool ShowWindowAsync(int hWnd, WinApi.Api.ShowWindowCommands cmd);
            [DllImport("user32")]
            private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
            [DllImport("user32.dll")]
            public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
            [DllImport("user32.dll")]
            public static extern bool IsWindowEnabled(int hWnd);
            [DllImport("user32.dll")]
            public static extern int GetTopWindow(int hWnd);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetWindowLong(int hWnd, int nlndex);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int SetWindowLong(int hWnd, int nlndex, int dwNewLong);
            public static int GetNextWindow(int hWnd)
            {
                return WinApi.Api.GetWindow(hWnd, WinApi.Api.GetWindow_Cmd.GW_HWNDNEXT);
            }
            public static int GetPrevWindow(int hWnd)
            {
                return WinApi.Api.GetWindow(hWnd, WinApi.Api.GetWindow_Cmd.GW_HWNDPREV);
            }
            [DllImport("user32.dll", SetLastError = true)]
            private static extern int GetWindow(int hWnd, WinApi.Api.GetWindow_Cmd cmd);
            [DllImport("user32.dll")]
            public static extern bool SetWindowPos(int hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
            [DllImport("user32.dll")]
            public static extern int IsIconic(int hWnd);
            [DllImport("user32.dll")]
            public static extern int IsZoomed(int hwnd);
            [DllImport("user32.dll")]
            public static extern int GetWindowRect(int hWnd, ref WinApi.Api.Rect rect);
            [DllImport("user32.dll")]
            public static extern int WindowFromPoint(WinApi.Api.Point pt);
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern int GetAncestor(int int_2, int int_3);
            [DllImport("user32.dll", EntryPoint = "GetWindow", SetLastError = true)]
            public static extern int GetWindow(int hWnd, WinApi.Api.GetWindowType type);
            [DllImport("user32.dll")]
            public static extern int GetForegroundWindow();

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool GetGUIThreadInfo(int idThread, ref WinApi.Api.GuiThreadInfo pgui);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern int GetClassName(int hwnd, StringBuilder lpClassName, int nMaxCount);
            [DllImport("user32.dll")]
            public static extern bool GetWindowPlacement(int hWnd, ref WinApi.Api.WindowPlacement lpwndpl);
            [DllImport("user32.dll")]
            public static extern bool SetWindowPlacement(int hWnd, ref WinApi.Api.WindowPlacement lpwndpl);
            [DllImport("user32.dll")]
            public static extern int GetWindowThreadProcessId(int hwnd, ref int lpdwProcessId);
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsWindow(int hWnd);
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool SendMessageTimeout(int hWnd, int Msg, int wParam, int lParam, WinApi.Api.SendMessageTimeoutFlags fuFlags, int uTimeout, out int lpdwResult);
            [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessageTimeout", SetLastError = true)]
            public static extern bool SendMessageTimeout(int hWnd, int Msg, int wParam, StringBuilder lParam, WinApi.Api.SendMessageTimeoutFlags fuFlags, int uTimeout, out int lpdwResult);
            [DllImport("user32.dll")]
            public static extern int SendMessage(int hWnd, int Msg, int wParam, StringBuilder lParam);
            [DllImport("user32.dll", EntryPoint = "SendMessage")]
            public static extern int SendMessage(int hWnd, int Msg, int wParam, int lParam);
            [DllImport("user32.dll")]
            public static extern int PostMessage(int hWnd, int Msg, int wParam, StringBuilder lParam);
            [DllImport("user32.dll", EntryPoint = "PostMessage")]
            public static extern int PostMessage(int hWnd, int Msg, int wParam, int lParam);
            [DllImport("user32.dll")]
            public static extern int FindWindowEx(int hwndParent, int hwndChildAfter, string lpszClass, string lpszWindow);
            [DllImport("user32.dll")]
            public static extern bool IsWindowVisible(int hWnd);


            public struct GuiThreadInfo
            {
                public int cbSize;
                public uint flags;
                public int hwndActive;
                public int hwndFocus;
                public int hwndCapture;
                public int hwndMenuOwner;
                public int hwndMoveSize;
                public int hwndCaret;
                public Rect rcCaret;
            }

            static Api()
            {
                WinApi.Api.HWND_TOPMOST = -1;
                WinApi.Api.HWND_NOTOPMOST = -2;
            }
        }
        public static int GetZOrderHighestHwnd(HashSet<int> hlist)
        {
            for (int i = WinApi.Api.GetTopWindow(0); i > 0; i = WinApi.Api.GetNextWindow(i))
            {
                if (hlist.Contains(i))
                {
                    return i;
                }
            }
            throw new Exception("GetZOrderHighestHwnd,找不到顶层窗口,hlist=" + hlist.xToString(",", true));
        }
        public static string GetLastError(out bool isInvalid)
        {
            string text = null;
            isInvalid = false;
            try
            {
                int lastWin32Error = Marshal.GetLastWin32Error();
                int num = lastWin32Error;
                if (num != 1400)
                {
                    if (num == 1460)
                    {
                        text = "ERROR_TIMEOUT";
                    }
                }
                else
                {
                    text = "ERROR_INVALID_WINDOW_HANDLE";
                    isInvalid = true;
                }
                text = ((text == null) ? ("0x" + lastWin32Error.ToString("x")) : string.Format("{0}(0x{1})", text, lastWin32Error.ToString("x")));
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return text;
        }
        public static bool SetFocus(HwndInfo hwnd)
        {
            return WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwnd, "focus", 7, 0, 0, 2000);
        }
        public static bool IsWindowEnabled(int hwnd)
        {
            return hwnd > 0 && WinApi.Api.IsWindowEnabled(hwnd);
        }
        public static void BringTopAndSetWindowSize(int hwnd, int x, int y, int cx, int cy)
        {
            WinApi.Api.SetWindowPos(hwnd, WinApi.Api.HWND_TOPMOST, x, y, cx, cy, 64u);
        }
        public static bool BringTop(int hwnd)
        {
            //return WinApi.TopMost(hwnd) && WinApi.CancelTopMost(hwnd);
            bool rt = false;
            try
            {
                if (WinApi.IsWindowMinimized(hwnd))
                {
                    WinApi.ShowNormal(hwnd);
                }
                rt = (WinApi.TopMost(hwnd) && WinApi.CancelTopMost(hwnd) && WinApi.SetForegroundWindow(hwnd));
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return rt;
        }
        public static bool TopMost(int hwnd)
        {
            return WinApi.Api.SetWindowPos(hwnd, WinApi.Api.HWND_TOPMOST, 1, 1, 1, 1, 67u);
        }
        public static bool CancelTopMost(int hwnd)
        {
            return WinApi.Api.SetWindowPos(hwnd, WinApi.Api.HWND_NOTOPMOST, 1, 1, 1, 1, 3u);
        }

        public static void ShowAfter(int hwnd, int insertafter)
        {
            if (!WinApi.Api.SetWindowPos(hwnd, insertafter, 1, 1, 1, 1, 67u))
            {
                Log.Info("ShowAfterFail.");
            }
        }
        public static int FindChildHwnd(int parentHwnd, WinApi.WindowClue clue)
        {
            int hWnd = 0;
            if (clue.SkipCount > 0)
            {
                for (int i = 0; i <= clue.SkipCount; i++)
                {
                    hWnd = WinApi.Api.FindWindowEx(parentHwnd, hWnd, clue.ClsName, clue.Text);
                    if (hWnd == 0)
                    {
                        break;
                    }
                }
            }
            else
            {
                hWnd = WinApi.Api.FindWindowEx(parentHwnd, hWnd, clue.ClsName, clue.Text);
            }
            return hWnd;
        }
        public static string GetWindowStructInfo(int hwnd)
        {
            string result;
            if (hwnd == 0)
            {
                result = "";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(WinApi.GetWindowClass(hwnd));
                WinApi.TraverseChildHwnd(hwnd, (hchild, level) =>
                {
                    string winClass = "";
                    string winText = "";
                    bool isVisible = false;
                    try
                    {
                        winClass = WinApi.GetWindowClass(hchild);
                        winText = WinApi.GetText(hchild);
                        isVisible = WinApi.IsVisible(hchild);
                    }
                    catch (Exception ex)
                    {
                        winClass = "Error:" + ex.Message;
                    }
                    sb.AppendLine(string.Format("{0} {1},{2},{3}",(level > 0) ? new string('+', level) : "",winClass,isVisible,	winText));
                }, true, 0, null, null);
                result = sb.ToString();
            }
            return result;
        }
        public static int FindDescendantHwnd(int parentHwnd, List<WinApi.WindowClue> clueList, [System.Runtime.CompilerServices.CallerMemberName] string callerName = "")
        {
            int hWnd = WinApi.FindDescendantHwnd(parentHwnd, clueList, 0);
            if (hWnd == 0)
            {
                Log.Error("获取hwnd失败！CallerName=" + callerName);
            }
            return hWnd;
        }

        private static int FindDescendantHwnd(int parentHwnd, List<WinApi.WindowClue> clueList, int startIdx)
        {
            if (clueList == null || clueList.Count == 0)
            {
                throw new Exception("没有WindowClue");
            }
            int hwndChild = 0;
            WinApi.WindowClue windowClue = clueList[startIdx];
            int skipCnt = (windowClue.SkipCount > 0) ? windowClue.SkipCount : 0;
            for (int i = 0; i <= skipCnt; i++)
            {
                hwndChild = WinApi.Api.FindWindowEx(parentHwnd, hwndChild, windowClue.ClsName, windowClue.Text);
                if (hwndChild == 0)
                {
                    return hwndChild;
                }
            }
            if (startIdx < clueList.Count - 1)
            {
                int dehWnd = WinApi.FindDescendantHwnd(hwndChild, clueList, startIdx + 1);
                if (dehWnd == 0)
                {
                    while (dehWnd == 0)
                    {
                        hwndChild = WinApi.Api.FindWindowEx(parentHwnd, hwndChild, windowClue.ClsName, windowClue.Text);
                        if (hwndChild == 0)
                        {
                            return hwndChild;
                        }
                        dehWnd = WinApi.FindDescendantHwnd(hwndChild, clueList, startIdx + 1);
                    }
                    hwndChild = dehWnd;
                }
                else
                {
                    hwndChild = dehWnd;
                }
            }
            return hwndChild;
        }
        public static bool ShowNormal(int hwnd)
        {
            WinApi.Api.WindowPlacement windowPlacement = default(WinApi.Api.WindowPlacement);
            bool result = false;
            if (WinApi.Api.GetWindowPlacement(hwnd, ref windowPlacement))
            {
                windowPlacement.showCmd = WinApi.Api.ShowCmdEnum.SHOWNORMAL;
                result = WinApi.Api.SetWindowPlacement(hwnd, ref windowPlacement);
            }
            return result;
        }
        public static bool SetLocation(int handle, int left, int top)
        {
            bool result = false;
            WinApi.Api.WindowPlacement windowPlacement = default(WinApi.Api.WindowPlacement);
            if (WinApi.Api.GetWindowPlacement(handle, ref windowPlacement))
            {
                if (windowPlacement.rcNormalPosition.left != left || windowPlacement.rcNormalPosition.top != top)
                {
                    int num = left - windowPlacement.rcNormalPosition.left;
                    windowPlacement.rcNormalPosition.left = left;
                    windowPlacement.rcNormalPosition.right = windowPlacement.rcNormalPosition.right + num;
                    num = top - windowPlacement.rcNormalPosition.top;
                    windowPlacement.rcNormalPosition.top = top;
                    windowPlacement.rcNormalPosition.bottom = windowPlacement.rcNormalPosition.bottom + num;
                    result = WinApi.Api.SetWindowPlacement(handle, ref windowPlacement);
                }
                else
                {
                    result = true;
                }
            }
            return result;
        }
        public static bool SetRect(int handle, int left, int top, int w, int h)
        {
            bool result = false;
            WinApi.Api.WindowPlacement windowPlacement = default(WinApi.Api.WindowPlacement);
            if (WinApi.Api.GetWindowPlacement(handle, ref windowPlacement))
            {
                int num = windowPlacement.rcNormalPosition.right - windowPlacement.rcNormalPosition.left;
                int num2 = windowPlacement.rcNormalPosition.bottom - windowPlacement.rcNormalPosition.top;
                if (windowPlacement.rcNormalPosition.left != left || windowPlacement.rcNormalPosition.top != top || num != w || num2 != h)
                {
                    windowPlacement.rcNormalPosition.left = left;
                    windowPlacement.rcNormalPosition.right = left + w;
                    windowPlacement.rcNormalPosition.top = top;
                    windowPlacement.rcNormalPosition.bottom = top + h;
                    result = WinApi.Api.SetWindowPlacement(handle, ref windowPlacement);
                }
                else
                {
                    result = true;
                }
            }
            return result;
        }
        public static void ClickHwndBySendMessage(int hwnd, int maxTry = 1)
        {
            if (hwnd > 0)
            {
                if (maxTry < 1)
                {
                    maxTry = 1;
                }
                int num = 0;
                while (num < maxTry && !WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwnd, 245, 0, 0, 2000, "ClickHwndBySendMessage"))
                {
                    num++;
                }
            }
        }
        public static void ClickHwndByPostMessage(int hwnd)
        {
            WinApi.Api.PostMessage(hwnd, 245, 0, 0);
        }
        public static bool IsWindowPointVisable(System.Drawing.Point point, int hwnd)
        {
            return WinApi.IsWindowPointVisable(new WinApi.Api.Point(point.X, point.Y), hwnd);
        }
        public static bool IsWindowPointVisable(WinApi.Api.Point point, int hwnd)
        {
            bool result = false;
            if (WinApi.IsPointInsideScreen(point.x, point.y))
            {
                int qnHwnd = WinApi.Api.WindowFromPoint(point);
                if (hwnd == qnHwnd || WinApi.IsChild(hwnd, qnHwnd))
                {
                    result = true;
                }
            }
            return result;
        }
        public static bool IsChild(int parentHwnd, int childHwnd)
        {
            return WinApi.ChildHwndManager.IsChild(parentHwnd, childHwnd);
        }
        public static bool IsPointInsideScreen(int x, int y)
        {
            return x >= 0 && x < SystemParameters.WorkArea.Width && y >= 0 && y < SystemParameters.WorkArea.Height;
        }
        public static void ClickPointBySendMessage(int hwnd, int x, int y)
        {
            if (x >= 0 && y >= 0)
            {
                y <<= 16;
                y = (x | y);
                WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwnd, 513, 1, y, 2000, "ClickPointBySendMessage");
                WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwnd, 514, 0, y, 2000, "ClickPointBySendMessage");
            }
        }
        public static bool HideDeskWindow(int hwnd)
        {
            bool result = true;
            try
            {
                WinApi.Api.SetWindowPos(hwnd, 1, 1, 1, 1, 1, 131u);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                result = false;
            }
            return result;
        }
        public static bool CancelHideDeskWindow(int hwnd)
        {
            bool result = true;
            try
            {
                if (!WinApi.Api.IsWindowVisible(hwnd))
                {
                    WinApi.Api.SetWindowPos(hwnd, 1, 1, 1, 1, 1, 67u);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
                result = false;
            }
            return result;
        }
        public static bool IsWindowMinimized(int hwnd)
        {
            return WinApi.Api.IsIconic(hwnd) > 0;
        }
        public static bool IsWindowMaximized(int hwnd)
        {
            return WinApi.Api.IsZoomed(hwnd) > 0;
        }
        public static string GetWindowClass(int hwnd)
        {
            var sb = new StringBuilder(512);
            WinApi.Api.GetClassName(hwnd, sb, 512);
            return sb.ToString();
        }
        public static bool IsHwndAlive(int hwnd)
        {
            return WinApi.Api.IsWindow(hwnd);
        }

        public static int GetWindowThreadProcessId(int hWnd)
        {
            int pid = 0;
            WinApi.Api.GetWindowThreadProcessId(hWnd, ref pid);
            return pid;
        }

        public static int GetWindowThreadProcessId(int hWnd, out int pid)
        {
            pid = 0;
            return WinApi.Api.GetWindowThreadProcessId(hWnd, ref pid);
        }
        public static bool IsVisible(int hwnd)
        {
            return WinApi.Api.IsWindowVisible(hwnd);
        }
        public static Task<bool> CloseWindowAsyn(int hwnd, int timeoutMs = 3000)
        {
            return Task.Factory.StartNew<bool>(() => WinApi.CloseWindow(hwnd, timeoutMs));
        }
        public static bool CloseWindow(int hwnd, int timeoutMs = 2000)
        {
            return WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwnd, 16, 0, 0, timeoutMs, "CloseWindow");
        }
        public static bool IsForeground(int hwnd)
        {
            return WinApi.Api.GetForegroundWindow() == hwnd && !WinApi.IsWindowMinimized(hwnd);
        }

        public static bool BringTopAndDoAction(int hwnd, Action act, int cnt = 2)
        {
            bool rt = false;
            int i = 0;
            while (i < cnt)
            {
                if (!WinApi.IsForeground(hwnd))
                {
                    WinApi.BringTop(hwnd);
                    if (!WinApi.IsForeground(hwnd))
                    {
                        Thread.Sleep(50);
                        i++;
                        continue;
                    }
                    if (act != null)
                    {
                        act();
                    }
                    rt = true;
                }
                else
                {
                    if (act != null)
                    {
                        act();
                    }
                    rt = true;
                }
                return rt;
            }
            return rt;
        }

        public static WinApi.WindowPlacement GetWindowPlacement(int hwnd)
        {
            WinApi.WindowPlacement result;
            try
            {
                WinApi.AssertHwndNonZero(hwnd);
                WinApi.Api.WindowPlacement windowPlacement = default(WinApi.Api.WindowPlacement);
                windowPlacement.length = Marshal.SizeOf(windowPlacement);
                if (!WinApi.Api.GetWindowPlacement(hwnd, ref windowPlacement))
                {
                    throw new Exception("GetWindowPlacement return false");
                }
                WinApi.WindowPlacement windowPlacement2 = new WinApi.WindowPlacement();
                windowPlacement2.RestoreWindow = new Rectangle(windowPlacement.rcNormalPosition.left, windowPlacement.rcNormalPosition.top, windowPlacement.rcNormalPosition.right - windowPlacement.rcNormalPosition.left, windowPlacement.rcNormalPosition.bottom - windowPlacement.rcNormalPosition.top);
                switch (windowPlacement.showCmd)
                {
                    case (WinApi.Api.ShowCmdEnum)1:
                        windowPlacement2.WinState = WindowState.Normal;
                        break;
                    case (WinApi.Api.ShowCmdEnum)2:
                        windowPlacement2.WinState = WindowState.Minimized;
                        break;
                    case (WinApi.Api.ShowCmdEnum)3:
                        windowPlacement2.WinState = WindowState.Maximized;
                        break;
                    default:
                        throw new Exception("GetWindowPlacement,unknown showCmd=" + windowPlacement.showCmd);
                }
                result = windowPlacement2;
                return result;
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            result = null;
            return result;
        }
        public static bool RestoreWindow(int hwnd)
        {
            WinApi.Api.WindowPlacement windowPlacement = default(WinApi.Api.WindowPlacement);
            bool result = false;
            if (WinApi.Api.GetWindowPlacement(hwnd, ref windowPlacement))
            {
                windowPlacement.showCmd = WinApi.Api.ShowCmdEnum.RESTORE;
                result = WinApi.Api.SetWindowPlacement(hwnd, ref windowPlacement);
            }
            return result;
        }
        public static bool SetWindowState(int hwnd, WindowState states)
        {
            WinApi.Api.WindowPlacement windowPlacement = default(WinApi.Api.WindowPlacement);
            bool rt = false;
            if (WinApi.Api.GetWindowPlacement(hwnd, ref windowPlacement))
            {
                bool flag = false;
                switch (states)
                {
                    case WindowState.Normal:
                        if (windowPlacement.showCmd != WinApi.Api.ShowCmdEnum.SHOWNORMAL)
                        {
                            windowPlacement.showCmd = WinApi.Api.ShowCmdEnum.SHOWNORMAL;
                            flag = true;
                        }
                        break;
                    case WindowState.Minimized:
                        if (windowPlacement.showCmd != WinApi.Api.ShowCmdEnum.MINIMIZE)
                        {
                            windowPlacement.showCmd = WinApi.Api.ShowCmdEnum.MINIMIZE;
                            flag = true;
                        }
                        break;
                    case WindowState.Maximized:
                        if (windowPlacement.showCmd != WinApi.Api.ShowCmdEnum.SHOWMAXIMIZED)
                        {
                            windowPlacement.showCmd = WinApi.Api.ShowCmdEnum.SHOWMAXIMIZED;
                            flag = true;
                        }
                        break;
                }
                rt = (!flag || WinApi.Api.SetWindowPlacement(hwnd, ref windowPlacement));
            }
            return rt;
        }
        private static void AssertHwndNonZero(int hwnd)
        {
            Util.Assert(hwnd > 0);
        }
        public static void TraverseAllHwnd(Action<int> action, string className = null, string title = null)
        {
            WinApi.TraverseChildHwnd(0, action, true, className, title);
        }
        public static void TraverAllDesktopHwnd(Action<int> action, string className = null, string title = null,int pid = 0)
        {
            WinApi.TraverseChildHwnd(0, action, false, className, title,pid);
        }

        public static void TraverDesktopHwnd(Func<int, bool> func, string className = null, string title = null)
        {
            WinApi.TraverseChildHwnd(0, func, false, className, title);
        }

        public static void TraverseChildHwnd(int parent, Func<int, bool> func, bool traverseDescandent, string className = null, string title = null,int pid = 0)
        {
            for (int i = WinApi.Api.FindWindowEx(parent, 0, className, title); i > 0; i = WinApi.Api.FindWindowEx(parent, i, className, title))
            {
                var rt = true;
                if (i == 0 || pid == 0 || WinApi.GetWindowThreadProcessId(i) == pid)
                {
                    rt = func(i);
                }
                if (!rt)
                {
                    break;
                }
                if (traverseDescandent)
                {
                    WinApi.TraverseChildHwnd(i, func, traverseDescandent, className, title);
                }
            }
        }

        public static void TraverseChildHwnd(int parent, Action<int> act, bool traverseDescandent, string className = null, string title = null,int pid = 0)
        {
            for (int i = WinApi.Api.FindWindowEx(parent, 0, className, title); i > 0; i = WinApi.Api.FindWindowEx(parent, i, className, title))
            {
                if (i == 0 || pid == 0 || WinApi.GetWindowThreadProcessId(i) == pid)
                {
                    act(i);
                    if (traverseDescandent)
                    {
                        WinApi.TraverseChildHwnd(i, act, traverseDescandent, className, title);
                    }
                }
            }
        }

        public static void TraverseChildHwnd(int parent, Action<int, int> act, bool traverseDescandent, int level, string className = null, string title = null)
        {
            int i = WinApi.Api.FindWindowEx(parent, 0, className, title);
            level++;
            while (i > 0)
            {
                act(i, level);
                if (traverseDescandent)
                {
                    WinApi.TraverseChildHwnd(i, act, traverseDescandent, level, className, title);
                }
                i = WinApi.Api.FindWindowEx(parent, i, className, title);
            }
        }

        public static List<int> FindAllWindowByTitlePattern(string pattern)
        {
            List<int> rtlist = new List<int>();
            WinApi.TraverseAllHwnd(delegate(int hwnd)
            {
                string input;
                if (WinApi.GetText(new HwndInfo(hwnd, "FindAllWindowByTitlePattern"), out input) && Regex.IsMatch(input, pattern))
                {
                    rtlist.Add(hwnd);
                }
            }, null, null);
            return rtlist;
        }
        public static List<int> FindAllDesktopWindowWithClassName(string cname)
        {
            List<int> rtlist = new List<int>();
            WinApi.TraverAllDesktopHwnd((int hwnd) =>
            {
                rtlist.Add(hwnd);
            }, cname, null);
            return rtlist;
        }
        public static int FindFirstDesktopWindowByClassNameAndTitle(string cname, string title)
        {
            return WinApi.Api.FindWindowEx(0, 0, cname, title);
        }
        public static List<int> FindAllDesktopWindowByClassNameAndTitlePattern(string cname, string pattern = null, Action<int, string> action = null,int processId = 0)
        {
            List<int> rtlist = new List<int>();
            WinApi.TraverAllDesktopHwnd((int hwnd) =>
            {
                bool isMatch = true;
                string title = null;
                if (!string.IsNullOrEmpty(pattern))
                {
                    isMatch = (WinApi.GetText(new HwndInfo(hwnd, "FindAllDesktopWindowByClassNameAndTitlePattern"), out title) && Regex.IsMatch(title, pattern));
                    var abc = title;
                }
                if (isMatch)
                {
                    rtlist.Add(hwnd);
                    if (action != null)
                    {
                        action(hwnd, title);
                    }
                }
            }, cname, null,processId);
            return rtlist;
        }
        public static bool GetText(HwndInfo hwnd, out string title)
        {
            return WinApi.Editor.GetText(hwnd, out title);
        }
        public static string GetText(int hwnd)
        {
            string text;
            if (!WinApi.Editor.GetText(new HwndInfo(hwnd, "GetText"), out text))
            {
                text = null;
            }
            return text;
        }
        public static void PressDot()
        {
            WinApi.Api.keybd_event(110, 0, 0, 0);
            WinApi.Api.keybd_event(110, 0, 2, 0);
        }
        public static void PressBackSpace()
        {
            WinApi.Api.keybd_event(8, 0, 0, 0);
            WinApi.Api.keybd_event(8, 0, 2, 0);
        }
        public static void PressEnterKey()
        {
            WinApi.Api.keybd_event(13, 0, 0, 0);
            WinApi.Api.keybd_event(13, 0, 2, 0);
        }
        public static void PressF12()
        {
            WinApi.Api.keybd_event(123, 0, 0, 0);
            WinApi.Api.keybd_event(123, 0, 2, 0);
        }
        public static void PressCtrlV()
        {
            WinApi.Api.keybd_event(17, 0, 0, 0);
            WinApi.Api.keybd_event(86, 0, 0, 0);
            WinApi.Api.keybd_event(86, 0, 2, 0);
            WinApi.Api.keybd_event(17, 0, 2, 0);
        }
        public static void PressEnter()
        {
            WinApi.Api.keybd_event(17, 0, 0, 0);
            WinApi.Api.keybd_event(13, 0, 0, 0);
            WinApi.Api.keybd_event(13, 0, 2, 0);
            WinApi.Api.keybd_event(17, 0, 2, 0);
        }
        public static void PressEsc()
        {
            WinApi.Api.keybd_event(17, 0, 0, 0);
            WinApi.Api.keybd_event(192, 0, 0, 0);
            WinApi.Api.keybd_event(192, 0, 2, 0);
            WinApi.Api.keybd_event(17, 0, 2, 0);
        }
        public static void FocusWnd(int hWnd)
        {
            if (hWnd != 0)
            {
                WinApi.Api.PostMessage(hWnd, 256, 40, 0);
                WinApi.Api.PostMessage(hWnd, 257, 40, 0);
            }
        }
        public static bool FocusWnd(HwndInfo hwnd)
        {
            return WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwnd, "focus", 7, 0, 0, 2000);
        }
        public static bool PasteText(HwndInfo hwnd)
        {
            return WinApi.HwndTextReaderFromTalkerNoAi.MessageSender.SendMessage(hwnd, "Paste", 770, 0, 0, 2000);
        }
        public static int GetOwner(int hwnd)
        {
            return WinApi.Api.GetWindow(hwnd, WinApi.Api.GetWindowType.GW_OWNER);
        }
        public static int GetRootHwnd(int hwnd)
        {
            return WinApi.Api.GetAncestor(hwnd, 2);
        }
        public static int WindowFromPoint(System.Drawing.Point p)
        {
            return WinApi.Api.WindowFromPoint(new WinApi.Api.Point(p.X, p.Y));
        }
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(int hwnd);
        public static Rectangle GetWindowRectangle(int hwnd)
        {
            Rectangle rectRlt;
            if (hwnd > 0)
            {
                WinApi.Api.Rect rect = default(WinApi.Api.Rect);
                WinApi.Api.GetWindowRect(hwnd, ref rect);
                rectRlt = new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
            }
            else
            {
                rectRlt = default(Rectangle);
            }
            return rectRlt;
        }

        public static void SetTransparentWindow(int hwnd)
        {
            WinApi.Api.SetWindowLong(hwnd, -20, (WinApi.Api.GetWindowLong(hwnd, -20) & -262145) | 128);
            WinApi.Api.SetWindowPos(hwnd, 0, 0, 0, 0, 0, 35u);
        }

        public static int GetFocusHwnd()
        {
            int hWnd = 0;
            WinApi.Api.GuiThreadInfo guiThreadInfo = default(WinApi.Api.GuiThreadInfo);
            guiThreadInfo.cbSize = Marshal.SizeOf(guiThreadInfo);
            if (WinApi.Api.GetGUIThreadInfo(0, ref guiThreadInfo))
            {
                hWnd = guiThreadInfo.hwndFocus;
            }
            return hWnd;
        }

        [DllImport("kernel32.dll")]
        private static extern int Process32First(IntPtr hSnapshot, ref ProcessEntry32 lppe);

        [DllImport("kernel32.dll")]
        private static extern int Process32Next(IntPtr hSnapshot, ref ProcessEntry32 lppe);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hSnapshot);


        private const uint TH32CS_SNAPPROCESS = 0x00000002;

        [StructLayout(LayoutKind.Sequential)]
        public struct ProcessEntry32
        {
            public int dwSize;
            public int cntUsage;
            public int th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public int th32ModuleID;
            public int cntThreads;
            public int th32ParentProcessID;
            public int pcPriClassBase;
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        }

        public static List<ProcessEntry32> EnumProcesses()
        {
            var ps = new List<ProcessEntry32>();
            IntPtr handle = IntPtr.Zero;
            try
            {
                // Create snapshot of the processes
                handle = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
                ProcessEntry32 pe32 = new ProcessEntry32();
                pe32.dwSize = System.Runtime.InteropServices.
                              Marshal.SizeOf(typeof(ProcessEntry32));

                // Get the first process
                int first = Process32First(handle, ref pe32);
                // If we failed to get the first process, throw an exception
                if (first == 0)
                    throw new Exception("Cannot find first process.");

                // While there's another process, retrieve it
                do
                {
                    IntPtr temp = Marshal.AllocHGlobal((int)pe32.dwSize);
                    Marshal.StructureToPtr(pe32, temp, true);
                    ProcessEntry32 pe = (ProcessEntry32)Marshal.PtrToStructure(temp, typeof(ProcessEntry32));
                    Marshal.FreeHGlobal(temp);
                    ps.Add(pe);
                }
                while (Process32Next(handle, ref pe32) != 0);
            }
            catch
            {
                throw;
            }
            finally
            {
                // Release handle of the snapshot
                CloseHandle(handle);
                handle = IntPtr.Zero;
            }
            return ps;
        }

        public static T GetTopWindow<T>() where T : Window
        {
            var rt = default(T);
            try
            {
                var winds = Application.Current.Windows.OfType<T>();
                if (!winds.xIsNullOrEmpty())
                {
                    var hdict = new Dictionary<int, T>();
                    foreach (T wind in winds)
                    {
                        var hWndSrc = (HwndSource)PresentationSource.FromVisual(wind);
                        int key = (hWndSrc == null) ? 0 : hWndSrc.Handle.ToInt32();
                        hdict[key] = wind;
                    }
                    WinApi.TraverDesktopHwnd(hWnd =>
                    {
                        if (hdict.ContainsKey(hWnd))
                        {
                            rt = hdict[hWnd];
                        }
                        return rt == null;
                    });
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return rt;
        }

        public static Bitmap GetWindowCapture(IntPtr handle)
        {
            Bitmap result = null;
            try
            {
                DateTime now = DateTime.Now;
                IntPtr windowDC = WinApi.User32.GetWindowDC(handle);
                WinApi.User32.ShowWindow(windowDC, 1);
                WinApi.User32.RECT rECT = default(WinApi.User32.RECT);
                WinApi.User32.GetWindowRect(handle, ref rECT);
                int num = rECT.right - rECT.left;
                int num2 = rECT.bottom - rECT.top;
                IntPtr intptr_ = WinApi.GDI32.CreateCompatibleDC(windowDC);
                IntPtr intPtr = WinApi.GDI32.CreateCompatibleBitmap(windowDC, num, num2);
                IntPtr intptr_2 = WinApi.GDI32.SelectObject(intptr_, intPtr);
                WinApi.GDI32.BitBlt(intptr_, 0, 0, num, num2, windowDC, 0, 0, 1087111200);
                WinApi.GDI32.SelectObject(intptr_, intptr_2);
                WinApi.GDI32.DeleteDC(intptr_);
                WinApi.User32.ReleaseDC(handle, windowDC);
                result = Image.FromHbitmap(intPtr);
                WinApi.GDI32.DeleteObject(intPtr);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return result;
        }

        public static Bitmap GetWindowCapture(IntPtr srcHwnd, Rectangle rect)
        {
            Bitmap result = null;
            try
            {
                DateTime now = DateTime.Now;
                IntPtr windowDC = WinApi.User32.GetWindowDC(srcHwnd);
                WinApi.User32.ShowWindow(windowDC, 1);
                WinApi.User32.RECT rECT = default(WinApi.User32.RECT);
                WinApi.User32.GetWindowRect(srcHwnd, ref rECT);
                IntPtr intptr_ = WinApi.GDI32.CreateCompatibleDC(windowDC);
                IntPtr intPtr = WinApi.GDI32.CreateCompatibleBitmap(windowDC, rect.Width, rect.Height);
                IntPtr intptr_2 = WinApi.GDI32.SelectObject(intptr_, intPtr);
                WinApi.GDI32.BitBlt(intptr_, 0, 0, rect.Width, rect.Height, windowDC, rect.Left, rect.Top, 1087111200);
                WinApi.GDI32.SelectObject(intptr_, intptr_2);
                WinApi.GDI32.DeleteDC(intptr_);
                WinApi.User32.ReleaseDC(srcHwnd, windowDC);
                result = Image.FromHbitmap(intPtr);
                WinApi.GDI32.DeleteObject(intPtr);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return result;
        }

        public class User32
        {
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref WinApi.User32.RECT lpRect);

            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hdc);

            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
        }

        public class GDI32
        {
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, int dwRop);

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hdc);

            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);

            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

            public const int CAPTUREBLT = 1073741824;

            public const int SRCCOPY = 13369376;
        }

        public static int GetForegroundWindow()
        {
            return WinApi.Api.GetForegroundWindow();
        }
    }
}
