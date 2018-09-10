using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class EditNamedViewModelTest
    {
        public static IEnumerable<object[]> GetTestedInstances()
        {
            var tag = new EditTagViewModel(new Tag("tag", new Facet("facet", new FacetProperty("p"))));

            yield return new object[] { (Action<string>)(s => tag.Name = s), tag };
            yield return new object[] { (Action<string>)(s => tag.Facet.Name = s), tag.Facet };
            yield return new object[] { (Action<string>)(s => tag.Facet.Properties.Single().Name = s), tag.Facet.Properties.Single() };
        }

        [Theory]
        [MemberData(nameof(GetTestedInstances))]
        public void EditNamed_raises_property_changed_on_Name_change(Action<string> setter, INotifyPropertyChanged editor)
        {
            // ARRANGE

            INotifyPropertyChanged instance = null;
            PropertyChangedEventArgs args = null;
            editor.PropertyChanged += (i, e) => {
                instance = i as INotifyPropertyChanged;
                args = e;
            };


            // ACT

            setter("changed");

            // ASSERT

            Assert.Equal(instance, editor);
            Assert.Equal("Name", args.PropertyName);
        }
    }
}