using BotLib;
using BotLib.Extensions;
using BotLib.Wpf.Extensions;
using Bot.AssistWindow;
using Bot.AssistWindow.NotifyIcon;
using Bot.Automation;
using Bot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Bot.Options.HotKey
{
    public class HotKeyHelper
    {
		static HotKeyHelper()
		{
			HotKeyHelper.IsDownKeyRegistered = false;
			HotKeyHelper._isLatestRegisterDownKey = false;
			HotKeyHelper._synobj = new object();
		}

		public static void Dispatch(IntPtr op)
		{
			try
			{
				WndAssist wndAssist = WndAssist.GetTopWindow();
				if (wndAssist != null)
				{
					wndAssist.ctlBottomPanel.MonitorHotKey((HotKeyHelper.HotOp)((int)op));
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				MsgBox.ShowErrTip(ex.Message,null);
			}
		}

		public void HookDownKey()
		{
			try
			{
				if (this._hook != null)
				{
					this.UnHookDownKey();
				}
				this._hook = new KeyboardHook();
                this._hook.KeyDownEvent = (sender,e) =>
                {

                };
				this._hook.Start();
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
		}

		public static string Init()
		{
			var tipMsg = string.Empty;
			try
			{
                HotKeyHelper.RegisterDownKey();
				Keys keys;
				if (Params.HotKey.GetHotKey(HotKeyHelper.HotOp.QinKong, out keys) && !HotKeyHelper.Register(keys, HotKeyHelper.HotOp.QinKong))
				{
					tipMsg += "清空,";
				}
				if (Params.HotKey.GetHotKey(HotKeyHelper.HotOp.ZhiSi, out keys) && !HotKeyHelper.Register(keys, HotKeyHelper.HotOp.ZhiSi))
				{
					tipMsg += "知识,";
				}
				if (tipMsg.Length > 0)
				{
					tipMsg = tipMsg.Substring(0, tipMsg.Length - 1);
					tipMsg = string.Format("“{0}”等功能的“快捷键”失效，原因是这些快捷键已经被其它程序占用。", tipMsg);
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				tipMsg += ex.Message;
			}
			return tipMsg;
		}

		public static bool Register(Keys keys, HotKeyHelper.HotOp op)
		{
			bool rt = false;
			try
			{
                rt = HotKeyHelper.WindowsShell.RegisterHotKey(WndNotifyIcon.Inst, keys, (int)op);
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
			return rt;
		}

        public static void RegisterDownKey()
		{
			if (Params.InputSuggestion.IsUseDownKey)
			{
				HotKeyHelper.TryRegisterDownKey();
			}
			else
			{
				HotKeyHelper.TryUnRegisterDownKey();
			}
		}

        public static string GetHotKeyDesc(Keys key)
		{
			var evt = new System.Windows.Forms.KeyEventArgs(key);
			string text = "";
			string text2 = "";
			string result = "无";
			if (evt.KeyCode != Keys.Back && evt.KeyCode != Keys.Space && evt.KeyCode != Keys.Delete)
			{
				if (evt.Control)
				{
					text = "Ctrl";
				}
				if (evt.Shift)
				{
					text += ((text == "") ? "Shift" : " + Shift");
				}
				if (evt.Alt)
				{
					text += ((text == "") ? "Alt" : " + Alt");
				}
				if (evt.KeyCode != Keys.ShiftKey && evt.KeyCode != Keys.ControlKey && evt.KeyCode != Keys.Menu)
				{
					text2 = evt.KeyCode.ToString();
				}
				result = ((text == "") ? text2 : (text + " + " + text2));
			}
			return result;
		}


        public static void UnRegister(HotKeyHelper.HotOp op)
		{
			HotKeyHelper.WindowsShell.UnregisterHotKey(WndNotifyIcon.Inst, (int)op);
		}

		public static bool TryRegisterDownKey()
		{
			return LockEx.TryLock(_synobj, 100, ()=>{
                if (!HotKeyHelper.IsDownKeyRegistered)
				{
					try
					{
						HotKeyHelper.Register(Keys.Down, HotKeyHelper.HotOp.ArrowDown);
                        HotKeyHelper.Register(Keys.Back | Keys.Space | Keys.Control, HotKeyHelper.HotOp.ArrowDown2);
						HotKeyHelper.IsDownKeyRegistered = true;
					}
					catch (Exception e)
					{
						Log.Exception(e);
					}
				}
            });
		}

		public static bool TryUnRegisterDownKey()
		{
			bool rt;
			if (!(rt = LockEx.TryLockMultiTime(_synobj, 100, ()=>{
                if (HotKeyHelper.IsDownKeyRegistered)
				{
					HotKeyHelper.IsDownKeyRegistered = false;
					try
					{
                        HotKeyHelper.UnRegister(HotKeyHelper.HotOp.ArrowDown);
                        HotKeyHelper.UnRegister(HotKeyHelper.HotOp.ArrowDown2);
					}
					catch (Exception e)
					{
						Log.Exception(e);
					}
				}
            }, 5, 10)))
			{
				Log.Error("TryUnRegisterDownKey Failed.");
			}
			return rt;
		}

		public void UnHookDownKey()
		{
			try
			{
                if (_hook != null)
				{
                    _hook.Stop();
				}
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
		}

		public static string QinKongHotKeyDesc
		{
			get
			{
				Keys keys;
                return Params.HotKey.GetHotKey(HotKeyHelper.HotOp.QinKong, out keys) ? HotKeyHelper.GetHotKeyDesc(keys) : "无";
			}
		}

		public static string ZhiSiHotKeyDesc
		{
			get
			{
				Keys keys;
                return Params.HotKey.GetHotKey(HotKeyHelper.HotOp.ZhiSi, out keys) ? HotKeyHelper.GetHotKeyDesc(keys) : "无";
			}
		}

		public static bool IsDownKeyRegistered;

		private KeyboardHook _hook;

		private static bool _isLatestRegisterDownKey;

		private static object _synobj;

		public enum HotOp
		{
			QinKong,
			ZhiSi,
			Unknown,
			ArrowDown,
			ArrowDown2
		}

		public class WindowsShell
		{
			static WindowsShell()
			{
				HotKeyHelper.WindowsShell.MOD_ALT = 1;
				HotKeyHelper.WindowsShell.MOD_CONTROL = 2;
				HotKeyHelper.WindowsShell.MOD_SHIFT = 4;
				HotKeyHelper.WindowsShell.MOD_WIN = 8;
				HotKeyHelper.WindowsShell.WM_HOTKEY = 786;
			}

			[DllImport("user32.dll")]
			private static extern bool RegisterHotKey(int hWnd, int id, int fsModifiers, int vk);

            public static bool RegisterHotKey(Window f, Keys key, int keyid)
			{
				var isok = false;
				DispatcherEx.xInvoke(()=>{
                    isok = RegisterHotKey(f.xHandle(), keyid, 0, (int)key);
                    if (!isok)
                    {
                        var isInvalid = false;
                        var lastErr = WinApi.GetLastError(out isInvalid);
                        Log.Error(string.Format("RegisterHotKey失败，keyid={0},GetLastError={1},,isInvalid={2}", keyid, lastErr, isInvalid));
                    }
                });
                return isok;
			}

			[DllImport("user32.dll")]
			private static extern bool UnregisterHotKey(int hWnd, int id);

			public static bool UnregisterHotKey(Window f, int keyid)
			{
				var isok = false;
				DispatcherEx.xInvoke(()=>{
                    try 
	                {
                        isok = UnregisterHotKey(f.xHandle(), keyid);
                        if(!isok){
                            var isInvalid = false;
                            var lastErr = WinApi.GetLastError(out isInvalid);
                            Log.Error(string.Format("UnregisterHotKey失败，keyid={0},GetLastError={1},,isInvalid={2}",keyid,lastErr,isInvalid));
                        }
	                }
	                catch (Exception ex)
	                {
                        Log.Exception(ex);
	                }
                });
                return isok;
			}

			public static int MOD_ALT;
			public static int MOD_CONTROL;
			public static int MOD_SHIFT;
			public static int MOD_WIN;
			public static int WM_HOTKEY;
		}
    }
}
