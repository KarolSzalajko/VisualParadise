using System;
using System.IO;
using Assets.Scripts.Common.Extensions;
using Assets.Scripts.Model;
using Assets.Scripts.Mqtt.Deniose;

namespace Assets.Scripts.Mqtt
{
  class NodeData : IDisposable
  {
    //C:\Users\User\Desktop\Ja\PeWueR\praca magisterska\visual paradise

    public Node node;
    StreamWriter _rawStreamWriter;
    StreamWriter _deniosedStreamWriter;
    IDenoiseMethod _denoiseMethod;

    public NodeData(Node node, IDenoiseMethod denoiseMethod)
    {
      this.node = node;
      _denoiseMethod = denoiseMethod;
      var rawFilePath = $"node/{node.Id}_raw_{DateTime.Now:yyyy-MM-dd HH-mm-ss}.csv";
      var denoisedFilePath = $"node/{node.Id}_{DateTime.Now:yyyy-MM-dd HH-mm-ss}.csv";
      _rawStreamWriter = File.CreateText(rawFilePath);
      _rawStreamWriter.WriteLine($"uaX; uaY; uaZ; gX; gY; gZ");
      _deniosedStreamWriter = File.CreateText(denoisedFilePath);
      _deniosedStreamWriter.WriteLine($"uaX; uaY; uaZ; gX; gY; gZ");
    }

    public void Dispose()
    {
      _rawStreamWriter.Dispose();
      _deniosedStreamWriter.Dispose();
    }

    public void AddMessage(MqttMessage message)
    {
      _rawStreamWriter.WriteLine($"{message.userAcceleration.ToCsvRow()}; {message.gyroscope.ToCsvRow()}");
      var deniosedMessage = _denoiseMethod.Deniose(message);
      _deniosedStreamWriter.WriteLine($"{deniosedMessage.userAcceleration.ToCsvRow()}; {deniosedMessage.gyroscope.ToCsvRow()}");
    }
  }
}
