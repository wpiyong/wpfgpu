using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ImageProcessing.ViewModels
{
    public delegate void ChangeSettingsHandler();

    class SettingsViewModel : ViewModelBase
    {
        public event ChangeSettingsHandler ChangeSettings;

        public SettingsViewModel()
        {
            base.DisplayName = "SettingsViewModel";

            DarkAreaThreshold = App.appSettings.DarkAreaThreshold;
            BrightAreaThreshold = App.appSettings.BrightAreaThreshold;
            Iterations = App.appSettings.Iterations;
            Iterations2 = App.appSettings.Iterations2;
            Gamma = App.appSettings.Gamma;
            Clusters = App.appSettings.Clusters;
            LevelGamma = App.appSettings.LevelGamma;

            CommandUpdate = new RelayCommand(param => UpdateSettings(param));
            CommandCancel = new RelayCommand(param => CancelSettings(param));
        }

        public RelayCommand CommandUpdate { get; set; }
        public RelayCommand CommandCancel { get; set; }

        double _gamma;
        public double Gamma
        {
            get
            {
                return _gamma;
            }

            set
            {
                if(_gamma == value)
                {
                    return;
                }
                _gamma = value;
                OnPropertyChanged("Gamma");
            }
        }

        double _levelgamma;
        public double LevelGamma
        {
            get
            {
                return _levelgamma;
            }

            set
            {
                if (_levelgamma == value)
                {
                    return;
                }
                _levelgamma = value;
                OnPropertyChanged("LevelGamma");
            }
        }

        int _darkAreaThreshold;
        public int DarkAreaThreshold
        {
            get
            {
                return _darkAreaThreshold;
            }
            set
            {
                if (_darkAreaThreshold == value)
                {
                    return;
                }
                _darkAreaThreshold = value;
                OnPropertyChanged("DarkAreaThreshold");
            }
        }

        int _brightAreaThreshold;
        public int BrightAreaThreshold
        {
            get
            {
                return _brightAreaThreshold;
            }
            set
            {
                if (_brightAreaThreshold == value)
                {
                    return;
                }
                _brightAreaThreshold = value;
                OnPropertyChanged("BrightAreaThreshold");
            }
        }

        int _iterations;
        public int Iterations
        {
            get
            {
                return _iterations;
            }
            set
            {
                if(_iterations == value)
                {
                    return;
                }
                _iterations = value;
                OnPropertyChanged("Iterations");
            }
        }

        int _iterations2;
        public int Iterations2
        {
            get
            {
                return _iterations2;
            }
            set
            {
                if (_iterations2 == value)
                {
                    return;
                }
                _iterations2 = value;
                OnPropertyChanged("Iterations2");
            }
        }

        int _clusters;
        public int Clusters
        {
            get
            {
                return _clusters;
            }
            set
            {
                if (_clusters == value)
                {
                    return;
                }
                _clusters = value;
                OnPropertyChanged("Clusters");
            }
        }

        void RaiseChangeSettingsEvent()
        {
            ChangeSettings?.Invoke();
        }

        public void AddChangeSettingsSubscriber(ChangeSettingsHandler handler)
        {
            ChangeSettings += handler;
        }

        void CancelSettings(object param)
        {
            ((Window)param).Close();
        }

        void UpdateSettings(object param)
        {
            bool settingsChanged = false;

            if (App.appSettings.DarkAreaThreshold != DarkAreaThreshold)
            {
                settingsChanged = true;
                App.appSettings.DarkAreaThreshold = DarkAreaThreshold;
            }

            if (App.appSettings.BrightAreaThreshold != BrightAreaThreshold)
            {
                settingsChanged = true;
                App.appSettings.BrightAreaThreshold = BrightAreaThreshold;
            }

            if (App.appSettings.Iterations != Iterations)
            {
                settingsChanged = true;
                App.appSettings.Iterations = Iterations;
            }

            if (App.appSettings.Iterations2 != Iterations2)
            {
                settingsChanged = true;
                App.appSettings.Iterations2 = Iterations2;
            }

            if (App.appSettings.Gamma != Gamma)
            {
                settingsChanged = true;
                App.appSettings.Gamma = Gamma;
            }

            if (App.appSettings.LevelGamma != LevelGamma)
            {
                settingsChanged = true;
                App.appSettings.LevelGamma = LevelGamma;
            }

            if (App.appSettings.Clusters != Clusters)
            {
                settingsChanged = true;
                App.appSettings.Clusters = Clusters;
            }


            if (settingsChanged)
            {
                App.appSettings.Save();
                RaiseChangeSettingsEvent();
            }

            ((Window)param).Close();
        }

        #region dispose
        private bool _disposed = false;

        protected override void OnDispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // dispose 
                }
                _disposed = true;
            }
        }
        #endregion
    }
}
