using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model;
using UnityEngine;

namespace Assets.Scripts
{
  public class TcpListenerService : MonoBehaviour
  {
    private const int _defaultPort = 5000;
    private List<TcpListenerNode> _listeners = new List<TcpListenerNode>();

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
      var listener = new TcpListenerNode(port, node);
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
  }
}
