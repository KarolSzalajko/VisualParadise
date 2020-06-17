﻿using System.IO;
using Assets.Scripts.Common;
using Assets.Scripts.Model;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts
{
  public class GraphLoader : MonoBehaviour
  {
    public GraphService graphService;

    public void Start()
    {
      var graphFilePath = PlayerPrefs.GetString(Constants.GraphFilePathKey);
      var graph = LoadGraph(graphFilePath) ?? new Graph();

      FindObjectOfType<MaterialCache>().Load(graph.classes);

      Debug.Log($"Nodes count: {graph.nodes.Count}\nEdges count: {graph.edges.Count}");
      graphService.SetGraph(graph);
    }

    private static Graph LoadGraph(string filePath)
    {
      //TODO: Try/catch file for bad formats.
      var graphJson = File.ReadAllText(filePath);
      var graph = JsonConvert.DeserializeObject<Graph>(graphJson);
      return graph;
    }
  }
}
