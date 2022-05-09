using SimHub.MQTTPublisher.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimHub.MQTTPublisher.Settings
{
    /// <summary>
    /// Interaction logic for PropertyEditWindow.xaml
    /// </summary>
    public partial class PropertyEditWindow : Window
    {
        private SimHubMQTTPublisherPlugin Plugin;

        private EditingPropertyModel Model;

        public PropertyEditWindow()
        {
            InitializeComponent();
        }

        internal void Init(SimHubMQTTPublisherPlugin plugin, EditingPropertyModel model)
        {
            Plugin = plugin;
            Model = model;

            this.DataContext = Model;
        }

        private void BtnAgree_Click(object sender, RoutedEventArgs e)
        {
            if (Model.IsEditing)
            {
                if (Model.Row.Name != Model.Name)
                {
                    if (!CheckIfNameAvailable())
                        return;

                    Plugin.PropertiesSettings.DataPoints.Remove(Model.Row.Name);

                    AddField();

                    Model.Row.Name = Model.Name;
                    Model.Row.Property = Model.Property;
                }
                else
                {
                    Plugin.PropertiesSettings.DataPoints[Model.Name] = Model.Property;

                    Model.Row.Property = Model.Property;
                }
            }
            else
            {
                if (!CheckIfNameAvailable())
                    return;

                AddField();
                Model.Mother.Properties.Add(new PropertyRow(Model.Name, Model.Property));
            }


            this.Close();
        }

        private bool CheckIfNameAvailable()
        {
            var keys = Plugin.PropertiesSettings.DataPoints.Keys;

            string lowerCaseName = Model.Name.ToLower();

            foreach (var key in keys)
            {
                //We won't allow keys with the same spelling but different case
                if (key.ToLower() == lowerCaseName)
                {
                    MessageBox.Show("Field name " + Model.Name + " is already in use. Pick another Name", "Field Name Collision", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        private void AddField()
        {
            //Field has to be added to previousValues prior to being added to DataPoints,
            //else there is a chance for the DataUpdate trying to update our entry and then fail on the check for the previous value
            Plugin.previousValues.Add(Model.Name, "");

            Plugin.PropertiesSettings.DataPoints.Add(Model.Name, Model.Property);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
