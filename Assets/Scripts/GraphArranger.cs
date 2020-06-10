﻿using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
  public class GraphArranger : MonoBehaviour
  {
    private const float _repelFunCoefficient = 5.0f; // higher values causes more distortion
    private const float _attractFunPower = 1.5f; // safe range <1, 3>
    private const float _maxVelocityMagnitude = 25f;

    private bool _shouldArrange;
    private int _velocityModifier = 1;
    private GraphService graphService;

    private void Start()
    {
      graphService = FindObjectOfType<GraphService>();
    }

    // Update for physics
    private void FixedUpdate()
    {
      if (GameService.Instance.IsPaused)
      {
        return;
      }

      if (!_shouldArrange)
      {
        return;
      }

      Attract();
      Repel();
      graphService.FixEdges();
    }

    public void ToggleArrangement()
    {
      _shouldArrange = !_shouldArrange;
      Debug.Log("Arranger " + (_shouldArrange ? "enabled" : "disabled"));
    }

    public void ToggleReverse()
    {
      _velocityModifier *= -1;
    }

    public void DisableArrangement()
    {
      if (_shouldArrange)
      {
        ToggleArrangement();
      }
    }

    /// <summary>
    ///   Repel each Node from every other node
    /// </summary>
    private void Repel()
    {
      for (var i = 0; i < graphService.Graph.nodes.Count; i++)
      {
        for (var j = i + 1; j < graphService.Graph.nodes.Count; j++)
        {
          var node1 = graphService.Graph.nodes[i];
          var node2 = graphService.Graph.nodes[j];

          var direction = node1.Position - node2.Position;
          var distance = direction.magnitude;

          var velocityMagnitude = CalculateRepelVelocityMagnitude(distance);
          var velocity = direction.normalized * Mathf.Min(_maxVelocityMagnitude, velocityMagnitude);

          node1.Position += velocity * Time.deltaTime * _velocityModifier;
          node2.Position -= velocity * Time.deltaTime * _velocityModifier;
        }
      }
    }

    private float CalculateRepelVelocityMagnitude(float distance)
    {
      var rawResult = _repelFunCoefficient / distance;
      return rawResult;
    }

    /// <summary>
    ///   Attract two nodes if there is an edge to connect them
    /// </summary>
    private void Attract()
    {
      foreach (var e in graphService.Graph.edges)
      {
        var node1 = graphService.Graph.nodes.Single(n => n.id == e.from);
        var node2 = graphService.Graph.nodes.Single(n => n.id == e.to);

        var direction = node1.Position - node2.Position;
        var distance = direction.magnitude;

        var velocityMagnitude = CalculateAttractVelocityMagnitude(distance);
        var velocity = direction.normalized * Mathf.Min(_maxVelocityMagnitude, velocityMagnitude);

        node1.Position -= velocity * Time.deltaTime * _velocityModifier;
        node2.Position += velocity * Time.deltaTime * _velocityModifier;
      }
    }

    private float CalculateAttractVelocityMagnitude(float distance)
    {
      var rawResult = Mathf.Pow(distance, _attractFunPower);
      return rawResult;
    }
  }
}
