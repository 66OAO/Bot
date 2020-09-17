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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BotLib.Extensions;
using BotLib;
using BotLib.Wpf.Extensions;
using DbEntity;

namespace Bot.Common.Windows
{
    public partial class EtWindow : Window, System.Windows.Forms.IWin32Window
    {
        public IntPtr Handle
        {
            get
            {
                return new WindowInteropHelper(this).Handle;
            }
        }

        private string TitleTail
        {
            get
            {
                return string.IsNullOrEmpty(this.Seller) ? "" : string.Format(" --- {0}", this.Seller);
            }
        }

        private string _seller;
        public string Seller
        {
            get
            {
                return this._seller;
            }
            set
            {
                this._seller = value;
                this.Title = this.Title;
            }
        }

        public new string Title
        {
            get
            {
                return base.Title.xTrimIfEndWith(this.TitleTail);
            }
            set
            {
                base.Title = value.xAppendIfNotEndWith(this.TitleTail);
            }
        }
        public EtWindow()
        {

            this.Loaded += EtWindow_Loaded;
        }

        void EtWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitShowLocation();
        }

        public void ShowTip(string message, string title = null)
        {
            MsgBox.ShowTip(message, title, this, null);
        }

        private void InitShowLocation()
        {
            if (this.Owner != null && (this.ActualWidth > this.Owner.ActualWidth - 30.0 || this.ActualHeight > this.Owner.ActualHeight - 30.0))
            {
                this.Left = this.Owner.Left + 30.0;
                this.Top = this.Owner.Top + 30.0;
            }
        }


        public static T GetMainNickSameWindow<T>(string seller) where T : EtWindow
        {
            List<T> appWindows = WindowEx.GetAppWindows<T>();
            foreach (T t in appWindows.xSafeForEach<T>())
            {
                if (TbNickHelper.IsSameShopAccount(t.Seller, seller))
                {
                    return t;
                }
            }
            return default(T);
        }

        public static T GetSubNickSameWindow<T>(string seller) where T : EtWindow
        {
            List<T> appWindows = WindowEx.GetAppWindows<T>();
            foreach (T t in appWindows.xSafeForEach<T>())
            {
                if (t.Seller == seller)
                {
                    return t;
                }
            }
            return default(T);
        }

        public void FirstShow(string seller = null, Window owner = null, Action callback = null, bool startUpCenterOwner = false)
        {
            this.Seller = seller;
            this.xSetOwner(owner);
            this.xSetStartUpLocation(startUpCenterOwner);
            this.xKeepWindowFullVisibleAtResize();
            Closed += (sender,e) => {
                if(callback !=null) callback();
                if(Owner !=null)
                    this.Owner.xBrintTop();
            };
            this.xShowFirstTime();
        }

        public bool? ShowDialogEx(Window owner, string seller = null, bool startUpCenterOwner = false)
        {
            bool? dlg = null;
            try
            {
                this.Seller = (seller ?? this.Seller);
                this.xSetOwner(owner);
                this.xSetStartUpLocation(startUpCenterOwner);
                this.xKeepWindowFullVisibleAtResize();
                dlg = ShowDialog();
                if (owner != null)
                {
                    owner.xBrintTop();
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            return dlg;
        }

        public static T ShowSameShopOneInstance<T>(string seller, Func<T> creater = null, Window owner = null) where T : EtWindow
        {
            T t = EtWindow.GetMainNickSameWindow<T>(seller);
            if (t == null)
            {
                if (creater == null)
                {
                    t = Activator.CreateInstance<T>();
                }
                else
                {
                    t = creater();
                }
                t.FirstShow(seller, owner, null, false);
            }
            else
            {
                t.xReShow();
            }
            return t;
        }

        public static T ShowSameNickOneInstance<T>(string seller, Func<T> creater = null, Window owner = null, bool startUpCenterOwner = false) where T : EtWindow
        {
            T t = EtWindow.GetSubNickSameWindow<T>(seller);
            if (t == null)
            {
                if (creater == null)
                {
                    t = Activator.CreateInstance<T>();
                }
                else
                {
                    t = creater();
                }
                t.FirstShow(seller, owner, null, startUpCenterOwner);
            }
            else
            {
                t.xReShow();
            }
            return t;
        }
    }
}
