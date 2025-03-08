using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Git_profiles.Views
{
    public partial class ConfirmDialog : Window
    {
        private string _message = "";

        public ConfirmDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        public new string Title
        {
            get => base.Title ?? "";
            set => base.Title = value;
        }

        public string Message
        {
            get => _message;
            set => _message = value;
        }

        private void OnOkClick(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}