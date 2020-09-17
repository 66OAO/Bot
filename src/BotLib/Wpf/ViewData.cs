﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using BotLib.Extensions;
using BotLib.Misc;

namespace BotLib.Wpf
{
    public class ViewData : IDataErrorInfo
    {
        private const int _validateDelay = 400;
        public const string RequiredValidateString = "必填!";
        private Window _wnd;
        private static ControlTemplate _errTemplate;
        private DelayCaller _validateCaller;
        private ConcurrentDictionary<TextBox, DateTime> _textBoxChangedTimeDict = new ConcurrentDictionary<TextBox, DateTime>();
        private Dictionary<string, ViewData.BindingInfo> _bdict = new Dictionary<string, ViewData.BindingInfo>();
        private int _errCount = 0;
        private class BindingInfo
        {
            public string PropertyName;
            public TextBox TextBox;
            public Func<string> Validator;
        }
        public ViewData(Window wnd)
        {
            this._wnd = wnd;
            wnd.DataContext = this;
            System.Windows.Controls.Validation.AddErrorHandler(this._wnd, new EventHandler<ValidationErrorEventArgs>(this.OnValidateError));
            this.InitErrorTemplate();
            this._validateCaller = new DelayCaller(new Action(this.TextChangedValidate), 400, true);
        }

        public void SetBinding(string property, TextBox tbox, Func<string> validator = null)
        {
            this._bdict[property] = new ViewData.BindingInfo
            {
                PropertyName = property,
                TextBox = tbox,
                Validator = validator
            };
            Binding binding = new Binding(property);
            if (validator != null)
            {
                binding.ValidatesOnDataErrors = true;
                binding.NotifyOnValidationError = true;
                System.Windows.Controls.Validation.SetErrorTemplate(tbox, ViewData._errTemplate);
                tbox.TextChanged += this.Tbox_TextChanged;
            }
            tbox.SetBinding(TextBox.TextProperty, binding);
        }

        public bool Submitable()
        {
            return !this.HasError && this.ValidateAll();
        }

        private void TextChangedValidate()
        {
            if (this.IsTextBoxValidateTimeout())
            {
                foreach (var kv in this._textBoxChangedTimeDict)
                {
                    this.ValidateTextBox(kv.Key);
                }
            }
        }

        private bool IsTextBoxValidateTimeout()
        {
            bool isTimeout = false;
            foreach (var kv in this._textBoxChangedTimeDict)
            {
                isTimeout = kv.Value.xIsTimeElapseMoreThanMs(200);
                if (isTimeout)
                {
                    break;
                }
            }
            return isTimeout;
        }

        private void InitErrorTemplate()
        {
            if (ViewData._errTemplate == null)
            {
                string xamlText = "<ControlTemplate  xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'><Border BorderBrush=\"Red\" BorderThickness=\"1\"><Grid><AdornedElementPlaceholder x:Name=\"_el\" /><TextBlock Text=\"{Binding [0].ErrorContent}\" Foreground=\"Red\" HorizontalAlignment=\"Right\" VerticalAlignment=\"Center\" Margin=\"0,0,6,0\"/></Grid></Border></ControlTemplate>";
                ViewData._errTemplate = (XamlReader.Parse(xamlText) as ControlTemplate);
                Util.Assert(ViewData._errTemplate != null);
            }
        }

        private void Tbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                this._textBoxChangedTimeDict[textBox] = DateTime.Now;
                this._validateCaller.CallAfterDelay();
            }
        }

        private bool ValidateAll()
        {
            foreach (var kv in this._bdict)
            {
                if (kv.Value.Validator != null)
                {
                    this.ValidateTextBox(kv.Value.TextBox);
                }
            }
            return !this.HasError;
        }

        public bool HasError
        {
            get
            {
                return this._errCount > 0;
            }
        }

        private void ValidateTextBox(TextBox textBox)
        {
            BindingExpression bindingExpression = BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty);
            if (bindingExpression != null)
            {
                bindingExpression.UpdateSource();
            }
        }

        private void OnValidateError(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                this._errCount++;
            }
            else
            {
                this._errCount--;
            }
        }

        public string this[string propName]
        {
            get
            {
                string val;
                if (this._wnd.IsLoaded && this._bdict.ContainsKey(propName) && this._bdict[propName].Validator != null)
                {
                    val = this._bdict[propName].Validator();
                }
                else
                {
                    val = null;
                }
                return val;
            }
        }

        public string Error
        {
            get
            {
                return null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetValue<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propName = null)
		{
			if (!EqualityComparer<T>.Default.Equals(field, value))
			{
				field = value;
                if (PropertyChanged != null)
				{
                    PropertyChanged(this, new PropertyChangedEventArgs(propName));
				}
			}
		}

    }
}
