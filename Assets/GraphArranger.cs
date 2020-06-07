﻿using Assets.Scripts;
using UnityEngine;

public class GraphArranger : MonoBehaviour
{
    GraphService graphService;
    MovementExecutor movementExecutor;
    public float repelFunCoefficient = 5.0f; // safe range <1, 50+)
    public float attractFunPower = 1.5f; // safe range <1, 1.5>
    const float maxVelocityMagnitude = 25f;

    private bool arrange = false;

    // Start is called before the first frame update
    // needs to be called after GraphLoader.Start() and after MovementExecutor.Start()
    void Start()
    {
        graphService = GameObject.FindObjectOfType<GraphService>();
        movementExecutor = GameObject.FindObjectOfType<MovementExecutor>();
    }

    // Update for physics
    void FixedUpdate()
    {
        if (GameService.Instance.IsPaused)
          return;

        if (arrange)
        {
            movementExecutor.StopNodes();
            Attract();
            Repel();
            movementExecutor.FixEdges();
        }
    }

    public void HandleArrangeButtonPress()
    {
        arrange = !arrange;
        Debug.Log("Arranger " + (arrange ? "on" : "off"));
        movementExecutor.StopNodes();
    }

    /// <summary>
    /// Repel each Node from every other node
    /// </summary>
    private void Repel()
    {
        foreach (var n in graphService.physicalNodes)
        {
            Rigidbody nodeRb = n.physicalNode.GetComponent<Rigidbody>();
            foreach (var o in graphService.physicalNodes)
            {
                if (n == o) continue;
                Rigidbody otherRb = o.physicalNode.GetComponent<Rigidbody>();

                Vector3 direction = nodeRb.position - otherRb.position;
                float distance = direction.magnitude;

                float velocityMagnitude = CalculateRepellVelocityMagnitude(distance);
                Vector3 velocity = direction.normalized * Mathf.Min(maxVelocityMagnitude, velocityMagnitude);
                otherRb.velocity += -velocity;
                n.node.point.SetPosition(nodeRb.position);
            }
        }
    }

    private float CalculateRepellVelocityMagnitude(float distance)
    {
        float rawResult = repelFunCoefficient / distance;
        return rawResult;
    }

    /// <summary>
    /// Attract two nodes if there is an edge to connect them
    /// </summary>
    private void Attract()
    {
        foreach (var e in graphService.physicalEdges)
        {
            Rigidbody fromRb = e.nodeFrom.physicalNode.GetComponent<Rigidbody>();
            Rigidbody toRb = e.nodeTo.physicalNode.GetComponent<Rigidbody>();

            Vector3 direction = fromRb.position - toRb.position;
            float distance = direction.magnitude;

            float velocityMagnitude = CalculateAttractVelocityMagnitude(distance);
            Vector3 velocity = direction.normalized * Mathf.Min(maxVelocityMagnitude, velocityMagnitude);

            fromRb.velocity += -velocity;
            toRb.velocity += velocity;
        }
    }

    private float CalculateAttractVelocityMagnitude(float distance)
    {
        float rawResult = Mathf.Pow(distance, attractFunPower);
        return rawResult;
    }
}