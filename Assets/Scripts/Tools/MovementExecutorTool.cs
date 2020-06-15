﻿using UnityEngine;

namespace Assets.Scripts.Tools
{
  public class MovementExecutorTool : ITool, IMovementTool
  {
    readonly MovementExecutor _movementExecutor;
    readonly ToolPanelController _toolPanelController;

    public MovementExecutorTool(MovementExecutor movementExecutor, ToolPanelController toolPanelController)
    {
      _movementExecutor = movementExecutor;
      _toolPanelController = toolPanelController;
    }

    public void Disable() => _movementExecutor.DisableMovement();

    public string ToolName => "Move";

    public bool CanInteractWith(RaycastHit hitInfo) => false;

    public void OnLeftClick(Transform cameraTransform, bool isRayCastHit, RaycastHit raycastHit)
    {
      _movementExecutor.ToggleMovement();
      _toolPanelController.SetBackgroundColor(GetPanelColor());
    }

    public void OnRightClick(Transform cameraTransform, bool isRayCastHit, RaycastHit raycastHit) =>
      _movementExecutor.ToggleReverse();

    public void OnLeftMouseButtonHeld(Transform cameraTransform) { }

    public void OnLeftMouseButtonReleased() { }

    public void OnSelect() => _toolPanelController.SetBackgroundColor(GetPanelColor());

    private Color GetPanelColor() => _movementExecutor.MovementEnabled ? Color.green : Color.red;
  }
}
