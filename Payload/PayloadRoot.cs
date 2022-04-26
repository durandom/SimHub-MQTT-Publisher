using GameReaderCommon;
using SimHub.Plugins;
using System;

namespace SimHub.MQTTPublisher.Payload
{
    public class PayloadRoot
    {
        public PayloadRoot(GameData data, SimHubMQTTPublisherPluginUserSettings userSettings, PluginManager pluginManager)
        {
            time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            userId = userSettings.UserId.ToString();
            carState = new Car(data, pluginManager);
            track = new TrackInformation(data);
        }

        public long time { get; set; }
        public string userId { get; set; }
        public Car carState { get; set; }
        public TrackInformation track { get;  set; }
    }
}