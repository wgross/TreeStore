using System.Windows;
using System.Windows.Controls;

namespace Kosmograph.Desktop.Controls
{
    public partial class EditorHeaderControl : UserControl
    {
        public static readonly DependencyProperty TextProperty;
        

        static EditorHeaderControl()
        {
            TextProperty = TextBlock.TextProperty.AddOwner(typeof(EditorHeaderControl), new FrameworkPropertyMetadata { Inherits = true });
        }

        public EditorHeaderControl()
        {
            this.InitializeComponent();
        }

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

      
    }
}