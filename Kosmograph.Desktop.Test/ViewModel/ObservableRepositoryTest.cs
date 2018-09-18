using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
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

            var observableRepository = new ObservableRepository<TagViewModel, Tag>(repository.Object);

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
                .Setup(r => r.Delete(model.Id))
                .Returns(true);

            var observableRepository = new ObservableRepository<TagViewModel, Tag>(repository.Object);
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

            var observableRepository = new ObservableRepository<TagViewModel, Tag>(repository.Object);

            // ACT

            observableRepository.FillAll(m => new TagViewModel(m));

            // ASSERT

            Assert.Single(observableRepository);
        }
    }
}