using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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

            var titleBar = this.Find<Border>("TitleBarBorder");
            if (titleBar != null)
            {
                titleBar.PointerPressed += TitleBar_PointerPressed;
            }
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

        private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
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