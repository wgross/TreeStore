using System;
using System.Windows;

namespace Kosmograph.Desktop.Graph.View
{
    public class EditTagByIdRoutedEventArgs : RoutedEventArgs
    {
        public EditTagByIdRoutedEventArgs(RoutedEvent e, Guid tagId)
            : base(e)
        {
            this.TagId = tagId;
        }

        public Guid TagId { get; set; }
    }
}