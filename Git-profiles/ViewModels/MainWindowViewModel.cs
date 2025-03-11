using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
            RemoveProfileCommand = new AvaloniaCommand(RemoveProfile);
            EditProfileCommand = new AvaloniaCommand(EditProfile);
            SetActiveProfileCommand = new AvaloniaCommand(SetActiveProfile, CanSetActiveProfile);
        }

        private async void LoadProfiles()
        {
            Profiles.Clear();
            var profiles = await Task.Run(() => _databaseService.GetAllProfiles());
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
                    var oldProfile = _currentProfile;
                    _currentProfile = value;
                    OnPropertyChanged();

                    // Ejecutar operaciones de base de datos y Git en segundo plano
                    Task.Run(async () =>
                    {
                        if (oldProfile != null)
                        {
                            oldProfile.IsActive = false;
                            await Task.Run(() => _databaseService.SaveProfile(oldProfile));
                        }

                        if (_currentProfile != null)
                        {
                            _currentProfile.IsActive = true;
                            await Task.Run(() => _databaseService.SaveProfile(_currentProfile));
                            await Task.Run(() => ApplyGitConfig(_currentProfile));
                        }
                    });
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
            var dialog = new ProfileDialog { DialogTitle = "New Profile" };
            if (_parentWindow != null)
            {
                var result = await dialog.ShowDialog<bool>(_parentWindow);
                if (result)
                {
                    var newProfile = dialog.GetProfile();
                    newProfile = await Task.Run(() => _databaseService.SaveProfile(newProfile));
                    Profiles.Add(newProfile);
                }
            }
        }

        private async void RemoveProfile()
        {
            var selected = GetSelectedProfiles();
            if (selected != null && selected.Count > 0)
            {
                var dialog = new ConfirmDialog
                {
                    Title = "Confirm deletion",
                    Message = selected.Count == 1
                        ? "Are you sure you want to delete the selected profile?"
                        : $"Are you sure you want to delete the selected {selected.Count} profiles?"
                };

                if (_parentWindow != null)
                {
                    var result = await dialog.ShowDialog<bool>(_parentWindow);
                    if (result)
                    {
                        await Task.Run(() =>
                        {
                            foreach (var profile in selected)
                            {
                                _databaseService.DeleteProfile(profile.Id);
                            }
                        });

                        foreach (var profile in selected.ToList())
                        {
                            Profiles.Remove(profile);
                        }

                        if (Profiles.Count > 0)
                        {
                            CurrentProfile = Profiles[0];
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Dialog result: " + result);
                        foreach (var profile in selected)
                        {
                            profile.IsSelected = false;
                        }
                    }
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
                var dialog = new ProfileDialog { DialogTitle = "Edit Profile" };
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

                    // Guardar en base de datos de manera asíncrona
                    await Task.Run(() => _databaseService.SaveProfile(selected));

                    if (selected == CurrentProfile)
                    {
                        OnPropertyChanged(nameof(CurrentProfile));
                        await Task.Run(() => ApplyGitConfig(selected));
                    }
                }
                selected.IsSelected = false;
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
            return Profiles.Where(p => p.IsSelected).ToList().FirstOrDefault();
        }
        private List<GitProfileModel>? GetSelectedProfiles()
        {
            return Profiles.Where(p => p.IsSelected).ToList();
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