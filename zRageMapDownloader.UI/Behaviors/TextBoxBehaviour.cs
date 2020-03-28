using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using zRageMapDownloader.Utils;

namespace zRageMapDownloader.Behaviors
{
    public static class TextBoxBehavior
    {
        public static readonly DependencyProperty TextChangedCommand = EventBehaviourFactory.CreateCommandExecutionEventBehaviour(TextBox.TextChangedEvent, "TextChangedCommand", typeof(TextBoxBehavior));

        public static void SetTextChangedCommand(DependencyObject o, ICommand value)
        {
            o.SetValue(TextChangedCommand, value);
        }

        public static ICommand GetTextChangedCommand(DependencyObject o)
        {
            return o.GetValue(TextChangedCommand) as ICommand;
        }
    }
}
