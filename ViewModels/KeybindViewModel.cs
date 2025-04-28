// In KeybindViewModel.cs
using System;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using Bachelor.Models;
using Bachelor.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bachelor.ViewModels
{
    public class KeybindViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly SettingsModel _settingsModel;
        private ObservableCollection<MovementSettingViewModel> _movementSettings;
        public IEnumerable<string> AvailableMouseActionTypes => MovementManagerService.MouseActionTypes.List;       
        public ObservableCollection<MovementSettingViewModel> MovementSettings => _movementSettings;

        public ReactiveCommand<Unit, Unit> ResetToDefaultsCommand { get; }

        public KeybindViewModel(SettingsModel settingsModel)
        {
            _settingsModel = settingsModel;
            LoadSettings();
            
            ResetToDefaultsCommand = ReactiveCommand.Create(() =>
            {
                _settingsModel.ResetToDefaults();
                LoadSettings();
            });
            
            // Subscribe to settings changes
            _settingsModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(SettingsModel.Settings))
                {
                    LoadSettings();
                }
            };
        }
        
        private void LoadSettings()
        {
            var settings = new ObservableCollection<MovementSettingViewModel>();
            foreach (var kvp in _settingsModel.Settings)
            {
                settings.Add(new MovementSettingViewModel(kvp.Key, kvp.Value, this));
            }
            _movementSettings = settings;
            OnPropertyChanged(nameof(MovementSettings));
        }

        public void SaveSettingProperty(string movementName, string propertyName, object value)
        {
            _settingsModel.UpdateSettingProperty(movementName, propertyName, value);
        }
        
        public new event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Wrapper class for each movement setting
    public class MovementSettingViewModel : INotifyPropertyChanged
    {
        private readonly KeybindViewModel _parent;
        private readonly string _movementName;
        private readonly MovementManagerService.MovementSetting _setting;
        public IEnumerable<string> AvailableMouseActionTypes => _parent.AvailableMouseActionTypes;
        public string MovementName => _movementName;

        public double Threshold
        {
            get => _setting.Threshold;
            set
            {
                if (_setting.Threshold != value)
                {
                    _parent.SaveSettingProperty(_movementName, nameof(Threshold), value);
                    OnPropertyChanged();
                }
            }
        }

        public double Sensitivity
        {
            get => _setting.Sensitivity;
            set
            {
                if (_setting.Sensitivity != value)
                {
                    _parent.SaveSettingProperty(_movementName, nameof(Sensitivity), value);
                    OnPropertyChanged();
                }
            }
        }
        public string Key
        {
            get => _setting.Key;
            set
            {
                if (_setting.Key != value)
                {
                    _parent.SaveSettingProperty(_movementName, nameof(Key), value);
                    OnPropertyChanged();
                }
            }
        }

        public bool Enabled
        {
            get => _setting.Enabled;
            set
            {
                if (_setting.Enabled != value)
                {
                    _parent.SaveSettingProperty(_movementName, nameof(Enabled), value);
                    OnPropertyChanged();
                }
            }
        }

        public bool Continuous
        {
            get => _setting.Continuous;
            set
            {
                if (_setting.Continuous != value)
                {
                    _parent.SaveSettingProperty(_movementName, nameof(Continuous), value);
                    OnPropertyChanged();
                }
            }
        }
        public string SelectedMouseActionType
        {
            get => _setting.MouseActionType;
            set
            {
                if (_setting.MouseActionType != value)
                {
                    _setting.MouseActionType = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public MovementSettingViewModel(string movementName, MovementManagerService.MovementSetting setting, KeybindViewModel parent)
        {
            _movementName = movementName;
            _setting = setting;
            _parent = parent;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}