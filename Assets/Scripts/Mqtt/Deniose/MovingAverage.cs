using Assets.Scripts.Common;
using UnityEngine;

namespace Assets.Scripts.Mqtt.Deniose
{
  class MovingAverage : IDenoiseMethod
  {
    public FixedSizedQueue<MqttMessage> messages;

    public MovingAverage(int windowSize)
    {
      messages = new FixedSizedQueue<MqttMessage>(windowSize);
    }

    public MqttMessage Deniose(MqttMessage message)
    {
      messages.Enqueue(message);
      return GetAverage();
    }

    private MqttMessage GetAverage()
    {
      var acc = Vector3.zero; 
      var gyr = Vector3.zero;

      foreach (var message in messages)
      {
        acc += message.userAcceleration;
        gyr += message.gyroscope;
      }

      return new MqttMessage
      {
        gyroscope = gyr / messages.Size,
        userAcceleration = acc / messages.Size
      };
    }
  }
}
