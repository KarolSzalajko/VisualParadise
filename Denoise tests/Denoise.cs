using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Denoise_tests
{
  [TestClass]
  // this not actually a test class, but it is easier to run this way
  public class Denoise
  {
    const string flatFileFullPath = @"C:\Users\User\Desktop\Ja\PeWueR\praca magisterska\visual paradise\VisualParadise\node\flat\flat_split.csv";
    const string sinFileFullPath = @"C:\Users\User\Desktop\Ja\PeWueR\praca magisterska\visual paradise\VisualParadise\node\sin\swing_split.csv";

    const string fileFullPath = sinFileFullPath;
    static List<float> ExpectedValues(int size) => ExpectedSin(size);


    [TestMethod]
    public void RunMetricsOnRawData()
    {
      var metrics = new List<Metrics>();
      for(int i = 0; i < 35; i++)
      {
        var values = GetValues(i);
        var metric = CalculateMetrics(values, ExpectedValues(values.Count));
        metrics.Add(metric);
        //Console.WriteLine($"{i}:");
        //Console.WriteLine(metric);
      }
      Console.WriteLine("Average metrics:");
      Console.WriteLine(AverageMetrics(metrics));
    }

    [TestMethod]
    public void RunMovingAverage()
    {
      var windows = new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
      foreach(var window in windows)
      {
        Console.WriteLine($"MA({window})");
        RunMovingAverageDenoise(window);
      }
    }

    [TestMethod]
    public void RunKalmanFilter()
    {
      var processNoiseVariances = new[] { 0.02, 0.04, 0.06, 0.08, 0.1, 0.2, 0.4, 0.6, 0.8, 1, 1.2, 1.4 };
      var measurementNoisevariances = new[] { 0.02, 0.04, 0.06, 0.08, 0.1, 0.2, 0.4, 0.6, 0.8, 1, 1.2, 1.4 };
      foreach (var processNoiseVariance in processNoiseVariances)
      {
        foreach (var measurementNoisevariance in measurementNoisevariances)
        {
          //Console.WriteLine($"Kalman({processNoiseVariance}, {measurementNoisevariance})");
          RunKalmanDenoise(processNoiseVariance, measurementNoisevariance);
        }
        Console.WriteLine();
      }
    }

    [TestMethod]
    public void RunSGFilter()
    {
      var sidePoints = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
      var polynominalOrders = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
      foreach (var sidePoint in sidePoints)
      {
        Console.WriteLine();
        foreach (var polynominalOrder in polynominalOrders)
        {
          //Console.WriteLine($"SGF({sidePoint}, {polynominalOrder})");
          RunSGFDenoise(sidePoint, polynominalOrder);
        }
      }
    }

    public void RunMovingAverageDenoise(int window)
    {
      var metrics = new List<Metrics>();
      for (int i = 0; i < 35; i++)
      {
        var values = GetValues(i);
        var MA = new SimpleMovingAverage(window);
        var denoised = values
          .Select(MA.Deniose)
          .ToList();
        var metric = CalculateMetrics(denoised, ExpectedValues(denoised.Count));
        metrics.Add(metric);
        //Console.WriteLine($"{i}:");
        //Console.WriteLine(metric);
      }
      Console.WriteLine("Average metrics:");
      Console.WriteLine(AverageMetrics(metrics));
    }

    public void RunKalmanDenoise(double processNoiseVariance, double measurementNoisevariance)
    {
      var metrics = new List<Metrics>();
      for (int i = 0; i < 35; i++)
      {
        var values = GetValues(i);
        var kalman = new UnscentedKalmanFilter(q: processNoiseVariance, r: measurementNoisevariance);

        var denoised = values
          .Select(v => { 
            kalman.Update(v);
            return (float)kalman.getState()[0];
          })
          .ToList();

        var metric = CalculateMetrics(denoised, ExpectedValues(denoised.Count));
        metrics.Add(metric);
        //Console.WriteLine($"{i}:");
        //Console.WriteLine(metric);
      }
      Console.Write($"{AverageMetrics(metrics).RMSE};");
      //Console.WriteLine("Average metrics:");
      //Console.WriteLine(AverageMetrics(metrics));
    }

    public void RunSGFDenoise(int sidePoints, int polynominalOrder)
    {
      var metrics = new List<Metrics>();
      for (int i = 0; i < 35; i++)
      {
        var values = GetValues(i);
        var sgf = new SGF(sidePoints, polynominalOrder);

        var denoised = values
          .Select(v => (float)sgf.Process(v))
          .ToList();

        var metric = CalculateMetrics(denoised, ExpectedValues(denoised.Count));
        metrics.Add(metric);
        //Console.WriteLine($"{i}:");
        //Console.WriteLine(metric);
      }
      Console.Write($"{AverageMetrics(metrics).RMSE};");
      //Console.WriteLine("Average metrics:");
      //Console.WriteLine(AverageMetrics(metrics));
    }

    private List<float> GetValues(int columnNumToCheck)
    {
      var values = new List<float>();
      using (var fileStream = File.OpenRead(fileFullPath))
      using (var streamReader = new StreamReader(fileStream))
      {
        streamReader.ReadLine();
        string line;
        while ((line = streamReader.ReadLine()) != null)
        {
          var valueStr = line.Split(';')[columnNumToCheck];
          values.Add(float.Parse(valueStr));
        }
      }
      return values;
    }

    private Metrics AverageMetrics(List<Metrics> metrics)
    {
      return new Metrics
      {
        RMSE = metrics.Select(m => m.RMSE).Average(),
        MAE = metrics.Select(m => m.MAE).Average(),
        //RMSPE = metrics.Select(m => m.RMSPE).Average(),
        //MAPE = metrics.Select(m => m.MAPE).Average()
      };
    }

    private static Metrics CalculateMetrics(List<float> denoised, List<float> expected)
    {
      return new Metrics
      {
        RMSE = CalculateRMSE(denoised, expected),
        MAE = CalculateMAE(denoised, expected),
        //RMSPE = CalculateRMSPE(denoised, expected),
        //MAPE = CalculateMAPE(denoised, expected)
      };
    }

    private static double CalculateRMSE(List<float> measured, List<float> real)
    {
      if (real.Count != measured.Count)
      {
        throw new ArgumentException("The lengths of predictedValues and actualValues must be the same.");
      }

      int n = real.Count;
      double sumSquaredErrors = 0.0;

      for (int i = 0; i < n; i++)
      {
        double error = real[i] - measured[i];
        sumSquaredErrors += Math.Pow(error, 2);
      }

      double meanSquaredError = sumSquaredErrors / n;
      double rmse = Math.Sqrt(meanSquaredError);

      return rmse;
    }

    public static double CalculateMAE(List<float> measured, List<float> real)
    {
      if (real.Count != measured.Count)
        throw new ArgumentException("The lengths of actualValues and predictedValues must be equal.");

      int n = real.Count;
      double sum = 0;

      for (int i = 0; i < n; i++)
      {
        sum += Math.Abs(real[i] - measured[i]);
      }

      return sum / n;
    }

    //public static double CalculateMAPE(List<float> measured, List<float> real)
    //{
    //  if (real.Count != measured.Count)
    //    throw new ArgumentException("The lengths of measured and true values must be equal.");

    //  int n = real.Count;
    //  double sum = 0;

    //  for (int i = 0; i < n; i++)
    //  {
    //    sum += Math.Abs((real[i] - measured[i]) / real[i]);
    //  }

    //  return (sum / n) * 100;
    //}

    //public static double CalculateRMSPE(List<float> measured, List<float> real)
    //{
    //  if (real.Count != measured.Count)
    //    throw new ArgumentException("The lengths of actualValues and predictedValues must be equal.");

    //  int n = real.Count;
    //  double sumSquaredPercentageDiff = 0;

    //  for (int i = 0; i < n; i++)
    //  {
    //    double percentageDiff = (real[i] - measured[i]) / real[i];
    //    sumSquaredPercentageDiff += Math.Pow(percentageDiff, 2);
    //  }

    //  double meanSquaredPercentageDiff = sumSquaredPercentageDiff / n;
    //  double rmspe = Math.Sqrt(meanSquaredPercentageDiff) * 100;

    //  return rmspe;
    //}

    private static List<float> ExpectedSationary(int size) => Enumerable.Range(0, size).Select(x => 0F).ToList();

    //private static List<float> ExpectedDrop(int size) => Enumerable.Range(0, size).Select(x => 9.8F).ToList(); // TODO: Verify

    private static List<float> ExpectedSin(int size)
    {
      List<float> values = new List<float>(size);
      double step = 4 * Math.PI / size;

      for (int i = 0; i < size; i++)
      {
        double angle = i * step;
        var value = (float)Math.Sin(angle)* 4.368326571F;
        values.Add(value);
      }

      return values;
    }
  }
}
