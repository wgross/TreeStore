using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class TagEditModelTest
    {
        [Fact]
        public void TagEditModel_mirrors_ViewModel()
        {
            // ARRANGE

            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);
            var editModel = new TagEditModel(viewModel, delegate { }, delegate { });

            // ASSERT

            Assert.Equal("tag", editModel.Name);
            Assert.Single(editModel.Properties);
        }

        [Fact]
        public void TagEditModel_delays_change_at_ViewModel()
        {
            // ARRANGE

            var p2 = new FacetProperty("p2");
            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);
            var editModel = new TagEditModel(viewModel, delegate { }, delegate { });

            // ACT

            editModel.Name = "changed";

            // ASSERT

            Assert.Equal("changed", editModel.Name);
            Assert.Equal("tag", viewModel.Name);
            Assert.Equal("tag", model.Name);
        }

        [Fact]
        public void TagEditModel_commits_change_to_ViewModel()
        {
            // ARRANGE

            var p2 = new FacetProperty("p2");
            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);

            Tag committed = null;
            var commitCB = new Action<Tag>(t => committed = t);

            Tag reverted = null;
            var revertCB = new Action<Tag>(t => reverted = t);

            var editModel = new TagEditModel(viewModel, commitCB, revertCB);

            editModel.Name = "changed";

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, committed);
            Assert.Null(reverted);

            Assert.Equal("changed", editModel.Name);
            Assert.Equal("changed", viewModel.Name);
            Assert.Equal("changed", model.Name);
        }

        [Fact]
        public void TagEditModel_reverts_change_from_ViewModel()
        {
            // ARRANGE

            var p2 = new FacetProperty("p2");
            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);

            Tag reverted = null;
            var revertCB = new Action<Tag>(t => reverted = t);

            var editModel = new TagEditModel(viewModel, delegate { }, revertCB);

            editModel.Name = "changed";

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, reverted);

            Assert.Equal("tag", editModel.Name);
            Assert.Equal("tag", viewModel.Name);
            Assert.Equal("tag", model.Name);
        }

        [Fact]
        public void TagEditModel_delays_add_property_at_ViewModel()
        {
            // ARRANGE

            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);
            var editModel = new TagEditModel(viewModel, delegate { }, delegate { });

            // ACT

            editModel.CreatePropertyCommand.Execute(null);

            // ASSERT

            Assert.Equal(2, editModel.Properties.Count());
            Assert.Equal("new property", editModel.Properties.ElementAt(1).Name);
            Assert.Single(viewModel.Properties);
            Assert.Single(model.Facet.Properties);
        }

        [Fact]
        public void TagEditModel_commits_add_property_at_ViewModel()
        {
            // ARRANGE

            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);

            Tag committed = null;
            var commitCB = new Action<Tag>(t => committed = t);

            Tag reverted = null;
            var revertCB = new Action<Tag>(t => reverted = t);

            var editModel = new TagEditModel(viewModel, commitCB, revertCB);

            editModel.CreatePropertyCommand.Execute(null);

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, committed);
            Assert.Null(reverted);

            Assert.Equal(2, editModel.Properties.Count());
            Assert.Equal(2, viewModel.Properties.Count());
            Assert.Equal(2, model.Facet.Properties.Count());
            Assert.Equal("new property", editModel.Properties.ElementAt(1).Name);
            Assert.Equal("new property", viewModel.Properties.ElementAt(1).Name);
            Assert.Equal("new property", model.Facet.Properties.ElementAt(1).Name);
        }

        [Fact]
        public void TagEditModel_reverts_add_property_from_ViewModel()
        {
            // ARRANGE

            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);

            Tag reverted = null;
            var revertCB = new Action<Tag>(t => reverted = t);

            var editModel = new TagEditModel(viewModel, delegate { }, revertCB);

            editModel.CreatePropertyCommand.Execute(null);

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, reverted);

            Assert.Equal("tag", editModel.Name);
            Assert.Equal("tag", viewModel.Name);
            Assert.Equal("tag", model.Name);
            Assert.Single(editModel.Properties);
            Assert.Single(viewModel.Properties);
            Assert.Single(model.Facet.Properties);
        }

        [Fact]
        public void TagEditModel_delays_remove_property_at_ViewModel()
        {
            // ARRANGE

            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);
            var editModel = new TagEditModel(viewModel, delegate { }, delegate { });

            // ACT

            editModel.RemovePropertyCommand.Execute(editModel.Properties.Single());

            // ASSERT

            Assert.Empty(editModel.Properties);
            Assert.Single(viewModel.Properties);
            Assert.Single(model.Facet.Properties);
        }

        [Fact]
        public void TagEditModel_commits_remove_property_to_ViewModel()
        {
            // ARRANGE

            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);

            Tag committed = null;
            var commitCB = new Action<Tag>(t => committed = t);

            Tag reverted = null;
            var revertCB = new Action<Tag>(t => reverted = t);

            var editModel = new TagEditModel(viewModel, commitCB, revertCB);

            editModel.RemovePropertyCommand.Execute(editModel.Properties.Single());

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, committed);
            Assert.Null(reverted);

            Assert.Empty(editModel.Properties);
            Assert.Empty(viewModel.Properties);
            Assert.Empty(model.Facet.Properties);
        }

        [Fact]
        public void TagEditModel_reverts_remove_property_at_ViewModel()
        {
            // ARRANGE

            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);

            Tag reverted = null;
            var revertCB = new Action<Tag>(t => reverted = t);

            var editModel = new TagEditModel(viewModel, delegate { }, revertCB);

            editModel.CreatePropertyCommand.Execute(null);

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, reverted);

            Assert.Single(editModel.Properties);
            Assert.Single(viewModel.Properties);
            Assert.Single(model.Facet.Properties);
        }
    }
}