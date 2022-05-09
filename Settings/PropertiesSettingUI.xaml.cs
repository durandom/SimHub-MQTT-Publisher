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
    /// Interaction logic for PropertiesSettingUI.xaml
    /// </summary>
    public partial class PropertiesSettingUI : UserControl
    { 
        private SimHubMQTTPublisherPlugin Plugin { get; set; }

        private PropertiesModel Model { get; set; }


        public PropertiesSettingUI()
        {
            InitializeComponent();
            BtnEdit.IsEnabled = false;
            BtnDelete.IsEnabled = false;
        }

        internal void Init(SimHubMQTTPublisherPlugin plugin)
        {
            Plugin = plugin;
            this.Model = new PropertiesModel()
            {
                Properties = new System.Collections.ObjectModel.ObservableCollection<PropertyRow>(),
            };

            var dataPoints = Plugin.PropertiesSettings.DataPoints;
            foreach (var point in dataPoints)
            {
                Model.Properties.Add(new PropertyRow(point.Key, point.Value));
            }

            this.DataContext = this.Model;
        }

        private void LvPropertys_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetSelectedIndex();
        }

        private PropertyRow GetSelectedIndex()
        {
            object item = LvPropertys.SelectedItem;

            if (item is PropertyRow)
            {
                BtnDelete.IsEnabled = true;
                BtnEdit.IsEnabled = true;
                return item as PropertyRow;
            }
            else
            {
                BtnEdit.IsEnabled = false;
                BtnDelete.IsEnabled = false;
            }

            return null;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var item = GetSelectedIndex();

            if (item == null)
                return;

            var result = MessageBox.Show("Do you really want to delete the data point with field " + item.Name + " and property " + item.Property + " with ", "Are you sure?", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            if (result == MessageBoxResult.OK)
            {
                var dataPoints = Plugin.PropertiesSettings.DataPoints;
                if (dataPoints.Remove(item.Name))
                    Model.Properties.Remove(item);
                else
                    MessageBox.Show("Something went wrong deleting the " + item.Name + ". It might have been already deleted", "Something went wrong", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
