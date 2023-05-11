using UnityEngine;

namespace Assets.Scripts.Mqtt
{
  class MqttMessage
  {
    public Vector3 acceleration { get; set; } = Vector3.zero;
    public Vector3 userAcceleration { get; set; } = Vector3.zero;
    public Vector3 gyroscope { get; set; } = Vector3.zero;
    public Vector3 magnetometer { get; set; } = Vector3.zero;
  }
}
