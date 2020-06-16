﻿using UnityEngine;

namespace Assets.Scripts.Tools
{
  internal class LabelVisibilityTool : ITool
  {
    readonly GraphService _graphService;
    readonly ToolPanelController _toolPanelController;

    public string ToolName => "Label\nvisibility";

    public LabelVisibilityTool(GraphService graphService, ToolPanelController toolPanelController)
    {
      _graphService = graphService;
      _toolPanelController = toolPanelController;
    }

    public bool CanInteractWith(RaycastHit hitInfo) => false;
    public void UpdateRaycast(bool isHit, RaycastHit hitInfo) { }

    public void OnLeftClick(Transform cameraTransform, bool isRayCastHit, RaycastHit raycastHit)
    {
      _graphService.ToggleLabelVisibility();
      _toolPanelController.SetBackgroundColor(GetPanelColor());
    }

    public void OnRightClick(Transform cameraTransform, bool isRayCastHit, RaycastHit raycastHit) { }
    public void OnLeftMouseButtonHeld(Transform cameraTransform) { }

    public void OnLeftMouseButtonReleased() { }

    public void OnSelect() => _toolPanelController.SetBackgroundColor(GetPanelColor());

    private Color GetPanelColor() => _graphService.LabelVisibility ? Color.green : Color.red;
  }
}
