using System.Windows;
using System.Windows.Controls;

namespace Kosmograph.Desktop.Editors.View
{
    public partial class EditorSectionHeaderControl : UserControl
    {
        public static readonly DependencyProperty TextProperty;

        static EditorSectionHeaderControl()
        {
            TextProperty = TextBlock.TextProperty.AddOwner(typeof(EditorSectionHeaderControl), new FrameworkPropertyMetadata { Inherits = true });
        }

        public EditorSectionHeaderControl()
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