﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Model
{
  [Serializable]
  public class Graph
  {
    public List<Edge> edges;
    public List<Node> nodes;
    public Node FindNodeById(int id) => nodes.SingleOrDefault(n => n.id == id);

    public IList<Edge> FindEdgesByNodes(Node node1, Node node2)
    {
      var argSet = new HashSet<Node> {node1, node2};
      return (
        from edge in edges
        where argSet.Contains(edge.nodeFrom) && argSet.Contains(edge.nodeTo)
        select edge
      ).ToList();
    }

    public IList<Edge> FindNodeEdges(Node node) =>
      edges.Where(e => e.from == node.id || e.to == node.id).ToList();

    public IDictionary<HashSet<Node>, List<Edge>> GetEdgesGroupedByNodes()
    {
      var groups = new Dictionary<HashSet<Node>, List<Edge>>();
      foreach (var edge in edges)
      {
        var nodesSet = new HashSet<Node> {edge.nodeFrom, edge.nodeTo};
        var existing = groups
          .FirstOrDefault(x => x.Key.SetEquals(nodesSet));
        if (existing.Key == null)
          groups[nodesSet] = new List<Edge> {edge};
        else
          groups[existing.Key].Add(edge);
      }

      return groups;
    }
  }
}
