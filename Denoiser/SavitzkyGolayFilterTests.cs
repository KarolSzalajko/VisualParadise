using Denoiser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class SavitzkyGolayFilterTests
    {
        [TestMethod]
        public void SGFTests()
        {
            var signal = Enumerable.Range(0, 100).Select(x => Math.Sin(x * 0.1));
            var random = new Random();
            var noise = Enumerable.Range(0, 100).Select(_ => random.NextDouble() * 0.5);
            var noisySignal = signal.Zip(noise, (x, y) => x + y).ToArray();
            var filteredSignal = new SavitzkyGolayFilter(5, 1).Process(noisySignal);
            Console.WriteLine($"noisy; filtered original; filtered continous");

            var sgf = new SGF(5, 1);

            for (int i = 0; i < noisySignal.Length; i++)
            {
                var filtered2 = sgf.Process(noisySignal[i]);
                Console.WriteLine($"{noisySignal[i]}; {filteredSignal[i]}; {filtered2}");
            }
        }
    }
}
