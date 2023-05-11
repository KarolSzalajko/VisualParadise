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

namespace Assets.Scripts.Mqtt
{
  public class MqttService : MonoBehaviour
  {
    private readonly Func<NodeData, MqttMessage> method = MessageProcessor.SimpleMovingAverage;
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
        Unsubscribe(kvp.Value.node.Id);
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
      var transformedMessage = MessageProcessor.Transform(message);
      nodeData.AddRawMessage(transformedMessage); //Include in thesis that we are adjusting so this is initially transformed data

      var processedMessage = method(nodeData);
      nodeData.AddProcessedMessage(processedMessage);

      AddForExecution(nodeData.node, processedMessage);

      return Task.CompletedTask;
    }

    private void AddForExecution(Node node,MqttMessage message)
    {
      var newRotation = message.gyroscope * 0.02f;
      var newVelocity = message.acceleration * 0.02f;

      mainThreadActions.Enqueue(() =>
      {
        node.Rotation += newRotation;
        node.Velocity *= 0.9f;
        node.Velocity += newVelocity;
        var newPosition = node.Velocity * 0.02f;
        node.Position += newPosition;
      });
    }

    private static string GetLocalIPv4()
      => Dns.GetHostEntry(Dns.GetHostName())
          .AddressList
          .First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
          .ToString();
  }
}
