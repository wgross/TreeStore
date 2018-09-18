using Kosmograph.Desktop.ViewModel.Base;
using Kosmograph.Model;
using Kosmograph.Model.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Kosmograph.Desktop.ViewModel
{
    public class ObservableRepository<VM, M> : ObservableCollection<VM>
        where M : NamedItemBase
        where VM : NamedViewModelBase<M>
    {
        private readonly IRepository<M> repository;
        private readonly IDictionary<Guid, VM> locals = new Dictionary<Guid, VM>();
        private bool filling;

        public ObservableRepository(IRepository<M> repository)
        {
            this.repository = repository;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var viewModel in e.NewItems.OfType<VM>())
                    {
                        if (!this.filling)
                            this.repository.Upsert(viewModel.Model);
                        this.locals[viewModel.Model.Id] = viewModel;
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var viewModel in e.OldItems.OfType<VM>())
                    {
                        if (!this.filling)
                            this.repository.Delete(viewModel.Model.Id);
                        this.locals.Remove(viewModel.Model.Id);
                    }
                    break;
            }
            base.OnCollectionChanged(e);
        }

        public void FillAll(Func<M, VM> makeViewModel)
        {
            try
            {
                this.filling = true;
                foreach (var m in this.repository.FindAll())
                    this.Add(makeViewModel(m));
            }
            finally
            {
                this.filling = false;
            }
        }
    }
}