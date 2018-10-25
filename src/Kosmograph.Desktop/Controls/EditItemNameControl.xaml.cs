using System.Windows;
using System.Windows.Controls;

namespace Kosmograph.Desktop.Controls
{
    public partial class EditItemNameControl : UserControl
    {
        public static readonly DependencyProperty ErrorTextProperty;

        static EditItemNameControl()
        {
            ErrorTextProperty = DependencyProperty.Register(nameof(ErrorText), typeof(string), typeof(EditorHeaderControl));
        }

        public EditItemNameControl()
        {
            this.InitializeComponent();
        }

        public string ErrorText
        {
            get => (string)this.GetValue(ErrorTextProperty);
            set => this.SetValue(ErrorTextProperty, value);
        }
    }
}