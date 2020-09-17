using Bot.Automation.ChatDeskNs;
using System;
using System.Collections.Concurrent;
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
using BotLib;
using BotLib.Extensions;
using BotLib.Wpf.Extensions;

namespace Bot.AssistWindow.Widget.Robot
{
    /// <summary>
    /// CtlRobotQa.xaml 的交互逻辑
    /// </summary>
    public partial class CtlRobotQA : UserControl
    {
        private RightPanel _rightPanel;
        private WndAssist _wnd;
        private static ConcurrentBag<string> _adjustedBag;
        private bool _isInit;
        private TabItem _parentTab;
        private TabControl _tabControl;
        private ChatDesk _desk;
        private WndAssist _wndDontUse;
        public string Seller { get; private set; }
        public ContextMenu MenuQuestion { get; private set; }
        //public RuleEditor RuleEditor { get; private set; }

        public CtlRobotQA(WndAssist wnd)
        {
            this._isInit = false;
			this._wnd = wnd;
			this.InitializeComponent();
			this.tvMain.xSetRightClickSelectTreeviewItem();
			this.Init();
			Loaded += this.CtlRobotQA_Loaded;
            SizeChanged += this.CtlRobotQA_Loaded;
        }

        private void Init()
        {
            this._desk = this._wnd.Desk;
            this.Seller = this._desk.Seller;
            this.MenuQuestion = (ContextMenu)base.FindResource("menuQuestion");
        }

        private void CtlRobotQA_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btOption_Click(object sender, RoutedEventArgs e)
        {
            //WndOption.MyShow(this._desk.Seller, this.Wnd, OptionEnum.Robot, null);
        }

        private void cboxAuto_Click(object sender, RoutedEventArgs e)
        {
        }

        private void mNewRule_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mAppendRule_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mOpenRuleManager_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mSend_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mInput_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mCopy_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mRefresh_Click(object sender, RoutedEventArgs e)
        {

        }
        private void mHelp_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
