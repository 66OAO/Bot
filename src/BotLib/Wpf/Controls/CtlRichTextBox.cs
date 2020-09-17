using BotLib.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using BotLib.Wpf.Extensions;

namespace BotLib.Wpf.Controls
{
    public class CtlRichTextBox : RichTextBox
    {

        private DelayCaller _dcaller;
        private bool _hasUnhandledInput = false;
        private bool _isImeProcessed = false;

        public static readonly RoutedEvent EvCaretMoveByHumanEvent;

        static CtlRichTextBox()
        {
            EvCaretMoveByHumanEvent = EventManager.RegisterRoutedEvent("EvCaretMoveByHuman", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CtlRichTextBox));
        }

        public event RoutedEventHandler EvCaretMoveByHuman
        {
            add
            {
                base.AddHandler(EvCaretMoveByHumanEvent, value);
            }
            remove
            {
                base.RemoveHandler(EvCaretMoveByHumanEvent, value);
            }
        }

        protected RoutedEventArgs RaiseEvCaretMoveByHumanEvent()
        {
            var e = new RoutedEventArgs(EvCaretMoveByHumanEvent, this);
            base.RaiseEvent(e);
            return e;
        }

        public CtlRichTextBox()
        {
            this.InitDelayCaller();
        }

        public string Text
        {
            get
            {
                return this.GetText(true);
            }
            set
            {
                this.SetText(value);
            }
        }

        private void InitDelayCaller()
        {
            this._dcaller = new DelayCaller(()=>
            {
                if (this._hasUnhandledInput && Keyboard.FocusedElement == this)
                {
                    this._hasUnhandledInput = false;
                    if (!this._isImeProcessed || this.GetTextBeforeCaret(1) != " ")
                    {
                        this.RaiseEvCaretMoveByHumanEvent();
                    }
                }
            }, 100, true);
        }

        public void ClearErrorMark()
        {
            this.GetDocumentRange().ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
        }

        public void ShowErrorMark(SyntaxErrorException err)
        {
            TextRange range = this.GetRange(err.StartIndex, err.Length);
            range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);
            this._hasUnhandledInput = true;
        }

        protected override void OnSelectionChanged(RoutedEventArgs e)
        {
            base.OnSelectionChanged(e);
            this._dcaller.CallAfterDelay();
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);
            this._hasUnhandledInput = true;
            this._isImeProcessed = false;
            this._dcaller.CallAfterDelay();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            this._isImeProcessed = (e.Key == Key.ImeProcessed);
            if (!this._isImeProcessed)
            {
                this._hasUnhandledInput = true;
            }
        }

        public class SyntaxErrorException : Exception
        {
            public string SourceText { get; private set; }
            public string Error { get; private set; }
            public int StartIndex { get; private set; }
            public int Length { get; private set; }

            public SyntaxErrorException(string source, string err, int startIndex, int length)
            {
                this.SourceText = source;
                this.Error = err;
                this.StartIndex = startIndex;
                this.Length = length;
            }

            public override string Message
            {
                get
                {
                    return this.Error;
                }
            }
        }
    }
}
