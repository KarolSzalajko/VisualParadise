﻿using Assets.Scripts.Common;
using Assets.Scripts.Model;
using UnityEngine;

namespace Assets.Scripts
{
  public class NodeGameObjectFactory : MonoBehaviour
  {
    MaterialCache _materialCache;

    public GameObject nodePrefab;
    public GameObject ghostNodePrefab;
    public GameObject nodeCanvasPrefab;

    const PrimitiveType c_defaultNodeType = PrimitiveType.Sphere;

    public void Start()
    {
      _materialCache = FindObjectOfType<MaterialCache>();
    }


    public GameObject CreateNodeGameObject(Node node)
    {
      if (_materialCache == null)
        _materialCache = FindObjectOfType<MaterialCache>();

      if (node.nodeClass == null)
        return CreateDefaultNode(node.Position, Quaternion.Euler(node.Rotation));

      var material = _materialCache.GetByTexturePath(node.nodeClass.TexturePath);

      var gameObject = node.nodeClass.Shape == null
        ? GameObject.CreatePrimitive(c_defaultNodeType)
        : GameObject.CreatePrimitive(node.nodeClass.Shape.Value);

      gameObject.transform.localScale = Vector3.one * (node.nodeClass.Scale ?? 1);
      gameObject.transform.position = node.Position;
      gameObject.transform.rotation = Quaternion.Euler(node.Rotation);
      gameObject.tag = Constants.PhysicalNodeTag;

      var renderer = gameObject.GetComponent<Renderer>();
      renderer.material = material;

      Instantiate(nodeCanvasPrefab, gameObject.transform);

      return gameObject;
    }


    public GameObject CreateGhostNodeGameObject(Vector3 position, Quaternion rotation) =>
      Instantiate(ghostNodePrefab, position, rotation);

    public GameObject CreateDefaultNode(Vector3 position, Quaternion rotation) =>
      Instantiate(nodePrefab, position, rotation);

    void Log(string message)
    {
      Debug.Log(message);
    }
  }
}
