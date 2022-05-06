using SimHub.MQTTPublisher.ViewModels;
using System.Windows.Controls;

namespace SimHub.MQTTPublisher.Settings
{
    /// <summary>
    /// Logique d'interaction pour SimHubMQTTPublisherPluginUI.xaml
    /// </summary>
    public partial class SimHubMQTTPublisherPluginUI : UserControl
    {
        public SimHubMQTTPublisherPluginUI()
        {
            InitializeComponent();
        }

        internal void Init(SimHubMQTTPublisherPlugin simHubMQTTPublisherPlugin)
        {
            SimHubMQTTPublisherPlugin = simHubMQTTPublisherPlugin;

            this.Model = new SimHubMQTTPublisherPluginUIModel()
            {
                Enabled = simHubMQTTPublisherPlugin.Settings.Enabled,
                Server = simHubMQTTPublisherPlugin.Settings.Server,
                Port = simHubMQTTPublisherPlugin.Settings.Port,
                Topic = simHubMQTTPublisherPlugin.Settings.Topic,
                Login = simHubMQTTPublisherPlugin.Settings.Login,
                Password = simHubMQTTPublisherPlugin.Settings.Password,
                UserId = simHubMQTTPublisherPlugin.UserSettings.UserId,
                UpdateRateLimit = simHubMQTTPublisherPlugin.Settings.UpdateRateLimit,
            };

            this.DataContext = Model;
        }

        private SimHubMQTTPublisherPluginUIModel Model { get; set; }

        private SimHubMQTTPublisherPlugin SimHubMQTTPublisherPlugin { get; set; }

        private void Apply_Settings(object sender, System.Windows.RoutedEventArgs e)
        {
            SimHubMQTTPublisherPlugin.Settings.Enabled = Model.Enabled;
            SimHubMQTTPublisherPlugin.Settings.Port = Model.Port;
            SimHubMQTTPublisherPlugin.Settings.Server = Model.Server;
            SimHubMQTTPublisherPlugin.Settings.Topic = Model.Topic;
            SimHubMQTTPublisherPlugin.Settings.Login = Model.Login;
            SimHubMQTTPublisherPlugin.Settings.Password = Model.Password;
            SimHubMQTTPublisherPlugin.Settings.UpdateRateLimit = Model.UpdateRateLimit;

            SimHubMQTTPublisherPlugin.CreateMQTTClient();
        }
    }
}