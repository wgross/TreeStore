namespace Kosmograph.Desktop.Editors.ViewModel
{
    public sealed class EditModelCommitted
    {
        public EditModelCommitted(object model)
        {
            this.Model = model;
        }

        public object Model { get; set; }

        public (bool, T) TryGetViewModel<T>() where T : class
        {
            if (this.Model is T t)
                return (true, t);
            else return (false, null);
        }
    }
}