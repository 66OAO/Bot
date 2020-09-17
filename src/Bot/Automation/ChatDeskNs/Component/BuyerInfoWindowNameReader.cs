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
using System.Collections.Concurrent;
using Bot.ChromeNs;

namespace Bot.Automation.ChatDeskNs.Component
{
    public class BuyerInfoWindowNameReader : Disposable
    {
        private bool _isTest;
        public EventHandler<BuyerSwitchedEventArgs> EvBuyerSwitched;
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

        private DateTime? _cantGetInfoWindowTime;
        public ConcurrentDictionary<int, DateTime> _openedHwnd;
        private List<WinApi.WindowClue> _innerWindowClue;
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

        public BuyerInfoWindowNameReader(ChatDesk chatDesk)
        {
            _isTest = false;
            _isFrozenDetected = false;
            _preFetchTime = DateTime.MinValue;
            _getGuestNameImageB64CacheTime = DateTime.MinValue;
            _bmpB64_buyerNameDict = new Cache<string, string>(150, 0, null);
            _openedHwnd = new ConcurrentDictionary<int, DateTime>();
            _getTaskWindowSuccessOnce = false;
            _innerWindowClue = null;
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
                CloseOpenedInfoWindow();
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
        private void CloseOpenedInfoWindow()
        {
            try
            {
                foreach (var kv in this._openedHwnd)
                {
                    if (WinApi.IsHwndAlive(kv.Key))
                    {
                        CloseInfoWindow(kv.Key);
                    }
                    else
                    {
                        _openedHwnd.xTryRemove(kv.Key);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        private void CloseInfoWindow(int targeth)
        {
            try
            {
                WinApi.CloseWindow(targeth, 2000);
                if (!WinApi.IsHwndAlive(targeth))
                {
                    _openedHwnd.xTryRemove(targeth);
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
                    Util.WriteTrace("BuyerInfoWindowBuyerNameReader,buyer=" + guestName);
                }
                if (guestName != _desk.Buyer)
                {
                    if (EvBuyerSwitched != null)
                    {
                        //EvBuyerSwitched(this, new BuyerChangedEventArgs(guestName, _desk.Buyer));
                        EvBuyerSwitched(this, new BuyerSwitchedEventArgs
                        {
                            CurBuyer = guestName,
                            FromTaskWindow = true
                        });
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
                    buyerName = GetBuyerNameFromBuyerInfoWindow();
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
        public string GetBuyerNameFromBuyerInfoWindow()
        {
            string buyerName = "";
            if (!CanGetTaskWindow()) return buyerName;

            int buyerInfoHwnd = TryGetBuyerInfoWindow();
            if (buyerInfoHwnd > 0)
            {
                WinApi.HideDeskWindow(buyerInfoHwnd);
                buyerName = GetBuyerNameFromBuyerInfoWindow(buyerInfoHwnd);
                WinApi.CloseWindow(buyerInfoHwnd);
                _desk.DelayFocusEditor();
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
            if (string.IsNullOrEmpty(buyerName))
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
            return buyerName;
        }
        private bool CanGetTaskWindow()
        {
            bool rt;
            if (!(rt = (_continuousFailCount < 5)))
            {
                if (_cantGetInfoWindowTime == null)
                {
                    _cantGetInfoWindowTime = new DateTime?(DateTime.Now);
                }
                else if (_cantGetInfoWindowTime.Value.xIsTimeElapseMoreThanSecond(10))
                {
                    _continuousFailCount = 0;
                    _cantGetInfoWindowTime = null;
                    rt = true;
                }
            }
            return rt;
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

        private List<WinApi.WindowClue> GetInnerWindowClue()
        {
            if (_innerWindowClue == null)
            {
                _innerWindowClue = new List<WinApi.WindowClue>
				{
					new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, -1),
					new WinApi.WindowClue(WinApi.ClsNameEnum.StandardWindow, null, 1)
				};
            }
            return _innerWindowClue;
        }

        private string GetBuyerNameFromBuyerInfoWindow(int pHwnd)
        {
            string buyerName = string.Empty;
            for (int i = 0; i < 50; i++)
            {
                int buyerInfoHwnd = WinApi.FindDescendantHwnd(pHwnd, GetInnerWindowClue(), "GetBuyerNameFromBuyerInfoWindow");
                if (buyerInfoHwnd > 0)
                {
                    ClickMemberName(buyerInfoHwnd);
                    for (int j = 0; j < 10; j++)
                    {
                        Thread.Sleep(20);
                        int buyerNameEditorHwnd = WinApi.FindChildHwnd(buyerInfoHwnd, new WinApi.WindowClue(WinApi.ClsNameEnum.EditComponent, null, -1));
                        if (buyerNameEditorHwnd != 0)
                        {
                            buyerName = WinApi.GetText(buyerNameEditorHwnd);
                            break;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(buyerName))
                {
                    break;
                }
                Thread.Sleep(20);
            }
            return buyerName;
        }


        private void ClickMemberName(int hWnd)
        {
            if (hWnd != 0)
            {
                if (_desk.AssistWindow != null)
                {
                    WndAssist.XYRatio toLogicalRatioWithUIThread = _desk.AssistWindow.ToLogicalRatioWithUIThread;
                    int x = (int)(88.0 / toLogicalRatioWithUIThread.XRatio);
                    int y = (int)(54.0 / toLogicalRatioWithUIThread.YRatio);
                    WinApi.ClickPointBySendMessage(hWnd, x, y);
                }
                else
                {
                    Log.Info("AssistWindow==null，无法ClickMemberName,seller=" + _desk.Seller);
                }
            }
        }

        private int TryGetBuyerInfoWindow()
        {
            int hWnd = 0;
            var existInfoHwnds = GetExistInfoHwnds();
            ClickInfoWindow();

            int loopNum = 0;
            do
            {
                var existInfoHwnds2 = GetExistInfoHwnds();
                var exceptList = existInfoHwnds2.Except(existInfoHwnds).ToList();
                if (exceptList.Count > 0)
                {
                    hWnd = exceptList[0];
                }

                if (hWnd == 0 && existInfoHwnds.Count > 0 && existInfoHwnds.Count > 0)
                {
                    int topInfoHwnd = WinApi.GetForegroundWindow();
                    if (topInfoHwnd != 0 && existInfoHwnds.Contains(topInfoHwnd))
                    {
                        hWnd = topInfoHwnd;
                    }
                }
                if (hWnd == 0)
                {
                    Thread.Sleep(20);
                    loopNum++;
                }
            }
            while (hWnd == 0 && loopNum < 5);

            if (hWnd != 0)
            {
                _openedHwnd[hWnd] = DateTime.Now;
            }
            return hWnd;
        }

        private int GetTopTaskWindow(List<int> taskWinHwnds)
        {
            int hWnd = 0;
            List<int> newTaskWinHwnds = GetExistInfoHwnds();
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

        private void ClickInfoWindow()
        {
            try
            {
                if (_desk.AssistWindow != null)
                {
                    var toLogicalRatioWithUIThread = _desk.AssistWindow.ToLogicalRatioWithUIThread;
                    int x = (int)(268.0 / toLogicalRatioWithUIThread.XRatio);
                    int y = (int)(104.0 / toLogicalRatioWithUIThread.YRatio);
                    WinApi.ClickPointBySendMessage(_desk.Hwnd.Handle, x, y);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
        private List<int> GetExistInfoHwnds()
        {
            List<int> hwnds = new List<int>();
            WinApi.TraverAllDesktopHwnd(hWnd =>
            {
                if (IsBuyerInfoWindow(hWnd))
                {
                    hwnds.Add(hWnd);
                }
            }, "#32770", null, _desk.ProcessId);
            return hwnds;
        }

        public bool IsBuyerInfoWindow(int dialogHwnd)
        {
            bool rt = false;
            try
            {
                var txt = WinApi.GetText(dialogHwnd);
                if (!string.IsNullOrEmpty(txt))
                {
                    rt = Regex.IsMatch(txt, ".+的资料");
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return rt;
        }

        protected override void CleanUp_Managed_Resources()
        {
            _timer.Dispose();
        }
    }
}
