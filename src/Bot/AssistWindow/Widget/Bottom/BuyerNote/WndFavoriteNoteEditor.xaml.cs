using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Bot.Common;
using Bot.Common.Windows;
using BotLib.Wpf;
using BotLib.Wpf.Extensions;
using DbEntity;

namespace Bot.AssistWindow.Widget.Bottom.BuyerNote
{
    public partial class WndFavoriteNoteEditor : EtWindow
    {
        private string _sellerMain;
        private WndFavoriteNoteEditor.VdWndFavoriteNoteEditor _data;
        private FavoriteNoteEntity _result;
        private FavoriteNoteEntity _pre;

        public WndFavoriteNoteEditor(string sellerMain, FavoriteNoteEntity preFavNote = null)
        {
            InitializeComponent();
            base.Title = ((preFavNote == null) ? "新增【常用】顾客便签" : "编辑【常用】顾客便签");
            _data = new WndFavoriteNoteEditor.VdWndFavoriteNoteEditor((preFavNote != null) ? preFavNote.Note : null, this, null);
            _pre = preFavNote;
            _sellerMain = sellerMain;
        }

        public static void Create(string sellerMain, Window owner, Action onClosed)
        {
            WndFavoriteNoteEditor wndFavoriteNoteEditor = EtWindow.ShowSameShopOneInstance<WndFavoriteNoteEditor>(sellerMain, new Func<WndFavoriteNoteEditor>(() =>
            {
                return new WndFavoriteNoteEditor(sellerMain);
            }), owner);
            wndFavoriteNoteEditor.Closed += (s, e) =>
            {
                if (onClosed != null) onClosed();
            };
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (_data.Submitable())
            {
                if (this.xIsModal())
                {
                    base.DialogResult = new bool?(true);
                }
                if (_pre == null)
                {
                    _result = EntityHelper.Create<FavoriteNoteEntity>(_sellerMain);
                    _result.Creator = _sellerMain;
                    _result.Note = _data.Text;
                }
                else
                {
                    _result = _pre;
                    _result.Creator = _sellerMain;
                    _result.Note = _data.Text;
                }
                DbHelper.SaveToDb(_result, true);
                Close();
            }
        }

        public static void Edit(string sellerMain, FavoriteNoteEntity noteEntity, Window owner, Action onClosed)
        {
            WndFavoriteNoteEditor wndFavoriteNoteEditor = new WndFavoriteNoteEditor(sellerMain, noteEntity);
            wndFavoriteNoteEditor.FirstShow(sellerMain, owner, onClosed, false);
        }

        private class VdWndFavoriteNoteEditor : ViewData
        {
            private WndFavoriteNoteEditor _wnd;
            public string Text { get; set; }

            public VdWndFavoriteNoteEditor(string txt, WndFavoriteNoteEditor wnd, Func<string, string> func = null)
                : base(wnd)
            {
                Text = (txt ?? "");
                _wnd = wnd;
                base.SetBinding("Text", wnd.tboxContent, () =>
                {
                    return string.IsNullOrEmpty(Text) ? "必填!" : null;
                });
            }
        }

    }
}
