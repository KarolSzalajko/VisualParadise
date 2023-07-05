using System;
using UnityEngine;

namespace Assets.Scripts.Mqtt
{
  static class MessageProcessor
  {
    public static MqttMessage Transform(MqttMessage message)
    {
      return new MqttMessage
      {
        userAcceleration = AdjustAcceleration(message.userAcceleration),
        gyroscope = AdjustAngularVelocity(message.gyroscope)
      };
    }

    private static Vector3 AdjustAcceleration(Vector3 acceleration)
      => new Vector3(
        x: -acceleration.x,
        y: -acceleration.y,
        z: acceleration.z);

    private static Vector3 AdjustAngularVelocity(Vector3 angularVelocity)
      => new Vector3(
        x: -angularVelocity.x,
        y: -angularVelocity.y,
        z: angularVelocity.z) * 57.3f;
  }
}
