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
                filter.Update(noisySin);

                Console.WriteLine($"{noisySin};{filter.getState()[0]}");
            }
        }

        [TestMethod]
        public void UKFTest_MultipleStates()
        {
            var filter1 = new UnscentedKalmanFilter(L: 2, q: 0.05, r: 0.3);
            var filter2 = new UnscentedKalmanFilter(L: 1, q: 0.05, r: 0.3);
            var filter3 = new UnscentedKalmanFilter(L: 1, q: 0.05, r: 0.3);
            Random rnd = new Random();
            Console.WriteLine($"noisy1; noisy2; filtered1_1; filtered1_2; filtered2; filtered3");

            for (int k = 0; k < 100; k++)
            {
                var noisySin1 = Math.Sin(k * 3.14 * 5 / 180) + (double)rnd.Next(50) / 100;
                var noisySin2 = Math.Sin(k * 3.14 * 5 / 180) + (double)rnd.Next(50) / 100;

                filter1.Update(noisySin1, noisySin2);
                filter2.Update(noisySin1);
                filter3.Update(noisySin2);

                Console.WriteLine($"{noisySin1};{noisySin2};{filter1.getState()[0]};{filter1.getState()[1]};{filter2.getState()[0]};{filter3.getState()[0]}");
            }
        }
    }
}
