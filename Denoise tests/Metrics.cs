

namespace Denoise_tests
{
  class Metrics
  {
    public double RMSE { get; set; } // Root Mean Square Error
    public double MAE { get; set; } // Mean Absolute Error
                                    //public double RMSPE { get; set; }
                                    //public double MAPE { get; set; }


    public override string ToString() => $"RMSE: {RMSE}\nMAE: {MAE}\n";
  }
}
