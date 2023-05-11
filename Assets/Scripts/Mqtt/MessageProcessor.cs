using System;
using System.Linq;
using Assets.Scripts.Common.Extensions;
using UnityEngine;

namespace Assets.Scripts.Mqtt
{
  static class MessageProcessor
  {
    public static MqttMessage Transform(MqttMessage message)
    {
      return new MqttMessage
      {
        acceleration = message.acceleration, //TODO
        userAcceleration = AdjustAcceleration(message.userAcceleration),
        gyroscope = AdjustAngularVelocity(message.gyroscope),
        magnetometer = message.magnetometer //TODO
      };
    }

    public static MqttMessage SimpleMovingAverage(NodeData nodeData)
    {
      const int count = 5;
      var elements = nodeData.rawMessages.TakeLast(count);

      // TODO: consider dropping accelerometer support
      var acc = Vector3.zero;
      var userAcc = Vector3.zero;
      var gyro = Vector3.zero;
      var mag = Vector3.zero;

      if(elements.Count() == count)
      {
        foreach (var elem in elements)
        {
          acc += elem.acceleration;
          userAcc += elem.userAcceleration;
          gyro += elem.gyroscope;
          mag += elem.magnetometer;
        }
      }

      return new MqttMessage
      {
        acceleration = acc / count,
        userAcceleration = userAcc / count,
        gyroscope = gyro / count,
        magnetometer = mag / count,
      };
    }

    public static MqttMessage ExponentialMovingAverage(NodeData nodeData)
    {
      const int count = 5;
      const float alpha = 2 / ((float)count + 1);

      if (nodeData.rawMessages.Count() < count + 1)
      {
        return SimpleMovingAverage(nodeData);
      }

      var last= nodeData.processedMessages.Last();
      var penultimateRaw= nodeData.rawMessages.TakeLast(2).First();
      var lastRaw= nodeData.rawMessages.Last();

      return new MqttMessage
      {
        acceleration = last.acceleration + alpha * (lastRaw.acceleration - penultimateRaw.acceleration),
        userAcceleration = last.userAcceleration + alpha * (lastRaw.userAcceleration - penultimateRaw.userAcceleration),
        gyroscope = last.gyroscope + alpha * (lastRaw.gyroscope - penultimateRaw.gyroscope),
        magnetometer = last.magnetometer + alpha * (lastRaw.magnetometer - penultimateRaw.magnetometer),
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
