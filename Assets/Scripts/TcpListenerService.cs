using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Assets.Scripts.Model;
using UnityEngine;

namespace Assets.Scripts
{
  public class TcpListenerService : MonoBehaviour
  {
    public readonly string IP;

    public TcpListenerService()
    {
      IP = GetLocalIPv4();
    }

    private const int _defaultPort = 8000;
    private List<TcpListenerNode> _listeners = new List<TcpListenerNode>();
    // An object used to LOCK for thread safe accesses
    public readonly object Lock = new object();
    // Here we will add actions from the background thread
    // that will be "delayed" until the next Update call => Unity main thread
    public readonly Queue<Action> MainThreadActions = new Queue<Action>();

    public void FixedUpdate()
    {
      //Debug.Log(Time.deltaTime);
      // Lock for thread safe access 
      //lock (Lock)
      //{
        // Run all queued actions in order and remove them from the queue
        while (MainThreadActions.Count > 0)
        {
          var action = MainThreadActions.Dequeue();
          action?.Invoke();
        }
      //}
    }

    public void OnDestroy()
    {
      foreach (var listener in _listeners)
      {
        listener.OnDestroy();
      }
    }

    public void StartListener(Node node)
    {
      var port = _listeners
        .Select(l => l.Port)
        .DefaultIfEmpty(_defaultPort)
        .Max() + 1;
      var listener = new TcpListenerNode(port, node, this);
      _listeners.Add(listener);
    }

    public void StopListener(Node node)
    {
      if(NodeHasListener(node))
      {
        var listener = FindListener(node);
        listener.OnDestroy();
        _listeners.Remove(listener);
      }
    }

    public bool NodeHasListener(Node node) 
      => _listeners
      .Select(listener => listener.Node)
      .Contains(node);

    public TcpListenerNode FindListener(Node node)
      => _listeners
      .First(listener => listener.Node == node);

    public string GetLocalIPv4()
    {
      return Dns.GetHostEntry(Dns.GetHostName())
          .AddressList.First(
              f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
          .ToString();
    }
  }
}
