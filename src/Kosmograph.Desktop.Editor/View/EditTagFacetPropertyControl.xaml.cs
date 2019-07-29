using System.Windows;
using System.Windows.Controls;

namespace Kosmograph.Desktop.Editors.View
{
    /// <summary>
    /// Interaction logic for EditTagFacetPropertyControl.xaml
    /// </summary>
    public partial class EditTagFacetPropertyControl : UserControl
    {
        public static readonly DependencyProperty ErrorTextProperty;

        static EditTagFacetPropertyControl() => ErrorTextProperty = DependencyProperty.Register(nameof(ErrorText), typeof(string), typeof(EditTagFacetPropertyControl));

        public EditTagFacetPropertyControl() => this.InitializeComponent();

        public string ErrorText
        {
            get => (string)this.GetValue(ErrorTextProperty);
            set => this.SetValue(ErrorTextProperty, value);
        }
    }
}