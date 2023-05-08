﻿using System;
using UnityEngine;

namespace Assets.Scripts
{
  static class MqttMessageReader
  {
    //The message has a char poutix that indicates IBU measurement and x, y, z values.
    public static MqttMessage ReadMessage(byte[] message)
    {
      var processedMessage = new MqttMessage();
      for(var offset = 0; offset < message.Length; offset += 1 + 3*8)
      {
        var indicator = Convert.ToChar(message[offset]);
        var vector = new Vector3(
          x: (float)BitConverter.ToDouble(message, offset + 1),
          y: (float)BitConverter.ToDouble(message, offset + 1 + 8),
          z: (float)BitConverter.ToDouble(message, offset + 1 + 16));

        switch (indicator)
        {
          case 'a':
            processedMessage.acceleration = vector; break;
          case 'u':
            processedMessage.userAcceleration = vector; break;
          case 'g':
            processedMessage.gyroscope = vector; break;
          case 'm':
            processedMessage.magnetometer = vector; break;
        }
      }

      return processedMessage;
    }
  }

  class MqttMessage
  {
    public Vector3 acceleration { get; set; } = Vector3.zero;
    public Vector3 userAcceleration { get; set; } = Vector3.zero;
    public Vector3 gyroscope { get; set; } = Vector3.zero;
    public Vector3 magnetometer { get; set; } = Vector3.zero;
  }

  static class ArraySegmentExtensions
  {
    public static ArraySegment<T> PushOffset<T>(this ArraySegment<T> segment, int additionalOffset)
      => new ArraySegment<T>(segment.Array, segment.Offset + additionalOffset, segment.Count);
  }
}
