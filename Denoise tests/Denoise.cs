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
    // uaX; uaY; uaZ; gX; gY; gZ
    const int columnNumToCheck = 0;
    const string folder = "";
    static List<float> expectedValues = ExpectedSationary(200);

    [TestMethod]
    public void RunMetricsOnRawData()
    {
      var filesInFolder = Directory.GetFiles(folder, "*.csv");
      var metrics = new List<Metrics>();
      foreach (var file in filesInFolder)
      {
        var values = GetValues(file, columnNumToCheck);
        metrics.Add(CalculateMetrics(values, expectedValues)); 
      }
      var avgMetrics = AverageMetrics(metrics);
    }

    [TestMethod]
    public void RunMovingAverageDenoiseOnFolder()
    {
      const int window = 2;

      var filesInFolder = Directory.GetFiles(folder, "*.csv");
      var metrics = new List<Metrics>();
      foreach (var file in filesInFolder)
      {
        var values = GetValues(file, columnNumToCheck);
        var MA = new SimpleMovingAverage(2);
        var denoised = values
          .Select(MA.Deniose)
          .ToList();
        metrics.Add(CalculateMetrics(denoised, expectedValues));
      }
      var avgMetrics = AverageMetrics(metrics);
    }

    private List<float> GetValues(string fileName, int columnNumToCheck)
    {
      var values = new List<float>();
      using (var fileStream = File.OpenRead(fileName))
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
        RMSE = metrics.Select(m => m.RMSE).Average()
      };
    }

    private static Metrics CalculateMetrics(List<float> denoised, List<float> expected)
    {
      return new Metrics
      {
        RMSE = CalculateRMSE(denoised, expected)
      };
    }

    private static double CalculateRMSE(List<float> denoised, List<float> expected)
    {
      if (expected.Count != denoised.Count)
      {
        throw new ArgumentException("The lengths of predictedValues and actualValues must be the same.");
      }

      int n = expected.Count;
      double sumSquaredErrors = 0.0;

      for (int i = 0; i < n; i++)
      {
        double error = expected[i] - denoised[i];
        sumSquaredErrors += Math.Pow(error, 2);
      }

      double meanSquaredError = sumSquaredErrors / n;
      double rmse = Math.Sqrt(meanSquaredError);

      return rmse;
    }

    private static List<float> ExpectedSationary(int size) => Enumerable.Range(0, size).Select(x => 0F).ToList();
  }
}
