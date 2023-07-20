using System;
using MathNet.Numerics.LinearAlgebra;

namespace Denoise_tests
{
    public class SGF
    {
        private readonly int sidePoints;
        private Matrix<double> coefficients;
        private FixedSizedQueue<double> window;

        public SGF(int sidePoints, int polynomialOrder)
        {
            this.sidePoints = sidePoints;
            Design(polynomialOrder);
            window = new FixedSizedQueue<double>(sidePoints * 2 + 1);
        }

        public double Process(double measurement)
        {
            window.Enqueue(measurement);

            if(window.Count < window.Size)
            {
                return measurement;
            }

            return coefficients.Column(window.Size - 1).DotProduct(Vector<double>.Build.DenseOfArray(window.ToArray()));
        }

        private void Design(int polynomialOrder)
        {
            double[,] a = new double[(sidePoints << 1) + 1, polynomialOrder + 1];

            for (int m = -sidePoints; m <= sidePoints; ++m)
            {
                for (int i = 0; i <= polynomialOrder; ++i)
                {
                    a[m + sidePoints, i] = Math.Pow(m, i);
                }
            }

            Matrix<double> s = Matrix<double>.Build.DenseOfArray(a);
            coefficients = s.Multiply(s.TransposeThisAndMultiply(s).Inverse()).Multiply(s.Transpose());
        }
    }
}
