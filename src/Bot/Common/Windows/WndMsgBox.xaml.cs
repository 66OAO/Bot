﻿using BotLib.Wpf.Extensions;
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
    public partial class WndMsgBox : EtWindow
    {
        private Action<bool> _callback;

        private bool _isYesButtonClicked;

        public WndMsgBox(string message, string title, bool showCancelButton, Action<bool> callback = null)
		{
			this._isYesButtonClicked = false;
			this.InitializeComponent();
			this.tblContent.Inlines.AddRange(InlineEx.ConvertTextToInlineConsiderUrl(message));
			if (!string.IsNullOrEmpty(title))
			{
				Title = title;
			}
			if (!showCancelButton)
			{
				this.btnCancel.Visibility = Visibility.Collapsed;
			}
			this._callback = callback;
			Closed += new EventHandler(this.WndMsgBox_Closed);
		}

        private void WndMsgBox_Closed(object sender, EventArgs e)
        {
            if (_callback != null)
            {
                _callback(this._isYesButtonClicked);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this._isYesButtonClicked = false;
            if (WindowEx.xIsModal(this))
            {
                DialogResult = new bool?(false);
            }
            Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this._isYesButtonClicked = true;
            if (WindowEx.xIsModal(this))
            {
                base.DialogResult = new bool?(true);
            }
            Close();
        }
    }
}
