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
        private bool _keysLoaded;
        private string? _pendingGpgKeyId;

        public ProfileDialogViewModel()
        {
            _title = "";
            _profile = new GitProfileModel();
            _gpgKeys = new ObservableCollection<GpgKey>();
            _keysLoaded = false;
            _pendingGpgKeyId = null;
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
            _keysLoaded = true;

            // Si tenemos una key pendiente de seleccionar, la seleccionamos ahora
            if (_pendingGpgKeyId != null)
            {
                _profile.GpgKeyId = _pendingGpgKeyId;
                _pendingGpgKeyId = null;
                OnPropertyChanged(nameof(SelectedGpgKey));
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
            get => _keysLoaded ? _gpgKeys.FirstOrDefault(k => k.KeyId == _profile.GpgKeyId) : null;
            set
            {
                if (value != null && _profile.GpgKeyId != value.KeyId)
                {
                    _profile.GpgKeyId = value.KeyId;

                    // Extraer nombre y correo del UserInfo, ignorando la descripción que puede venir después
                    var userInfo = value.UserInfo;
                    var emailMatch = System.Text.RegularExpressions.Regex.Match(userInfo, @".*?<(.+?)>");
                    if (emailMatch.Success)
                    {
                        var email = emailMatch.Groups[1].Value.Trim();
                        // Obtener el nombre eliminando el email y cualquier descripción después
                        var name = userInfo.Split(' ', 2)[0].Trim();

                        _profile.Name = name;
                        _profile.Email = email;
                        OnPropertyChanged(nameof(Profile));
                    }

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
                    if (!string.IsNullOrEmpty(value.GpgKeyId))
                    {
                        if (_keysLoaded)
                        {
                            // Si las keys ya están cargadas, actualizamos la selección inmediatamente
                            OnPropertyChanged(nameof(SelectedGpgKey));
                        }
                        else
                        {
                            // Si las keys no están cargadas, guardamos la key para seleccionarla después
                            _pendingGpgKeyId = value.GpgKeyId;
                        }
                    }
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(UseGpg));
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