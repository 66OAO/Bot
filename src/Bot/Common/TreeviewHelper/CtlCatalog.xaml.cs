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
using BotLib.Wpf.Controls;

namespace Bot.Common.TreeviewHelper
{
    public partial class CtlCatalog : UserControl
    {
        public CtlCatalog()
        {
            InitializeComponent();
        }

        public CtlCatalog(string text, string[] highlightKeys)
        {
            InitializeComponent();
			this.tbkHeader.Text= text;
			this.tbkHeader.HighlightKeys= highlightKeys;
		}

        public void Toggle(bool isExpand)
        {
            if (isExpand)
            {
                this.imgOpen.Visibility = Visibility.Visible;
                this.imgClose.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.imgOpen.Visibility = Visibility.Collapsed;
                this.imgClose.Visibility = Visibility.Visible;
            }
        }

        public string Header
        {
            get
            {
                return this.tbkHeader.Text;
            }
            set
            {
                this.tbkHeader.Text = (value ?? "");
            }
        }
    }
}
