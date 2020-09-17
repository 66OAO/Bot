using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Threading;
using BotLib.Extensions;

namespace Bot.AssistWindow.Widget.Bottom
{
	public partial class CtlBuyer : UserControl
    {
        private bool _chatReadable;
        private DispatcherTimer _errTimer;
        private bool toggle;
		public CtlBuyer()
		{
			this._chatReadable = false;
			this.toggle = false;
			this.InitializeComponent();
		}

		public void ShowBuyer(string buyer)
		{
			if (buyer.xIsNullOrEmptyOrSpace())
			{
				this.ShowImageTip(ImageEnum.GrayErr);
				this.tbkBuyer.Text = "没有检测到顾客";
			}
			else
			{
				this.tbkBuyer.Text = buyer;
				this.ShowImageTip(this._chatReadable ? ImageEnum.WangWang : ImageEnum.GrayErr);
			}
		}

		public void ShowBuyerAndChatReadable(string buyer)
		{
			this._chatReadable = true;
            this.StopErrTimer();
			this.tbkBuyer.Text = "";
			this.ShowBuyer(buyer);
		}

        private void ShowImageTip(ImageEnum imageEnum)
		{
            switch (imageEnum)
			{
			    case ImageEnum.Unknown:
				    this.imgErr.Visibility = Visibility.Collapsed;
				    this.imgNoBuyer.Visibility = Visibility.Collapsed;
				    this.imgWangWang.Visibility = Visibility.Collapsed;
				    break;
			    case ImageEnum.WangWang:
				    this.imgErr.Visibility = Visibility.Collapsed;
				    this.imgNoBuyer.Visibility = Visibility.Collapsed;
				    this.imgWangWang.Visibility = Visibility.Visible;
				    break;
			    case ImageEnum.RedErr:
				    this.imgErr.Visibility = Visibility.Visible;
				    this.imgNoBuyer.Visibility = Visibility.Collapsed;
				    this.imgWangWang.Visibility = Visibility.Collapsed;
				    break;
			    case ImageEnum.GrayErr:
				    this.imgErr.Visibility = Visibility.Collapsed;
				    this.imgNoBuyer.Visibility = Visibility.Visible;
				    this.imgWangWang.Visibility = Visibility.Collapsed;
				    break;
			}
		}

        private void CheckChatReadable()
		{
			if (this._errTimer == null)
			{
				this._errTimer = new DispatcherTimer();
				this._errTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
				this._errTimer.Tick += this._errTimer_Tick;
			}
			this._errTimer.Start();
		}

		private void _errTimer_Tick(object sender, EventArgs e)
		{
			if (this.imgErr.Visibility > Visibility.Visible)
			{
				this.ShowImageTip(CtlBuyer.ImageEnum.RedErr);
			}
			else
			{
				this.ShowImageTip(CtlBuyer.ImageEnum.GrayErr);
			}
		}

        private void StopErrTimer()
		{
			if (this._errTimer != null)
			{
				this.imgErr.Visibility = Visibility.Collapsed;
				this._errTimer.Stop();
			}
		}

		private enum ImageEnum
		{
			Unknown,
			WangWang,
			RedErr,
			GrayErr
		}
	}
}
