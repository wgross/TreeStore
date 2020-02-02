using Moq;
using System;
using System.Management.Automation;
using TreeStore.LiteDb;
using TreeStore.Model;
using Xunit;

namespace TreeStore.PsModule.Test
{
    [Collection("UsesPowershell")]
    public class TreeStoreCmdletProviderTestBase : IDisposable
    {
        public MockRepository Mocks { get; }

        public Mock<ITreeStorePersistence> PersistenceMock { get; }

        public Mock<ITagRepository> TagRepositoryMock { get; }

        public Mock<IEntityRepository> EntityRepositoryMock { get; }

        public Mock<ICategoryRepository> CategoryRepositoryMock { get; }

        public Mock<IRelationshipRepository> RelationshipRepositoryMock { get; }

        public PowerShell PowerShell { get; }

        public TreeStoreCmdletProviderTestBase()
        {
            this.Mocks = new MockRepository(MockBehavior.Strict);
            this.PersistenceMock = this.Mocks.Create<ITreeStorePersistence>();
            this.TagRepositoryMock = this.Mocks.Create<ITagRepository>();
            this.EntityRepositoryMock = this.Mocks.Create<IEntityRepository>();
            this.RelationshipRepositoryMock = this.Mocks.Create<IRelationshipRepository>();
            this.CategoryRepositoryMock = this.Mocks.Create<ICategoryRepository>();
            this.PowerShell = PowerShell.Create();

            // inject mock of the treestore model
            TreeStoreCmdletProvider.NewTreeStorePersistence = _ => this.PersistenceMock.Object;

            this.PowerShell
                .AddStatement()
                    .AddCommand("Import-Module")
                    .AddArgument("./TreeStore.dll")
                    .Invoke();

            this.PowerShell
                .AddStatement()
                    .AddCommand("New-PsDrive")
                        .AddParameter("Name", "kg")
                        .AddParameter("PsProvider", "TreeStore")
                        .AddParameter("Root", string.Empty) // in memory model
                        .Invoke();

            this.PowerShell.Commands.Clear();
        }

        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Mocks.VerifyAll();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            this.Dispose(true);
        }

        #endregion IDisposable Members

        #region Arrangements

        protected void ArrangeEmptyRootCategory(out Category rootCategory)
        {
            rootCategory = DefaultCategory(AsRoot);
            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            this.CategoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(rootCategory);
        }

        protected void ArrangeSubCategory(out Category rootCategory, out Category subCategory)
        {
            var subCategory_ = subCategory = DefaultCategory();
            var rootCategory_ = rootCategory = DefaultCategory(AsRoot);
            rootCategory.AddSubCategory(subCategory);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            this.CategoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(rootCategory);

            // todo: resolve must use FindByCategeoryAndName instead of mathicng the names itself
            //this.CategoryRepositoryMock
            //    .Setup(r => r.FindByCategoryAndName(rootCategory_, subCategory_.Name))
            //    .Returns(subCategory);
        }

        #endregion Arrangements

        #region Default Tag

        protected static Tag DefaultTag(params Action<Tag>[] setup)
        {
            var tmp = new Tag("t", new Facet("f"));
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        protected static void WithDefaultProperty(Tag tag) => tag.Facet.AddProperty(new FacetProperty("p", FacetPropertyTypeValues.String));

        protected static void WithoutProperty(Tag tag) => tag.Facet.Properties.Clear();

        #endregion Default Tag

        #region Default Entity

        protected Entity DefaultEntity(params Action<Entity>[] setup)
        {
            var tmp = new Entity("e")
            {
                Category = this.CategoryRepositoryMock.Object.Root()
            };

            setup.ForEach(s => s(tmp));

            return tmp;
        }

        protected static void WithDefaultTag(Entity e) => e.AddTag(DefaultTag(WithDefaultProperty));

        protected static void WithoutTag(Entity e) => e.Tags.Clear();

        protected static Action<Entity> WithEntityCategory(Category c) => e => e.SetCategory(c);

        #endregion Default Entity

        #region Default Category

        protected Category DefaultCategory(params Action<Category>[] setup)
        {
            var category = new Category("c");
            setup.ForEach(s => s(category));
            return category;
        }

        protected static void AsRoot(Category category)
        {
            category.Id = CategoryRepository.CategoryRootId;
            category.Name = "";
            category.Parent = null;
        }

        protected static Action<Category> WithSubCategory(Category category) => c => c.AddSubCategory(category);

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        #endregion IDisposable Support

        #endregion Default Category
    }
}