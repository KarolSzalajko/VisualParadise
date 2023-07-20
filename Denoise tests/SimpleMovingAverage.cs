namespace Denoise_tests
{
  class SimpleMovingAverage
  {
    public FixedSizedQueue<float> values;

    public SimpleMovingAverage(int window)
    {
      values = new FixedSizedQueue<float>(window);
    }

    public float Deniose(float value)
    {
      values.Enqueue(value);
      return GetAverage();
    }

    private float GetAverage()
    {
      var avg = 0F;

      foreach (var value in values)
      {
        avg += value;
      }

      return avg / values.Size;
    }
  }
}
