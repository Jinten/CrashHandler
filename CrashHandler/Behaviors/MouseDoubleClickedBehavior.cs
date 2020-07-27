using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CrashHandler.Behaviors
{
    public class MouseDoubleClickedBehavior
    {
        public static void SetDoubleClickedCommand(DependencyObject target, ICommand value) => target.SetValue(DoubleClickedCommandProperty, value);
        public static ICommand GetDoubleClickedCommand(DependencyObject target) => (ICommand)target.GetValue(DoubleClickedCommandProperty);

        public static readonly DependencyProperty DoubleClickedCommandProperty = DependencyProperty.RegisterAttached(
            "DoubleClickedCommand",
            typeof(ICommand),
            typeof(MouseDoubleClickedBehavior),
            new PropertyMetadata(null, MouseDoubleClickedCommandPropertyChanged));

        static void MouseDoubleClickedCommandPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as Control;
            if (control == null)
            {
                return;
            }

            if(e.NewValue != null)
            {
                control.MouseDoubleClick += Control_MouseDoubleClick;
            }
            else
            {
                control.MouseDoubleClick -= Control_MouseDoubleClick;
            }
        }

        private static void Control_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            GetDoubleClickedCommand(sender as DependencyObject).Execute(e);
        }
    }
}
