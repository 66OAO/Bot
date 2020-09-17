using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Bot.Automation.ChatDeskNs;
using BotLib;
using BotLib.Extensions;
using BotLib.Misc;
using BotLib.Wpf.Extensions;

namespace Bot.AssistWindow.Widget
{
	public partial class CtlImage : UserControl
    {
        private ChatDesk _desk;
        private string _imgUrl;
        private string _goodsUrl;
        private WndAssist _wndDontUse;
        private bool _isMouseInsideImageControl;
        private DateTime _mouseEnterTime;

		public CtlImage()
		{
			this._isMouseInsideImageControl = false;
			this._mouseEnterTime = DateTime.MaxValue;
			this.InitializeComponent();
		}

		public void Init(string imgUrl, string goodsUrl, ChatDesk desk)
		{
			this._imgUrl = imgUrl;
			this._desk = desk;
			this._goodsUrl = goodsUrl;
			WebImageHelper.GetImageFromUrl(imgUrl, this.imgGoods, false);
		}

		private void imgGoods_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 1)
			{
				if (e.ChangedButton == MouseButton.Left)
				{
					this._desk.Editor.SetPlainText(this._goodsUrl, true, true);
				}
				else
				{
					WebImageHelper.GetImageFromUrl(this._imgUrl, this.imgGoods, true);
				}
			}
			else if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Left)
			{
				this._desk.Editor.SendPlainText(this._goodsUrl);
			}
		}

		private WndAssist Wnd
		{
			get
			{
				if (this._wndDontUse == null)
				{
					this._wndDontUse = this.xFindAncestor<WndAssist>();
				}
				return this._wndDontUse;
			}
		}

		private void imgGoods_MouseEnter(object sender, MouseEventArgs e)
		{
			this.OnMouseEnter();
		}

		private void OnMouseEnter()
		{
			try
			{
				this._isMouseInsideImageControl = true;
				this._mouseEnterTime = DateTime.Now;
				if (this.imgGoods.Source != null && this.imgGoods.Source != WebImageHelper.ImgLoading && this.Wnd != null)
				{
                    DelayCaller.CallAfterDelayInUIThread(() => {
                        if (this._isMouseInsideImageControl && this.Wnd.imgBig.Source == null)
                        {
                            this.Wnd.BringTop();
                            this.Wnd.imgBig.Width = 300.0;
                            this.Wnd.imgBig.Height = 300.0;
                            Point point = base.TranslatePoint(new Point(base.ActualWidth + 5.0, 0.0), this.Wnd);
                            if (point.Y + this.Wnd.imgBig.Height > SystemParameters.WorkArea.Height)
                            {
                                point.Y = SystemParameters.WorkArea.Height - this.Wnd.imgBig.Height - 5.0;
                            }
                            this.Wnd.imgBig.SetValue(Canvas.LeftProperty, point.X);
                            this.Wnd.imgBig.SetValue(Canvas.TopProperty, point.Y);
                            WebImageHelper.GetImageFromUrl(this._imgUrl, this.Wnd.imgBig, false);
                            DelayCaller.CallAfterDelayInUIThread(() => {
                                if (this._isMouseInsideImageControl && this._mouseEnterTime.xElapse().TotalSeconds > 9.0)
                                {
                                    this.OnMouseLeave();
                                }
                            }, 10000);
                        }
                    }, 200);
				}
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
		}

		private void OnMouseLeave()
		{
			try
			{
				this._isMouseInsideImageControl = false;
				this._mouseEnterTime = DateTime.MaxValue;
				if (this.Wnd != null)
				{
					this.Wnd.imgBig.Source = null;
				}
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
		}

		private void imgGoods_MouseLeave(object sender, MouseEventArgs e)
		{
			this.OnMouseLeave();
		}

	}
}
