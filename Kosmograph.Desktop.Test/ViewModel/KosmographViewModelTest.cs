using Kosmograph.Desktop.ViewModel;
using Kosmograph.LiteDb;
using Kosmograph.Model;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class KosmographViewModelTest
    {
        [Fact]
        public void KosmographViewModel_returns_observalble_tags()
        {
            // ARRANGE

            var model = new KosmographViewModel(new KosmographModel(new KosmographLiteDbPersistence()));

            // ACT

            var result = model.Tags;

            // ASSERT

            Assert.NotNull(result);
        }
    }
}