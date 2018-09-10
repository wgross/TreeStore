using System.Windows.Input;

namespace Kosmograph.Desktop.Dialogs
{
    public static class DialogCommands
    {
        public static RoutedCommand Ok = new RoutedCommand();
        public static RoutedCommand Cancel = new RoutedCommand();
        public static RoutedCommand Escape = new RoutedCommand();
    }
}