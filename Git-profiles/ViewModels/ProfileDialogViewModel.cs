using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Git_profiles.Models;

namespace Git_profiles.Views
{
    public class ProfileDialogViewModel : INotifyPropertyChanged
    {
        private string _title;
        private GitProfileModel _profile;
        private ObservableCollection<GpgKey> _gpgKeys;

        public ProfileDialogViewModel()
        {
            _title = "";
            _profile = new GitProfileModel();
            _gpgKeys = new ObservableCollection<GpgKey>();
            _ = LoadGpgKeysAsync();
        }

        private async Task LoadGpgKeysAsync()
        {
            var keys = await GpgService.GetGpgKeys();
            _gpgKeys.Clear();
            foreach (var key in keys)
            {
                _gpgKeys.Add(key);
            }
        }

        public ObservableCollection<GpgKey> GpgKeys => _gpgKeys;

        public bool UseGpg
        {
            get => _profile.UseGpg;
            set
            {
                if (_profile.UseGpg != value)
                {
                    _profile.UseGpg = value;
                    if (!value)
                    {
                        _profile.GpgKeyId = string.Empty;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public GpgKey? SelectedGpgKey
        {
            get => _gpgKeys.FirstOrDefault(k => k.KeyId == _profile.GpgKeyId);
            set
            {
                if (value != null && _profile.GpgKeyId != value.KeyId)
                {
                    _profile.GpgKeyId = value.KeyId;
                    OnPropertyChanged();
                }
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        public GitProfileModel Profile
        {
            get => _profile;
            set
            {
                if (_profile != value)
                {
                    _profile = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(UseGpg));
                    OnPropertyChanged(nameof(SelectedGpgKey));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}