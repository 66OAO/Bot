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
using Bot.Common;
using BotLib;
using BotLib.Misc;
using BotLib.Wpf.Controls;
using BotLib.Wpf.Extensions;
using DbEntity;

namespace Bot.AssistWindow.Widget.Right.ShortCut
{
    public partial class CtlShortcutEntity : UserControl
    {
        private string _partShortcutImageFn;
        private Action<string, Action<BitmapImage>> _actUseImage;
        private WndAssist _wndDontUse;
        private bool _isMouseInsideImageControl;
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
        public CtlShortcutEntity()
		{
			this._isMouseInsideImageControl = false;
			this.InitializeComponent();
		}

        public CtlShortcutEntity(ShortcutEntity shortcut ,string[] highlightKeys, SolidColorBrush backgroundBrush = null)
        {
            this._isMouseInsideImageControl = false;
            InitializeComponent();
            this.tbkHeader.Text = shortcut.GetShowTitle();
			this.tbkHeader.HighlightKeys = highlightKeys;
            if (string.IsNullOrEmpty(shortcut.Code))
			{
				this.tbkKeyText.Visibility = Visibility.Collapsed;
			}
			else
			{
                this.tbkKeyText.Text = shortcut.Code;
				this.tbkKeyText.HighlightKeys = highlightKeys;
			}
			if (backgroundBrush != null)
			{
				this.tbkKeyText.Background = backgroundBrush;
			}
            if (!string.IsNullOrEmpty(shortcut.ImageName))
			{
                this._partShortcutImageFn = shortcut.ImageName;
                this._actUseImage = new Action<string, Action<BitmapImage>>(ShortcutImageHelper.UseImage);
				this.imgMain.Visibility = Visibility.Visible;
			}
            this.tblkContent.Text = shortcut.Text;
			this.tblkContent.HighlightKeys = highlightKeys;
            if (!string.IsNullOrEmpty(shortcut.ImageName))
			{
                this._actUseImage(this._partShortcutImageFn, (bitmapImage) =>
                {
                    if (bitmapImage != null)
                    {
                        try
                        {
                            this.imgContent.Source = bitmapImage;
                            this.imgContent.Visibility = Visibility.Visible;
                        }
                        catch (Exception e)
                        {
                            Log.Exception(e);
                        }
                    }
                });
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

        private void imgMain_MouseLeave(object sender, MouseEventArgs e)
        {
            this._isMouseInsideImageControl = false;
            if (this.Wnd != null)
            {
                this.Wnd.imgBig.Source = null;
            }
        }
    }
}
