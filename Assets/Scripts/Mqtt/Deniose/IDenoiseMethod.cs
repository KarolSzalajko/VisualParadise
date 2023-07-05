namespace Assets.Scripts.Mqtt.Deniose
{
  interface IDenoiseMethod
  {
    MqttMessage Deniose(MqttMessage message);
  }
}
