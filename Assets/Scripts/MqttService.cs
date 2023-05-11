using System.Collections.Generic;
using System.Linq;
using System.Net;
using Assets.Scripts.Model;
using UnityEngine;
using MQTTnet;
using MQTTnet.Client;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.IO;
using Assets.Scripts.Common;
using Assets.Scripts.Common.Extensions;

namespace Assets.Scripts
{
  public class MqttService : MonoBehaviour
  {
    public readonly string IP = GetLocalIPv4();
    private readonly Dictionary<int, NodeData> nodes = new Dictionary<int, NodeData>();
    private static readonly MqttFactory mqttFactory = new MqttFactory();
    private readonly IMqttClient mqttClient = mqttFactory.CreateMqttClient();
    // Here we will add actions from the background thread
    // that will be "delayed" until the next Update call => Unity main thread
    private readonly Queue<Action> mainThreadActions = new Queue<Action>();

    public void Start()
    {
      // TODO: this can be done when the first node should be connected
      Connect().Wait(10);
    }

    public void FixedUpdate()
    {
      while (mainThreadActions.Count > 0)
      {
        var action = mainThreadActions.Dequeue();
        action?.Invoke();
      }
    }

    public void OnDestroy()
    {
      foreach(var kvp in nodes)
      {
        Unsubscribe(kvp.Value.Id);
      }

      mqttClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build()).Wait();
      mqttClient.Dispose();
    }

    public void Subscribe(Node node)
    {
      var nodeData = new NodeData(node);
      nodes[node.Id] = nodeData;
    }

    public void Unsubscribe(int nodeId)
    {
      nodes[nodeId].Dispose();
      nodes.Remove(nodeId);
    }
    public void Unsubscribe(Node node) => Unsubscribe(node.Id);
    public bool IsNodeIsConnected(Node node) => nodes.ContainsKey(node.Id);
 
    private async Task Connect()
    {
      var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(IP).Build();
      var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic("node/#"))
                .Build();
      mqttClient.ApplicationMessageReceivedAsync += OnMessageReceived;

      try
      {
        using (var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
        {
          Debug.Log($"Trying to connect to MQTT broker at {IP}");
          await mqttClient.ConnectAsync(mqttClientOptions, timeoutToken.Token);
          Debug.Log("MQTT client connected");
          Debug.Log("Trying to subscribe to all node topics");
          await mqttClient.SubscribeAsync(mqttSubscribeOptions, timeoutToken.Token);
          Debug.Log("MQTT client subscribed to all node topics");
        }
      }
      catch (OperationCanceledException)
      {
        Debug.LogError("Timeout while connecting");
      }
    }

    private Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs args)
    {
      var nodeIndex = int.Parse(args.ApplicationMessage.Topic.Substring(5));

      if (nodes.ContainsKey(nodeIndex) == false)
      {
        Debug.LogWarning($"Received message that cannot be processed from topic '{args.ApplicationMessage.Topic}'");

        return Task.CompletedTask;
      }

      var nodeData = nodes[nodeIndex];
      var message = MqttMessageReader.ReadMessage(args.ApplicationMessage.PayloadSegment.Array);

      //var newRotation = AdjustAngularVelocity(message.gyroscope) * 0.02f;
      //var newVelocity = AdjustAcceleration(message.userAcceleration) * 0.02f;

      //TODO: add some sort of a service that looks at the NodeData history and processes it and creates action
      //mainThreadActions.Enqueue(() =>
      //{
      //  node.Rotation += newRotation;
      //  node.Velocity *= 0.9f;
      //  node.Velocity += newVelocity;
      //  var newPosition = node.Velocity * 0.02f;
      //  node.Position += newPosition;
      //});

      nodeData.AddMessage(message);
 
      //TODO: magnetometer?

      return Task.CompletedTask;
    }

    private static string GetLocalIPv4()
      => Dns.GetHostEntry(Dns.GetHostName())
          .AddressList
          .First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
          .ToString();

    private Vector3 AdjustAcceleration(Vector3 acceleration)
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

    private Vector3 AdjustAngularVelocity(Vector3 angularVelocity) 
      => new Vector3(
        x: -angularVelocity.x,
        y: -angularVelocity.y,
        z: angularVelocity.z) * 57.3f;
  }

  class NodeData : IDisposable
  {
    Node _node;

    FixedSizedQueue<MqttMessage> _history;

    StreamWriter _streamWriter;

    public NodeData(Node node)
    {
      _node = node;
      _history = new FixedSizedQueue<MqttMessage>(50);
      var filePath = $"node/{Id}_{DateTime.Now:yyyy-MM-dd HH-mm-ss}.csv";
      _streamWriter = File.CreateText(filePath);
      _streamWriter.WriteLine($"accX; accY; accZ; uaX; uaY; uaZ; gX; gY; gZ; mX; mY; mZ");
    }
    public int Id => _node.Id;

    public void Dispose() => _streamWriter.Dispose();

    public void AddMessage(MqttMessage message)
    {
      _history.Enqueue(message);
      _streamWriter.WriteLine($"{message.acceleration.ToCsvRow()}; {message.userAcceleration.ToCsvRow()}; {message.gyroscope.ToCsvRow()}; {message.magnetometer.ToCsvRow()}");
    }
  }
}
