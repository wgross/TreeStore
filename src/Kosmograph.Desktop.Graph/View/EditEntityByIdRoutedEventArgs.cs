using System;
using System.Windows;

namespace Kosmograph.Desktop.Graph.View
{
    public class EditEntityByIdRoutedEventArgs : RoutedEventArgs
    {
        public EditEntityByIdRoutedEventArgs(RoutedEvent e, Guid entityId)
            : base(e)
        {
            this.EntityId = entityId;
        }

        public Guid EntityId { get; set; }
    }
}