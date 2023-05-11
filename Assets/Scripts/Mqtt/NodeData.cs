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
    public FixedSizedQueue<MqttMessage> rawMessages;
    public FixedSizedQueue<MqttMessage> processedMessages;
    StreamWriter _rawStreamWriter;
    StreamWriter _processedStreamWriter;

    public NodeData(Node node)
    {
      this.node = node;
      rawMessages = new FixedSizedQueue<MqttMessage>(50);
      processedMessages = new FixedSizedQueue<MqttMessage>(50);
      var rawFilePath = $"node/{node.Id}_raw_{DateTime.Now:yyyy-MM-dd HH-mm-ss}.csv";
      _rawStreamWriter = File.CreateText(rawFilePath);
      _rawStreamWriter.WriteLine($"accX; accY; accZ; uaX; uaY; uaZ; gX; gY; gZ; mX; mY; mZ");

      var processedFilePath = $"node/{node.Id}_proc_{DateTime.Now:yyyy-MM-dd HH-mm-ss}.csv";
      _processedStreamWriter = File.CreateText(processedFilePath);
      _processedStreamWriter.WriteLine($"accX; accY; accZ; uaX; uaY; uaZ; gX; gY; gZ; mX; mY; mZ");
    }

    public void Dispose()
    {
      _rawStreamWriter.Dispose();
      _processedStreamWriter.Dispose();
    }

    public void AddRawMessage(MqttMessage message)
    {
      rawMessages.Enqueue(message);
      _rawStreamWriter.WriteLine($"{message.acceleration.ToCsvRow()}; {message.userAcceleration.ToCsvRow()}; {message.gyroscope.ToCsvRow()}; {message.magnetometer.ToCsvRow()}");
    }

    public void AddProcessedMessage(MqttMessage message)
    {
      processedMessages.Enqueue(message);
      _processedStreamWriter.WriteLine($"{message.acceleration.ToCsvRow()}; {message.userAcceleration.ToCsvRow()}; {message.gyroscope.ToCsvRow()}; {message.magnetometer.ToCsvRow()}");
    }
  }
}
