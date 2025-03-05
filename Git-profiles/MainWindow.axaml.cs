using Avalonia.Controls;
using Git_profiles.ViewModels;

namespace Git_profiles
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainWindowViewModel();
            viewModel.SetParentWindow(this);
            DataContext = viewModel;
        }
    }
}