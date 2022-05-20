using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimHub.MQTTPublisher.ViewModels
{
    internal class EditingPropertyModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private PropertiesModel _mother;

        private PropertyRow _row;

        private string _name;

        private string _property;

        private bool _isEditing;

        public PropertiesModel Mother
        {
            get => _mother;
            set
            {
                _mother = value;
                OnPropertyChanged();
            }
        }

        public PropertyRow Row
        {
            get => _row;
            set
            {
                _row = value;
                OnPropertyChanged();

                Name = value.Name;
                Property = value.Property;
            }
        }

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

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string WindowTitle
        {
            get
            {
                if (IsEditing)
                    return "Edit";
                else
                    return "Add";
            }
        }

        public string AgreeLabel
        {
            get
            {
                if (IsEditing)
                    return "Save";
                else
                    return "Add";
            }
        }
    }
}
