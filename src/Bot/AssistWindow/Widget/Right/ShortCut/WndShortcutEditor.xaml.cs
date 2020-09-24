using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using Bot.Asset;
using Bot.Common;
using Bot.Common.Windows;
using BotLib;
using BotLib.Extensions;
using BotLib.Misc;
using BotLib.Wpf;
using BotLib.Wpf.Extensions;
using Xceed.Wpf.Toolkit;
using Bot.Common.EmojiInputer;
using Bot.AI.WordSplitterNs.ElementParser;
using DbEntity;

namespace Bot.AssistWindow.Widget.Right.ShortCut
{
	public partial class WndShortcutEditor : EtWindow
	{
        private VdWndShortcutEditor _data;
        private bool _isSubmit;
        private List<IndexRange> _emojiRanges;
        private EmojiParser _eParser;

        public WndShortcutEditor(string title = null, string content = null, string code = null, string imageName = null)
		{
			this._isSubmit = false;
			this.InitializeComponent();
            this._eParser = new EmojiParser();
            this._data = new VdWndShortcutEditor(content, code, this);
            this.ShowImage(imageName);
            this.tboxQuestion.Text = (title ?? "");
			Loaded += this.WndShortcutEditor_Loaded;
		}

		private void WndShortcutEditor_Loaded(object sender, RoutedEventArgs e)
		{
			this.tboxContent.Focus();
			this.tboxContent.xMoveCaretToTail();
		}


        private void tboxCode_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

		public static void CreateNew(string parentId, string dbAccount, string seller, Window ownnerWnd, Action<TreeNode> callback)
		{
            var wnd = new WndShortcutEditor(null, null, null);
            wnd.FirstShow(seller, ownnerWnd, () =>
            {
                if (wnd._isSubmit)
                {
                    var et = EntityHelper.Create<ShortcutEntity>(dbAccount);
                    et.ParentId = parentId;
                    et.Title = wnd.tboxQuestion.Text.Trim();
                    et.Text = wnd._data.Content;
                    et.Code = wnd._data.Code;
                    et.ImageName = wnd.imgMain.Tag == null ? "" : wnd.imgMain.Tag.ToString();
                    callback(et);
                }
            }, false);

		}

		public static void Edit(ShortcutEntity pre, string seller, string dbAccount, Window ownnerWnd, Action<TreeNode> callback)
		{
            var wnd = new WndShortcutEditor(pre.Title,pre.Text, pre.Code, pre.ImageName);
            wnd.FirstShow(seller, ownnerWnd, () =>
            {
                if (wnd._isSubmit)
                {
                    var et = pre.Clone<ShortcutEntity>(false);
                    et.Title = wnd.tboxQuestion.Text.Trim();
                    et.Text = wnd._data.Content;
                    et.Code = wnd._data.Code;
                    et.ImageName =  wnd.imgMain.Tag==null ? "" : wnd.imgMain.Tag.ToString();

                    EntityHelper.SetModifyTick(et);
                    callback(et);
                }
            }, false);
		}

		private void imgEmoji_MouseDown(object sender, MouseButtonEventArgs e)
		{
            WndEmojiInputer.MyShow(this, (emojisText) => {
                if (!string.IsNullOrEmpty(emojisText))
                {
                    this.tboxContent.xInsertOrAppend(emojisText);
                }
            });
		}

        private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
        private void btnOk_Click(object sender, RoutedEventArgs e)
		{
            if (this.Validate())
			{
				this._isSubmit = true;
				Close();
			}
		}

        private bool Validate()
		{
			bool rt = false;
			if ((rt = this._data.Submitable()) && this.imgMain.Tag != null)
			{
				var code = this.tboxCode.Text;
				if (string.IsNullOrEmpty((code != null) ? code.Trim() : null))
				{
                    rt = !string.IsNullOrEmpty((tboxContent.Text != null) ? tboxContent.Text.Trim() : null);
				}
				else
				{
                    rt = true;
				}
				if (!rt)
				{
                    MsgBox.ShowErrTip("选中图片时，至少还要设置【快捷编码】或者【分类短语】", null);
				}
			}
			return rt;
		}

		private void ShowImage(string imageName)
		{
			if (string.IsNullOrEmpty(imageName))
			{
				this.tbkImage.Visibility = Visibility.Collapsed;
				this.imgMain.Visibility = Visibility.Collapsed;
				this.imgMain.Tag = null;
				this.btnAddImage.Visibility = Visibility.Visible;
				this.btnAddImage.IsEnabled = true;
				this.btnUpdateImage.Visibility = Visibility.Collapsed;
			}
			else
			{
				this.tbkImage.Visibility = Visibility.Visible;
				this.imgMain.Visibility = Visibility.Visible;
				this.imgMain.Tag = imageName;
				this.btnAddImage.Visibility = Visibility.Collapsed;
				this.btnUpdateImage.Visibility = Visibility.Visible;
				this.btnUpdateImage.IsEnabled = false;
                ShortcutImageHelper.UseImage(imageName, (imgSrc) => {
                    this.imgMain.Source = imgSrc;
                    this.btnUpdateImage.IsEnabled = true;
                });
			}
		}

