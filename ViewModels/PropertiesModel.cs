using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimHub.MQTTPublisher.ViewModels
{
    internal class PropertiesModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<PropertyRow> _properties = new ObservableCollection<PropertyRow>();

        public ObservableCollection<PropertyRow> Properties
        {
            get => _properties;
            
            set
            {
                _properties = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class PropertyRow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;

        private string _property;

        public string Name
        {
            get => _name;

            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Property
        {
            get => _property;

            set
            {
                _property = value;
                OnPropertyChanged();
            }
        }

        public PropertyRow() { }

        public PropertyRow(string name, string property)
        {
            _name = name;
            _property = property;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
