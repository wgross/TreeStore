using System;

namespace Kosmograph.Messaging
{
    public interface IIdentifiable
    {
        Guid Id { get; }
    }
}