		private void btnAddImage_Click(object sender, RoutedEventArgs e)
		{
			this.btnAddImage.IsEnabled = false;
            var imageName = this.SelectImageFile();
            this.ShowImage(imageName);
		}

		private string SelectImageFile(string parnFnOld = null)
		{
            string imageName = null;
			try
			{
				imageName = OpenFileDialogEx.GetOpenFileName("选择图片", "图片文件|*.gif;*.jpeg;*.jpg;*.png;*.tif;*.tiff", null);
                if (!string.IsNullOrEmpty(imageName))
				{
                    if (FileEx.IsFileLengthMoreKB(imageName, 1024))
					{
						throw new Exception("图片大小不能超过1MB");
					}
					BitmapImage bitmapImage;
                    if (!BitmapImageEx.TryCreateFromFile(imageName, out bitmapImage))
					{
						throw new Exception("无法解析图片，请选择正常的图片");
					}
                    imageName = ShortcutImageHelper.AddNewImage(imageName, parnFnOld);
				}
			}
			catch (Exception ex)
			{
                Log.Exception(ex);
                MsgBox.ShowErrTip(ex.Message, null);
			}
            return imageName;
		}

        private void btnHelp_Click(object sender, RoutedEventArgs e)
		{
		}

		private void btnUpdateImage_Click(object sender, RoutedEventArgs e)
		{
			this.btnUpdateImage.IsEnabled = false;
			string text = this.SelectImageFile(this.imgMain.Tag as string);
			if (!string.IsNullOrEmpty(text))
			{
				this.ShowImage(text);
			}
			else
			{
				this.btnUpdateImage.IsEnabled = true;
			}
		}

		private void btnDeleteImage_Click(object sender, RoutedEventArgs e)
		{
            MsgBox.ShowNotTipAgain("确定要删除图片？", "操作确认", "WndShortcutEditor_DeleteImage", (b1, okClicked) => {
                if (!b1 || okClicked)
                {
                    string text = this.imgMain.Tag as string;
                    if (!string.IsNullOrEmpty(text))
                    {
                        ShortcutImageHelper.DeleteImage(text);
                    }
                    this.ShowImage(null);
                }
            });
		}

		private void tboxContent_SelectionChanged(object sender, RoutedEventArgs e)
		{
			int selectionStart = this.tboxContent.SelectionStart;
			bool hasShow = false;
			if (selectionStart >= 0 && !this._emojiRanges.xIsNullOrEmpty())
			{
				foreach (var ir in this._emojiRanges)
				{
					if (selectionStart >= ir.Start && selectionStart < ir.NextStart)
					{
                        var rect = EmojiHelper.FindEmojisRect(this.tboxContent.Text.Substring(ir.Start, ir.Length));
                        this.ShowEmojiImage(rect);
                        hasShow = true;
                        break;
					}
				}
			}
			if (!hasShow)
			{
				this.imgEmojiSelected.Visibility = Visibility.Collapsed;
			}
		}

		private void ShowEmojiImage(Int32Rect imgRect)
		{
			if (imgRect == null)
			{
				this.imgEmojiSelected.Visibility = Visibility.Collapsed;
			}
			else
			{
				CroppedBitmap source = new CroppedBitmap(AssetImageHelper.GetImageFromWpfCache(AssetImageEnum.imgEmojiAll), imgRect);
				this.imgEmojiSelected.Source = source;
				this.imgEmojiSelected.Visibility = Visibility.Visible;
			}
		}

		private void tboxContent_TextChanged(object sender, TextChangedEventArgs e)
		{
            this._emojiRanges = this._eParser.GetExceptRanges(this.tboxContent.Text);
		}

		private void imgMain_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (this.imgMain.Tag != null && e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
			{
				e.Handled = true;
                Process.Start(ShortcutImageHelper.GetFullPath(this.imgMain.Tag as string));
			}
		}




		private class VdWndShortcutEditor : ViewData
		{
			public string Content { get; set; }
            private string _oriCode;
            private string _code;
			private WndShortcutEditor _wnd;

			public string Code
			{
				get
				{
					return this._code;
				}
				set
				{
                    this._oriCode = value;
					this._code = value.xToBanJiao().ToLower();
				}
			}

			public VdWndShortcutEditor(string content, string code, WndShortcutEditor wnd) :base(wnd)
			{
				this.Content = (content ?? "");
				this.Code = (code ?? "");
				this._wnd = wnd;
                SetBinding("Content", wnd.tboxContent, () => { 
                    return (!string.IsNullOrEmpty(this.Content) || this._wnd.imgMain.Tag != null) ? null : "必填!";
                });
                SetBinding("Code", wnd.tboxCode, () => {
                    string err = null;
                    var c = _oriCode.Trim();
                    if (!string.IsNullOrEmpty(c))
                    {
                        if (char.IsDigit(c[0]))
                        {
                            err = "快捷编码的“第一个字”不能为数字";
                        }
                        else if (char.IsDigit(c.ToCharArray()[c.Length - 1]))
                        {
                            err = "快捷编码的“最后一个字”不能为数字";
                        }
                    }
                    return err;
                });
			}
		}

	}
}
