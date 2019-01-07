using Kosmograph.Model;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Editors.ViewModel
{
    public class ObservableRepositoryTest : IDisposable
    {
        private readonly MockRepository mocks;

        public ObservableRepositoryTest()
        {
            this.mocks = new MockRepository(MockBehavior.Strict);
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void ObservableRepository_upserts_added_items()
        {
            // ARRANGE

            var model = new Tag();
            var repository = this.mocks.Create<ITagRepository>();
            repository
                .Setup(r => r.Upsert(model))
                .Returns(model);

            var observableRepository = new RepositoryViewModel<TagViewModel, Tag>(repository.Object, m => new TagViewModel(m));

            // ACT

            observableRepository.Add(new TagViewModel(model));

            // ASSERT

            Assert.Equal(model, observableRepository.Single().Model);
        }

        [Fact]
        public void ObservableRepository_deletes_removed_items()
        {
            // ARRANGE

            var model = new Tag();
            var repository = this.mocks.Create<ITagRepository>();
            repository
              .Setup(r => r.Upsert(model))
              .Returns(model);
            repository
                .Setup(r => r.Delete(model))
                .Returns(true);

            var observableRepository = new RepositoryViewModel<TagViewModel, Tag>(repository.Object, m => new TagViewModel(m));
            var viewModel = new TagViewModel(model);

            observableRepository.Add(viewModel);

            // ACT

            observableRepository.Remove(viewModel);

            // ASSERT

            Assert.Empty(observableRepository);
        }

        [Fact]
        public void ObservableRepository_fills_from_repo_without_upsert()
        {
            // ARRANGE

            var model = new Tag();
            var repository = this.mocks.Create<ITagRepository>();
            repository
              .Setup(r => r.FindAll())
              .Returns(model.Yield());

            var observableRepository = new RepositoryViewModel<TagViewModel, Tag>(repository.Object, m => new TagViewModel(m));

            // ACT

            observableRepository.FillAll();

            // ASSERT

            Assert.Single(observableRepository);
        }

        [Fact]
        public void ObservableRepository_gets_ViewModel_of_Model()
        {
            // ARRANGE

            var model = new Tag();
            var repository = this.mocks.Create<ITagRepository>();
            repository
              .Setup(r => r.FindAll())
              .Returns(model.Yield());

            var observableRepository = new RepositoryViewModel<TagViewModel, Tag>(repository.Object, m => new TagViewModel(m));
            observableRepository.FillAll();

            // ACT

            var result = observableRepository.GetViewModel(new Tag { Id = model.Id });

            // ASSERT

            Assert.Same(observableRepository.Single(), result);
        }

        [Fact]
        public void ObservableRepository_getting_unknown_ViewModel_throws()
        {
            // ARRANGE

            var model = new Tag();
            var repository = this.mocks.Create<ITagRepository>();
            repository
              .Setup(r => r.FindAll())
              .Returns(model.Yield());

            var observableRepository = new RepositoryViewModel<TagViewModel, Tag>(repository.Object, m => new TagViewModel(m));
            observableRepository.FillAll();

            // ACT

            Assert.Throws<InvalidOperationException>(() => observableRepository.GetViewModel(new Tag { Id = Guid.NewGuid() }));
        }
    }
}