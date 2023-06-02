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
    {
      var minSignificantThreshold = 0.015f;
      var adjustedX = -acceleration.x;// - 0.021f;
      var adjustedY = -acceleration.y;// + 0.0378f;
      var adjustedZ = acceleration.z;// - 0.165f;

      return new Vector3(
        x: Math.Abs(adjustedX) < minSignificantThreshold ? 0 : adjustedX,
        y: Math.Abs(adjustedY) < minSignificantThreshold ? 0 : adjustedY,
        z: Math.Abs(adjustedZ) < minSignificantThreshold ? 0 : adjustedZ);//* 0.02f;
    }

    private static Vector3 AdjustAngularVelocity(Vector3 angularVelocity)
      => new Vector3(
        x: -angularVelocity.x,
        y: -angularVelocity.y,
        z: angularVelocity.z) * 57.3f;
  }
}
