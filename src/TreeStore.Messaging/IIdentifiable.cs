using System;

namespace TreeStore.Messaging
{
    public interface IIdentifiable
    {
        Guid Id { get; }
    }
}