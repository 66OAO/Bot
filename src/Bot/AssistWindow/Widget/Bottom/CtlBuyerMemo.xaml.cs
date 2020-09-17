using BotLib;
using Bot.Asset;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BotLib.Wpf.Extensions;
using BotLib.Extensions;
using Bot.AssistWindow.Widget.Bottom.BuyerNote;
using Bot.Common.Account;
using Bot.Common;
using BotLib.Misc;
using Bot.Options;
using DbEntity;

namespace Bot.AssistWindow.Widget.Bottom
{
    /// <summary>
    /// CtlBuyerMemo.xaml 的交互逻辑
    /// </summary>
    public partial class CtlBuyerMemo : UserControl, IComponentConnector
    {
		private WndAssist _wndDontUse;
		private List<string> _preFavMemo;
		private HashSet<string> _preFavoriteNoteSet;
		private HashSet<string> _downingMemoset;
        private WndAssist Wnd
        {
            get
            {
                if (_wndDontUse == null)
                {
                    _wndDontUse = this.xFindAncestor<WndAssist>();
                }
                return _wndDontUse;
            }
        }

        public CtlBuyerMemo()
        {
	        _downingMemoset = new HashSet<string>();
	        InitializeComponent();
	        if (!DesignerProperties.GetIsInDesignMode(this))
	        {
                Loaded += CtlBuyerMemo_Loaded;
		        tboxBuyerNote.LostFocus += tboxBuyerNote_LostFocus;
	        }
        }

        private void CtlBuyerMemo_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= CtlBuyerMemo_Loaded;
            InitUI();
            DelayCaller.CallAfterDelayInUIThread(() =>
            {
                LoadBuyerNote(Wnd.Desk.BuyerMainNick,Wnd.Desk.Seller);
            }, 1000);
        }

        internal void LoadBuyerNote()
        {
            this.LoadBuyerNote(this.Wnd.Desk.BuyerMainNick, this.Wnd.Desk.Seller);
        }

        public void LoadBuyerNote(string buyer, string seller)
        {
            SaveBuyerNote();
            tboxBuyerNote.Tag = null;
            if (string.IsNullOrEmpty(buyer))
            {
                tboxBuyerNote.Text = "";
                tboxBuyerNote.Tag = null;
                tboxBuyerNote.IsEnabled = false;
                tboxBuyerNote.ToolTip = null;
            }
            else
            {
                tboxBuyerNote.IsEnabled = true;
                var note = BuyerNoteHelper.GetNewestBuyerNote(buyer, seller);
                var text = string.Empty;
                if (note != null)
                {
                    text = note.Note;
                }
                tboxBuyerNote.Text = text;
                tboxBuyerNote.Tag = new BuyerNoteTag
                {
                    BuyerMain = buyer,
                    Note = text,
                    Seller = seller
                };
                if (Params.BuyerNote.GetIsShowDetailAsTooltip(seller))
                {
                    tboxBuyerNote.ToolTip = WndDetail.GetBuyerNotes(buyer, seller);
                }
                else
                {
                    tboxBuyerNote.ToolTip = null;
                }
            }
        }

        private void InitUI()
        {
            var favNotes = BuyerNoteHelper.GetFavNotes(Wnd.Desk.SellerMainNick);
            if (HasNewFavNote(favNotes))
            {
                ContextMenu contextMenu = (ContextMenu)base.FindResource("menuSynBuyerNote");
                contextMenu.Items.Clear();
                MenuItem menuItem = new MenuItem();
                menuItem.Header = "打开【顾客便签】管理器";
                menuItem.Click += openBuyerNoteMgrMenuItem_Click;
                contextMenu.Items.Add(menuItem);
                menuItem = new MenuItem();
                menuItem.Header = "设置";
                menuItem.Click += settingMenuItem_Click;
                menuItem.Icon = new Image
                {
                    Source = AssetImageHelper.GetImageFromWpfCache(AssetImageEnum.imgOption),
                    MaxHeight = 20.0,
                    MaxWidth = 20.0,
                    Margin = new Thickness(3.0)
                };
                contextMenu.Items.Add(menuItem);
                menuItem = new MenuItem();
                menuItem.Header = "帮助";
                menuItem.Click +=helpMenuItem_Click;
                contextMenu.Items.Add(menuItem);
                if (!favNotes.xIsNullOrEmpty())
                {
                    contextMenu.Items.Add(new Separator());
                    foreach (FavoriteNoteEntity favoriteNoteEntity in favNotes.xSafeForEach())
                    {
                        MenuItem menuItem2 = new MenuItem();
                        string note = favoriteNoteEntity.Note;
                        menuItem2.Tag = note;
                        menuItem2.Header = "输入：" + ((note.Length > 20) ? (note.Substring(0, 20) + "...") : note);
                        menuItem2.Click += FavoriteNoteMenuItem_Click;
                        contextMenu.Items.Add(menuItem2);
                    }
                }
            }
        }

