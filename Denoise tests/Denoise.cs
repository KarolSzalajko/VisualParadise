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
      AverageMetrics();
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
      AverageMetrics();
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


    private static List<float> ExpectedSationary(int size) => Enumerable.Range(0, size).Select(x => 0F).ToList();
  }
}
