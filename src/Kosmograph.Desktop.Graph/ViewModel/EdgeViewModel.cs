using GraphX.PCL.Common.Models;
using System;
using System.ComponentModel;

namespace Kosmograph.Desktop.Graph.ViewModel
{
    public sealed class EdgeViewModel : EdgeBase<VertexViewModel>, INotifyPropertyChanged
    {
        public EdgeViewModel(VertexViewModel source, VertexViewModel target, double weight = 1)
            : base(source, target, weight)
        { }

        private string label;

        public string Label
        {
            get => this.label;
            set
            {
                if (StringComparer.Ordinal.Equals(this.label, value))
                    return;
                this.label = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Label)));
            }
        }

        public Guid ModelId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}