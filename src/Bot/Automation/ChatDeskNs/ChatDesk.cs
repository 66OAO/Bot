using BotLib.BaseClass;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BotLib.Extensions;
using BotLib;
using BotLib.Misc;
using Bot.Automation.ChatDeskNs.Automators;
using Bot.Automation.ChatDeskNs.Component;
using Bot.Automation.ChatDeskNs.InputSuggestion;
using System.Threading;
using Bot.AssistWindow;
using Bot.Automation.ChromeDebugger;
using DbEntity;
using Bot.ChromeNs;

namespace Bot.Automation.ChatDeskNs
{
    public class ChatDesk : Disposable
    {
        public class CloseBuyerInfo
        {
            public DateTime Time;
            public string Buyer;
            public bool IsGroupChat;
            public CloseBuyerInfo()
            {
            }
        }
        private class DescendantHwndInfo
        {
            private int Hwnd;
            private int RootHwnd;
            private DateTime CacheTime;
            private static ConcurrentDictionary<int, DescendantHwndInfo> _cache;
            public DescendantHwndInfo(int hwnd, int rootHwnd)
            {
                CacheTime = DateTime.Now;
                Hwnd = hwnd;
                RootHwnd = rootHwnd;
            }
            private static bool GetDescendantHwndFromCache(int buyerNameHwnd)
            {
                return _cache.ContainsKey(buyerNameHwnd)
                    && !_cache[buyerNameHwnd].CacheTime.xIsTimeElapseMoreThanMinute(1.0);
            }
            public static bool IsDescendantHwnd(int buyerNameHwnd, int deskHwnd)
            {
                bool isDescendant = false; ;
                if (deskHwnd == 0) return isDescendant;
                if (GetDescendantHwndFromCache(buyerNameHwnd))
                {
                    isDescendant = (_cache[buyerNameHwnd].RootHwnd == deskHwnd);
                }
                else
                {
                    int rootHwnd = WinApi.GetRootHwnd(buyerNameHwnd);
                    _cache[buyerNameHwnd] = new DescendantHwndInfo(buyerNameHwnd, rootHwnd);
                    isDescendant = (rootHwnd == deskHwnd);
                }
                return isDescendant;
            }
            static DescendantHwndInfo()
            {
                _cache = new ConcurrentDictionary<int, DescendantHwndInfo>();
            }
        }
        public class DialogEventArgs : EventArgs
        {
            public int DialogHwnd;
        }
        private bool _isBuyerRegionVisibleCache;
        private DateTime _isBuyerRegionVisibleCacheTime;
        public static ChatDesk LatestForegroundDesk;
        private Workbench _workbench;
        public readonly string Seller;
        private string _sellerMainNick;
        private DateTime _foregroundChangedTime;
        private bool _isSingleChatcloseButtonEnable;

        private DateTime _isSingleChatcloseButtonEnableCacheTime;
        public static HashSet<ChatDesk> DeskSet;

        //private TaskWindowBuyerNameReader _buyerReader;
        private BuyerInfoWindowNameReader _buyerReader;
        private DelayCaller _setOwnerDelayCaller;
        private WndAssist _assistWindow;
        private string _buyer;
        private DateTime _isGroupChatCheckTime;
        private bool? _isGroupChat;
        private DateTime _preLogGroupChatNullTime;
        private int _logGroupChatNullCount;
        private DateTime _getVisiblePercentCacheTime;
        private double _getVisiblePercentCache;
        private readonly int _processId;
        private readonly int _qnThreadId;
        private Rectangle _cacheRec;
        private List<System.Drawing.Point> _cachePoint;
        private Rectangle _restoreRect;
        private WindowState _winState;
        private bool _isForeground;
        private bool _isVisible;
        public DeskAutomator Automator;
        private WinEventHooker _winEventHooker;
        private NoReEnterTimer _timer;
        private object _synObj;
        private ChatDeskEventArgs _args;
        private DateTime _preCheckCloseBuyerTime;
        private string _preCheckBuyer;
        private int _sameBuyerCount;
        private DateTime preUpdateRectAndWindowStateIfNeedTime;
        private DateTime _preCheckForegroundWindowTime;
        private DateTime _preUpdateLocationTime;
        private bool _isEditorTextChanged;
        private bool _isLocationChanged;
        private bool _isAlive;
        private DeskSuggestion _sugguestion;
        public readonly HwndInfo EnterpriseHwnd;
        private bool _isDeskVisible;
        private bool _isEvShowFinished;
        private bool _isEvHideFinished;
        private IChatRecordChrome chatRecordChrome;

