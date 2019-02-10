using Kosmograph.Desktop.Editors.ViewModel;
using Kosmograph.Model;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Editors.Test.ViewModel

{
    public class FacetPropertyEditModelTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly Mock<ITagEditCallback> tagEditCallback;

        private Tag DefaultTag(params FacetProperty[] properties) => new Tag("tag", new Facet("f", properties));

        private Tag DefaultTag() => new Tag("tag", new Facet("f", new FacetProperty("p")));

        public FacetPropertyEditModelTest()
        {
            this.tagEditCallback = this.mocks.Create<ITagEditCallback>();
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void FacetPropertyEditModel_mirrors_Model()
        {
            // ARRANGE

            var model = DefaultTag();

            // ACT

            var result = new TagEditModel(model, this.tagEditCallback.Object).Properties.Single();

            // ASSERT

            Assert.Equal("p", result.Name);
        }

        [Fact]
        public void FacetPropertyEditModel_delays_changes_of_Model()
        {
            // ARRANGE

            var model = DefaultTag();
            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            // ACT

            editModel.Properties.Single().Name = "changed";

            // ASSERT

            Assert.Equal("changed", editModel.Properties.Single().Name);
            Assert.Equal("p", model.Facet.Properties.Single().Name);
        }

        [Fact]
        public void FacetPropertyEditModel_trims_property_name()
        {
            // ARRANGE

            var model = DefaultTag();
            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            // ACT

            editModel.Properties.Single().Name = " changed \t";

            // ASSERT

            Assert.Equal("changed", editModel.Properties.Single().Name);
        }

        [Fact]
        public void FacetPropertyEditModel_invalidates_duplicate_name()
        {
            // ARRANGE

            var model = DefaultTag(new FacetProperty("p"), new FacetProperty("q"));
            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            // ACT

            editModel.Properties.ElementAt(1).Name = editModel.Properties.ElementAt(0).Name;

            // ASSERT

            Assert.True(editModel.Properties.ElementAt(1).HasErrors);
            Assert.Equal("Property name must be unique", editModel.Properties.ElementAt(1).NameError);
        }

        [Fact]
        public void FacetPropertyEditModel_invalidates_empty_name()
        {
            // ARRANGE

            var model = DefaultTag(new FacetProperty("p"), new FacetProperty("q"));
            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            // ACT

            editModel.Properties.ElementAt(1).Name = "";
            var result = editModel.Properties.ElementAt(1).CommitCommand.CanExecute(null);

            // ASSERT

            Assert.False(result);
            Assert.True(editModel.Properties.ElementAt(1).HasErrors);
            Assert.Equal("Property name must not be empty", editModel.Properties.ElementAt(1).NameError);
        }

        [Fact]
        public void FacetPropertyEditModel_commits_changes_to_Model()
        {
            // ARRANGE

            var model = new Tag("", new Facet("", new FacetProperty("p")));
            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            editModel.Properties.Single().Name = "changed";

            // ACT

            editModel.Properties.Single().CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal("changed", editModel.Properties.Single().Name);
            Assert.Equal("changed", model.Facet.Properties.Single().Name);
        }

        [Fact]
        public void FacetPropertyEditModel_reverts_changes_to_Model()
        {
            // ARRANGE

            var model = DefaultTag();
            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            editModel.Properties.Single().Name = "changed";

            // ACT

            editModel.Properties.Single().RollbackCommand.Execute(null);
            editModel.Properties.Single().CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal("p", editModel.Properties.Single().Name);
            Assert.Equal("p", model.Facet.Properties.Single().Name);
        }
    }
}