using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using Bot.AssistWindow.Widget.Right.ShortCut;
using Bot.Common.TreeviewHelper;
using Bot.Automation.ChatDeskNs;
using Bot.Automation.ChatDeskNs.InputSuggestion;
using BotLib;
using BotLib.Collection;
using BotLib.Extensions;
using BotLib.Misc;
using BotLib.Wpf.Extensions;
using System.Windows.Forms;
using Bot.AssistWindow.Widget.Right;
using Bot.Robot.Rule.QaCiteTableV2;
using Bot.Automation;
using System.Windows.Media;
using DbEntity;

namespace Bot.AssistWindow.Widget.Bottom
{
    public partial class CtlAnswer : System.Windows.Controls.UserControl, IComponentConnector
    {
        private ChatDesk _desk;
        private BottomPanel.Tipper _tipper;
        private WndAssist _wnd;
        private static Thickness ItemMargin;
        private DateTime _preSetTagInfoTime;
        private object _preTag;
        private ShowListTypeEnum _showListType;
        private object _showListKey;
        private object _updateAnswerSynObj;
        private bool _isUpdateAnswerBusy;
        private int _updateAnswerUnDealCount;
        private bool _isDoubleClick;
        private DateTime _doubleClickTime;
        public bool HasItems
        {
            get
            {
                return tvMain.HasItems;
            }
        }

        static CtlAnswer()
        {
            ItemMargin = new Thickness(-10.0, 0.0, 0.0, 5.0);
        }

        public CtlAnswer()
        {
            _preSetTagInfoTime = DateTime.MinValue;
            _showListType = ShowListTypeEnum.Unknown;
            _showListKey = null;
            _updateAnswerSynObj = new object();
            _isUpdateAnswerBusy = false;
            _updateAnswerUnDealCount = 0;
            _isDoubleClick = false;
            _doubleClickTime = DateTime.MinValue;
            InitializeComponent();
            tvMain.xSetRightClickSelectTreeviewItem();
        }

        public void Init(WndAssist wnd)
        {
            _desk = wnd.Desk;
            _tipper = wnd.ctlBottomPanel.TheTipper;
            _wnd = wnd;
        }

        private void tvMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _isDoubleClick = true;
            _doubleClickTime = DateTime.Now;
            SendTagInfo();
        }

