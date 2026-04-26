using UnityEngine;
using UnityEngine.InputSystem;

public class RobotUI : MonoBehaviour
{
    [SerializeField] private RobotCharacter _robot;
    [SerializeField] private GameObject _menuPanel;

    private PlayerControls _controls;
    
    private void OnEnable()
    {
        _controls = new PlayerControls();
        _controls.Enable();
        _controls.Gameplay.ModuleInventory.performed += MenuSwitchState;
    }

    private void OnDisable()
    {
        _controls.Disable();
    }
    
    private void MenuSwitchState(InputAction.CallbackContext obj)
    {
        _menuPanel.SetActive(!_menuPanel.activeSelf);
        _robot.InputIsLocked = _menuPanel.activeSelf;
    }
}
