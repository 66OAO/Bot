using BotLib;
using BotLib.BaseClass;
using BotLib.Collection;
using BotLib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BotLib.Extensions;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using Bot.AssistWindow;

namespace Bot.Automation.ChatDeskNs.Component
{
    public class TaskWindowBuyerNameReader : Disposable
    {
        private bool _isTest;
        public EventHandler<BuyerChangedEventArgs> EvBuyerChanged;
        private NoReEnterTimer _timer;
        private Cache<string, HashSet<string>> _frozenCache;
        private bool _isFrozenDetected;
        private string _preGuestNameImageB64;
        private int _continuousFailCount;
        private ChatDesk _desk;
        private WndAssist _wndDontuse;
        private DateTime _preFetchTime;
        private string _guestName;
        private DateTime _getGuestNameImageB64CacheTime;
        private string _getImageB64StrCache;
        private DateTime _latestFailTime;
        private string _preB64;
        private Cache<string, string> _bmpB64_buyerNameDict;
        private bool _getTaskWindowSuccessOnce;
        private int _getTaskWindowFailCount;
        private string _taskBuyerPt;
        private List<WinApi.WindowClue> _taskWindowClueDontUse;
        private WndAssist Wnd
        {
            get
            {
                if (_wndDontuse == null)
                {
                    _wndDontuse = WndAssist.AssistBag.FirstOrDefault(k => k.Desk == _desk);
                }
                return _wndDontuse;
            }
        }
        private List<WinApi.WindowClue> TaskWindowClue
        {
            get
            {
                if (_taskWindowClueDontUse == null)
                {
                    _taskWindowClueDontUse = new List<WinApi.WindowClue>();
                    _taskWindowClueDontUse.Add(new WinApi.WindowClue(WinApi.ClsNameEnum.PrivateWebCtrl, null, -1));
                    _taskWindowClueDontUse.Add(new WinApi.WindowClue(WinApi.ClsNameEnum.Aef_WidgetWin_0, null, -1));
                }
                return _taskWindowClueDontUse;
            }
        }

        public bool IsFrozenDetected
        {
            get
            {
                return _isFrozenDetected;
            }
            set
            {
                _isFrozenDetected = value;
                if (!value && _isFrozenDetected)
                {
                    _frozenCache = new Cache<string, HashSet<string>>(50, 0, null);
                }
            }
        }

        public TaskWindowBuyerNameReader(ChatDesk chatDesk)
        {
            _isTest = false;
            _isFrozenDetected = false;
            _preFetchTime = DateTime.MinValue;
            _getGuestNameImageB64CacheTime = DateTime.MinValue;
            _bmpB64_buyerNameDict = new Cache<string, string>(150, 0, null);
            _getTaskWindowSuccessOnce = false;
            _getTaskWindowFailCount = 0;
            _taskBuyerPt = "(?<=^benchwebctrl:.*_bench_CQNLightTaskBiz_qtask_create_).*";
            _desk = chatDesk;
            _timer = new NoReEnterTimer(Loop, 500, 1000);
            _frozenCache = new Cache<string, HashSet<string>>(50, 0, null);
        }

