namespace Assets.Scripts.Mqtt.Deniose
{
  class KalFil : IDenoiseMethod
  {
    private UnscentedKalmanFilter kfAx, kfAy, kfAz, kfGx, kfGy, kfGz;

    public KalFil(double processNoiseVariance, double measurementNoisevariance)
    {
      kfAx = new UnscentedKalmanFilter(q: processNoiseVariance, r: measurementNoisevariance);
      kfAy = new UnscentedKalmanFilter(q: processNoiseVariance, r: measurementNoisevariance);
      kfAz = new UnscentedKalmanFilter(q: processNoiseVariance, r: measurementNoisevariance);
      kfGx = new UnscentedKalmanFilter(q: processNoiseVariance, r: measurementNoisevariance);
      kfGy = new UnscentedKalmanFilter(q: processNoiseVariance, r: measurementNoisevariance);
      kfGz = new UnscentedKalmanFilter(q: processNoiseVariance, r: measurementNoisevariance);
    }

    public MqttMessage Deniose(MqttMessage message)
    {
      kfAx.Update(message.userAcceleration.x);
      kfAy.Update(message.userAcceleration.y);
      kfAz.Update(message.userAcceleration.z);
      kfGx.Update(message.gyroscope.x);
      kfGy.Update(message.gyroscope.y);
      kfGz.Update(message.gyroscope.z);


      return new MqttMessage
      {
        userAcceleration = new UnityEngine.Vector3(
          x: (float)kfAx.getState()[0],
          y: (float)kfAy.getState()[0],
          z: (float)kfAz.getState()[0]),
        gyroscope = new UnityEngine.Vector3(
          x: (float)kfGx.getState()[0],
          y: (float)kfGy.getState()[0],
          z: (float)kfGz.getState()[0]),
      };
    }
  }
}
