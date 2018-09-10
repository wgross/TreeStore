using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosmograph.Desktop.ViewModel
{
    public class KosmographViewModel
    {
        private KosmographModel kosmographModel;

        public KosmographViewModel(KosmographModel kosmographModel)
        {
            this.kosmographModel = kosmographModel;
        }

        public ObservableCollection<Tag> Tags => new ObservableCollection<Tag>(this.kosmographModel.Tags.FindAll());

    }
}
