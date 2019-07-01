using System;
using System.Windows;

namespace Kosmograph.Desktop.Graph.View
{
    public class EditRelationshipByIdRoutedEventArgs : RoutedEventArgs
    {
        public EditRelationshipByIdRoutedEventArgs(RoutedEvent e, Guid relationshipId)
            : base(e)
        {
            this.RelationshipId = relationshipId;
        }

        public Guid RelationshipId { get; set; }
    }
}