﻿using Kosmograph.Desktop.EditModel.Base;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Linq;

namespace Kosmograph.Desktop.EditModel
{
    public class AssignedTagEditModel : NamedEditModelBase<AssignedTagViewModel, Tag>
    {
        public AssignedTagEditModel(AssignedTagViewModel viewModel)
            : base(viewModel)
        {
            this.Properties = new CommitableObservableCollection<AssignedFacetPropertyEditModel>(
                viewModel.Properties.Select(p => new AssignedFacetPropertyEditModel(p)));
        }

        public CommitableObservableCollection<AssignedFacetPropertyEditModel> Properties { get; }

        public override void Commit()
        {
            this.Properties.ForEach(p => p.Commit());
            base.Commit();
        }

        public override void Rollback()
        {
            this.Properties.ForEach(p => p.Rollback());
            base.Rollback();
        }
    }
}