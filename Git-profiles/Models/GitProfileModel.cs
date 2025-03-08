using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;

namespace Git_profiles.Models
{
    public class GitProfileModel : INotifyPropertyChanged
    {
        private int _id;
        private string _name = string.Empty;
        private string _email = string.Empty;
        private bool _isSelected;
        private bool _isActive;
        private bool _useGpg;
        private string _gpgKeyId = string.Empty;
        private string _color = string.Empty;

        private static readonly Color[] PredefinedColors = new[]
        {
            Avalonia.Media.Color.Parse("#D04444"), Avalonia.Media.Color.Parse("#D35F5F"), Avalonia.Media.Color.Parse("#E67E22"), Avalonia.Media.Color.Parse("#F39C12"),
            Avalonia.Media.Color.Parse("#27AE60"), Avalonia.Media.Color.Parse("#2ECC71"), Avalonia.Media.Color.Parse("#1ABC9C"), Avalonia.Media.Color.Parse("#16A085"),
            Avalonia.Media.Color.Parse("#3498DB"), Avalonia.Media.Color.Parse("#2980B9"), Avalonia.Media.Color.Parse("#9B59B6"), Avalonia.Media.Color.Parse("#8E44AD")
        };

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Initials));
                    OnPropertyChanged(nameof(AvatarBackground));
                }
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UseGpg
        {
            get => _useGpg;
            set
            {
                if (_useGpg != value)
                {
                    _useGpg = value;
                    OnPropertyChanged();
                }
            }
        }

        public string GpgKeyId
        {
            get => _gpgKeyId;
            set
            {
                if (_gpgKeyId != value)
                {
                    _gpgKeyId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Initials => !string.IsNullOrEmpty(Name)
            ? (Name.Length > 1 ? Name.Substring(0, 2).ToUpper() : Name.Substring(0, 1).ToUpper())
            : "GP";

        public IBrush AvatarBackground
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return new SolidColorBrush(PredefinedColors[0]);

                // Calcular un índice basado en la suma de los códigos ASCII de las letras del nombre
                int sum = 0;
                foreach (char c in Name)
                {
                    sum += c;
                }

                // Usar el módulo para obtener un índice dentro del rango del array
                int colorIndex = sum % PredefinedColors.Length;
                return new SolidColorBrush(PredefinedColors[colorIndex]);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}