using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Controls;
using Git_profiles.Models;
using Git_profiles.Views;

namespace Git_profiles.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private GitProfileModel? _currentProfile;
        private Window? _parentWindow;

        public MainWindowViewModel()
        {
            // Inicializar con datos de ejemplo
            Profiles = new ObservableCollection<GitProfileModel>
            {
                new GitProfileModel { Name = "John Doe", Email = "john@example.com" },
                new GitProfileModel { Name = "Mary Smith", Email = "mary@example.com" },
                new GitProfileModel { Name = "David Jones", Email = "david@example.com" },
                new GitProfileModel { Name = "Emma Wilson", Email = "emma@example.com" },
                new GitProfileModel { Name = "Robert Brown", Email = "robert@example.com" }
            };

            // Establecer perfil actual
            CurrentProfile = Profiles[0];

            // Inicializar comandos
            AddProfileCommand = new AvaloniaCommand(AddProfile);
            RemoveProfileCommand = new AvaloniaCommand(RemoveProfile, CanRemoveProfile);
            EditProfileCommand = new AvaloniaCommand(EditProfile, CanEditProfile);
            SetActiveProfileCommand = new AvaloniaCommand(SetActiveProfile, CanSetActiveProfile);
        }

        public void SetParentWindow(Window window)
        {
            _parentWindow = window;
        }

        public GitProfileModel? CurrentProfile
        {
            get => _currentProfile;
            set
            {
                if (_currentProfile != value)
                {
                    if (_currentProfile != null)
                        _currentProfile.IsActive = false;

                    _currentProfile = value;

                    if (_currentProfile != null)
                        _currentProfile.IsActive = true;

                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<GitProfileModel> Profiles { get; }

        public ICommand AddProfileCommand { get; }
        public ICommand RemoveProfileCommand { get; }
        public ICommand EditProfileCommand { get; }
        public ICommand SetActiveProfileCommand { get; }

        private async void AddProfile()
        {
            var dialog = new ProfileDialog { DialogTitle = "Nuevo Perfil" };
            if (_parentWindow != null)
            {
                var result = await dialog.ShowDialog<bool>(_parentWindow);
                if (result)
                {
                    var newProfile = dialog.GetProfile();
                    Profiles.Add(newProfile);
                }
            }
        }

        private void RemoveProfile()
        {
            var selected = GetSelectedProfile();
            if (selected != null)
            {
                Profiles.Remove(selected);

                if (selected == CurrentProfile && Profiles.Count > 0)
                {
                    CurrentProfile = Profiles[0];
                }
            }
        }

        private bool CanRemoveProfile()
        {
            return GetSelectedProfile() != null;
        }

        private async void EditProfile()
        {
            var selected = GetSelectedProfile();
            if (selected != null && _parentWindow != null)
            {
                var dialog = new ProfileDialog { DialogTitle = "Editar Perfil" };
                dialog.SetProfile(selected);

                var result = await dialog.ShowDialog<bool>(_parentWindow);
                if (result)
                {
                    var editedProfile = dialog.GetProfile();
                    selected.Name = editedProfile.Name;
                    selected.Email = editedProfile.Email;

                    if (selected == CurrentProfile)
                    {
                        OnPropertyChanged(nameof(CurrentProfile));
                    }
                }
            }
        }

        private bool CanEditProfile()
        {
            return GetSelectedProfile() != null;
        }

        private void SetActiveProfile()
        {
            var selected = GetSelectedProfile();
            if (selected != null)
            {
                CurrentProfile = selected;
            }
        }

        private bool CanSetActiveProfile()
        {
            var selected = GetSelectedProfile();
            return selected != null && selected != CurrentProfile;
        }

        private GitProfileModel? GetSelectedProfile()
        {
            foreach (var profile in Profiles)
            {
                if (profile.IsSelected)
                {
                    return profile;
                }
            }
            return null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AvaloniaCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public AvaloniaCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}