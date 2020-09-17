using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Bot.AssistWindow;
using Bot.Common;
using Bot.Common.Windows;
using Bot.Help;
using BotLib.Extensions;
using BotLib.Wpf.Extensions;

namespace Bot.Options
{
    public partial class CtlOtherOptions : UserControl, IOptions
    {
        private string _seller;
        public CtlOtherOptions(string seller)
        {
            InitializeComponent();
            InitUI(seller);
        }

        public OptionEnum OptionType
        {
            get
            {
                return OptionEnum.Other;
            }
        }

        public HelpData GetHelpData()
        {
            throw new NotImplementedException();
        }

        public void InitUI(string seller)
        {
            _seller = seller;
            cboxIme.IsChecked = Params.ForceActiveIME;
            switch (Params.Other.FontSize)
            {
                case 12:
                    rbtDefault.IsChecked = true;
                    rbtBigger.IsChecked = false;
                    rbtBigist.IsChecked = false;
                    break;
                case 13:
                    rbtDefault.IsChecked = false;
                    rbtBigger.IsChecked = true;
                    rbtBigist.IsChecked = false;
                    break;
                case 14:
                    rbtDefault.IsChecked = false;
                    rbtBigger.IsChecked = false;
                    rbtBigist.IsChecked = true;
                    break;
                default:
                    MsgBox.ShowDialog("字体大小出错", null, null);
                    IsEnabled = false;
                    break;
            }
        }

        public void NavHelp()
        {
            throw new NotImplementedException();
        }
        public void RestoreDefault()
        {
            Params.ForceActiveIME = true;
            Params.Other.FontSize = 12;
            SetAppFontSize();
            InitUI(_seller);
        }

        public void Save(string seller)
        {
            Params.ForceActiveIME = (cboxIme.IsChecked.HasValue && cboxIme.IsChecked.Value);
            if (rbtDefault.IsChecked.HasValue && rbtDefault.IsChecked.Value)
            {
                Params.Other.FontSize = 12;
            }

            if (rbtBigger.IsChecked.HasValue && rbtBigger.IsChecked.Value)
            {
                Params.Other.FontSize = 13;
            }

            if (rbtBigist.IsChecked.HasValue && rbtBigist.IsChecked.Value)
            {
                Params.Other.FontSize = 14;
            }
            SetAppFontSize();
        }

        private void SetAppFontSize()
        {
            var appWindows = WindowEx.GetAppWindows<WndAssist>();
            foreach (WndAssist wndAssist in appWindows.xSafeForEach())
            {
                wndAssist.FontSize =Params.Other.FontSize;
            }
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            WndNotTipAgain.DeleteWndNotTipAgainNeedShow();
            MsgBox.ShowDialog("已恢复", null, this, null);
        }

    }
}
