using System;
using UnityEngine;

namespace Assets.Scripts.Mqtt
{
  static class MqttMessageReader
  {
    //The message has a char prefix that indicates IBU measurement and x, y, z values.
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
          case 'u':
            processedMessage.userAcceleration = vector; break;
          case 'g':
            processedMessage.gyroscope = vector; break;
        }
      }

      return processedMessage;
    }
  }
}
