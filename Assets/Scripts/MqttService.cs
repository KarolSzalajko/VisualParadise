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
    private readonly List<Node> nodes = new List<Node>(); //TODO: dictionary might be faster
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
      var nodeIndex = int.Parse(args.ApplicationMessage.Topic.Substring(5));
      var node = nodes.FirstOrDefault(n => n.Id == nodeIndex);

      if (node == null)
      {
        Debug.LogWarning($"Received message that cannot be processed from topic '{args.ApplicationMessage.Topic}'");

        return Task.CompletedTask;
      }

      var message = MqttMessageReader.ReadMessage(args.ApplicationMessage.PayloadSegment.Array);

      var newRotation = AdjustAngularVelocity(message.gyroscope) * 0.02f;
      var newVelocity = AdjustAcceleration(message.userAcceleration) * 0.02f;

      mainThreadActions.Enqueue(() =>
      {
        node.Rotation += newRotation;
        node.Velocity *= 0.9f;
        node.Velocity += newVelocity;
        var newPosition = node.Velocity * 0.02f;
        node.Position += newPosition;
      });
 
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
}
