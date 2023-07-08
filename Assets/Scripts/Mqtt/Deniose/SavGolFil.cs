namespace Assets.Scripts.Mqtt.Deniose
{
  class SavGolFil : IDenoiseMethod
  {
    private SGF sgfAx, sgfAy, sgfAz, sgfGx, sgfGy, sgfGz;

    public SavGolFil(int sidePoints, int polynominalOrder)
    {
      sgfAx = new SGF(sidePoints, polynominalOrder);
      sgfAy = new SGF(sidePoints, polynominalOrder);
      sgfAz = new SGF(sidePoints, polynominalOrder);
      sgfGx = new SGF(sidePoints, polynominalOrder);
      sgfGy = new SGF(sidePoints, polynominalOrder);
      sgfGz = new SGF(sidePoints, polynominalOrder);
    }

    public MqttMessage Deniose(MqttMessage message)
    {
      return new MqttMessage
      {
        userAcceleration = new UnityEngine.Vector3(
          x: (float)sgfAx.Process(message.userAcceleration.x),
          y: (float)sgfAy.Process(message.userAcceleration.y),
          z: (float)sgfAz.Process(message.userAcceleration.z)),
        gyroscope = new UnityEngine.Vector3(
          x: (float)sgfGx.Process(message.gyroscope.x),
          y: (float)sgfGy.Process(message.gyroscope.y),
          z: (float)sgfGz.Process(message.gyroscope.z)),
      };
    }
  }
}