        private void Loop()
        {
            try
            {
                if (IsFrozenDetected || (string.IsNullOrEmpty(_desk.Buyer) && _desk.GetIsSingleChatCloseButtonEnable(true)))
                {
                    BuyerChanged();
                }
                if (!IsFrozenDetected && _desk.IsVisible && _desk.GetIsSingleChatCloseButtonEnable(true) && _desk.GetBuyerRegionVisibleUseCache(true) && _desk.IsForegroundOrVisibleMoreThanHalf(true))
                {
                    DetectFrozen();
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
        private void DetectFrozen()
        {
            try
            {
                string item = GetImageB64StrNoCache(true);
                string buyer = _desk.Buyer;
                Cache<string, HashSet<string>> frozenCache = _frozenCache;
                HashSet<string> imageB64StrCache = new HashSet<string>();
                imageB64StrCache.Add(item);
                if (imageB64StrCache.Count > 4)
                {
                    Log.Info("BuyerNameFrozenDetector,Buyer name frozened.seller=" + _desk.Seller);
                    IsFrozenDetected = true;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
        private void BuyerChanged()
        {
            if (_desk.GetBuyerRegionVisibleUseCache(false)
                && (string.IsNullOrEmpty(_desk.Buyer) || HasGuestName())
                && _desk.IsForegroundOrVisibleMoreThanHalf(true) && Wnd != null)
            {
                string guestName = GetGuestName();
                if (_isTest)
                {
                    Util.WriteTrace("TaskWindowBuyerNameReader,buyer=" + guestName);
                }
                if (guestName != _desk.Buyer)
                {
                    if (EvBuyerChanged != null)
                    {
                        EvBuyerChanged(this, new BuyerChangedEventArgs(guestName, _desk.Buyer));
                    }
                }
            }
        }
        private bool HasGuestName()
        {
            string guestNameImageB64 = GetImageB64StrNoCache(true);
            var hasGuest = guestNameImageB64 != _preGuestNameImageB64;
            if (hasGuest) _preGuestNameImageB64 = guestNameImageB64;
            return hasGuest;
        }
        public string GetGuestName(int ms = 10)
        {
            if (_preFetchTime.xIsTimeElapseMoreThanMs(ms))
            {
                _guestName = GetBuyerNameUseCache();
            }
            return _guestName;
        }
        private string GetImageB64StrNoCache(bool noCache)
        {
            if (!noCache || (DateTime.Now - _getGuestNameImageB64CacheTime).TotalMilliseconds > 10.0)
            {
                Rectangle buyerNameRegion = _desk.Automator.GetBuyerNameRegion();
                _getImageB64StrCache = GetImageB64Str(buyerNameRegion);
                _getGuestNameImageB64CacheTime = DateTime.Now;
            }
            return _getImageB64StrCache;
        }
        public void SaveBuyerNameRegionImage()
        {
            Rectangle buyerNameRegion = _desk.Automator.GetBuyerNameRegion();
            Rectangle tmpRect = new Rectangle(buyerNameRegion.Left, buyerNameRegion.Top - 10, buyerNameRegion.Width, 40);
            SaveDrawBitmap(tmpRect);
        }

        private string GetImageB64Str(Rectangle buyerNameRegion)
        {
            string newB64 = "";
            try
            {
                Bitmap buyerNameRegionImage = DrawToBitmap(buyerNameRegion);
                newB64 = BitmapToBase64String(buyerNameRegionImage);
                if (_preB64 != newB64)
                {
                    _preB64 = newB64;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return newB64;
        }
        public void SaveDrawBitmap(Rectangle rect)
        {
            Bitmap bitmap = DrawToBitmap(rect);
            bitmap.Save(DateTime.Now.Ticks.ToString() + ".bmp", ImageFormat.Bmp);
        }
        private string BitmapToBase64String(Bitmap bitmap)
        {
            string b64Str = "";
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Png);
                b64Str = Convert.ToBase64String(memoryStream.ToArray());
            }
            return b64Str;
        }
        private Bitmap DrawToBitmap(Rectangle rect)
        {
            Bitmap bitmap = new Bitmap(rect.Width, rect.Height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(rect.Location, new Point(0, 0), rect.Size);
            }
            return bitmap;
        }
        private string GetBuyerNameUseCache()
        {
            string buyerName = "";
            Rectangle buyerNameRegion = _desk.Automator.GetBuyerNameRegion();
            if (!_desk.IsBoundaryVisable(buyerNameRegion))
            {
                _desk.BringTop();
            }
            if (_desk.IsBoundaryVisable(buyerNameRegion))
            {
                string buyerImageB64Str = GetImageB64StrNoCache(true);
                if (_bmpB64_buyerNameDict.ContainsKey(buyerImageB64Str))
                {
                    buyerName = _bmpB64_buyerNameDict[buyerImageB64Str];
                }
                else
                {
                    buyerName = GetBuyerNameFromTaskWindow();
                    if (!string.IsNullOrEmpty(buyerName))
                    {
                        string buyerImageB64StrNoCache = GetImageB64StrNoCache(false);
                        if (buyerImageB64StrNoCache == buyerImageB64Str)
                        {
                            _bmpB64_buyerNameDict[buyerImageB64Str] = buyerName;
                        }
                    }
                }
            }
            return buyerName;
        }
        public string GetBuyerNameFromTaskWindow()
        {
            string buyerName = "";
            if (!CanGetTaskWindow()) return buyerName;

            List<int> taskWinHwnds;
            int taskHwnd = TryGetTaskWindow(out taskWinHwnds);
            if (taskHwnd > 0)
            {
                buyerName = GetBuyerNameFromTaskWindow(taskHwnd);
                WinApi.CloseWindow(taskHwnd, 2000);
                _desk.DelayFocusEditor(200);
            }
            if (buyerName != null)
            {
                buyerName = buyerName.Trim();
                string endStr = "(阿里巴巴中国站)";
                if (buyerName.EndsWith(endStr))
                {
                    buyerName = buyerName.Substring(0, buyerName.Length - endStr.Length);
                }
            }
            return buyerName;
        }
        private bool CanGetTaskWindow()
        {
            return _getTaskWindowSuccessOnce || _getTaskWindowFailCount < 3;
        }
        private List<WinApi.WindowClue> GetTaskWindowClueDontUse()
        {
            if (_taskWindowClueDontUse == null)
            {
                _taskWindowClueDontUse = new List<WinApi.WindowClue>();
                _taskWindowClueDontUse.Add(new WinApi.WindowClue(WinApi.ClsNameEnum.PrivateWebCtrl, null, -1));
                _taskWindowClueDontUse.Add(new WinApi.WindowClue(WinApi.ClsNameEnum.Aef_WidgetWin_0, null, -1));
            }
            return _taskWindowClueDontUse;
        }
        private string GetBuyerNameFromTaskWindow(int pHwnd)
        {
            string buyerName = string.Empty;
            for (int i = 0; i < 4; i++)
            {
                int taskHwnd = WinApi.FindDescendantHwnd(pHwnd, GetTaskWindowClueDontUse(), "GetBuyerNameFromTaskWindow");
                if (taskHwnd > 0)
                {
                    int j = 0;
                    do
                    {
                        string text = WinApi.GetText(taskHwnd);
                        if (!string.IsNullOrEmpty(text))
                        {
                            Match match = Regex.Match(text, _taskBuyerPt);
                            if (match != null && match.Success)
                            {
                                buyerName = match.ToString().Trim();
                            }
                            return buyerName;
                        }
                        else
                        {
                            Util.WriteTrace("GetBuyerNameFromTaskWindow,empty");
                        }
                        j++;
                        Util.SleepWithDoEvent(100);
                    } while (j < 4);
                }
                Util.SleepWithDoEvent(100);
            }
            return buyerName;
        }
        private int TryGetTaskWindow(out List<int> taskWinHwnds)
        {
            int hWnd = 0;
            taskWinHwnds = GetExistTaskHwnds();
            if (taskWinHwnds.Count < 1)
            {
                ClickTask();
                int loopNum = 0;
                do
                {
                    hWnd = GetTopTaskWindow(taskWinHwnds);
                    if (hWnd == 0)
                    {
                        Thread.Sleep(20);
                        loopNum++;
                    }
                }
                while (hWnd == 0 && loopNum < 5);
                _desk.BringTop();
            }
            if (hWnd == 0)
            {
                _getTaskWindowFailCount++;
                if (_latestFailTime.xElapse().TotalMinutes < 3.0)
                {
                    _continuousFailCount++;
                }
                else
                {
                    _latestFailTime = DateTime.Now;
                    _continuousFailCount = 1;
                }
            }
            else
            {
                _getTaskWindowSuccessOnce = true;
                _getTaskWindowFailCount = 0;
            }
            return hWnd;
        }

        private int GetTopTaskWindow(List<int> taskWinHwnds)
        {
            int hWnd = 0;
            List<int> newTaskWinHwnds = GetExistTaskHwnds();
            if (newTaskWinHwnds.Count > 0)
            {
                foreach (int newHwnd in newTaskWinHwnds)
                {
                    if (!taskWinHwnds.Contains(newHwnd))
                    {
                        hWnd = newHwnd;
                        break;
                    }
                }
            }
            return hWnd;
        }

        private void ClickTask()
        {
            try
            {
                int toolbarPlusHwnd = _desk.Automator.ToolbarPlusHwnd;
                Rectangle windowRectangle = WinApi.GetWindowRectangle(toolbarPlusHwnd);
                WndAssist.XYRatio toLogicalRatioWithUIThread = Wnd.ToLogicalRatioWithUIThread;
                int xr = (int)(100.0 / toLogicalRatioWithUIThread.XRatio);
                int yr = (int)(15.0 / toLogicalRatioWithUIThread.YRatio);
                int x = windowRectangle.Width - xr;
                int y = windowRectangle.Height - yr;
                WinApi.ClickPointBySendMessage(toolbarPlusHwnd, x, y);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
        private List<int> GetExistTaskHwnds()
        {
            List<int> hwnds = new List<int>();
            WinApi.TraverAllDesktopHwnd(hWnd =>
            {
                if (IsTaskWindow(hWnd))
                {
                    hwnds.Add(hWnd);
                }
            }, "StandardFrame", string.Format("{0} - 添加任务",_desk.Seller));
            return hwnds;
        }
        private bool IsTaskWindow(int hwnd)
        {
            return WinApi.FindDescendantHwnd(hwnd, GetTaskWindowClueDontUse(), "IsTaskWindow") > 0;
        }
        protected override void CleanUp_Managed_Resources()
        {
            _timer.Dispose();
        }
    }
}
