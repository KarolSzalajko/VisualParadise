using System;
using System.IO;
using Assets.Scripts.Common;
using Assets.Scripts.Common.Extensions;
using Assets.Scripts.Model;

namespace Assets.Scripts.Mqtt
{
  class NodeData : IDisposable
  {
    public Node node;
    public FixedSizedQueue<MqttMessage> messages;
    StreamWriter _streamWriter;

    public NodeData(Node node)
    {
      this.node = node;
      messages = new FixedSizedQueue<MqttMessage>(50);
      var rawFilePath = $"node/{node.Id}_raw_{DateTime.Now:yyyy-MM-dd HH-mm-ss}.csv";
      _streamWriter = File.CreateText(rawFilePath);
      _streamWriter.WriteLine($"uaX; uaY; uaZ; gX; gY; gZ");
    }

    public void Dispose()
    {
      _streamWriter.Dispose();
    }

    public void AddMessage(MqttMessage message)
    {
      messages.Enqueue(message);
      _streamWriter.WriteLine($"{message.userAcceleration.ToCsvRow()}; {message.gyroscope.ToCsvRow()}");
    }
  }
}
