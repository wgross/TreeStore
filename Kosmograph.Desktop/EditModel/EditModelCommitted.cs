namespace Kosmograph.Desktop.EditModel
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
            var t = this.ViewModel as T;
            return (t != null, t);
        }
    }
}