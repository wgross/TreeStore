using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Kosmograph.Desktop.ViewModel
{
    public class CommitableObservableCollection<T> : ObservableCollection<T>
    {
        private readonly List<(NotifyCollectionChangedAction, IEnumerable<T>)> changes = new List<(NotifyCollectionChangedAction, IEnumerable<T>)>();
        private bool isRollingBack = false;

        public CommitableObservableCollection(IEnumerable<T> underlying)
            : base(underlying)
        { }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!this.isRollingBack)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        this.changes.Add((e.Action, e.NewItems.OfType<T>().ToArray()));
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        this.changes.Add((e.Action, e.OldItems.OfType<T>().ToArray()));
                        break;
                }
                base.OnCollectionChanged(e);
            }
        }

        public void Commit(Action<T> onAdd, Action<T> onRemove)
        {
            foreach (var (action, items) in this.changes)
            {
                switch (action)
                {
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in items)
                            onRemove(item);
                        break;

                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in items)
                            onAdd(item);
                        break;
                }
            }
            this.changes.Clear();
        }
    }
}