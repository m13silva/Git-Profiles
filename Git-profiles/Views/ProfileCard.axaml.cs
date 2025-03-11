using Avalonia.Controls;
using Avalonia.Input;
using Git_profiles.Models;
using Git_profiles.ViewModels;

namespace Git_profiles.Views
{
    public partial class ProfileCard : UserControl
    {
        public ProfileCard()
        {
            InitializeComponent();
        }

        private void OnTapped(object? sender, TappedEventArgs e)
        {
            // Verificamos si el evento fue disparado por un CheckBox
            var PressedSource = e.Source as Control;
            if (PressedSource?.TemplatedParent is CheckBox) return;

            if (DataContext is GitProfileModel profile)
            {
                // Buscamos el MainWindowViewModel
                if (this.GetParentWindow()?.DataContext is MainWindowViewModel viewModel)
                {
                    // Establecemos este perfil como activo
                    viewModel.CurrentProfile = profile;
                }
            }
        }
    }

    // Extensión para encontrar la ventana padre
    public static class ControlExtensions
    {
        public static Window? GetParentWindow(this Control control)
        {
            var current = control;
            while (current != null)
            {
                if (current is Window window)
                    return window;
                current = current.Parent as Control;
            }
            return null;
        }
    }
}