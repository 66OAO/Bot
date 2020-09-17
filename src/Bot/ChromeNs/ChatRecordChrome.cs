using BotLib;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BotLib.Extensions;
using Bot.Automation.ChatDeskNs;
using Bot.Common;
using Bot.Automation.ChromeDebugger;

namespace Bot.ChromeNs
{
    public class ChatRecordChrome : ChromeConnector,IChatRecordChrome
    {
        private HashSet<string> _chatRecordChromeTitle;
        private DateTime _preListeningTime;
        public event EventHandler<BuyerSwitchedEventArgs> EvBuyerSwitched;
        public event EventHandler<BuyerEventArgs> EvBuyerClosed;
        public event EventHandler<RecieveNewMessageEventArgs> EvRecieveNewMessage;
        public event EventHandler<HtmlChangedEventArgs> EvHtmlUpdated;
        public string CachedHtml
        {
            get;
            set;
        }
        public string PreBuyer
        {
            get;
            private set;
        }
        public string CurBuyer
        {
            get;
            private set;
        }
        public ChatRecordChrome(ChatDesk desk)
            : base(desk.Hwnd.Handle, "ocr_" + desk.Seller)
        {
            this._chatRecordChromeTitle = new HashSet<string>
			{
				"当前聊天窗口",
				"IMKIT.CLIENT.QIANNIU",
				"聊天窗口",
				"imkit.qianniu",
                "千牛聊天消息"
			};
            this._preListeningTime = DateTime.Now;
            this.Timer.AddAction(FetchRecordLoop, 300, 300);
        }
        public bool GetHtml(out string html, int timeoutMs = 500)
        {
            html = "";
            bool result = false;
            if (this.WaitForChromeOk(timeoutMs))
            {
                result = true;
            }
            return result;
        }
        protected override void ClearStateValues()
        {
            base.ClearStateValues();
            this.CurBuyer = "";
            this.PreBuyer = "";
        }
        protected override ChromeOperator CreateChromeOperator(string chromeSessionInfoUrl)
        {
            ChromeOperator chromeOperator = new ChromeOperator(chromeSessionInfoUrl, this._chatRecordChromeTitle, true);
            chromeOperator.ClearChromeConsole();
            chromeOperator.EvalForMessageListen();
            chromeOperator.ListenChromeConsoleMessageAddedMessage(new Action<ConsoleMessage>(this.DealChromeConsoleMessage));
            return chromeOperator;
        }

        private void DealChromeConsoleMessage(ConsoleMessage consoleMessage)
        {
            try
            {
                string text = consoleMessage.Text.Trim();
                if (text.StartsWith("onConversationChange,"))
                {
                    string buyer = text.Substring("onConversationChange,".Length);
                    this.BuyerSwitched(buyer, null);
                }
                else if (text.StartsWith("onSendNewMsg,"))
                {
                    string buyer = text.Substring("onSendNewMsg,".Length);
                    if (string.IsNullOrEmpty(this.CurBuyer) || this.CurBuyer == buyer)
                    {
                    }
                }
                else if (text.StartsWith("onReceiveNewMsg,"))
                {
                    string buyer = text.Substring("onReceiveNewMsg,".Length);
                    if (string.IsNullOrEmpty(this.CurBuyer) || this.CurBuyer == buyer)
                    {
                    }
                    this.RecieveNewMessage(buyer);
                }
                else if (text.StartsWith("onConversationAdd,"))
                {
                    Util.WriteTrace("ChromeMessageConsumer" + text);
                }
                else if (text.StartsWith("onConversationClose,"))
                {
                    string buyer = text.Substring("onConversationClose,".Length);
                    if (EvBuyerClosed != null)
                    {
                        EvBuyerClosed(this, new BuyerEventArgs
                        {
                            Buyer = buyer
                        });
                    }
                }
                else if (text.StartsWith("onNetDisConnect,"))
                {
                    Log.WriteLine("ChromeMessageConsumer:" + text, new object[0]);
                }
                else if (text.StartsWith("onNetReConnectOK,"))
                {
                    Util.WriteTrace("ChromeMessageConsumer" + text);
                    Log.WriteLine("ChromeMessageConsumer:" + text, new object[0]);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
        private void RecieveNewMessage(string buyer)
        {
            if (this.EvRecieveNewMessage != null)
            {
                EvRecieveNewMessage(this, new RecieveNewMessageEventArgs
                {
                    Buyer = buyer,
                    Connector = this
                });
            }
        }
        private void FetchRecordLoop()
        {
            try
            {
                if (this.IsChromeOk)
                {
                    if ((DateTime.Now - this._preListeningTime).TotalSeconds > 5.0)
                    {
                        this._preListeningTime = DateTime.Now;
                        if (this.ChromOp != null)
                        {
                            ChromOp.EvalForMessageListen();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        private void BuyerSwitched(string preBuyer, string curBuyer = null)
        {
            if (preBuyer != curBuyer)
            {
                this.PreBuyer = (curBuyer ?? this.CurBuyer);
                this.CurBuyer = preBuyer;
                if (this.EvBuyerSwitched != null)
                {
                    EvBuyerSwitched(this, new BuyerSwitchedEventArgs
                    {
                        CurBuyer = preBuyer,
                        PreBuyer = this.PreBuyer,
                        Connector = this
                    });
                }
            }
        }


    }

}
