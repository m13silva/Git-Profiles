using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Git_profiles.Models;

namespace Git_profiles.Views
{
    public partial class ProfileDialog : Window
    {
        private ProfileDialogViewModel _viewModel;

        public ProfileDialog()
        {
            InitializeComponent();
            _viewModel = new ProfileDialogViewModel();
            DataContext = _viewModel;
        }

        public void SetProfile(GitProfileModel profile)
        {
            _viewModel.Profile = new GitProfileModel
            {
                Name = profile.Name,
                Email = profile.Email
            };
        }

        public GitProfileModel GetProfile() => _viewModel.Profile;

        public string DialogTitle
        {
            get => _viewModel.Title;
            set => _viewModel.Title = value;
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