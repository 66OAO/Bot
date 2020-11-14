using Bot.Automation.ChromeDebugger;
using BotLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotLib.Extensions;
using Bot.Automation.ChatDeskNs;
using BotLib.Misc;
using Bot.HttpSvr;
using Bot.ChromeNs;

namespace Bot.Automation.ChromeDebugger
{
    public class ChatRecordChromeV2: IChatRecordChrome
    {
        private string _cachedHtml;
        private DateTime _latestRecieveMsgTime;
        private DateTime _unloadTime;
        private bool _preIsConnected;
        private MyHttpServer _httpSvr;
        private ChatDesk _desk;
        private static object _connectSynObj;
        private static bool _isConnecting;
        private DateTime _preTryConnectTime;
        private DateTime _preOpenChromeDevTime;
        public readonly NoReEnterTimer Timer;
        public event EventHandler<BuyerSwitchedEventArgs> EvBuyerSwitched;
        public event EventHandler<RecieveNewMessageEventArgs> EvRecieveNewMessage;
        public event EventHandler<BuyerEventArgs> EvBuyerClosed;
        public event EventHandler<HtmlChangedEventArgs> EvHtmlUpdated;
        public event EventHandler<ChromeAdapterEventArgs> EvChromeDetached;
        public event EventHandler<ChromeAdapterEventArgs> EvChromeConnected;

        public string CachedHtml
        {
            get
            {
                return _cachedHtml;
            }
            private set
            {
                if (value != _cachedHtml)
                {
                    _cachedHtml = value;
                    Util.WritaTrace(" EvHtmlUpdated?.Invoke(this, new");
                    if (EvHtmlUpdated != null)
                    {
                        EvHtmlUpdated(this, new HtmlChangedEventArgs
                        {
                            Html = value
                        });
                    }
                }
            }
        }

        public string PreBuyer { get; private set; }

        public string CurBuyer { get; private set; }

        private bool IsConnected
        {
            get
            {
                bool isConnected;
                if ((isConnected = (_unloadTime < _latestRecieveMsgTime && _latestRecieveMsgTime.xElapse().TotalSeconds < 2.0)) != _preIsConnected)
                {
                    _preIsConnected = isConnected;
                    if (isConnected)
                    {
                        Log.Info("connected:" + _desk.Seller);

                        if (EvChromeConnected != null)
                        {
                            EvChromeConnected(this, new ChromeAdapterEventArgs());
                        }
                    }
                    else
                    {
                        Log.Info("detached:" + _desk.Seller);
                        if (EvChromeDetached != null)
                        {
                            EvChromeDetached(this, new ChromeAdapterEventArgs());
                        }
                    }
                }
                return isConnected;
            }
        }

        public bool IsChromeOk
        {
            get
            {
                return IsConnected;
            }
        }

        static ChatRecordChromeV2()
        {
            _connectSynObj = new object();
            _isConnecting = false;
        }

        public ChatRecordChromeV2(ChatDesk desk)
        {
            try
            {
                _latestRecieveMsgTime = DateTime.MinValue;
                _unloadTime = DateTime.MinValue;
                _preIsConnected = false;
                _preTryConnectTime = DateTime.MinValue;
                _preOpenChromeDevTime = DateTime.Now;
                _desk = desk;
                _httpSvr = MyHttpServer.HttpSvrInst;
                _httpSvr.Start();
                _httpSvr.OnRecieveMessage += httpSvr_OnRecieveMessage;
                Timer = new NoReEnterTimer(Loop, 1000, 500);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        private void httpSvr_OnRecieveMessage(object sender, RecieveMsgEventArgs e)
        {
            _latestRecieveMsgTime = DateTime.Now;
            try
            {
                var name = e.Name;
                if (name == "onConversationChange")
                {
                    if (e.Value != PreBuyer)
                    {
                        if (EvBuyerSwitched != null)
                        {
                            EvBuyerSwitched(this, new BuyerSwitchedEventArgs
                            {
                                CurBuyer = e.Value,
                                PreBuyer = PreBuyer
                            });
                        }
                        PreBuyer = e.Value;
                    }
                }
                else if (name == "onSendNewMsg")
                {
                }
                else if (name == "onReceiveNewMsg")
                {
                    if (EvRecieveNewMessage != null)
                    {
                        EvRecieveNewMessage(this, new RecieveNewMessageEventArgs
                        {
                            Buyer = e.Value
                        });
                    }
                }
                else if (name == "html")
                {
                    if (e.Value != CachedHtml)
                    {
                        CachedHtml = e.Value;
                    }
                }
                else if (name.StartsWith("onConversationAdd,"))
                {
                    Util.WriteTrace("ChromeMessageConsumer" + name);
                }
                else if (name == "onUnload")
                {
                    _unloadTime = DateTime.Now;
                }
                else if (name.StartsWith("onNetDisConnect,"))
                {
                    Log.WriteLine("ChromeMessageConsumer:" + name, new object[0]);
                }
                else if (name == "onNetReConnectOK")
                {
                    Util.WriteTrace("ChromeMessageConsumer" + name);
                    Log.WriteLine("ChromeMessageConsumer:" + name, new object[0]);
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        private void ConnectToQn()
        {
            Log.Info("ocr s1," + _desk.Seller);
            lock (_connectSynObj)
            {
                if (_isConnecting)
                {
                    return;
                }
                _isConnecting = true;
            }
            try
            {
                if (_desk.IsAlive)
                {
                    string url = string.Format("http://localhost:{0}/api/qn", _httpSvr.HttpPort);
                    Injector.Inject(_desk, url, () => IsConnected);
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            _isConnecting = false;
        }

        private void Connectable()
        {
            if (!IsConnected && _preTryConnectTime.xElapse().TotalSeconds > 1.0
                && _desk.IsAlive && _desk.IsForegroundOrVisibleMoreThanHalf(false)
                && Injector.ContinuousFailCount < 3)
            {
                _preTryConnectTime = DateTime.Now;
                ConnectToQn();
            }
        }

        private void Loop()
        {
            Connectable();
            if (_preOpenChromeDevTime.xElapse().TotalMinutes > (28.0 + (double)RandomEx.Rand.Next(1, 100) * 1.0 / 60))
            {
                _preOpenChromeDevTime = DateTime.Now;
                Injector.ActiveChromeDev(_desk);
            }
        }

        public void Dispose()
        {
            try
            {
                Timer.Dispose();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

    }

}
