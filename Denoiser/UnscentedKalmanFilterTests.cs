using Denoiser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests
{
    [TestClass]
    public class UnscentedKalmanFilterTests
    {
        [TestMethod]
        public void UKFTest()
        {
            var filter = new UnscentedKalmanFilter(q: 0.05, r: 0.3);
            Random rnd = new Random();
            Console.WriteLine($"noisy; filtered");

            for (int k = 0; k < 100; k++)
            {
                var noisySin = Math.Sin(k * 3.14 * 5 / 180) + (double)rnd.Next(50) / 100;
                var measurement = new[] { noisySin };
                filter.Update(measurement);

                Console.WriteLine($"{noisySin};{filter.getState()[0]}");
            }
        }
    }
}
