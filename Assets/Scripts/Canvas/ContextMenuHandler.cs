using System;
using Assets.Scripts.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Canvas
{
  public class ContextMenuHandler : MonoBehaviour
  {
    public GameObject contextMenu;
    public Text Ip;
    public Text Id;
    public Text ConnectButtonText;
    private Node _node;
    private GraphService _graphService;
    private MqttService _mqttService;
    private Action _connectButtonAction;

    public void Start()
    {
      _graphService = FindObjectOfType<GraphService>();
      _mqttService = FindObjectOfType<MqttService>();
      contextMenu.SetActive(false);
    }

    public void Update()
    {
      if(_node != null)
      {
        SetConnectionInfo(_node);
      }
    }

    public void OpenContextMenu(GameObject gameObjectHit)
    {
      var node = _graphService.FindNodeByGameObject(gameObjectHit);
      _node = node;
      contextMenu.SetActive(true);
      GameService.Instance.PauseGameWithoutResume();
    }

    private void SetConnectionInfo(Node node)
    {
      Ip.text = _mqttService.IP;
      Id.text = $"ID: {node.Id}";

      if (_mqttService.IsNodeIsConnected(node))
      {
        ConnectButtonText.text = "Disconnect";
        _connectButtonAction = Disconnect;
      }
      else
      {
        ConnectButtonText.text = "Connect to device";
        _connectButtonAction = Connect;
      }
    }

    public void ChangeParametersButtonOnClick() => FindObjectOfType<PropertiesMenuHandler>().OpenPropertiesMenu(_node, contextMenu);
    public void ConnectButtonOnClick() => _connectButtonAction();

    private void Connect() => _mqttService.Subscribe(_node);

    private void Disconnect() => _mqttService.Unsubscribe(_node);

    public void DeleteButtonOnClick()
    {
      Disconnect();
      _graphService.RemoveNode(_node);
      ExitButtonOnClick();
    }

    public void ExitButtonOnClick()
    {
      contextMenu.SetActive(false);
      GameService.Instance.UnPauseGameWithoutResume();
    }
  }
}
