using BotLib;
using BotLib.BaseClass;
using BotLib.Misc;
using Bot.Automation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.ChromeNs
{
	public abstract class ChromeConnector : Disposable
	{
		public const bool IsBanChromeForTest = false;
		public readonly NoReEnterTimer Timer;
		private bool _isDisposed;
		private ManualResetEventSlim _evslim;
		private bool _isChromeOk;
		private string _logMark;
		protected int _hwnd;
		protected string _siUrl;
		private ChromeOperator _chromeOperator;
		private int _continuousInitSessionCount;
		public event EventHandler<ChromeAdapterEventArgs> EvChromeDetached;
		public event EventHandler<ChromeAdapterEventArgs> EvChromeConnected;
		public bool IsChromeOk
		{
			get
			{
				if (this._isChromeOk)
				{
					_isChromeOk = (ChromOp != null && !ChromOp.IsDisposed);
				}
				return _isChromeOk;
			}
			private set
			{
				if (value)
				{
					this._evslim.Set();
				}
				else
				{
					this._evslim.Reset();
				}
				if (this._isChromeOk != value)
				{
					Log.Info("IsOcrOk=" + value.ToString() + "," + this._logMark);
					this._isChromeOk = value;
				}
			}
		}

		protected ChromeOperator ChromOp
		{
			get
			{
				return this._chromeOperator;
			}
			set
			{
				ChromeOperator chromeOperator = this._chromeOperator;
				this._chromeOperator = value;
				if (chromeOperator != null)
				{
					chromeOperator.Dispose();
					if (this.EvChromeDetached != null)
					{
						EvChromeDetached(this, new ChromeAdapterEventArgs
						{
							Connector = this
						});
					}
				}
			}
		}
		public bool WaitForChromeOk(int timeoutMs = 1000)
		{
			bool result;
			if (this.IsChromeOk)
			{
				result = true;
			}
			else
			{
				if (!(result = this._evslim.WaitHandle.WaitOne(timeoutMs)))
				{
					Log.Error("cc,等待超时," + this._logMark);
				}
			}
			return result;
		}
		protected ChromeConnector(int hwnd, string logMark)
		{
			this._isDisposed = false;
			this._evslim = new ManualResetEventSlim(false);
			this._isChromeOk = false;
			this._logMark = "Ocr";
			this._continuousInitSessionCount = 0;
			this._logMark = logMark;
			this.ClearStateValues();
			this._hwnd = hwnd;
			this._siUrl = ChromeOperator.GetSessionInfoUrl(hwnd);
			Task.Factory.StartNew(new Action(this.ListenService));
			this.Timer = new NoReEnterTimer(this.ReconnectLoop, 2000, 1000);
		}
		protected override void CleanUp_Managed_Resources()
		{
			if (this.Timer != null)
			{
				this.Timer.Dispose();
			}
			if (ChromOp != null)
			{
				ChromOp.Dispose();
			}
			this.IsChromeOk = false;
		}
		protected virtual void ClearStateValues()
		{
			this.IsChromeOk = false;
		}
		protected abstract ChromeOperator CreateChromeOperator(string chromeSessionInfoUrl);
		private void ReconnectLoop()
		{
			try
			{
				if (this._isDisposed)
				{
					this.Timer.Dispose();
					Log.Info(this._logMark + ",timer closed.");
				}
				else
				{
					if (!this.IsChromeOk)
					{
						this.ListenService();
					}
				}
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
		}
		private void ListenService()
		{
			SkipIfExcuting.Excute(new Action(this.InitServiceInner));
		}
		private void InitServiceInner()
		{
			try
			{
				this.ClearStateValues();
				this.WaitForInit();
				if (!this._isDisposed)
				{
					if (!WinApi.IsHwndAlive(this._hwnd))
					{
						Log.Info("窗口已关闭，" + this._logMark + "关闭");
						this._isDisposed = true;
					}
					else
					{
						Log.Info("开始初始化" + this._logMark + "...");
						this.ChromOp = this.CreateChromeOperator(this._siUrl);
						if (this.ChromOp == null)
						{
							throw new Exception("无法获取  operator," + this._logMark);
						}
						this.ChromOp.ListenChromeDetachedTurbo(new Action<string>(this.Detached));
						this._continuousInitSessionCount = 0;
						this.IsChromeOk = true;
						this.Timer.AddAction(new Action(this.ChromOp.VerifySessionAlive), 2000, 0);
						if (this.EvChromeConnected != null)
						{
							EvChromeConnected(this, new ChromeAdapterEventArgs
							{
								Connector = this
							});
						}
						Log.Info(this._logMark + "初始化成功！");
					}
				}
			}
			catch (Exception ex)
			{
				this.ClearStateValues();
				this._continuousInitSessionCount++;
				if (this._continuousInitSessionCount > 2)
				{
					this.ChromOp = null;
				}
				Log.Error(this._logMark + "初始化出错，原因=" + ex.Message);
			}
			finally
			{
				Log.Info("结束初始化" + this._logMark);
			}
		}
		private void WaitForInit()
		{
			int ms = 0;
			if (this._continuousInitSessionCount > 0)
			{
				ms = ((this._continuousInitSessionCount > 2) ? 2000 : 500);
			}
			if (ms > 0)
			{
				Thread.Sleep(ms);
			}
		}
		private void Detached(string reason)
		{
			Log.Info("!!!!!!!!!!!!!!" + this._logMark + " Detached,Reason=" + reason);
			this.ClearStateValues();
			this.ChromOp = null;
		}
	}
}
