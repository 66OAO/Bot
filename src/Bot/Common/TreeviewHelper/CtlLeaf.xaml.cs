using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BotLib.Wpf.Controls;
using Bot.AssistWindow;
using BotLib.Extensions;
using BotLib.Misc;
using BotLib.Wpf.Extensions;
using BotLib;

namespace Bot.Common.TreeviewHelper
{
    public partial class CtlLeaf : UserControl
    {
        private string _realHeader;

        private string _partShortcutImageFn;

        private Action<string, Action<BitmapImage>> _actUseImage;

        private WndAssist _wndDontUse;

        private bool _isMouseInsideImageControl;

        public CtlLeaf()
        {
            this._isMouseInsideImageControl = false;
            InitializeComponent();
        }


        public CtlLeaf(string realHeader, string keyText, string[] highlightKeys, string partShortcutImageFn = null, Action<string, Action<BitmapImage>> actUseImage = null, SolidColorBrush backgroundColorBrush = null)
            :this()
        {
			this._realHeader = realHeader;
			if (realHeader.Length > 100)
			{
				realHeader = realHeader.Substring(0, 97) + "...";
			}
			this.tbkHeader.Text=realHeader;
			this.tbkHeader.HighlightKeys=highlightKeys;
			if (keyText.xIsNullOrEmptyOrSpace())
			{
				this.tbkKeyText.Visibility = Visibility.Collapsed;
			}
			else
			{
				this.tbkKeyText.Text = keyText;
				this.tbkKeyText.HighlightKeys=highlightKeys;
			}
			if (!string.IsNullOrEmpty(partShortcutImageFn))
			{
				this._partShortcutImageFn = partShortcutImageFn;
				this._actUseImage = actUseImage;
				this.imgMain.Visibility = Visibility.Visible;
			}
			if (backgroundColorBrush != null)
			{
				this.tbkKeyText.Background = backgroundColorBrush;
			}
		}

        private void imgMain_MouseEnter(object sender, MouseEventArgs e)
        {
            this._isMouseInsideImageControl = true;
            if (!string.IsNullOrEmpty(this._partShortcutImageFn) && this.Wnd != null)
            {
                DelayCaller.CallAfterDelayInUIThread(() => {
                    if (this._isMouseInsideImageControl)
                    {
                        this._actUseImage(this._partShortcutImageFn, (bitmapImage) =>
                        {
                            if (bitmapImage != null)
                            {
                                try
                                {
                                    this.Wnd.BringTop();
                                    Rect rect = this.ZoomRect(bitmapImage);
                                    this.Wnd.imgBig.SetValue(Canvas.LeftProperty, rect.X);
                                    this.Wnd.imgBig.SetValue(Canvas.TopProperty, rect.Y);
                                    this.Wnd.imgBig.Width = rect.Width;
                                    this.Wnd.imgBig.Height = rect.Height;
                                    this.Wnd.imgBig.Source = bitmapImage;
                                }
                                catch (Exception ex)
                                {
                                    Log.Exception(ex);
                                    MsgBox.ShowErrTip(ex.Message,null);
                                }
                            }
                        });
                    }
                }, 300);
            }
        }

        private Rect ZoomRect(BitmapImage img)
        {
            Point point = base.TranslatePoint(new Point(-5.0, 0.0), this.Wnd);
            double width = img.Width;
            double height = img.Height;
            double maxWidth = point.X - 5.0;
            double maxHeight = SystemParameters.WorkArea.Height - 10.0;
            if (width > maxWidth)
            {
                double scale = width / maxWidth;
                width = maxWidth;
                height /= scale;
            }
            if (height > maxHeight)
            {
                double scale = height / maxHeight;
                width /= scale;
                height = maxHeight;
            }
            point.X -= width;
            if (point.Y + height > SystemParameters.WorkArea.Height)
            {
                point.Y = SystemParameters.WorkArea.Height - height - 5.0;
            }
            if (width < 0.0)
            {
                width = 100.0;
            }
            if (height < 0.0)
            {
                height = 100.0;
            }
            return new Rect(point.X, point.Y, width, height);
        }


        private WndAssist Wnd
        {
            get
            {
                if (this._wndDontUse == null)
                {
                    this._wndDontUse = DependencyObjectEx.xFindAncestor<WndAssist>(this);
                }
                return this._wndDontUse;
            }
        }

        private void imgMain_MouseLeave(object sender, MouseEventArgs e)
        {
            this._isMouseInsideImageControl = false;
            if (this.Wnd != null)
            {
                this.Wnd.imgBig.Source = null;
            }
        }

        
        private void ShowBigImage(BitmapImage img)
        {
            if (img != null)
            {
                try
                {
                    this.Wnd.BringTop();
                    Rect rect = this.ZoomRect(img);
                    this.Wnd.imgBig.SetValue(Canvas.LeftProperty, rect.X);
                    this.Wnd.imgBig.SetValue(Canvas.TopProperty, rect.Y);
                    this.Wnd.imgBig.Width = rect.Width;
                    this.Wnd.imgBig.Height = rect.Height;
                    this.Wnd.imgBig.Source = img;
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                    MsgBox.ShowErrTip(ex.Message,null);
                }
            }
        }

    }
}
