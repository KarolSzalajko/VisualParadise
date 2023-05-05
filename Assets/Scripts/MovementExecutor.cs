﻿using UnityEngine;

namespace Assets.Scripts
{
  public class MovementExecutor : MonoBehaviour
  {
    int _velocityModifier = 1;
    GraphService graphService;

    public void Start() => graphService = FindObjectOfType<GraphService>();

    public bool Reverse() => _velocityModifier < 0;

    // Update for physics
    public void FixedUpdate()
    {
      if (GameService.Instance.IsPaused)
        return;

      if (MovementEnabled == false)
        return;

      Move();
      Accelerate();
      graphService.FixEdges();
    }

    public bool MovementEnabled { get; private set; }

    public void ToggleMovement()
    {
      MovementEnabled = !MovementEnabled;
      Debug.Log("Node movement " + (MovementEnabled ? "enabled" : "disabled"));
    }

    public void DisableMovement()
    {
      if (MovementEnabled)
        ToggleMovement();
    }

    public void ToggleReverse() => _velocityModifier *= -1;

    void Accelerate()
    {
      //Debug.Log(Time.deltaTime);
      foreach (var n in graphService.Graph.Nodes)
      {
        var newVelocity = n.Velocity + (n.Acceleration * Time.deltaTime);
        var newAngularVelocity = n.AngularVelocity + (n.AngularAcceleration * Time.deltaTime);

        n.Velocity = newVelocity;
        n.AngularVelocity = newAngularVelocity;
      }
    }

    void Move()
    {
      foreach (var n in graphService.Graph.Nodes)
      {
        n.Position += n.Velocity * Time.deltaTime * _velocityModifier;
        n.Rotation += n.AngularVelocity * Time.deltaTime * _velocityModifier;
      }
    }
  }
}
