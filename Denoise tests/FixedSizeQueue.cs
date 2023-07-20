using System.Collections.Generic;

namespace Denoise_tests
{
  public class FixedSizedQueue<T> : Queue<T>
  {
    public int Size { get; private set; }

    public FixedSizedQueue(int size)
    {
      Size = size;
    }

    public new void Enqueue(T obj)
    {
      base.Enqueue(obj);

      while (base.Count > Size)
      {
        base.Dequeue();
      }
    }
  }
}
