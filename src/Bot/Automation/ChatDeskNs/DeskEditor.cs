using BotLib;
using BotLib.Extensions;
using BotLib.Wpf.Extensions;
using Bot.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Bot.AssistWindow.Widget;
using Bot.AssistWindow;

namespace Bot.Automation.ChatDeskNs
{
    public class DeskEditor
    {
        private ChatDeskEventArgs _evtArgs;
        private string _cachedText;
        private DateTime _cacheTextTime;
        private DateTime _preCachedTime;
        private ChatDesk _desk;
        private DateTime _preSendPlainTextTime;
        private DateTime _ShowEditorErrorTipTime;
        private object _setPlainTextSynObj;
        private string _preSendText;
        private BitmapImage _preImg;
        private DateTime _preSetImgTime;
        private bool _isSetPlainTextAndImageBusy;
        private DateTime _preSendPlainTextAndImageTime;
        private string _preSendPlainTextAndImageText;
        private BitmapImage _preSendPlainTextAndImageImage;
        private object _sendPtaiSynobj;
        public DateTime LatestSetTextTime;
        public event EventHandler<ChatDeskEventArgs> EvEditorTextChanged;
        public string PlainTextCached
        {
            get
            {
                return this.GetPlainTextUseCached(true);
            }
        }
        public string LastSetPlainText
        {
            get;
            private set;
        }
        public DeskEditor(int editorHwnd, ChatDesk chatDesk)
        {
            this._cacheTextTime = DateTime.MinValue;
            this._preSendPlainTextTime = DateTime.MinValue;
            this._preSendText = null;
            this._preSendPlainTextAndImageTime = DateTime.Now;
            this._sendPtaiSynobj = new object();
            _setPlainTextSynObj = new object();
            this._desk = chatDesk;
            this._evtArgs = new ChatDeskEventArgs
            {
                Desk = chatDesk
            };
        }
        private void EditorTextChanged()
        {
            if (this._desk.AssistWindow != null)
            {
                if (EvEditorTextChanged != null)
                {
                    EvEditorTextChanged(this._desk, this._evtArgs);
                }
            }
        }

        public string GetPlainTextUnCached()
        {
            return this.GetPlainTextUseCached(false);
        }
        private string GetPlainTextUseCached(bool useCache)
        {
            if (!useCache || this._cachedText == null)
            {
                string cachedText = this._cachedText;
                this._cachedText = this.GetPlainTextInner();
                this._cacheTextTime = DateTime.Now;
                if (cachedText != this._cachedText)
                {
                    this.EditorTextChanged();
                }
            }
            return this._cachedText ?? "";
        }
        public void ClearCachedText()
        {
            this._cachedText = null;
        }
        private string GetPlainTextInner()
        {
            string result = "";
            HwndInfo hwndInfo = this.GetActivedEditorHwnd();
            if (hwndInfo.Handle != 0 && !WinApi.Editor.GetText(hwndInfo, out result))
            {
                Log.Error("无法从编辑器中获取文本！,hwnd=" + hwndInfo);
                this._desk.CheckAlive();
                result = "";
            }
            return result;
        }
        public void SetPlainTextAsync(string text, bool moveCaretToEnd = true, bool focusEditor = true, Action cb = null)
        {
            Task.Factory.StartNew(() =>
            {
                SetPlainText(text, moveCaretToEnd, focusEditor);
                if (cb != null) cb();
            });
        }
        public void SetPlainText()
        {
            for (int i = 0; i < 5; i++)
            {
                if (this.SetPlainText("", true))
                {
                    string value = this.GetPlainTextUnCached();
                    if (string.IsNullOrEmpty(value))
                    {
                        break;
                    }
                    Util.SleepWithDoEvent(50);
                }
                else
                {
                    Util.SleepWithDoEvent(50);
                }
            }
        }
        public bool SetPlainText(string txt, bool moveCaretToEnd = true, bool focusEditor = true)
        {
            if (string.IsNullOrEmpty(txt)) return true;
            var rt = false;
            lock (_setPlainTextSynObj)
            {
                HwndInfo hwndInfo = this.GetActivedEditorHwnd();
                if (hwndInfo.Handle > 0)
                {
                    if (!WinApi.Editor.SetText(hwndInfo, txt, moveCaretToEnd))
                    {
                        Log.Error("无法设置文本到编辑器");
                        this._desk.CheckAlive();
                    }
                    else
                    {
                        this.LastSetPlainText = txt;
                        this.GetPlainTextUnCached();
                        rt = true;

                        this.EnsureShowEmoji(txt, hwndInfo.Handle);
                        if (focusEditor)
                        {
                            this.FocusEditor(focusEditor);
                        }
                    }
                }
            }
            return rt;
        }

