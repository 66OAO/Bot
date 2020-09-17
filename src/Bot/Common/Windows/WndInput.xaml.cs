using BotLib;
using BotLib.Wpf;
using BotLib.Wpf.Extensions;
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

namespace Bot.Common.Windows
{
    public partial class WndInput : EtWindow
    {
        private WndInput(string tip, string title, string inputText, string helpUrl, Func<string, string> func)
        {
            this._isClosedByOkButton = false;
            this.InitializeComponent();
            this.tboxInput.Text = (inputText ?? "");
            if (!string.IsNullOrEmpty(tip))
            {
                tblkTip.Text = tip;
            }
            if (!string.IsNullOrEmpty(title))
            {
                Title = title;
            }
            if (!string.IsNullOrEmpty(helpUrl))
            {
                this.btnHelp.Visibility = Visibility.Visible;
                this._helpUrl = helpUrl;
            }
            this._data = new WndInput.VdWndInput(inputText, this, func);
        }

        public string Result
        {
            get
            {
                return this._data.Text;
            }
        }

        public static string MyShowDialog(string tip, string title, string inputDef = null, string helpurl = null, Func<string, string> validator = null, Window owner = null, bool startUpCenterOwner = false)
        {
            var txt = string.Empty;
            DispatcherEx.xInvoke(() =>
            {
                var editor = new WndInput(tip, title, inputDef, helpurl, validator);
                editor.Owner = owner;
                WindowEx.xSetStartUpLocation(editor, startUpCenterOwner);
                var dlgRlt = editor.ShowDialogEx(owner);
                if (dlgRlt.HasValue && dlgRlt.Value)
                {
                    txt = editor.Result;
                }
            });
            return txt;
        }

        public static void MyShow(string tip, string title, Action<string> callback, string inputDef = null, string helpurl = null, Func<string, string> validator = null, Window owner = null, bool startUpCenterOwner = false)
        {
            var editor = new WndInput(tip, title, inputDef, helpurl, validator);
            editor.Owner = owner;
            editor.Closed += (sender, e) =>
            {
                if (editor._isClosedByOkButton)
                {
                    callback(editor.Result);
                }
            };
            WindowEx.xSetStartUpLocation(editor, startUpCenterOwner);
            WindowEx.xShowFirstTime(editor);
        }

        static void editor_Closed(object sender, EventArgs e)
        {
            var wndInput = sender as WndInput;

        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            Util.Nav(this._helpUrl);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (WindowEx.xIsModal(this))
            {
                DialogResult = false;
            }
            Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this._data.Submitable())
            {
                if (WindowEx.xIsModal(this))
                {
                    DialogResult = true;
                }
                this._isClosedByOkButton = true;
                Close();
            }
        }

        private void tboxInput_Loaded(object sender, RoutedEventArgs e)
        {
            this.tboxInput.SelectionStart = this.tboxInput.Text.Length;
        }

        private string _helpUrl;

        private WndInput.VdWndInput _data;

        private bool _isClosedByOkButton;

        private class VdWndInput : ViewData
        {
            public string Text
            {
                get;
                set;
            }

            public VdWndInput(string text, WndInput wndInput, Func<string, string> func = null)
                : base(wndInput)
            {
                this.Text = (text ?? "");
                this._wnd = wndInput;
                SetBinding("Text", wndInput.tboxInput, () => func(this.Text));
            }

            private WndInput _wnd;
        }
    }
}
