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
                { "Brake", "DataCorePlugin.GameData.Brake"},
//              {"CarCoordinates", "DataCorePlugin.GameData.CarCoordinates"},
                { "CarCoordinates01", "DataCorePlugin.GameData.CarCoordinates01" },
                { "CarCoordinates02", "DataCorePlugin.GameData.CarCoordinates02" },
                { "CarCoordinates03", "DataCorePlugin.GameData.CarCoordinates03" },
                { "Clutch", "DataCorePlugin.GameData.Clutch"},
                { "CurrentLap", "DataCorePlugin.GameData.CurrentLap"},
                { "CurrentLapTime", "DataCorePlugin.GameData.CurrentLapTime"},
                { "Gear", "DataCorePlugin.GameData.Gear"},
                { "HandBrake", "DataCorePlugin.GameData.Handbrake"},
                { "LastLapTime", "DataCorePlugin.GameData.LastLapTime" },
                { "Position", "DataCorePlugin.GameData.Position" },
                { "Rpms", "DataCorePlugin.GameData.Rpms" },
                { "SpeedKmh", "DataCorePlugin.GameData.SpeedKmh"},
                { "SteeringAngle", "ExtraInputProperties.SteeringAngle"},
                { "Throttle", "DataCorePlugin.GameData.Throttle"},
                { "TrackPositionPercent", "DataCorePlugin.GameData.TrackPositionPercent"},
            };
            

            return this;
        }
    }
}