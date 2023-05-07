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

namespace Assets.Scripts
{
  public class MqttService : MonoBehaviour
  {
    public readonly string IP = GetLocalIPv4();
    private readonly List<Node> nodes = new List<Node>();
    private static readonly MqttFactory mqttFactory = new MqttFactory();
    private readonly IMqttClient mqttClient = mqttFactory.CreateMqttClient();

    public void Start()
    {
      // TODO: this can be done when the first node should be connected
      Connect().Wait(10);
    }

    public void OnDestroy()
    {
      mqttClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build()).Wait();
      mqttClient.Dispose();
    }

    public void Subscribe(Node node) => nodes.Add(node);
    public void Unsubscribe(Node node) => nodes.Remove(node);
    public bool IsNodeIsConnected(Node node) => nodes.Contains(node);
 
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
      var messageBytes = args.ApplicationMessage.PayloadSegment;
      //TODO: consider reading as doubles
      var message = System.Text.Encoding.UTF8.GetString(messageBytes.Array, messageBytes.Offset, messageBytes.Count);
      var nodeIndex = int.Parse(args.ApplicationMessage.Topic.Substring(5));
      var node = nodes.FirstOrDefault(n => n.Id == nodeIndex);
      Debug.Log($"Node {nodeIndex}");

      if (node == null)
      {
        Debug.LogWarning($"Received message that cannot be processed from topic '{args.ApplicationMessage.Topic}', message: '{message}'");
      }

      // TODO: on received: parse message and adjust nodes
      Debug.Log(message);

      return Task.CompletedTask;
    }

    private static string GetLocalIPv4()
      => Dns.GetHostEntry(Dns.GetHostName())
          .AddressList
          .First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
          .ToString();
  }
}
