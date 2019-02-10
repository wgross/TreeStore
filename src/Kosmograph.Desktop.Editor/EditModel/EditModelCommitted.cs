namespace Kosmograph.Desktop.Editors.ViewModel
{
    public sealed class EditModelCommitted
    {
        public EditModelCommitted(object model)
        {
            this.ViewModel = model;
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