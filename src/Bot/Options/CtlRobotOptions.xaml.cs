using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Bot.AssistWindow;
using Bot.Common;
using Bot.Common.Windows;
using Bot.Help;
using BotLib;
using BotLib.Misc;
using BotLib.Wpf.Extensions;
using BotLib.Extensions;
using DbEntity;

namespace Bot.Options
{
    public partial class CtlRobotOptions : UserControl, IOptions
    {
        private string _seller;
        private string _sellerMain;
        public CtlRobotOptions(string seller)
        {
            InitializeComponent();
            InitUI(seller);
        }

        public OptionEnum OptionType
        {
            get
            {
                return OptionEnum.Robot;
            }
        }

        public HelpData GetHelpData()
        {
            throw new NotImplementedException();
        }

        public void InitUI(string seller)
        {
            _seller = seller;
            _sellerMain = TbNickHelper.GetMainPart(seller);
            cboxCancelAutoOnReset.IsChecked = Params.Robot.CancelAutoOnReset;
            tboxNoAnswerTip.Text = Params.Robot.GetAutoModeNoAnswerTip(_seller);
            tboxAutoDelay.Text = Params.Robot.GetAutoModeReplyDelaySec(_seller).ToString();
            tboxSendDelay.Text = Params.Robot.GetSendModeReplyDelaySec(_seller).ToString();
            cboxAlwaysSend.IsChecked = Params.Robot.GetQuoteModeSendAnswerWhenFullMatch(_seller);
            cboxKeyExclude.IsChecked = Params.Robot.GetRuleIncludeExcept(_sellerMain);
        }

        public void NavHelp()
        {
            throw new NotImplementedException();
        }

        public void RestoreDefault()
        {
            Params.Robot.CancelAutoOnReset = true;
            Params.Robot.SetAutoModeNoAnswerTip(_seller, "亲,目前是机器人值班.这个问题机器人无法回答,等人工客服回来后再回复您.");
            Params.Robot.SetAutoModeReplyDelaySec(_seller, 0);
            Params.Robot.SetSendModeReplyDelaySec(_seller, 0);
            Params.Robot.SetQuoteModeSendAnswerWhenFullMatch(_seller, false);
            Params.Robot.SetRuleIncludeExcept(_sellerMain, false);

        }

        public void Save(string seller)
        {
            Params.Robot.CancelAutoOnReset = cboxCancelAutoOnReset.IsChecked.HasValue && cboxCancelAutoOnReset.IsChecked.Value;
            Params.Robot.SetAutoModeNoAnswerTip(seller, tboxNoAnswerTip.Text);
            Params.Robot.SetAutoModeReplyDelaySec(_seller, ConvertEx.ToInt32Safe(tboxAutoDelay.Text.Trim(), 0));
            Params.Robot.SetSendModeReplyDelaySec(_seller, ConvertEx.ToInt32Safe(tboxSendDelay.Text.Trim(), 0));
            Params.Robot.SetQuoteModeSendAnswerWhenFullMatch(_seller, cboxAlwaysSend.IsChecked.HasValue && cboxAlwaysSend.IsChecked.Value);

            bool ruleIncludeExcept = Params.Robot.GetRuleIncludeExcept(_sellerMain);
            if (ruleIncludeExcept != (cboxKeyExclude.IsChecked.HasValue && cboxKeyExclude.IsChecked.Value))
            {
                Params.Robot.SetRuleIncludeExcept(_sellerMain, cboxKeyExclude.IsChecked.HasValue && cboxKeyExclude.IsChecked.Value);
                var wndAssist = WndAssist.FindWndAssist(_seller);
                if (wndAssist != null)
                {
                    wndAssist.ctlRightPanel.UpdateRobotTab();
                }
            }
        }

        private void tboxNoAnswerTip_LostFocus(object sender, RoutedEventArgs e)
        {
            string value = tboxNoAnswerTip.Text.Trim();
            if (string.IsNullOrEmpty(value))
            {
                string string_ = "确定要清空？\r\n\r\n若清空，找不到回复答案时，将直接关闭顾客，不会回复顾客的问题！";
                MsgBox.ShowTip(string_, (bool_0) =>
                {
                    if (!bool_0)
                    {
                        tboxNoAnswerTip.Text = "亲,目前是机器人值班.这个问题机器人无法回答,等人工客服回来后再回复您.";
                    }
                }, null, null);
            }
        }

        private void tboxAutoDelay_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string value = tboxAutoDelay.Text.Trim();
                if (!string.IsNullOrEmpty(value))
                {
                    int n = Convert.ToInt32(value);
                    if (n < 0 || n > 10)
                    {
                        throw new Exception();
                    }
                }
            }
            catch
            {
                Util.Beep();
                tboxAutoDelay.Text = Params.Robot.GetAutoModeReplyDelaySec(_seller).ToString();
                MsgBox.ShowDialog("请输入0~10之间的整数", this);
            }
        }

        private void cboxKeyExclude_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void rbtNo_Click(object sender, RoutedEventArgs e)
        {
            spnOther.Visibility = Visibility.Collapsed;
        }

        private void rbtAllow_Click(object sender, RoutedEventArgs e)
        {
            spnOther.Visibility = Visibility.Visible;
        }

        private void tboxSendDelay_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string value = tboxSendDelay.Text.Trim();
                if (!string.IsNullOrEmpty(value))
                {
                    int n = Convert.ToInt32(value);
                    if (n < 0 || n > 10)
                    {
                        throw new Exception();
                    }
                }
            }
            catch
            {
                Util.Beep();
                tboxSendDelay.Text = Params.Robot.GetSendModeReplyDelaySec(_seller).ToString();
                MsgBox.ShowDialog("请输入0~10之间的整数", this);
            }
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
