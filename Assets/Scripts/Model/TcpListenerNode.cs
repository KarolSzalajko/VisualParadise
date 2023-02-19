using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Assets.Scripts.Model
{
  public class TcpListenerNode : System.IEquatable<TcpListenerNode>
  {
    public readonly string Ip;
    public readonly int Port;
    public readonly Node Node;
    private readonly TcpListener _listener;
    private TcpClient _client;

    public TcpListenerNode(int port, Node node)
    {
      Port = port;
      Node = node;
      _listener = new TcpListener(IPAddress.Any, port);
      _listener.Start();
      Ip = ((IPEndPoint)_listener.LocalEndpoint).Address.ToString();
      Debug.Log("Started listening on IP address " + Ip);
      _client = _listener.AcceptTcpClient();
    }

    public void OnDestroy()
    {
      _listener.Stop();
      _client.Close();
    }

    public void UpdateOnData()
    {
      if (_client.Available > 0)
      {
        NetworkStream stream = _client.GetStream();
        byte[] data = new byte[_client.Available];
        stream.Read(data, 0, data.Length);

        string message = System.Text.Encoding.UTF8.GetString(data);
        Debug.Log($"TcpListenerNode {Port} received message: {message}");

        var acceleration = GetAcceleration(message);

        Node.Acceleration = acceleration;
      }
    }

    private Vector3 GetAcceleration(string message)
    {
      //TODO
      return Vector3.zero;
    }

    public bool Equals(TcpListenerNode other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return Port == other.Port;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType() != GetType())
        return false;
      return Equals((TcpListenerNode)obj);
    }

    public override int GetHashCode() => Port;

    public static bool operator ==(TcpListenerNode left, TcpListenerNode right) => Equals(left, right);

    public static bool operator !=(TcpListenerNode left, TcpListenerNode right) => !Equals(left, right);
  }
}
