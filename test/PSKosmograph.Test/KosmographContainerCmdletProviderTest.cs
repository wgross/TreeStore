using Kosmograph.Model;
using Moq;
using System;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace PSKosmograph.Test
{
    public class KosmographContainerCmdletProviderTest : KosmographCmdletProviderTestBase
    {
        #region New-Item /Tags/<name>

        [Fact]
        public void Powershell_creates_Tag()
        {
            // ARRANGE

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("tag"))
                .Returns((Tag?)null);

            this.TagRepositoryMock
                .Setup(r => r.Upsert(It.Is<Tag>(t => t.Name.Equals("tag"))))
                .Returns<Tag>(t => t);

            // ACT

            this.PowerShell
                .AddCommand("New-Item")
                .AddParameter("Path", $@"kg:\Tags\tag");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::Tags\tag", ((string)result[0].Properties["PSPath"].Value));
            Assert.NotEqual(Guid.Empty, result[0].Property<Guid>("Id"));
            Assert.Equal("tag", result[0].Property<string>("Name"));
        }

        #endregion New-Item /Tags/<name>

        #region New-Item /Tags/<name>/<property-name>

        [Fact]
        public void Powershell_creates_facet_property()
        {
            // ARRANGE

            var tag = DefaultTag(t => t.Facet.Properties.Clear());

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            this.TagRepositoryMock
                .Setup(r => r.Upsert(tag))
                .Returns(tag);

            // ACT

            this.PowerShell
                .AddCommand("New-Item")
                .AddParameter("Path", $@"kg:\Tags\t\p")
                .AddParameter("ValueType", FacetPropertyTypeValues.Bool);

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::Tags\t\p", ((string)result[0].Properties["PSPath"].Value));
            Assert.NotEqual(Guid.Empty, result[0].Property<Guid>("Id"));
            Assert.Equal("p", result[0].Property<string>("Name"));
        }

        #endregion New-Item /Tags/<name>/<property-name>

        #region New-Item /Entities/<name>

        [Fact]
        public void PowerShell_creates_Entity()
        {
            // ARRANGE

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByName("e"))
                .Returns((Entity?)null);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.Is<Entity>(e => e.Name.Equals("e"))))
                .Returns<Entity>(e => e);

            // ACT

            this.PowerShell
                .AddCommand("New-Item")
                    .AddParameter("Path", @"kg:\Entities\e");

            var result = this.PowerShell.Invoke().ToArray();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);
            Assert.IsType<ProviderInfo>(result[0].Property<ProviderInfo>("PSProvider"));
            Assert.Equal("Kosmograph", result[0].Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal("PSKosmograph", result[0].Property<ProviderInfo>("PSProvider").ModuleName);
            Assert.Equal(@"PSKosmograph\Kosmograph::Entities\e", ((string)result[0].Properties["PSPath"].Value));
            Assert.NotEqual(Guid.Empty, result[0].Property<Guid>("Id"));
            Assert.Equal("e", result[0].Property<string>("Name"));
        }

        #endregion New-Item /Entities/<name>
    }
}