        private void SendTagInfo()
        {
            try
            {
                var obj = GetItemTag();
                if (obj != null)
                {
                    if (obj is ShortcutEntity)
                    {
                        var shortcut = obj as ShortcutEntity;
                        shortcut.SetOrSendShortcutAsync(_desk, true,false);
                    }
                    else if (obj is string)
                    {
                        var txt = obj as string;
                        _desk.Editor.SendPlainTextAsync(txt);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public object GetItemTag(int index)
        {
            object rt = null;
            if (index >= 0 && index < tvMain.Items.Count)
            {
                rt = GetItemTag(tvMain.Items[index] as TreeViewItem);
            }
            return rt;
        }

        private object GetItemTag()
        {
            return GetItemTag(tvMain.SelectedItem as TreeViewItem);
        }

        private object GetItemTag(TreeViewItem it)
        {
            object rt = null;
            try
            {
                if (it != null)
                {
                    DispatcherEx.xInvoke(()=>
                    {
                        rt = ((it != null) ? it.Tag : null);
                    });
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return rt;
        }

        private void tvMain_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetTagInfoToEditor(false);
        }

        private void SetTagInfoToEditor(bool focusEditor)
        {
            try
            {
                var tag = GetItemTag();
                if (tag != null && (tag != _preTag || (DateTime.Now - _preSetTagInfoTime).TotalSeconds >= 1.0))
                {
                    _preTag = tag;
                    _preSetTagInfoTime = DateTime.Now;
                    string txt = null;
                    if (tag is ShortcutEntity)
                    {
                        var shortcut= tag as ShortcutEntity;
                        shortcut.SetOrSendShortcutAsync(_desk, false, focusEditor);
                    }
                    else if (tag is string)
                    {
                        txt = (tag as string);
                        _desk.Editor.SetOrSendPlainText(txt, true);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        private long GetGoodsNumiidFromUrl(string txt)
        {
            long rt = 0L;
            string pattern = "(?<=id=)\\d+";
            try
            {
                string value = RegexEx.Match(txt, pattern);
                rt = Convert.ToInt64(value);
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("CtlAnswer,GetGoodsNumiidFromUrl,txt={0},exp={1}", txt, ex.Message));
            }
            return rt;
        }


        private void tvMain_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Key key = e.Key;
            if (key != Key.Return)
            {
                switch (key)
                {
                    case Key.Left:
                    case Key.Right:
                        _wnd.Desk.Editor.FocusEditor(true);
                        break;
                    case Key.Up:
                        if (SelectItem())
                        {
                            _wnd.Desk.Editor.FocusEditor(true);
                        }
                        break;
                    case Key.Down:
                        if ((System.Windows.Input.Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                        {
                            int idx = GetItemIndex(tvMain.SelectedItem);
                            if (idx < 0)
                            {
                                idx = 0;
                            }
                            if (idx < tvMain.Items.Count - 1)
                            {
                                var it = tvMain.Items[idx + 1] as TreeViewItem;
                                if (it != null)
                                {
                                    it.IsSelected = true;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                SendTagInfo();
            }
        }

        private int GetItemIndex(object it)
        {
            int idx = -1;
            if (it != null)
            {
                for (int i = 0; i < tvMain.Items.Count; i++)
                {
                    if (it == tvMain.Items[i])
                    {
                        idx = i;
                        break;
                    }
                }
            }
            return idx;
        }

        private bool SelectItem()
        {
            bool rt = false;
            if (tvMain.Items.Count > 0)
            {
                var it = tvMain.Items[0] as TreeViewItem;
                if (it != null)
                {
                    rt = it.IsSelected;
                }
            }
            return rt;
        }

        private void tvMain_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if ((DateTime.Now - _doubleClickTime).TotalMilliseconds > 500.0)
            {
                DelayCaller.CallAfterDelayInUIThread(() => {
                    if (!_isDoubleClick)
                    {
                        SetTagInfoToEditor(true);
                    }
                    _isDoubleClick = false;
                }, SystemInformation.DoubleClickTime);
            }
        }

        public void FocusItem()
        {
            int idx = GetItemIndex(tvMain.SelectedItem);
            FocusItem(idx);
        }

        public void FocusItem(int idx)
		{
			_wnd.BringTop();
			if (idx >= tvMain.Items.Count)
			{
				idx = tvMain.Items.Count - 1;
			}
			if (idx < 0)
			{
				idx = 0;
			}
			WinApi.BringTopAndDoAction(_wnd.Handle,()=>{
                tvMain.Focus();
                if (idx < tvMain.Items.Count)
                {
                    var it = tvMain.Items[idx] as TreeViewItem;
                    if (it != null) it.IsSelected = true;
                }
            });
		}

        public void AppendInpuTextForEndItem(string txt)
        {
            var item4Show = new Item4Show(txt, txt);
            var ctlLeaf = new CtlLeaf(item4Show.Text.xRemoveLineBreak(" ").Trim(), item4Show.HighlightKey, null, item4Show.ImageName, item4Show.ActUseImage, null);
            var sp = new StackPanel();
            sp.Orientation = System.Windows.Controls.Orientation.Horizontal;
            var noTxt = new TextBlock
            {
                Text = string.Format("{0}. ", tvMain.Items.Count + 1)
            };
            sp.Children.Add(noTxt);
            sp.Children.Add(ctlLeaf);
            ctlLeaf.Padding = new Thickness(0.0);
            var it = tvMain.xAppend(sp, null);
            it.Tag = item4Show.Tag;
            it.Margin = ItemMargin;
            it.MouseDoubleClick += tvMain_MouseDoubleClick;
            it.MouseLeftButtonUp += tvMain_MouseLeftButtonUp;
        }

        public bool IsShowListItem(string txt)
		{
			return _showListType == ShowListTypeEnum.InputLegend && _showListKey as string == txt;
		}

        public void ShowListItem(List<Item4Show> items, string key)
        {
            items = (items ?? new List<Item4Show>());
            ShowListItem(items);
            _showListType = ShowListTypeEnum.InputLegend;
            _showListKey = key;
        }

        private void ShowListItem(List<Item4Show> items)
		{
            DispatcherEx.xInovkeLowestPriority(() => {
                try
                {
                    tvMain.Items.Clear();
                    int no = 0;
                    foreach (var itShow in items.xSafeForEach())
                    {
                        no++;
                        var headerPnl = new StackPanel();
                        headerPnl.Orientation = System.Windows.Controls.Orientation.Horizontal;

                        var noTxt = new TextBlock();
                        noTxt.Text = string.Format("{0}.", no);
                        noTxt.Padding = new Thickness(0, 5, 0.0,0 );
                        var leaf = new CtlLeaf(itShow.Text.xRemoveLineBreak(" ").Trim(),
                                    itShow.HighlightKey,
                                    null, itShow.ImageName,
                                    itShow.ActUseImage,
                                    null);

                        headerPnl.Children.Add(noTxt);
                        headerPnl.Children.Add(leaf);

                        var it = tvMain.xAppend(headerPnl, null);
                        it.Tag = itShow.Tag;
                        it.Margin = ItemMargin;

                        it.MouseDoubleClick += tvMain_MouseDoubleClick;
                        it.MouseLeftButtonUp += tvMain_MouseLeftButtonUp;
                    }
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                }
            });
		}

        public class Item4Show
        {
            public Item4Show(ShortcutEntity shortcut)
            {
                ActUseImage = ShortcutImageHelper.UseImage;
                HighlightKey = shortcut.Code;
                ImageName = shortcut.ImageName;
                Tag = shortcut;
                Text = shortcut.Text;
            }

            public Item4Show(string text, object tag)
            {
                Text = text;
                Tag = tag;
            }
            public Action<string, Action<BitmapImage>> ActUseImage;

            public string HighlightKey;
            public string ImageName;
            public object Tag;

            public string Text;
        }

        private enum ShowListTypeEnum
        {
            Unknown,
            Nothing,
            InputLegend,
            InputCommand,
            AutoAnswer,
            GoodsLink
        }

    }
}