        public void FocusEditor()
        {
            this._desk.BringTopForMs(1000);
            var hwndInfo = this.GetActivedEditorHwnd();
            if (hwndInfo.Handle != 0)
            {
                WinApi.FocusWnd(hwndInfo);
            }
        }

        private void EnsureShowEmoji(string text, int hwnd)
        {
            if (text.Contains("/:"))
            {
                WinApi.Editor.MoveCaretToEnding(new HwndInfo(hwnd, "EnsureShowEmoji"));
                this.FocusEditor();
                WinApi.PressDot();
                if (this.PlainTextEndWithDot(1000))
                {
                    WinApi.PressBackSpace();
                }
            }
        }

        private bool PlainTextEndWithDot(int ms)
        {
            DateTime now = DateTime.Now;
            bool rt = false;
            while (!rt && (DateTime.Now - now).TotalMilliseconds < (double)ms)
            {
                rt = this.GetPlainTextUseCached(false).EndsWith(".");
                Thread.Sleep(20);
            }
            return rt;
        }

        public bool InsertRichTextToEditor(string rtf, string buyer)
        {
            var rt = false;
            try
            {
                HwndInfo hwndInfo = this.GetActivedEditorHwnd();
                if (hwndInfo.Handle > 0)
                {
                    WinApi.Editor.PasteRichText(hwndInfo, rtf);
                    rt = true;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
                rt = false;
            }
            return rt;
        }
        public void SendPlainTextAsync(string text, Action cb = null)
        {
            Task.Factory.StartNew(() =>
            {
                this.SendPlainText(text);
                if (cb != null) cb();
            });
        }
        public void SendPlainText(string text)
        {
            try
            {
                if ((DateTime.Now - this._preSendPlainTextTime).TotalSeconds >= 1.0 || !(text == this._preSendText))
                {
                    this._preSendPlainTextTime = DateTime.Now;
                    this._preSendText = text;
                    if (this.SetPlainText(text, false))
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            this.ClickSendButton();
                            Util.SleepWithDoEvent(50);
                            if (string.IsNullOrEmpty(this.GetPlainTextUnCached()))
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
        public void ClickSendButton()
        {
            bool? isGroupChat = this._desk.IsGroupChat;
            if (isGroupChat.HasValue)
            {
                if (isGroupChat.Value)
                {
                    this._desk.Automator.ClickGroupChatSendMessageButton();
                }
                else
                {
                    this._desk.Automator.ClickSingleChatSendMessageButton();
                }
            }
        }
        public void SetOrSendPlainText(string text, bool isSend)
        {
            if (isSend)
            {
                this.SendPlainText(text);
            }
            else
            {
                this.SetPlainTextAsync(text, true);
            }
        }

        private bool IsNewImage(BitmapImage image)
        {
            return image != null && image == this._preImg && this._preSetImgTime.xElapse().TotalSeconds < 1.0;
        }

        public bool SetPlainTextAndImage(string text, BitmapImage image, bool focusEditor = true)
        {
            bool rt = false;
            if (!this._isSetPlainTextAndImageBusy)
            {
                this._isSetPlainTextAndImageBusy = true;
                try
                {
                    if (!text.EndsWith("\r\n"))
                    {
                        text += "\r\n";
                    }
                    if (this.SetPlainText(text, false, focusEditor) && image != null
                        && !this.IsNewImage(image) && (rt = this.SendPlainImage(image)))
                    {
                        this._preImg = image;
                        this._preSetImgTime = DateTime.Now;
                    }
                }
                catch (Exception e)
                {
			        Log.Exception(e);
                }

                this._isSetPlainTextAndImageBusy = false;
            }
            return rt;
        }
        public bool SendPlainTextAndImage(string text, BitmapImage image)
        {
            bool rt = false;
            lock (_sendPtaiSynobj)
            {
                if ((DateTime.Now - this._preSendPlainTextAndImageTime).TotalSeconds < 1.1 
                    && this._preSendPlainTextAndImageText == text 
                    && this._preSendPlainTextAndImageImage == image)
                {
                    rt = false;
                }
                else
                {
                    this._preSendPlainTextAndImageTime = DateTime.Now;
                    this._preSendPlainTextAndImageText = text;
                    this._preSendPlainTextAndImageImage = image;
                    if (this.SetPlainTextAndImage(text, image))
                    {
                        this.ClickSendButton();
                        rt = true;
                    }
                    else
                    {
                        rt = false;
                    }
                }
            }
            return rt;
        }
        private bool SendPlainImage(BitmapImage img)
        {
            bool isok = false;
            DispatcherEx.xInvoke(() =>
            {
                HwndInfo hwndInfo = this.GetActivedEditorHwnd();
                if (hwndInfo.Handle > 0)
                {
                    this.FocusEditor(true);
                    WinApi.Editor.MoveCaretToEnding(hwndInfo);
                    var dict = ClipboardEx.Backup();
                    Clipboard.Clear();
                    Clipboard.SetImage(img);
                    string text = this.GetPlainTextUnCached();
                    WinApi.PressCtrlV();
                    DateTime now = DateTime.Now;
                    while ((DateTime.Now - now).TotalSeconds < 2.0)
                    {
                        string newText = this.GetPlainTextUnCached();
                        if (newText != text)
                        {
                            isok = true;
                        }
                        DispatcherEx.DoEvents();
                    }
                    Util.WriteTimeElapsed(now, "等待时间");
                    ClipboardEx.Restore(dict);
                    return;
                }
            });
            return isok;
        }
        public bool FocusEditor(bool focusEditor)
        {
            bool isok = false;
            DispatcherEx.xInvoke(() => 
            {
                if (focusEditor)
                {
                    this._desk.BringTop();
                }
                HwndInfo hwndInfo = this.GetActivedEditorHwnd();
                if (hwndInfo.Handle > 0)
                {
                    isok = WinApi.SetFocus(hwndInfo);
                }
            });
            return isok;
        }

        public void SendTextToSomebody(string buyer, string text)
        {
            if (!string.IsNullOrEmpty(buyer) && buyer != this._desk.Seller)
            {
                this.SendPlainTextToSomebody(buyer, text);
            }
        }
        public void SendTextToSomebodyAsync(string buyer, string text)
        {
            Task.Factory.StartNew(() =>
            {
                this.SendTextToSomebody(buyer, text);
            }, TaskCreationOptions.LongRunning);
        }
        private bool SendPlainTextToSomebody(string buyer, string text)
        {
            bool sendResult = false;
            string arguments = string.Format("aliim:sendmsg?uid=cntaobao{0}&touid=cntaobao{1}", this._desk.Seller, buyer);
            string wwcmdPath = QnHelper.WwcmdPath;
            ProcessStartInfo startInfo = new ProcessStartInfo(wwcmdPath, arguments);
            Process.Start(startInfo);
            if (!this._desk.IsVisible)
            {
                this._desk.Show();
                Util.WaitFor(new Func<bool>(() => this._desk.IsVisible), 3000, 10, false);
            }
            Thread.Sleep(500);
            Util.WaitFor(() => this._desk.Buyer == buyer || this._desk.HasBuyerButCantGetName(), 5000, 10, false);
            if (this._desk.Buyer == buyer)
            {
                this.SendPlainText(text);
                sendResult = true;
            }
            return sendResult;
        }
        public HwndInfo GetActivedEditorHwnd()
        {
            int groupChatEditorHwnd = 0;
            if (this._desk.IsGroupChat.HasValue)
            {
                if (this._desk.IsGroupChat.HasValue && this._desk.IsGroupChat.Value)
                {
                    groupChatEditorHwnd = this._desk.Automator.GroupChatEditorHwnd;
                }
                else
                {
                    groupChatEditorHwnd = this._desk.Automator.SingleChatEditorHwnd;
                }
            }
            return new HwndInfo(groupChatEditorHwnd, "DeskEditor");
        }
        public void GetPlainText()
        {
            if ((DateTime.Now - this._cacheTextTime).TotalMilliseconds > 500.0)
            {
                this.GetPlainTextUnCached();
            }
        }

        public HwndInfo GetChatEditorHwndInfo()
        {
            int chatEditorHwnd;
            if (_desk.IsGroupChat.GetValueOrDefault() & _desk.IsGroupChat.HasValue)
            {
                chatEditorHwnd = this._desk.Automator.GroupChatEditorHwnd;
            }
            else
            {
                chatEditorHwnd = this._desk.Automator.SingleChatEditorHwnd;
            }
            return new HwndInfo(chatEditorHwnd, "DeskEditor");
        }

        public bool IsEmptyForPlainCachedText(bool force = false)
        {
            string text = this.GetPlainCachedText(force);
            return string.IsNullOrEmpty((text != null) ? text.Trim() : null);
        }


        public string GetPlainCachedText(bool force = false)
        {
            string text;
            if (((this._cachedText == null | force) || this._preCachedTime.xIsTimeElapseMoreThanMs(50)) && this.GetPlainTextInner(out text))
            {
                if (text != this._cachedText)
                {
                    _cachedText = text;
                    _preCachedTime = DateTime.Now;
                    EditorTextChanged();
                }
                else
                {
                    this._preCachedTime = DateTime.Now;
                }
            }
            return this._cachedText ?? string.Empty;
        }

        private void HideEditorErrorTip()
        {
            if (this._ShowEditorErrorTipTime != DateTime.MinValue)
            {
                this._ShowEditorErrorTipTime = DateTime.MinValue;
                if (_desk.AssistWindow != null)
                {
                    if (_desk.AssistWindow.ctlBottomPanel.TheTipper != null)
                    {
                        _desk.AssistWindow.ctlBottomPanel.TheTipper.ShowTip(null);
                    }
                }
            }
        }

        private bool GetPlainTextInner(out string txt)
        {
            bool rt = false;
            txt = "";
            try
            {
                HwndInfo hwndInfo = this.GetChatEditorHwndInfo();
                if (hwndInfo.Handle != 0)
                {
                    string err;
                    if (WinApi.Editor.GetText(hwndInfo, out txt, out err, true))
                    {
                        rt = true;
                        this.HideEditorErrorTip();
                    }
                    else
                    {
                        txt = "";
                        Log.Error(string.Format("无法从编辑器中获取文本！,hwnd={0},err={1}", hwndInfo.Handle, err));
                        if (this._desk.HasSingleOrGroupChat)
                        {
                            this.ShowEditorErrorTipIfNeed("无法从千牛【回复框】中读取内容。请【重启】软件。");
                        }
                        else
                        {
                            this.HideEditorErrorTip();
                        }
                    }
                }
                else
                {
                    Log.Info("千牛回复框句柄为0,seller=" + this._desk.Seller);
                    this.ShowEditorErrorTipIfNeed("无法【检测到】千牛【回复框】。请【重启】软件。");
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return rt;
        }

        private void ShowEditorErrorTipIfNeed(string tip)
        {
            if (this._ShowEditorErrorTipTime.xIsTimeElapseMoreThanSecond(10))
            {
                try
                {
                    this._ShowEditorErrorTipTime = DateTime.Now;
                    WndAssist assistWindow = this._desk.AssistWindow;
                    if (assistWindow != null)
                    {
                        BottomPanel.Tipper theTipper = assistWindow.ctlBottomPanel.TheTipper;
                        if (theTipper != null)
                        {
                            theTipper.ShowTip(tip);
                        }
                    }
                    Log.Info(string.Format("ShowEditorError:IsGroupChat={0},IsSingleChat={1}", this._desk.IsGroupChat, this._desk.IsSingleChat));
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            }
        }

        public bool HasEmoji()
        {
            string text = this.GetPlainCachedText(false);
            return !string.IsNullOrEmpty(text) && text.Contains("/:");
        }


        public void ClearPlainText(int tryCnt = 3)
        {
            var n = 0;
            while (n < tryCnt && (!this.SetPlainText("", true, true) || this.HasText()))
            {
                Thread.Sleep(50);
                n++;
            }
        }

        private bool HasText()
        {
            var txt = this.GetPlainCachedText(true);
            return !string.IsNullOrEmpty(txt);
        }
    }

}