        private void FavoriteNoteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                string text = menuItem.Tag as string;
                Util.Assert(!string.IsNullOrEmpty(text));
                tboxBuyerNote.Text = text;
                tboxBuyerNote.Focus();
            }
        }

        private void openBuyerNoteMgrMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WndBuyerNoteMgr.MyShow(Wnd.Desk.Seller);
        }

        private void btnDetail_Click(object sender, RoutedEventArgs e)
        {
            WndDetail.MyShow(this.Wnd.Desk.BuyerMainNick, this.Wnd.Desk.Seller);
        }

        private void btnMemoCtxMenu_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.ContextMenu.IsOpen = true;
        }

        private void settingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WndOption.MyShow(Wnd.Desk.Seller, Wnd, OptionEnum.BuyerNote, () => {
                LoadBuyerNote(Wnd.Desk.BuyerMainNick,Wnd.Desk.Seller);
            });
        }

        private void helpMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private bool HasNewFavNote(List<FavoriteNoteEntity> fNotes)
        {
            bool rt = false;
            fNotes = fNotes ?? new List<FavoriteNoteEntity>();
            var notes = HashSetEx.Create<string>(fNotes.Select(k=>k.Note));
            if (_preFavoriteNoteSet == null || notes.Count != _preFavoriteNoteSet.Count)
            {
                rt = true;
            }
            else
            {
                foreach (string nt in notes)
                {
                    if (!_preFavoriteNoteSet.Contains(nt))
                    {
                        rt = true;
                        break;
                    }
                }
            }
            _preFavoriteNoteSet = notes;
            return rt;
        }

        private void tboxBuyerNote_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tboxBuyerNote.Text))
            {
                SaveBuyerNote();
                Log.Info(string.Format("buyerNote={0},seller={1},buyer={2}", tboxBuyerNote.Text, Wnd.Desk.Seller, Wnd.Desk.BuyerMainNick));
            }
        }

        private void SaveBuyerNote()
        {
            if (tboxBuyerNote.Tag != null)
            {
                string noteText = "";
                string buyer = "";
                string seller = "";
                try
                {
                    noteText = tboxBuyerNote.Text.Trim();
                    BuyerNoteTag buyerNoteTag = tboxBuyerNote.Tag as BuyerNoteTag;
                    if (noteText != buyerNoteTag.Note)
                    {
                        if (buyerNoteTag.NoteEntity == null)
                        {
                            seller = buyerNoteTag.Seller;
                            buyer = buyerNoteTag.BuyerMain;
                            buyerNoteTag.NoteEntity = BuyerNoteHelper.Create(buyer, seller, noteText);
                            buyerNoteTag.Note = noteText;
                        }
                        else
                        {
                            buyerNoteTag.Note = noteText;
                            BuyerNoteHelper.Update(noteText, buyerNoteTag.NoteEntity);
                        }
                    }
                }
                catch (Exception)
                {
                    string errTip = string.Format("无法保存顾客便签,客服={0},顾客={1}，便签={2}", seller, buyer, noteText);
                    MsgBox.ShowErrDialog(errTip, null);
                }
            }
        }

        private class BuyerNoteTag
        {
            public string BuyerMain;

            public string Seller;

            public string Note;

            public BuyerNoteEntity NoteEntity;
        }
    }
}
