using GraphX.PCL.Common.Models;
using System;
using System.ComponentModel;

namespace Kosmograph.Desktop.GraphXViewer.Model
{
    public class KosmographVisualVertexModel : VertexBase, INotifyPropertyChanged
    {
        public KosmographVisualVertexModel()
        {
        }

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