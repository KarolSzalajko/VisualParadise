using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Assets.Scripts.Common.Extensions;
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
    private TcpListenerService _service;

    public TcpListenerNode(int port, Node node, TcpListenerService service)
    {
      Port = port;
      Node = node;
      _service = service;
      _tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingDataAsync));
      _tcpListenerThread.IsBackground = true;
      _tcpListenerThread.Start();
    }

    private void ListenForIncommingDataAsync()
    {
      try
      {
        _isRunning = true;
        _listener = new TcpListener(IPAddress.Parse(_service.IP), Port);
        _listener.Start();
        Ip = ((IPEndPoint)_listener.LocalEndpoint).Address.ToString();
        Debug.Log($"Started listening on IP address {Ip}:{Port}");
        byte[] bytes = new byte[96];
        var filePath = $"sensors/test_{DateTime.Now:yyyy-MM-dd HH-mm-ss}.csv";
        while (_isRunning)
        {
          try
          {
            using (var client = _listener.AcceptTcpClient())
            {
              using (NetworkStream stream = client.GetStream())
              {
                int length;
                var lastMeasurementTime = DateTime.Now;
                var startDate = DateTime.Now;
                var dataCounter = 0;
                using (StreamWriter sw = File.CreateText(filePath))
                {
                  sw.WriteLine($"accX; accY; accZ; uaX; uaY; uaZ; gX; gY; gZ; mX; mY; mZ");
                  while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                  {
                    if (length != bytes.Length)
                    {
                      Debug.Log("The message was not complete. Length: " + length);
                      continue;
                    }
                    var acceleration = new Vector3(
                      x: (float)BitConverter.ToDouble(bytes, 0),
                      y: (float)BitConverter.ToDouble(bytes, 8),
                      z: (float)BitConverter.ToDouble(bytes, 16));

                    var userAcceleration = new Vector3(
                      x: (float)BitConverter.ToDouble(bytes, 24),
                      y: (float)BitConverter.ToDouble(bytes, 32),
                      z: (float)BitConverter.ToDouble(bytes, 40));

                    var angularVelocity = new Vector3(
                      x: (float)BitConverter.ToDouble(bytes, 48),
                      y: (float)BitConverter.ToDouble(bytes, 56),
                      z: (float)BitConverter.ToDouble(bytes, 64));

                    var magnetometer = new Vector3(
                      x: (float)BitConverter.ToDouble(bytes, 72),
                      y: (float)BitConverter.ToDouble(bytes, 80),
                      z: (float)BitConverter.ToDouble(bytes, 88));

                    sw.WriteLine($"{acceleration.ToCsvRow()}; {userAcceleration.ToCsvRow()}; {angularVelocity.ToCsvRow()}; {magnetometer.ToCsvRow()}");

                    dataCounter++;
                    var measurementTime = DateTime.Now;
                    var deltaTime = (float)(lastMeasurementTime - measurementTime).TotalSeconds;
                    //var deltaTotalTime = (float)(measurementTime - startDate).TotalSeconds;
                    lastMeasurementTime = measurementTime;

                    //Debug.Log("Avg sample rate: " + dataCounter / deltaTotalTime);

                    // Lock for thread safe access
                    //lock (_service.Lock)
                    //{
                    // Add an action that requires the main thread
                    _service.MainThreadActions.Enqueue(() =>
                      {
                        var newRotation = AdjustAngularVelocity(angularVelocity) * deltaTime;
                        Node.Velocity *= 0.9f;
                        Node.Velocity += AdjustAcceleration(userAcceleration) * deltaTime ;
                        var newPosition = Node.Velocity * deltaTime;
                        Node.Rotation += newRotation;
                        Node.Position += newPosition;
                      });
                    //}
                  }
                }
              }
            }
          }
          catch(SocketException se)
          {
            Debug.Log($"Stopped listening (with disconnect exception) on IP address {Ip}:{Port}");
          }
        }
        _listener.Stop();
       // Debug.Log($"Stopped listening on IP address {Ip}:{Port}");
      }
      catch (SocketException socketException)
      {
        Debug.Log("SocketException " + socketException.ToString());
      }
    }

    public void OnDestroy()
    {
      _isRunning = false;
      _listener.Stop();
    }

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
    {
      return new Vector3(
        x: -angularVelocity.x,
        y: -angularVelocity.y,
        z: angularVelocity.z) * 57.3f;
    }
  }
}
