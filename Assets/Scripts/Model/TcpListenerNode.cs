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
    private bool _isRunning = false;

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
        _isRunning = true;
        _listener = new TcpListener(IPAddress.Parse(TcpListenerService.IP), Port);
        _listener.Start();
        Ip = ((IPEndPoint)_listener.LocalEndpoint).Address.ToString();
        Debug.Log($"Started listening on IP address {Ip}:{Port}");
        byte[] bytes = new byte[1024]; 
        while (_isRunning)
        {
          using (var client = _listener.AcceptTcpClient())
          {
            using (NetworkStream stream = client.GetStream())
            {
              int length;		//TODO: this should read one message at a time, now multiple messages can be passed here
              while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
              {
                var incommingData = new byte[length];
                Array.Copy(bytes, 0, incommingData, 0, length);
                string message = Encoding.ASCII.GetString(incommingData);
                Debug.Log($"TcpListenerNode {Port} received message: {message}");

                ApplyAcceleration(GetAcceleration(message));
              }
            }
          }
        }
        _listener.Stop();
        Debug.Log($"Stopped listening on IP address {Ip}:{Port}");
      }
      catch (SocketException socketException)
      {
        Debug.Log("SocketException " + socketException.ToString());
      }
    }

    public void OnDestroy()
    {
      _isRunning = false;
    }

    private void ApplyAcceleration(Vector3 acceleration)
    {
      Node.Acceleration += acceleration;
      Debug.Log("Node acceleration = " + Node.Acceleration);
    }

    private Vector3 GetAcceleration(string message)
    {
      //TODO
      return new Vector3(0.1f, 0, 0);
    }
  }
}
