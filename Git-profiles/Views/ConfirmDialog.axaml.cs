using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Git_profiles.Views
{
    public partial class ConfirmDialog : Window, INotifyPropertyChanged
    {
        private string _message = "";

        public event PropertyChangedEventHandler? PropertyChanged;

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
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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