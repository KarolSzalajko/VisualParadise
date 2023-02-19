using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Model
{
  public class TcpListenerNode
  {
    public string Ip { get; private set; }
    public readonly int Port;
    public readonly Node Node;
    private TcpListener _listener;
    private Thread _tcpListenerThread;
    private bool isRunning = false;

    public TcpListenerNode(int port, Node node)
    {
      Port = port;
      Node = node;
      _tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingData));
      _tcpListenerThread.IsBackground = true;
      _tcpListenerThread.Start();
    }

    private void ListenForIncommingData()
    {
      try
      {
        isRunning = true;
        _listener = new TcpListener(IPAddress.Parse("192.168.2.110"), Port);
        _listener.Start();
        Ip = ((IPEndPoint)_listener.LocalEndpoint).Address.ToString();
        Debug.Log($"Started listening on IP address {Ip}:{Port}");
        byte[] bytes = new byte[1024];
        while (isRunning)
        {
          using (var client = _listener.AcceptTcpClient())
          {
            using (NetworkStream stream = client.GetStream())
            {
              int length;		
              while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
              {
                var incommingData = new byte[length];
                Array.Copy(bytes, 0, incommingData, 0, length);
                string message = Encoding.ASCII.GetString(incommingData);
                Debug.Log($"TcpListenerNode {Port} received message: {message}");
                var acceleration = GetAcceleration(message);

                Node.Acceleration = acceleration;
              }
            }
          }
        }
      }
      catch (SocketException socketException)
      {
        Debug.Log("SocketException " + socketException.ToString());
      }
    }

    public void OnDestroy()
    {
      isRunning = false;
      if (_listener != null)
      {
        _listener.Stop();
      }
      _tcpListenerThread.Join();
    }

    //public void UpdateOnData()
    //{
    //  if (_client.Available > 0)
    //  {
    //    NetworkStream stream = _client.GetStream();
    //    byte[] data = new byte[_client.Available];
    //    stream.Read(data, 0, data.Length);

    //    string message = System.Text.Encoding.UTF8.GetString(data);
    //    Debug.Log($"TcpListenerNode {Port} received message: {message}");

    //    var acceleration = GetAcceleration(message);

    //    Node.Acceleration = acceleration;
    //  }
    //}

    private Vector3 GetAcceleration(string message)
    {
      //TODO
      return Vector3.zero;
    }
  }
}
