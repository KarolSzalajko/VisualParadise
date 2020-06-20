﻿using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Model
{
  [Serializable]
  public class NodeClass
  {
    [JsonProperty] public int Id { get; private set; }
    [JsonProperty] public string Name { get; private set; }
    [JsonProperty] public PrimitiveType? Shape { get; private set; }
    [JsonProperty] public string TexturePath { get; private set; }
    [JsonProperty] public float? Scale { get; private set; }


    [OnDeserialized]
    public void OnDeserialized(StreamingContext context)
    {
      if (string.IsNullOrEmpty(Name))
        Name = Id.ToString();
    }
  }
}
