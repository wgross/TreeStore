using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmograph.Messaging
{
    public class RelationshipChangedMessage : ChangedMessage<IRelationship>
    {
        public RelationshipChangedMessage(ChangeTypeValues changeType, IRelationship changedRelationship)
            : base(changeType, changedRelationship)
        {

        }
    }
}
