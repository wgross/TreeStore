using System.Linq;
using Xunit;

namespace PSKosmograph.Test.Service
{
    public class KosmographPathTest
    {
        #region TryParse

        [Fact]
        public void TryParse_string_path()
        {
            // ACT

            var (result, resultPath) = KosmographPath.TryParse(path: "/test/test2", directorySeperator: "/");

            // ASSERT

            Assert.True(result);
            Assert.Equal(2, resultPath.Items.Count());
            Assert.Equal(new[] { "test", "test2" }, resultPath.Items.ToArray());
            Assert.Null(resultPath.Drive);
            Assert.False(resultPath.IsRoot);
        }

        [Fact]
        public void TryParse_string_path_with_drive()
        {
            // ACT

            var (result, resultPath) = KosmographPath.TryParse(path: "drive:/test/test2", driveSeperator: ":", directorySeperator: "/");

            // ASSERT

            Assert.True(result);
            Assert.Equal(2, resultPath.Items.Count());
            Assert.Equal(new[] { "test", "test2" }, resultPath.Items.ToArray());
            Assert.Equal("drive", resultPath.Drive);
            Assert.False(resultPath.IsRoot);
        }

        [Fact]
        public void TryParse_string_root_path()
        {
            // ACT

            var (result, resultPath) = KosmographPath.TryParse(path: "", driveSeperator: ":", directorySeperator: "/");

            // ASSERT

            Assert.True(result);
            Assert.Empty(resultPath.Items);
            Assert.Null(resultPath.Drive);
            Assert.True(resultPath.IsRoot);
        }

        [Fact]
        public void TryParse_string_root_path_with_drive()
        {
            // ACT

            var (result, resultPath) = KosmographPath.TryParse(path: "drive:/", driveSeperator: ":", directorySeperator: "/");

            // ASSERT

            Assert.True(result);
            Assert.Empty(resultPath.Items);
            Assert.Equal("drive", resultPath.Drive);
            Assert.True(resultPath.IsRoot);
        }

        #endregion TryParse

        #region ToString

        [Fact]
        public void ToString_joins_path_items()
        {
            // ARRANGE

            var resultPath = KosmographPath.Create(null, "test", "test2");

            // ACT

            string result = resultPath.ToString();

            // ARRANGE

            Assert.Equal(@"test\test2", result);
        }

        [Fact]
        public void ToString_joins_drive_and_path_items()
        {
            // ARRANGE

            var resultPath = KosmographPath.Create("drive", "test", "test2");

            // ACT

            string result = resultPath.ToString();

            // ARRANGE

            Assert.Equal(@"drive:\test\test2", result);
        }

        #endregion ToString
    }
}