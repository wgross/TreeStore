using Kosmograph.Messaging;
using Moq;
using System;

namespace Kosmograph.LiteDb.Test
{
    public class LiteDbTestBase
    {
        protected MockRepository mocks { get; } = new MockRepository(MockBehavior.Strict);

        protected KosmographMessageBus MessageBus { get; }

        protected KosmographLiteDbPersistence Persistence { get; }

        public LiteDbTestBase()
        {
            this.MessageBus = new KosmographMessageBus();
            this.Persistence = new KosmographLiteDbPersistence(this.MessageBus);
        }

        protected T Setup<T>(T t, Action<T> setup = null)
        {
            setup?.Invoke(t);
            return t;
        }
    }
}