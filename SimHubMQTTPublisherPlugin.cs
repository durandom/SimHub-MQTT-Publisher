using GameReaderCommon;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using SimHub.MQTTPublisher.Settings;
using SimHub.Plugins;
using System;
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

        private int UpdateSkipCounter;

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
            PluginManager.SetPropertyValue("Connected", this.GetType(), mqttClient.IsConnected);

            //Exits if the plugin is disabled
            if (!Settings.Enabled)
            {
                UpdateSkipCounter = 0; //once enabled again it will immediatly update
                return;
            }

            //Reduced update rate
            UpdateSkipCounter--;
            if (UpdateSkipCounter > 0)
                return;
            

            //Avoid issues with disconnected client
            if (!mqttClient.IsConnected)
            {
                UpdateSkipCounter = 3600; //Timeout of 1min at 60fps

                //Client reconnect is done in a seperate thread to not lock up the update thread when server is offline
                Task.Run(ReconnectClient); 
                return;
            }

            UpdateSkipCounter = Settings.UpdateRateLimit;

            if (data.NewData != null && data.GameRunning)
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
                    string track = "Unknown";
                    if (data.NewData.TrackCode != null)
                        track = data.NewData.TrackCode.Replace("/", string.Empty);

                    string carModel = "Unknown";
                    if (data.NewData.CarModel != null)
                        carModel = data.NewData.CarModel.Replace("/", string.Empty);

                    string sessionType = "Unknown";
                    if (data.NewData.SessionTypeName != null)
                        sessionType = data.NewData.SessionTypeName.Replace("/", string.Empty);

                    string topic = Settings.Topic +
                        "/" + UserSettings.UserId.ToString() +
                        "/" + data.SessionId +
                        "/" + data.GameName +
                        "/" + track +
                        "/" + carModel +
                        "/" + sessionType;

                    var applicationMessage = new MqttApplicationMessageBuilder()
                   .WithTopic(topic)
                   .WithPayload(JsonConvert.SerializeObject(payload))
                   .Build();

                    var task = mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
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
            Log("Starting plugin");
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

            //Properties
            pluginManager.AddProperty("Connected", this.GetType(), true.GetType(), null);
            
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

            if (Settings.Enabled)
                newmqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var oldMqttClient = this.mqttClient;

            mqttClient = newmqttClient;

            if (oldMqttClient != null)
            {
                oldMqttClient.Dispose();
            }
        }

        internal void Log(string text)
        {
            SimHub.Logging.Current.Info(LeftMenuTitle + ": " + text);
        }

        internal void LogError(string text)
        {
            SimHub.Logging.Current.Error(LeftMenuTitle + ": " + text);
        }

        private void ReconnectClient()
        {
            var recon = mqttClient.ReconnectAsync();

            try
            {
                recon.Wait();
            }
            catch (AggregateException ex)
            {
                LogError("Failed to connect to the server: " + ex.InnerException.Message);
            }


            if (!mqttClient.IsConnected) //No connection possible
                UpdateSkipCounter = 600; //Timeout of about 10s before retry
            else
                UpdateSkipCounter = 0; //So the next update can send data
        }
    }
}