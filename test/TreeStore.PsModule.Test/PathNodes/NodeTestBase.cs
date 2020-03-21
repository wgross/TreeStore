using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using TreeStore.LiteDb;
using TreeStore.Model;

namespace TreeStore.PsModule.Test.PathNodes
{
    public abstract class NodeTestBase : IDisposable
    {
        protected MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);

        protected Mock<ITreeStoreProviderContext> ProviderContextMock { get; }

        protected Mock<ITreeStorePersistence> PersistenceMock { get; }

        protected Mock<IEntityRepository> EntityRepositoryMock { get; }

        public Mock<ICategoryRepository> CategoryRepositoryMock { get; }

        protected Mock<ITagRepository> TagRepositoryMock { get; }

        public NodeTestBase()
        {
            this.ProviderContextMock = this.Mocks.Create<ITreeStoreProviderContext>();
            this.PersistenceMock = this.Mocks.Create<ITreeStorePersistence>();
            this.EntityRepositoryMock = this.Mocks.Create<IEntityRepository>();
            this.CategoryRepositoryMock = this.Mocks.Create<ICategoryRepository>();
            this.TagRepositoryMock = this.Mocks.Create<ITagRepository>();
        }

        public void Dispose() => this.Mocks.VerifyAll();

        public static IEnumerable<object[]> InvalidNameChars => System.IO.Path.GetInvalidFileNameChars().Select(c => new object[] { c });

        #region Default Tag

        protected Tag DefaultTag(params Action<Tag>[] setup)
        {
            var tmp = new Tag("t", new Facet("f", new FacetProperty("p", FacetPropertyTypeValues.String)));
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        protected static void WithDefaultProperty(Tag tag)
        {
            tag.Facet.Properties.Clear();
            tag.Facet.AddProperty(new FacetProperty("p", FacetPropertyTypeValues.String));
        }

        protected static void WithoutProperty(Tag tag) => tag.Facet.Properties.Clear();

        protected static Action<Tag> WithProperty(string name, FacetPropertyTypeValues type)
        {
            return tag => tag.Facet.AddProperty(new FacetProperty(name, type));
        }

        #endregion Default Tag

        #region Default Entity

        protected Entity DefaultEntity(params Action<Entity>[] setup)
        {
            var tmp = new Entity("e");
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        protected void WithAssignedDefaultTag(Entity entity) => entity.Tags.Add(DefaultTag(WithDefaultProperty));

        protected Action<Entity> WithAssignedTag(Tag tag) => e => e.Tags.Add(tag);

        protected Action<Entity> WithDefaultPropertySet<V>(V value)
            => e => e.SetFacetProperty(e.Tags.First().Facet.Properties.First(), value);

        protected void WithoutTags(Entity entity) => entity.Tags.Clear();

        protected Action<Entity> WithEntityCategory(Category category)
        {
            return e => e.Category = category;
        }

        #endregion Default Entity

        #region Default Category

        public void ArrangeRootCategory(out Category rootCategory)
        {
            var rootCategory_ = rootCategory = DefaultCategory(AsRoot);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            this.CategoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(rootCategory);
        }

        public void ArrangeSubCategory(out Category rootCategory, out Category subCategory)
        {
            var rootCategory_ = rootCategory = DefaultCategory(AsRoot);
            var subCategory_ = subCategory = DefaultCategory();
            rootCategory.AddSubCategory(subCategory);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);
        }

        protected static Category DefaultCategory(params Action<Category>[] setup)
        {
            var tmp = new Category("c");
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        protected static Action<Category> WithSubCategory(Category subcategory)
            => c => c.AddSubCategory(subcategory);

        protected static void AsRoot(Category category)
        {
            category.Id = CategoryRepository.CategoryRootId;
            category.Parent = null;
            category.Name = string.Empty;
        }

        #endregion Default Category
    }
}