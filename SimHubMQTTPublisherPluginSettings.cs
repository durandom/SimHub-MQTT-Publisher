using System;

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
    }

    public class SimHubMQTTPublisherPluginUserSettings
    {
        public Guid UserId { get; set; } = Guid.NewGuid();

    }
}