        private static ConcurrentDictionary<int, ChatDesk> _enterpriseFrontDeskDict;
        public bool IsEnterpriseDesk
        {
            get
            {
                return EnterpriseHwnd.Handle != 0;
            }
        }
        public int RootHwnd
        {
            get
            {
                return (EnterpriseHwnd.Handle == 0) ? Hwnd.Handle : EnterpriseHwnd.Handle;
            }
        }
        public event EventHandler<ChatDeskEventArgs> EvLostForeground;
        public event EventHandler<ChatDeskEventArgs> EvGetForeground;
        public event EventHandler<ChatDeskEventArgs> EvShow;
        public event EventHandler<ChatDeskEventArgs> EvHide;
        public event EventHandler<ChatDeskEventArgs> EvMaximize;
        public event EventHandler<ChatDeskEventArgs> EvMinimize;
        public event EventHandler<ChatDeskEventArgs> EvNormalize;
        public event EventHandler<ChatDeskEventArgs> EvMoved;
        public event EventHandler<ChatDeskEventArgs> EvResized;
        public event EventHandler<ChatDeskEventArgs> EvClosed;
        public event EventHandler<DialogEventArgs> EvDialogPopUp;
        public event EventHandler<BuyerChangedEventArgs> EvBuyerChanged;
        public event EventHandler<ChatDeskEventArgs> EvChromeConnected;
        public event EventHandler<ChatDeskEventArgs> EvChromeDetached;
        public event EventHandler<RecieveNewMessageEventArgs> EvRecieveNewMessage;
        public int AliappProcessId
        {
            get
            {
                return WinApi.EnumProcesses().First(k => k.th32ParentProcessID == _processId).th32ProcessID;
            }
        }
        public int ProcessId
        {
            get
            {
                return _processId;
            }
        }
        public string SellerMainNick
        {
            get
            {
                if (_sellerMainNick == null)
                {
                    _sellerMainNick = TbNickHelper.GetMainPart(Seller);
                }
                return _sellerMainNick;
            }
        }
        public HwndInfo Hwnd
        {
            get;
            private set;
        }
        public Rectangle Rect
        {
            get;
            private set;
        }
        public Rectangle RestoreRect
        {
            get
            {
                return _restoreRect;
            }
            set
            {
                if (!value.Equals(_restoreRect))
                {
                    Rect = WinApi.GetWindowRectangle(Hwnd.Handle); ;
                    Rectangle rectangle = _restoreRect;
                    _restoreRect = value;
                    if (!value.Location.Equals(rectangle.Location))
                    {
                        if (EvMoved != null)
                        {
                            EvMoved(this, _args);
                        }
                    }
                    if (!value.Size.Equals(rectangle.Size))
                    {
                        if (EvResized != null)
                        {
                            EvResized(this, _args);
                        }
                    }
                }
            }
        }
        public WindowState WinState
        {
            get
            {
                return _winState;
            }
            private set
            {
                if (value != _winState)
                {
                    Rect = WinApi.GetWindowRectangle(Hwnd.Handle);
                    _winState = value;
                    switch (_winState)
                    {
                        case WindowState.Normal:
                            {
                                if (EvNormalize != null)
                                {
                                    EvNormalize(this, _args);
                                }
                                break;
                            }
                        case WindowState.Minimized:
                            {
                                if (EvMinimize != null)
                                {
                                    EvMinimize(this, _args);
                                }
                                break;
                            }
                        case WindowState.Maximized:
                            {
                                if (EvMaximize != null)
                                {
                                    EvMaximize(this, _args);
                                }
                                break;
                            }
                    }
                }
            }
        }
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            private set
            {
                if (value != _isVisible)
                {
                    _isVisible = value;
                    if (_isVisible)
                    {
                        if (EvShow != null)
                        {
                            EvShow(this, _args);
                        }
                    }
                    else
                    {
                        if (EvHide != null)
                        {
                            EvHide(this, _args);
                        }
                    }
                }
            }
        }
        public bool IsForeground
        {
            get
            {
                return _isForeground;
            }
            private set
            {
                try
                {
                    if (value != _isForeground)
                    {
                        _foregroundChangedTime = DateTime.Now;
                        Util.WriteTrace("Foreground,Seller={0},visible={1}", new object[]
						{
							Seller,
							IsVisible
						});
                        _isForeground = value;
                        if (value)
                        {
                            LatestForegroundDesk = this;
                            if (_setOwnerDelayCaller != null)
                            {
                                _setOwnerDelayCaller.CallAfterDelay();
                            }
                            if (EvGetForeground != null)
                            {
                                EvGetForeground(this, _args);
                            }
                        }
                        else
                        {
                            if (EvLostForeground != null)
                            {
                                EvLostForeground(this, _args);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }
        public bool IsSingleChatCloseButtonVisible
        {
            get
            {
                return Automator.IsSingleChatCloseButtonVisible();
            }
        }
        public bool IsGroupChatCloseButtonEnable
        {
            get
            {
                return Automator.IsGroupChatCloseButtonEnable();
            }
        }
        public DeskEditor Editor
        {
            get;
            private set;
        }
        public string PreBuyer
        {
            get;
            private set;
        }
        public WndAssist AssistWindow
        {
            get
            {
                if (_assistWindow == null)
                {
                    _assistWindow = WndAssist.AssistBag.SingleOrDefault(wnd => wnd.Desk == this);
                }
                return _assistWindow;
            }
        }
        public string Buyer
        {
            get
            {
                return _buyer;
            }
            private set
            {
                if (_buyer != value)
                {
                    PreBuyer = _buyer;
                    _buyer = value;
                    if (EvBuyerChanged != null)
                    {
                        EvBuyerChanged(this, new BuyerChangedEventArgs(_buyer, PreBuyer));
                    }
                }
            }
        }
        public bool? IsGroupChat
        {
            get
            {
                if ((DateTime.Now - _isGroupChatCheckTime).TotalMilliseconds > 100.0)
                {
                    _isGroupChatCheckTime = DateTime.Now;
                    if (IsSingleChatCloseButtonVisible)
                    {
                        _isGroupChat = false;
                    }
                    else
                    {
                        if (Automator.IsControlVisible() && IsGroupChatCloseButtonEnable)
                        {
                            _isGroupChat = true;
                        }
                        else
                        {
                            _isGroupChat = null;
                        }
                    }
                }
                return _isGroupChat;
            }
        }
        public string BuyerMainNick
        {
            get
            {
                return TbNickHelper.GetMainPart(Buyer);
            }
        }
        public bool IsSingleChat
        {
            get
            {
                bool? isSingleChatCloseButtonEnable = Automator.IsSingleChatCloseButtonEnable();
                bool result;
                if (result = (isSingleChatCloseButtonEnable.GetValueOrDefault() & isSingleChatCloseButtonEnable.HasValue))
                {
                    isSingleChatCloseButtonEnable = Automator.IsSingleChatEditorVisible;
                    if (!(isSingleChatCloseButtonEnable.GetValueOrDefault() & isSingleChatCloseButtonEnable.HasValue))
                    {
                        Log.Error("IsSingleChat的判断出错，IsSingleChatCloseButtonEnable==true,但是IsSingleChatEditorVisible!=true");
                        result = false;
                    }
                }
                return result;
            }
        }
        public bool HasSingleOrGroupChat
        {
            get
            {
                if (!IsSingleChat)
                {
                    bool? fresh = Automator.IsGroupEditorVisible;
                    return Automator.IsGroupEditorVisible ?? false;
                }
                else
                {
                    return true;
                }
            }
        }
        public bool IsAlive
        {
            get
            {
                if (_isAlive)
                {
                    _isAlive = Automator.IsAlive(true);
                    if (!_isAlive)
                    {
                        base.Dispose();
                    }
                }
                return _isAlive;
            }
        }
        public bool IsMinimized
        {
            get
            {
                return WinApi.IsWindowMinimized(Hwnd.Handle);
            }
        }
        public bool IsMaximized
        {
            get
            {
                return WinApi.IsWindowMaximized(Hwnd.Handle);
            }
        }

        public bool IsChatRecordChromeOk
        {
            get
            {
                return chatRecordChrome != null ? chatRecordChrome.IsChromeOk : false;
            }
        }

        static ChatDesk()
        {
            DeskSet = new HashSet<ChatDesk>();
        }

        private ChatDesk(LoginedSeller loginedSeller, string seller)
        {
            _isBuyerRegionVisibleCacheTime = DateTime.MinValue;
            _foregroundChangedTime = DateTime.MaxValue;
            _isSingleChatcloseButtonEnableCacheTime = DateTime.MinValue;
            _assistWindow = null;
            _isGroupChatCheckTime = DateTime.MinValue;
            _isGroupChat = null;
            _isEvShowFinished = true;
            _isEvHideFinished = true;
            _preLogGroupChatNullTime = DateTime.MinValue;
            _logGroupChatNullCount = 0;
            _getVisiblePercentCacheTime = DateTime.MinValue;
            _getVisiblePercentCache = 0.0;
            _isForeground = false;
            _isVisible = true;
            _synObj = new object();
            _preCheckCloseBuyerTime = DateTime.MinValue;
            _sameBuyerCount = 0;
            preUpdateRectAndWindowStateIfNeedTime = DateTime.MinValue;
            _preCheckForegroundWindowTime = DateTime.MinValue;
            _preUpdateLocationTime = DateTime.MinValue;
            _isEditorTextChanged = false;
            _isLocationChanged = false;
            _isAlive = true;
            Log.Info("Begin Create ChatDesk,seller=" + seller);
            Hwnd = new HwndInfo(loginedSeller.SellerHwnd, "ChatDesk");
            Seller = seller;
            _args = new ChatDeskEventArgs
            {
                Desk = this
            };
            Automator = ChatDeskAutomatorFactory.Create(Hwnd, Seller);
            chatRecordChrome = ChromeDebugerCreator.Create(this);
            chatRecordChrome.EvChromeConnected += chatRecordChrome_EvChromeConnected;
            chatRecordChrome.EvChromeDetached += chatRecordChrome_EvChromeDetached;
            chatRecordChrome.EvRecieveNewMessage += chatRecordChrome_EvRecieveNewMessage;
            chatRecordChrome.EvBuyerSwitched += Contact_EvBuyerSwitched;

            //_buyerReader = new TaskWindowBuyerNameReader(this);
            //_buyerReader.EvBuyerChanged += Contact_EvBuyerChanged;

            _buyerReader = new BuyerInfoWindowNameReader(this);
            _buyerReader.EvBuyerSwitched += Contact_EvBuyerSwitched;

            Editor = new DeskEditor(Automator.SingleChatEditorHwnd, this);
            _qnThreadId = WinApi.GetWindowThreadProcessId(loginedSeller.SellerHwnd, out _processId);

            EnterpriseHwnd = new HwndInfo(loginedSeller.EpHwnd, "EnterpriseRootDesk");
            _winEventHooker = new WinEventHooker(_qnThreadId, loginedSeller.SellerHwnd);
            _winEventHooker.EvFocused += WinEventHooker_EvFocused;
            _winEventHooker.EvLocationChanged += WinEventHooker_EvLocationChanged;
            _winEventHooker.EvTextChanged += WinEventHooker_EvTextChanged;
            WinEventHooker.EvForegroundChanged += WinEventHooker_EvForegroundChanged;
            IsForeground = WinApi.IsForeground(Hwnd.Handle);
            IsVisible = WinApi.IsVisible(Hwnd.Handle);
            UpdateRectAndWindowState();
            _timer = new NoReEnterTimer(Loop, 100, 0);
            _sugguestion = new DeskSuggestion(this);
            DeskSet = new HashSet<ChatDesk>(DeskSet)
			{
				this
			};
            Log.Info("ChatDesk Created.seller=" + seller);
        }

        public bool GetBuyerRegionVisibleUseCache(bool noCache)
        {
            if (!noCache || (DateTime.Now - _isBuyerRegionVisibleCacheTime).TotalMilliseconds > 10.0)
            {
                _isBuyerRegionVisibleCache = IsBoundaryVisable(Automator.GetBuyerNameRegion());
                _isBuyerRegionVisibleCacheTime = DateTime.Now;
            }
            return _isBuyerRegionVisibleCache;
        }

        public void DelayFocusEditor(int delayMs = 200)
        {
            DelayCaller.CallAfterDelay(new Action(FocusEditor), delayMs, false);
        }

        public async void ShowDeskFromWorkBenchWhenDeskUIDisabled()
        {
            try
            {
                bool isWbAlive = _workbench != null && _workbench.IsAlive;
                if (!isWbAlive)
                {
                    _workbench = null;
                }
                if (_workbench == null)
                {
                    Workbench workbench = await Task.Factory.StartNew<Workbench>(GetWorkbench, TaskCreationOptions.LongRunning);
                    _workbench = workbench;
                    workbench = null;
                }
                if (_workbench != null)
                {
                    if (_workbench.IsAlreadyExist)
                    {
                        WaitOpenChatDesk(_workbench);
                    }
                    else
                    {
                        _workbench.HideWorkbench();
                        WaitOpenChatDesk(_workbench);
                        _workbench.CloseWorkbench();
                    }
                }
            }
            catch (Exception exp)
            {
                Log.Exception(exp);
            }
        }

        private void WaitOpenChatDesk(Workbench workbench)
        {
            for (int i = 0; i < 3; i++)
            {
                workbench.OpenChatDesk();
                Util.WaitFor(new Func<bool>(() => { return Automator.IsControlVisible(); }), 1000, 100, false);
                if (Automator.IsControlVisible()) break;
            }
        }

        public async void NavWithWorkbenchAsync(string url)
        {
            if (_workbench == null || !_workbench.IsAlive)
            {
                Workbench workbench = await Task.Factory.StartNew<Workbench>(() => GetWorkbench(), TaskCreationOptions.LongRunning);
                _workbench = workbench;
                workbench = null;
            }
            bool isok = false;
            if (_workbench != null)
            {
                isok = await Task.Factory.StartNew<bool>(() => _workbench.Nav(url, null));
            }
            if (!isok)
            {
                Util.Nav(url);
            }
            else
            {
                if (_workbench != null)
                {
                    _workbench.BringTop();
                }
            }
        }

        private Workbench GetWorkbench()
        {
            Workbench workbench = GetWorkbenchFromTitle(Seller);
            if (workbench != null)
            {
                workbench.IsAlreadyExist = true;
            }
            int cnt = 0;
            Func<bool> func = null; ;
            while (cnt < 3 && workbench == null)
            {
                Automator.OpenWorkbench();
                Func<bool> pred;
                if ((pred = func) == null)
                {
                    pred = (func = () =>
                    {
                        workbench = GetWorkbenchFromTitle(Seller);
                        return workbench != null;
                    });
                }
                Util.WaitFor(pred, 1000, 100, true);
                cnt++;
            }
            return workbench;
        }

        private Workbench GetWorkbenchFromTitle(string seller)
        {
            Workbench workbench = null;
            int hWnd = GetHwndFromWorkbenchTitle(seller);
            if (hWnd != 0)
            {
                workbench = new Workbench(hWnd, seller);
            }
            return workbench;
        }

        private int GetHwndFromWorkbenchTitle(string seller)
        {
            string title = string.Format("{0} - 工作台", seller);
            return WinApi.FindFirstDesktopWindowByClassNameAndTitle("StandardFrame", title);
        }

        private static ChatDesk GetTopDesk()
        {
            ChatDesk result = null;
            if (!DeskSet.xIsNullOrEmpty())
            {
                HashSet<int> hlist = new HashSet<int>();
                int hWnd = WinApi.GetZOrderHighestHwnd(hlist);
                if (hWnd > 0)
                {
                    result = DeskSet.Single(k => k.Hwnd.Handle == hWnd);
                }
            }
            return result;
        }

        public void Hide()
        {
            if (!WinApi.HideDeskWindow(Hwnd.Handle))
            {
            }
        }

        public static ChatDesk GetDeskFromCache(string seller)
        {
            return DeskSet.SingleOrDefault((desk) => desk.Seller == seller);
        }

        public void SetLocation(int left, int top)
        {
            Rect = new Rectangle(left, top, Rect.Width, Rect.Height);
            WinApi.SetLocation(Hwnd.Handle, left, top);
        }

        public void Show()
        {
            if (WinApi.CancelHideDeskWindow(Hwnd.Handle))
            {
                RestoreIfMinimized();
                IsVisible = true;
                if (IsAlive && !Automator.IsControlVisible())
                {
                    ShowDeskFromWorkBenchWhenDeskUIDisabled();
                }
            }
        }

        public void CloseCurrentBuyer()
        {
            var _prebuyer = Buyer;
            if (IsGroupChat.HasValue)
            {
                if (IsGroupChat.HasValue && IsGroupChat.Value)
                {
                    Automator.ClickGroupChatCloseBuyerButton();
                }
                else
                {
                    Automator.ClickSingleChatCloseBuyerButton();
                }
            }
            DelayCaller.CallAfterDelay(() =>
            {
                if (Buyer == _prebuyer && (Automator.IsGroupChatCloseButtonEnable() || Automator.IsSingleChatCloseButtonEnable()))
                {
                    CloseCurrentBuyer();
                }
            }, 500, false);
        }

        public bool HasBuyerButCantGetName()
        {
            bool isGroupChat;
            if (Buyer.xIsNullOrEmptyOrSpace())
            {
                isGroupChat = (IsGroupChat.HasValue && IsGroupChat.Value);
            }
            else
            {
                isGroupChat = false;
            }
            return isGroupChat;
        }

        public void RestoreIfMinimized()
        {
            if (IsMinimized)
            {
                WinApi.ShowNormal(Hwnd.Handle);
            }
        }

        public bool GetIsSingleChatCloseButtonEnable(bool useCache)
        {
            if (!useCache || (DateTime.Now - _isSingleChatcloseButtonEnableCacheTime).TotalMilliseconds > 10.0)
            {
                _isSingleChatcloseButtonEnable = Automator.IsSingleChatCloseButtonEnable();
                _isSingleChatcloseButtonEnableCacheTime = DateTime.Now;
            }
            return _isSingleChatcloseButtonEnable;
        }

        private void WinEventHooker_EvForegroundChanged(object sender, WinEventHooker.WinEventHookEventArgs e)
        {
            IsForeground = (e.Hwnd == Hwnd.Handle);
            if (!IsForeground && e.ThreadId == _qnThreadId)
            {
                var wndClassName = WinApi.GetWindowClass(e.Hwnd);
                if (wndClassName == "#32770")
                {
                    if (EvDialogPopUp != null)
                    {
                        EvDialogPopUp(this, new DialogEventArgs
                        {
                            DialogHwnd = e.Hwnd
                        });
                    }
                }
            }
        }

        private void WinEventHooker_EvTextChanged(object sender, WinEventHooker.WinEventHookEventArgs e)
        {
            Editor.ClearCachedText();
        }

        private void WinEventHooker_EvLocationChanged(object sender, WinEventHooker.WinEventHookEventArgs e)
        {
            lock (_synObj)
            {
                _isLocationChanged = true;
            }
        }

        private void WinEventHooker_EvFocused(object sender, WinEventHooker.WinEventHookEventArgs e)
        {
            if (!IsForeground && DescendantHwndInfo.IsDescendantHwnd(e.Hwnd, Hwnd.Handle))
            {
                IsForeground = true;
            }
        }

        void Contact_EvBuyerSwitched(object sender, BuyerSwitchedEventArgs e)
        {
            this.Buyer = e.CurBuyer;
            if (!e.FromTaskWindow)
            {
                this._buyerReader.IsFrozenDetected = false;
            }
        }

        private static void ClearDeskFromEnterpriseFrontDeskFromCache(ChatDesk desk)
        {
            try
            {
                if (desk != null)
                {
                    int hWnd = 0;
                    foreach (var kv in _enterpriseFrontDeskDict)
                    {
                        if (kv.Value == desk)
                        {
                            hWnd = kv.Key;
                            break;
                        }
                    }
                    if (hWnd > 0)
                    {
                        _enterpriseFrontDeskDict.xTryRemove(hWnd);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        private static void AddEnterpriseFrontDeskToCache(int hWnd, ChatDesk epDesk)
        {
            if (hWnd != 0 && epDesk != null)
            {
                _enterpriseFrontDeskDict[hWnd] = epDesk;
            }
        }

        public void ShowNormal()
        {
            WinApi.ShowNormal(Hwnd.Handle);
        }
        public static ChatDesk Create(LoginedSeller loginedSeller, string seller, out string errdesc)
        {
            ChatDesk desk = null;
            errdesc = null;
            try
            {
                desk = new ChatDesk(loginedSeller, seller);
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("创建desk失败,hwnd={0},seller={1},err={2}", loginedSeller.SellerHwnd, seller, errdesc));
                errdesc = ex.Message;
            }
            return desk;
        }
        public void SetRect(int left, int top, int w, int h)
        {
            Rect = new Rectangle(left, top, w, h);
            WinApi.SetRect(Hwnd.Handle, left, top, w, h);
        }
        public bool IsForegroundOrVisibleMoreThanHalf(bool useCache)
        {
            return IsForeground || IsMostlyVisible(useCache);
        }
        public bool IsMostlyVisible(bool useCache = true)
        {
            return GetVisiblePercent(useCache) > 0.7;
        }
        public double GetVisiblePercent(bool useCache)
        {
            double visiblePercent;
            if (!IsVisible || WinState == WindowState.Minimized)
            {
                visiblePercent = 0.0;
            }
            else
            {
                if (!useCache || (DateTime.Now - _getVisiblePercentCacheTime).TotalMilliseconds > 10.0)
                {
                    _getVisiblePercentCache = GetVisiblePercent();
                    _getVisiblePercentCacheTime = DateTime.Now;
                }
                visiblePercent = _getVisiblePercentCache;
            }
            return visiblePercent;
        }

        private double GetVisiblePercent()
        {
            var segPts = GetDeskSegPoints();
            int visiblePtCount = segPts.Count(pt => DescendantHwndInfo.IsDescendantHwnd(WinApi.WindowFromPoint(pt), Hwnd.Handle));
            return (double)visiblePtCount / (double)segPts.Count;
        }

        public bool IsBoundaryVisable()
        {
            return IsBoundaryVisable(Rect);
        }

        public bool IsBoundaryVisable(Rectangle rect)
        {
            bool isVisable;
            if (IsMinimized)
            {
                isVisable = false;
            }
            else
            {
                var pts = new List<System.Drawing.Point>();
                pts.Add(new System.Drawing.Point(rect.Left + 1, rect.Top + 1));
                pts.Add(new System.Drawing.Point(rect.Right - 2, rect.Top + 1));
                pts.Add(new System.Drawing.Point(rect.Left + 1, rect.Bottom - 2));
                pts.Add(new System.Drawing.Point(rect.Right - 2, rect.Bottom - 2));
                for (int i = 0; i < pts.Count; i++)
                {
                    int buyerNameHwnd = WinApi.WindowFromPoint(pts[i]);
                    if (!DescendantHwndInfo.IsDescendantHwnd(buyerNameHwnd, Hwnd.Handle))
                    {
                        isVisable = false;
                        return isVisable;
                    }
                }
                isVisable = true;
            }
            return isVisable;
        }

        public bool CheckAlive()
        {
            return IsAlive;
        }
        private List<System.Drawing.Point> GetDeskSegPoints()
        {
            var pts = new List<System.Drawing.Point>();
            if (_cachePoint != null && _cacheRec.Equals(Rect))
            {
                pts = _cachePoint;
            }
            else
            {
                _cacheRec = Rect;
                int space = 5;
                int x = (int)((_cacheRec.Width * 0.6 - 10) / 3.0);
                int y = (_cacheRec.Height - 10) / 3;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        pts.Add(new System.Drawing.Point
                        {
                            X = space + i * x + _cacheRec.Left,
                            Y = space + j * y + _cacheRec.Top
                        });
                    }
                }
                _cachePoint = pts;
            }
            return pts;
        }
        public void BringTop()
        {
            WinApi.BringTop(Hwnd.Handle);
        }
        public bool BringTopForMs(int ms = 1000)
        {
            bool rt = false;
            DateTime now = DateTime.Now;
            while (!rt && now.xElapse().TotalMilliseconds < (double)ms)
            {
                if (WinApi.BringTop(this.RootHwnd))
                {
                    rt = true;
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
            return rt;
        }
        public void FocusEditor(bool bringttop)
        {
            Editor.FocusEditor(bringttop);
        }
        public void MovedRelative(int leftDiff, int topDiff)
        {
            RestoreIfMinimized();
            UpdateRectAndWindowState();
            SetLocation(Rect.Left + leftDiff, Rect.Top + topDiff);
        }
        public void UpdateRectAndWindowState()
        {
            WinApi.WindowPlacement windowPlacement = WinApi.GetWindowPlacement(Hwnd.Handle);
            if (windowPlacement != null)
            {
                WinState = windowPlacement.WinState;
                Rect = ((WinState == WindowState.Maximized) ? WinApi.GetWindowRectangle(Hwnd.Handle) : windowPlacement.RestoreWindow);
            }
            preUpdateRectAndWindowStateIfNeedTime = DateTime.Now;
        }
        private void Loop()
        {
            try
            {
                ListenDeskVisibleChanged();
                ListenDeskRectAndWindowStateChanged();
                ListenDeskLocatoinChanged();
                Editor.GetPlainText();
                ListenDeskForegroundChanged();
                CheckCloseBuyer();
            }
            catch (Exception ex)
            {
                Log.Error("ChatDesk Loop Exception=" + ex.Message);
                Log.Exception(ex);
            }
        }
        private void ListenDeskVisibleChanged()
        {
            if (_foregroundChangedTime.xIsTimeElapseMoreThanMs(50))
            {
                _foregroundChangedTime = DateTime.MaxValue;
                IsVisible = WinApi.IsVisible(Hwnd.Handle);
            }
        }
        private void CheckCloseBuyer()
        {
            if (_preCheckCloseBuyerTime.xIsTimeElapseMoreThanSecond(1))
            {
                _preCheckCloseBuyerTime = DateTime.Now;
            }
        }
        private void ListenDeskRectAndWindowStateChanged()
        {
            if (preUpdateRectAndWindowStateIfNeedTime.xIsTimeElapseMoreThanSecond(3))
            {
                UpdateRectAndWindowState();
            }
        }
        private void ListenDeskForegroundChanged()
        {
            if (_preCheckForegroundWindowTime.xIsTimeElapseMoreThanMs(300))
            {
                _preCheckForegroundWindowTime = DateTime.Now;
                IsForeground = WinApi.IsForeground(Hwnd.Handle);
            }
        }
        private void ListenDeskLocatoinChanged()
        {
            if (_isLocationChanged || (DateTime.Now - _preUpdateLocationTime).TotalSeconds >= 2)
            {
                _isLocationChanged = false;
                _preUpdateLocationTime = DateTime.Now;
                WinApi.WindowPlacement windowPlacement = WinApi.GetWindowPlacement(Hwnd.Handle);
                if (windowPlacement != null)
                {
                    WinState = windowPlacement.WinState;
                    RestoreRect = windowPlacement.RestoreWindow;
                }
            }
        }
        protected override void CleanUp_UnManaged_Resources()
        {
            WinEventHooker.EvForegroundChanged -= WinEventHooker_EvForegroundChanged;
            if (_winEventHooker != null)
            {
                _winEventHooker.Dispose();
            }
            _winEventHooker = null;
        }
        protected override void CleanUp_Managed_Resources()
        {
            Log.Info(string.Format("ChatDesk disposed,seller={0}", Seller));
            _isAlive = false;
            if (_timer != null)
            {
                _timer.Dispose();
            }
            if (EvClosed != null)
            {
                EvClosed(this, new ChatDeskEventArgs
                {
                    Desk = this
                });
            }
            _buyerReader.Dispose();
            ClearDeskFromCache(this);
        }

        private static void ClearDeskFromCache(ChatDesk desk)
        {
            try
            {
                if (desk != null)
                {
                    ChatDesk delDesk = null;
                    foreach (var d in DeskSet)
                    {
                        if (d == desk)
                        {
                            delDesk = d;
                            break;
                        }
                    }
                    if (delDesk != null)
                    {
                        DeskSet.Remove(delDesk);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        private void FocusEditor()
        {
            if (LatestForegroundDesk == this && IsVisible && IsForegroundOrVisibleMoreThanHalf(true))
            {
                int loopNum = 0;
                while (loopNum < 20 && !IsForeground)
                {
                    WinApi.SetForegroundWindow(Hwnd.Handle);
                    Thread.Sleep(50);
                    loopNum++;
                }
                WinApi.SetFocus(Editor.GetActivedEditorHwnd());
            }
        }

        private void chatRecordChrome_EvChromeDetached(object sender, ChromeAdapterEventArgs e)
        {
            if (EvChromeDetached != null)
            {
                EvChromeDetached(sender, _args);
            }
        }
        private void chatRecordChrome_EvChromeConnected(object sender, ChromeAdapterEventArgs e)
        {
            if (EvChromeConnected != null)
            {
                EvChromeConnected(sender, _args);
            }
        }
        private void chatRecordChrome_EvRecieveNewMessage(object sender, RecieveNewMessageEventArgs e)
        {
            if (EvRecieveNewMessage != null)
            {
                EvRecieveNewMessage(this, e);
            }
        }
    }

}
