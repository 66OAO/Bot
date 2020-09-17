using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BotLib.Extensions;

namespace BotLib.Wpf.Controls
{
    public class CtlAddRemoveableGroup : ScrollViewer
    {
        private StackPanel _spanel = new StackPanel();
        private int _visibleRows = -1;
        private double _rowHeight;
        private SolidColorBrush _editorBackground;
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(CtlAddRemoveableGroup), new PropertyMetadata("", new PropertyChangedCallback(CtlAddRemoveableGroup.TitleChangedCallback)));

        public int VisibleRows
        {
            get
            {
                return this._visibleRows;
            }
            set
            {
                if (value != this._visibleRows)
                {
                    this._visibleRows = value;
                    base.MaxHeight = this._rowHeight * (double)value;
                }
            }
        }

        public CtlAddRemoveableGroup()
        {
            base.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.AddChild(this._spanel);
            this.AddCtl("", null);
            base.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            this._rowHeight = base.DesiredSize.Height;
            this.VisibleRows = 10;
        }

        public SolidColorBrush EditorBackground
        {
            get
            {
                return this._editorBackground;
            }
            set
            {
                this._editorBackground = value;
                foreach (CtlAddRemoveable ctlAddRemoveable in this._spanel.Children)
                {
                    if (ctlAddRemoveable != null)
                    {
                        ctlAddRemoveable.rtb.Background = this._editorBackground;
                    }
                }
            }
        }

        public void Init(IEnumerable<string> slist)
        {
            this._spanel.Children.Clear();
            if (slist != null)
            {
                foreach (string text in slist)
                {
                    this.AddCtl(text, null);
                }
            }
        }

        public string Title
        {
            get
            {
                return (string)GetValue(CtlAddRemoveableGroup.TitleProperty);
            }
            set
            {
                SetValue(CtlAddRemoveableGroup.TitleProperty, value);
            }
        }

        public new bool HasContent
        {
            get
            {
                List<string> texts = this.Texts;
                bool hasContent;
                if (texts == null || texts.Count == 0)
                {
                    hasContent = false;
                }
                else
                {
                    foreach (string s in texts)
                    {
                        if (!s.xIsNullOrEmptyOrSpace())
                        {
                            return true;
                        }
                    }
                    hasContent = false;
                }
                return hasContent;
            }
        }

        public List<string> Texts
        {
            get
            {
                List<string> list = new List<string>();
                for (int i = 0; i < this._spanel.Children.Count; i++)
                {
                    CtlAddRemoveable ctlAddRemoveable = this._spanel.Children[i] as CtlAddRemoveable;
                    if (ctlAddRemoveable != null)
                    {
                        list.Add(ctlAddRemoveable.Text.Trim());
                    }
                }
                return list;
            }
            set
            {
                this._spanel.Children.Clear();
                foreach (string text in value)
                {
                    CtlAddRemoveable ctlAddRemoveable = this.AddCtl("", null);
                    ctlAddRemoveable.Text = text;
                }
            }
        }

        private static void TitleChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CtlAddRemoveableGroup ctlAddRemoveableGroup = d as CtlAddRemoveableGroup;
            if (ctlAddRemoveableGroup != null)
            {
                ctlAddRemoveableGroup.UpdateTitle();
            }
        }

        public event RoutedEventHandler EvRemove
        {
            add
            {
                AddHandler(CtlAddRemoveable.EvRemoveEvent, value);
            }
            remove
            {
                RemoveHandler(CtlAddRemoveable.EvRemoveEvent, value);
            }
        }

        public event RoutedEventHandler EvAdd
        {
            add
            {
                AddHandler(CtlAddRemoveable.EvAddEvent, value);
            }
            remove
            {
                RemoveHandler(CtlAddRemoveable.EvAddEvent, value);
            }
        }

        public event RoutedEventHandler EvIntoEditMode
        {
            add
            {
                AddHandler(CtlAddRemoveable.EvIntoEditModeEvent, value);
            }
            remove
            {
                RemoveHandler(CtlAddRemoveable.EvIntoEditModeEvent, value);
            }
        }

        public event RoutedEventHandler EvExitEditMode
        {
            add
            {
                AddHandler(CtlAddRemoveable.EvExitEditModeEvent, value);
            }
            remove
            {
                RemoveHandler(CtlAddRemoveable.EvExitEditModeEvent, value);
            }
        }

        public event CtlAddRemoveable.RoutedValidateEventHandler EvValidate
        {
            add
            {
                AddHandler(CtlAddRemoveable.EvValidateEvent, value);
            }
            remove
            {
                RemoveHandler(CtlAddRemoveable.EvValidateEvent, value);
            }
        }

        public event RoutedEventHandler EvSubmit
        {
            add
            {
                AddHandler(CtlAddRemoveable.EvSubmitEvent, value);
            }
            remove
            {
                RemoveHandler(CtlAddRemoveable.EvSubmitEvent, value);
            }
        }


        public bool HasError()
        {
            foreach (object uie in this._spanel.Children)
            {
                CtlAddRemoveable ctlAddRemoveable = uie as CtlAddRemoveable;
                if (ctlAddRemoveable.HasError())
                {
                    return true;
                }
            }
            return false;
        }

        private CtlAddRemoveable AddCtl(string text = "", object insertAfter = null)
        {
            CtlAddRemoveable ctlAddRemoveable = new CtlAddRemoveable(this.Title);
            ctlAddRemoveable.EvAdd += this.Ctl_EvAdd;
            ctlAddRemoveable.EvRemove += this.Ctl_EvRemove;
            ctlAddRemoveable.Margin = new Thickness(5.0);
            ctlAddRemoveable.Text = text;
            if (this.EditorBackground != null)
            {
                ctlAddRemoveable.rtb.Background = this.EditorBackground;
            }
            if (insertAfter == null)
            {
                this._spanel.Children.Add(ctlAddRemoveable);
            }
            else
            {
                int index = this._spanel.Children.IndexOf(insertAfter as UIElement) + 1;
                this._spanel.Children.Insert(index, ctlAddRemoveable);
            }
            this.UpdateTitle();
            return ctlAddRemoveable;
        }

        private void UpdateTitle()
        {
            for (int i = 0; i < this._spanel.Children.Count; i++)
            {
                CtlAddRemoveable ctlAddRemoveable = this._spanel.Children[i] as CtlAddRemoveable;
                if (ctlAddRemoveable != null)
                {
                    ctlAddRemoveable.UpdateTitleWithIndex(i + 1, this.Title);
                }
            }
        }

        private void Ctl_EvRemove(object sender, RoutedEventArgs e)
        {
            this.RemoveCtl(sender);
        }

        private void RemoveCtl(object sender)
        {
            if (sender != null)
            {
                this._spanel.Children.Remove(sender as UIElement);
                if (_spanel.Children.Count == 0)
                {
                    this.AddCtl("", null);
                }
                this.UpdateTitle();
            }
        }

        private void Ctl_EvAdd(object sender, RoutedEventArgs e)
        {
            this.AddCtl("", sender);
        }

        public void FocusEx()
        {
            if (_spanel.Children.Count > 0)
            {
                CtlAddRemoveable ctlAddRemoveable = this._spanel.Children[0] as CtlAddRemoveable;
                ctlAddRemoveable.FocusEx();
            }
        }

    }

}
