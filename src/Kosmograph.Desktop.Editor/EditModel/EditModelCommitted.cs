﻿namespace Kosmograph.Desktop.EditModel
{
    public sealed class EditModelCommitted
    {
        public EditModelCommitted(object viewModel)
        {
            this.ViewModel = viewModel;
        }

        public object ViewModel { get; set; }

        public (bool, T) TryGetViewModel<T>() where T : class
        {
            if (this.ViewModel is T t)
                return (true, t);
            else return (false, null);
        }
    }
}