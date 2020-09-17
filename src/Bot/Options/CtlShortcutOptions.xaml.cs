using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Bot.AssistWindow;
using Bot.Help;

namespace Bot.Options
{
    public partial class CtlShortcutOptions : UserControl, IOptions
    {
        private string _seller;
        public CtlShortcutOptions(string seller)
        {
            InitializeComponent();
            InitUI(seller);
        }

        public OptionEnum OptionType
        {
            get
            {
                return OptionEnum.Shortcut;
            }
        }

        public HelpData GetHelpData()
        {
            throw new NotImplementedException();
        }

        public void InitUI(string seller)
        {
            _seller = seller;
            rbtSelf.Content = string.Format("仅显示 {0}【私人】的短语", seller);
            switch (Params.Shortcut.GetShowType(seller))
            {
                case Params.Shortcut.ShowType.ShopOnly:
                    rbtShop.IsChecked = true;
                    break;
                case Params.Shortcut.ShowType.ShopAndSelf:
                    rbtAll.IsChecked = true;
                    break;
                case Params.Shortcut.ShowType.SelfOnly:
                    rbtSelf.IsChecked = true;
                    break;
            }
            cboxShowButtons.IsChecked =Params.Shortcut.GetIsShowTitleButtons(seller);
        }

        public void NavHelp()
        {
            throw new NotImplementedException();
        }

        private void UpdateShortcutUI()
        {
            var wndAssist = WndAssist.FindWndAssist(_seller);
            wndAssist.ctlRightPanel.UpdateShortcutUI();
        }

        public void RestoreDefault()
        {
            Params.Shortcut.SetIsShowTitleButtons(_seller, true);
            Params.Shortcut.SetShowType(_seller, Params.Shortcut.ShowTypeDefault);
            UpdateShortcutUI();
        }

        public void Save(string seller)
        {
            Params.Shortcut.SetIsShowTitleButtons(_seller, cboxShowButtons.IsChecked.HasValue && cboxShowButtons.IsChecked.Value);
            Params.Shortcut.SetShowType(_seller, GetShowType());
            UpdateShortcutUI();
        }

        private Params.Shortcut.ShowType GetShowType()
        {
            if (rbtAll.IsChecked.HasValue && rbtAll.IsChecked.Value)
            {
                return Params.Shortcut.ShowType.ShopAndSelf;
            }
            if (rbtSelf.IsChecked.HasValue && rbtSelf.IsChecked.Value)
            {
                return Params.Shortcut.ShowType.SelfOnly;
            }
            return Params.Shortcut.ShowType.ShopOnly;
        }

    }
}
