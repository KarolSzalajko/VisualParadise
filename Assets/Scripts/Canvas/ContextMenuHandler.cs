using System;
using Assets.Scripts.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Canvas
{
  public class ContextMenuHandler : MonoBehaviour
  {
    private Node _node;
    private TcpListenerNode _tcpListenerNode;

    public GameObject contextMenu;
    public Text Ip;
    public Text ConnectButtonText;
    private GraphService _graphService;
    private TcpListenerService _tcpListenerService;
    private Action _connectButtonAction;

    public void Start()
    {
      _graphService = FindObjectOfType<GraphService>();
      _tcpListenerService = FindObjectOfType<TcpListenerService>();
      contextMenu.SetActive(false);
    }

    public void OpenContextMenu(GameObject gameObjectHit)
    {
      var node = _graphService.FindNodeByGameObject(gameObjectHit);
      _node = node;
      SetConnectionInfo(node);
      contextMenu.SetActive(true);
      GameService.Instance.PauseGameWithoutResume();
    }

    private void SetConnectionInfo(Node node)
    {
      if (_tcpListenerService.NodeHasListener(node))
      {
        _tcpListenerNode = _tcpListenerService.FindListener(node);
        Ip.text = _tcpListenerNode.Ip;
        ConnectButtonText.text = "Disconnect";
        _connectButtonAction = Disconnect;
      }
      else
      {
        Ip.text = "";
        ConnectButtonText.text = "Connect to device";
        _connectButtonAction = Connect;
      }
    }

    public void ChangeParametersButtonOnClick()
    {
      FindObjectOfType<PropertiesMenuHandler>().OpenPropertiesMenu(_node, contextMenu);
    }

    public void ConnectButtonOnClick() => _connectButtonAction();

    private void Connect() => _tcpListenerService.StartListener(_node);

    private void Disconnect() => _tcpListenerService.StopListener(_node);

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
