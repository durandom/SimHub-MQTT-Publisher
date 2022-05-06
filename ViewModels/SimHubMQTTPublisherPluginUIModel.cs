using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimHub.MQTTPublisher.ViewModels
{
    internal class SimHubMQTTPublisherPluginUIModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _server;

        private int _port;

        private string _topic;

        private string _login;

        private string _password;

        private Guid _UserId;

        private bool _enabled;

        private int _updateRateLimit;

        public string Server
        {
            get => _server;
            set
            {
                _server = value;
                OnPropertyChanged();
            }
        }

        public int Port
        {
            get => _port;
            set
            {
                _port = value;
                OnPropertyChanged();
            }
        }

        public string Topic
        {
            get => _topic;
            set
            {
                _topic = value;
                OnPropertyChanged();
            }
        }

        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public Guid UserId
        {
            get => _UserId;
            set
            {
                _UserId = value;
                OnPropertyChanged();
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                OnPropertyChanged();
            }
        }

        public int UpdateRateLimit
        {
            get => _updateRateLimit;
            set
            {
                _updateRateLimit = value;
                if (_updateRateLimit < 1)
                    _updateRateLimit = 1;

                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}