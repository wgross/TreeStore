using LiteDB;
using Moq;
using System;
using System.IO;
using TreeStore.Messaging;
using TreeStore.Model;

namespace TreeStore.LiteDb.Test
{
    public abstract class LiteDbTestBase : IDisposable
    {
        protected MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);

        protected TreeStoreMessageBus MessageBus { get; }
        protected Mock<IChangedMessageBus<IEntity>> EntityEventSource { get; }

        protected EntityRepository EntityRepository { get; }
        protected Mock<IChangedMessageBus<ITag>> TagEventSource { get; }

        protected TagRepository TagRepository { get; }

        protected CategoryRepository CategoryRepository { get; }

        protected LiteRepository LiteDb { get; } = new LiteRepository(new MemoryStream());

        public LiteDbTestBase()
        {
            this.MessageBus = new TreeStoreMessageBus();
            this.EntityEventSource = this.Mocks.Create<IChangedMessageBus<IEntity>>();
            this.EntityRepository = new EntityRepository(this.LiteDb, this.EntityEventSource.Object);
            this.TagEventSource = this.Mocks.Create<IChangedMessageBus<ITag>>();
            this.TagRepository = new TagRepository(this.LiteDb, this.TagEventSource.Object);
            this.CategoryRepository = new CategoryRepository(this.LiteDb);
        }

        protected T Setup<T>(T t, Action<T> setup = null)
        {
            setup?.Invoke(t);
            return t;
        }

        public void Dispose() => this.Mocks.VerifyAll();

        #region Default Entity

        protected Entity DefaultEntity(params Action<Entity>[] setup)
        {
            var tmp = new Entity("e");
            WithEntityCategory(this.CategoryRepository.Root())(tmp);

            setup.ForEach(s => s(tmp));
            return tmp;
        }

        public static void WithoutTags(Entity entity) => entity.Tags.Clear();

        public static Action<Entity> WithEntityCategory(Category c) => e => e.SetCategory(c);

        public static void WithoutCategory(Entity e) => e.Category = null;

        #endregion Default Entity

        #region Default Category

        protected Category DefaultCategory(params Action<Category>[] setup)
        {
            var tmp = new Category("c");
            this.CategoryRepository.Root().AddSubCategory(tmp);
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        protected Action<Category> WithParentCategory(Category parentCategory)
        {
            return c =>
            {
                c.Parent.DetachSubCategory(c);
                parentCategory.AddSubCategory(c);
            };
        }

        #endregion Default Category
    }
}