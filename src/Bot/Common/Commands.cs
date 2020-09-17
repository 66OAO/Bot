using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Bot.Common
{
    public static class Commands
    {
        public static RoutedUICommand New
        {
            get
            {
                return Commands.V.NewCommand;
            }
        }

        public static RoutedUICommand Edit
        {
            get
            {
                return Commands.V.EditCommand;
            }
        }

        public static RoutedUICommand Clear
        {
            get
            {
                return Commands.V.ClearCommand;
            }
        }

        public static RoutedUICommand Delete
        {
            get
            {
                return Commands.V.DeleteCommand;
            }
        }

        public static RoutedUICommand Submit
        {
            get
            {
                return Commands.V.SubmitCommand;
            }
        }

        public static RoutedUICommand Cancel
        {
            get
            {
                return Commands.V.CancelCommand;
            }
        }

        public static RoutedUICommand Toggle
        {
            get
            {
                return Commands.V.ToggleCommand;
            }
        }

        private class V
        {

            static V()
            {
                Commands.V.NewCommand = new RoutedUICommand("新建", "New", typeof(Commands), new InputGestureCollection(new KeyGesture[]
				{
					new KeyGesture(Key.N, ModifierKeys.Alt)
				}));
                Commands.V.EditCommand = new RoutedUICommand("编辑", "Edit", typeof(Commands), new InputGestureCollection(new KeyGesture[]
				{
					new KeyGesture(Key.E, ModifierKeys.Alt)
				}));
                Commands.V.DeleteCommand = new RoutedUICommand("删除", "Delete", typeof(Commands), new InputGestureCollection(new KeyGesture[]
				{
					new KeyGesture(Key.Delete)
				}));
                Commands.V.ClearCommand = new RoutedUICommand("清空", "Clear", typeof(Commands), new InputGestureCollection(new KeyGesture[]
				{
					new KeyGesture(Key.K, ModifierKeys.Alt)
				}));
                Commands.V.SubmitCommand = new RoutedUICommand("确定", "Submit", typeof(Commands), new InputGestureCollection(new KeyGesture[]
				{
					new KeyGesture(Key.Y, ModifierKeys.Alt)
				}));
                Commands.V.CancelCommand = new RoutedUICommand("取消", "Cancel", typeof(Commands), new InputGestureCollection(new KeyGesture[]
				{
					new KeyGesture(Key.Escape)
				}));
                Commands.V.ToggleCommand = new RoutedUICommand("切换", "Toggle", typeof(Commands), new InputGestureCollection(new KeyGesture[]
				{
					new KeyGesture(Key.T, ModifierKeys.Alt)
				}));
            }

            public static readonly RoutedUICommand NewCommand;

            public static readonly RoutedUICommand EditCommand;

            public static readonly RoutedUICommand DeleteCommand;

            public static readonly RoutedUICommand ClearCommand;

            public static readonly RoutedUICommand SubmitCommand;

            public static readonly RoutedUICommand CancelCommand;

            public static readonly RoutedUICommand ToggleCommand;
        }
    }
}
