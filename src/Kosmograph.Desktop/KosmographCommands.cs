using System.Windows.Input;

namespace Kosmograph.Desktop
{
    public static class KosmographCommands
    {
        public static RoutedCommand CreateEntity = new RoutedCommand();

        public static RoutedCommand EditEntity = new RoutedCommand();

        public static RoutedCommand CreateTag = new RoutedCommand();

        public static RoutedCommand EditTag = new RoutedCommand();
    }
}