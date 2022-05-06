using System;
using System.Collections.Generic;

namespace SimHub.MQTTPublisher
{
    /// <summary>
    /// Settings class, make sure it can be correctly serialized using JSON.net
    /// </summary>
    public class SimHubMQTTPublisherPluginSettings
    {
        public string Server { get; set; } = "telemetry.b4mad.racing";

        public string Topic { get; set; } = "racing/driver_name";

        public string Login { get; set; } = "admin";

        public string Password { get; set; } = "admin";

        public int Port { get; set; } = 31883;

        public int UpdateRateLimit { get; set; } = 10;

        public bool Enabled { get; set; } = true;
    }

    public class SimHubMQTTPublisherPluginUserSettings
    {
        public Guid UserId { get; set; } = Guid.NewGuid();

    }

    public class SimHubMQTTPublisherPluginProperties
    {
        public Dictionary<string, string> DataPoints { get; set; }

        public SimHubMQTTPublisherPluginProperties LoadDefaults()
        {
            DataPoints = new Dictionary<string, string>(){
                { "Rpms", "DataCorePlugin.GameData.Rpms" },
                { "SpeedKmh", "DataCorePlugin.GameData.SpeedKmh"},
                { "Clutch", "DataCorePlugin.GameData.Clutch"},
                { "Throttle", "DataCorePlugin.GameData.Throttle"},
                { "Brake", "DataCorePlugin.GameData.Brake"},
                { "Gear", "DataCorePlugin.GameData.Gear"},
                { "CurrentLap", "DataCorePlugin.GameData.CurrentLap"},
                { "CarCoordinates", "DataCorePlugin.GameData.CarCoordinates"},
                { "CurrentLapTime", "DataCorePlugin.GameData.CurrentLapTime"},
                { "SteeringAngle", "ExtraInputProperties.SteeringAngle"},
                { "HandBrake", "DataCorePlugin.GameData.Handbrake"},
                { "TrackPositionPercent", "DataCorePlugin.GameData.TrackPositionPercent"},
                { "CarCoordinates01", "DataCorePlugin.GameData.CarCoordinates01" },
                { "CarCoordinates02", "DataCorePlugin.GameData.CarCoordinates02" },
                { "CarCoordinates03", "DataCorePlugin.GameData.CarCoordinates03" },
            };

            return this;
        }
    }
}