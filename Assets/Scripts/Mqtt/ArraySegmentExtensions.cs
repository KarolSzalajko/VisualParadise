using System;

namespace Assets.Scripts.Mqtt
{
  static class ArraySegmentExtensions
  {
    public static ArraySegment<T> PushOffset<T>(this ArraySegment<T> segment, int additionalOffset)
      => new ArraySegment<T>(segment.Array, segment.Offset + additionalOffset, segment.Count);
  }
}
