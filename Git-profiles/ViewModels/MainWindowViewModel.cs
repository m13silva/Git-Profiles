using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
        private readonly DatabaseService _databaseService;

        public MainWindowViewModel()
        {
            _databaseService = new DatabaseService();
            Profiles = new ObservableCollection<GitProfileModel>();
            LoadProfiles();

            // Initialize commands
            AddProfileCommand = new AvaloniaCommand(AddProfile);
            RemoveProfileCommand = new AvaloniaCommand(RemoveProfile, CanRemoveProfile);
            EditProfileCommand = new AvaloniaCommand(EditProfile, CanEditProfile);
            SetActiveProfileCommand = new AvaloniaCommand(SetActiveProfile, CanSetActiveProfile);
        }

        private void LoadProfiles()
        {
            Profiles.Clear();
            var profiles = _databaseService.GetAllProfiles();
            foreach (var profile in profiles)
            {
                Profiles.Add(profile);
                if (profile.IsActive)
                {
                    CurrentProfile = profile;
                }
            }

            if (CurrentProfile == null && Profiles.Count > 0)
            {
                CurrentProfile = Profiles[0];
            }
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
                    {
                        _currentProfile.IsActive = false;
                        _databaseService.SaveProfile(_currentProfile);
                    }

                    _currentProfile = value;

                    if (_currentProfile != null)
                    {
                        _currentProfile.IsActive = true;
                        _databaseService.SaveProfile(_currentProfile);

                        if (_currentProfile.ExecuteImmediately)
                        {
                            ApplyGitConfig(_currentProfile);
                        }
                    }

                    OnPropertyChanged();
                }
            }
        }

        private void ApplyGitConfig(GitProfileModel profile)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "git",
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            // Configurar nombre y email
            processStartInfo.Arguments = $"config --global user.name \"{profile.Name}\"";
            Process.Start(processStartInfo)?.WaitForExit();

            processStartInfo.Arguments = $"config --global user.email \"{profile.Email}\"";
            Process.Start(processStartInfo)?.WaitForExit();

            if (profile.UseGpg && !string.IsNullOrEmpty(profile.GpgKeyId))
            {
                processStartInfo.Arguments = "config --global commit.gpgsign true";
                Process.Start(processStartInfo)?.WaitForExit();

                processStartInfo.Arguments = $"config --global user.signingkey {profile.GpgKeyId}";
                Process.Start(processStartInfo)?.WaitForExit();
            }
            else
            {
                processStartInfo.Arguments = "config --global commit.gpgsign false";
                Process.Start(processStartInfo)?.WaitForExit();
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
                    _databaseService.SaveProfile(newProfile);
                    Profiles.Add(newProfile);
                }
            }
        }

        private void RemoveProfile()
        {
            var selected = GetSelectedProfile();
            if (selected != null)
            {
                _databaseService.DeleteProfile(selected.Id);
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
                    selected.GpgKeyId = editedProfile.GpgKeyId;
                    selected.UseGpg = editedProfile.UseGpg;
                    selected.Color = editedProfile.Color;
                    selected.ExecuteImmediately = editedProfile.ExecuteImmediately;

                    _databaseService.SaveProfile(selected);

                    if (selected == CurrentProfile)
                    {
                        OnPropertyChanged(nameof(CurrentProfile));
                        if (selected.ExecuteImmediately)
                        {
                            ApplyGitConfig(selected);
                        }
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