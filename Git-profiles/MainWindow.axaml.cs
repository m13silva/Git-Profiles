using Avalonia.Controls;
using Avalonia.Input;
using Git_profiles.ViewModels;

namespace Git_profiles
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            ((MainWindowViewModel)DataContext).SetParentWindow(this);

            var titleBar = this.Find<Border>("TitleBarBorder");
            if (titleBar != null)
            {
                titleBar.PointerPressed += TitleBar_PointerPressed;
            }
        }

        private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }
    }
}