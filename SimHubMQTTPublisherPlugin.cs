using GameReaderCommon;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using SimHub.MQTTPublisher.Settings;
using SimHub.Plugins;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SimHub.MQTTPublisher
{
    [PluginDescription("MQTT Publisher")]
    [PluginAuthor("Wotever")]
    [PluginName("MQTT Publisher")]
    public class SimHubMQTTPublisherPlugin : IPlugin, IDataPlugin, IWPFSettingsV2
    {
        public SimHubMQTTPublisherPluginSettings Settings;

        public SimHubMQTTPublisherPluginUserSettings UserSettings { get; private set; }

        private MqttFactory mqttFactory;
        private IMqttClient mqttClient;
        private Dictionary<string, string> previousValues = new Dictionary<string, string>();

        private Dictionary<string, string> dataPoints = new Dictionary<string, string>()
        {
            {"Rpms", "DataCorePlugin.GameData.Rpms" },
            {"SpeedKmh", "DataCorePlugin.GameData.SpeedKmh"},
            {"Clutch", "DataCorePlugin.GameData.Clutch"},
            {"Throttle", "DataCorePlugin.GameData.Throttle"},
            {"Brake", "DataCorePlugin.GameData.Brake"},
            {"Gear", "DataCorePlugin.GameData.Gear"},
            {"CurrentLap", "DataCorePlugin.GameData.CurrentLap"},
            {"CarCoordinates", "DataCorePlugin.GameData.CarCoordinates"},
            {"CurrentLapTime", "DataCorePlugin.GameData.CurrentLapTime"},
            {"SteeringAngle", "ExtraInputProperties.SteeringAngle"},
            {"HandBrake", "DataCorePlugin.GameData.Handbrake"},
            {"TrackPositionPercent", "DataCorePlugin.GameData.TrackPositionPercent"},
            {"CarCoordinates01", "DataCorePlugin.GameData.CarCoordinates01" },
            {"CarCoordinates02", "DataCorePlugin.GameData.CarCoordinates02" },
            {"CarCoordinates03", "DataCorePlugin.GameData.CarCoordinates03" },
        };

        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager PluginManager { get; set; }

        /// <summary>
        /// Gets the left menu icon. Icon must be 24x24 and compatible with black and white display.
        /// </summary>
        public ImageSource PictureIcon => this.ToIcon(Properties.Resources.sdkmenuicon);

        /// <summary>
        /// Gets a short plugin title to show in left menu. Return null if you want to use the title as defined in PluginName attribute.
        /// </summary>
        public string LeftMenuTitle => "MQTT Publisher";

        /// <summary>
        /// Called one time per game data update, contains all normalized game data,
        /// raw data are intentionnally "hidden" under a generic object type (A plugin SHOULD NOT USE IT)
        ///
        /// This method is on the critical path, it must execute as fast as possible and avoid throwing any error
        ///
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <param name="data">Current game data, including current and previous data frame.</param>
        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            if (data.GameRunning)
            {
                var payload = new Dictionary<string, object>();
                var telemetry = new Dictionary<string, object>();
                object value;
                string stringValue;

                payload["time"] = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();

                foreach (var d in dataPoints)
                {
                    if ($"{d.Key}" == "CurrentLapTime")
                    {
                        value = data.NewData.CurrentLapTime.TotalMilliseconds;
                    }
                    else
                    {
                        value = pluginManager.GetPropertyValue(d.Value);
                    }
                    stringValue = $"{value}";
                    if (stringValue != previousValues[d.Key])
                    {
                        telemetry[d.Key] = value;
                        previousValues[d.Key] = stringValue;
                    }
                }

                if (telemetry.Count > 0)
                {
                    payload["telemetry"] = telemetry;

                    // FIXME: build topic at session start?
                    var topic = Settings.Topic +
                        "/" + UserSettings.UserId.ToString() +
                        "/" + data.SessionId +
                        "/" + data.GameName +
                        "/" + data.NewData.TrackCode.Replace("/", string.Empty) +
                        "/" + data.NewData.CarModel.Replace("/", string.Empty) +
                        "/" + data.NewData.SessionTypeName.Replace("/", string.Empty);

                    var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    //.WithPayload(JsonConvert.SerializeObject(new Payload.PayloadRoot(data, UserSettings, pluginManager)))
                    .WithPayload(JsonConvert.SerializeObject(payload))
                    .Build();

                    Task.Run(async () => await mqttClient.PublishAsync(applicationMessage, CancellationToken.None)).Wait();

                }
            }
        }

        /// <summary>
        /// Called at plugin manager stop, close/dispose anything needed here !
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void End(PluginManager pluginManager)
        {
            // Save settings
            this.SaveCommonSettings("GeneralSettings", Settings);
            this.SaveCommonSettings("UserSettings", UserSettings);
            mqttClient.Dispose();
        }

        /// <summary>
        /// Returns the settings control, return null if no settings control is required
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager)
        {
            return new SimHubMQTTPublisherPluginUI(this);
        }

        /// <summary>
        /// Called once after plugins startup
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void Init(PluginManager pluginManager)
        {
            SimHub.Logging.Current.Info("Starting plugin");
            //SimHub.Logging.Current.Info(string.Join("    \n", pluginManager.GetAllPropertiesNames()));
            foreach (var d in dataPoints)
            {
                previousValues[d.Key] = "";
            }

            // Load settings
            Settings = this.ReadCommonSettings<SimHubMQTTPublisherPluginSettings>("GeneralSettings", () => new SimHubMQTTPublisherPluginSettings());

            UserSettings = this.ReadCommonSettings<SimHubMQTTPublisherPluginUserSettings>("UserSettings", () => new SimHubMQTTPublisherPluginUserSettings());

            this.mqttFactory = new MqttFactory();

            CreateMQTTClient();
        }

        internal void CreateMQTTClient()
        {
            var newmqttClient = mqttFactory.CreateMqttClient();
            // var caCert = X509Certificate.CreateFromCertFile(@"C:\mosquitto_ca.crt");
            // var clientCert = new X509Certificate2(@"client-certificate.pfx", "ExportPasswordUsedWhenCreatingPfxFile");
            // X509Certificate2 caCrt = new X509Certificate2(File.ReadAllBytes(@"C:\mosquitto_ca.crt"));

            var mqttClientOptions = new MqttClientOptionsBuilder()
               .WithTcpServer(Settings.Server, Settings.Port)
               .WithCredentials(Settings.Login, Settings.Password)
               //.WithTls()
               //.WithTls()
               //.WithTls(new MqttClientOptionsBuilderTlsParameters()
               //{
               //    UseTls = true,
               //    SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
               //    Certificates = new List<X509Certificate>()
               //         {
               //             // clientCert, caCert
               //             caCert
               //         }
               //})
               .Build();

            newmqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var oldMqttClient = this.mqttClient;

            mqttClient = newmqttClient;

            if (oldMqttClient != null)
            {
                oldMqttClient.Dispose();
            }
        }
    }
}