using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public sealed class RobotCharacter : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private float _upRotation = 80;
    [SerializeField] private float _downRotation = -80;
    [SerializeField] private float _sensitivity = 0.3f;
    [SerializeField] private ModuleSlot[] _slots;
    
    public List<IRobotModule> AvailableRobotModules = new();

    public bool InputIsLocked = false;

    private CharacterController _controller;
    private PlayerControls _controls;

    private Vector2 _moveInput;
    private Vector2 _lookInput;

    private Vector3 _velocity;

    private IRobotModule _leftArm;
    private IRobotModule _rightArm;
    private IRobotModule _leftLeg;
    private IRobotModule _rightLeg;
    private IRobotModule _core;
    
    private Dictionary<ModuleSlotType, ModuleSlot> _slotMap;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _controls = new PlayerControls();
        _slotMap = new Dictionary<ModuleSlotType, ModuleSlot>();

        foreach (var slot in _slots)
            _slotMap[slot.Type] = slot;
    }

    private void OnEnable()
    {
        _controls.Enable();

        _controls.Gameplay.Move.performed += OnMove;
        _controls.Gameplay.Move.canceled += OnMove;

        _controls.Gameplay.Look.performed += OnLook;
        _controls.Gameplay.Look.canceled += OnLook;

        _controls.Gameplay.Primary.performed += _ => OnPrimaryActionPressed();
        _controls.Gameplay.Primary.canceled += _ => OnPrimaryActionReleased();

        _controls.Gameplay.Secondary.performed += _ => OnSecondaryActionPressed();
        _controls.Gameplay.Secondary.canceled += _ => OnSecondaryActionReleased();

        _controls.Gameplay.Jump.performed += _ => OnJumpPressed();
        _controls.Gameplay.Reload.performed += _ => OnReloadPressed();
    }

    private void OnDisable()
    {
        _controls.Disable();
    }

    private void Update()
    {
        if (!InputIsLocked)
        {
            HandleRotation();
            HandleMovement();
        }
        
        UpdateModules();
    }

    private void HandleMovement()
    {
        Vector3 input = new Vector3(_moveInput.x, 0f, _moveInput.y);

        if (input.sqrMagnitude > 0.01f)
        {
            Vector3 camForward = _cameraPivot.forward;
            Vector3 camRight = _cameraPivot.right;

            camForward.y = 0f;
            camRight.y = 0f;

            Vector3 moveDir = camForward.normalized * input.z + camRight.normalized * input.x;

            _controller.Move(moveDir * _moveSpeed * Time.deltaTime);
        }

        if (_controller.isGrounded && _velocity.y < 0)
            _velocity.y = -2f;

        _velocity.y += _gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void HandleRotation()
    {
        float lookX = _lookInput.x * _sensitivity;
        transform.Rotate(Vector3.up, lookX);

        if (_cameraPivot != null)
        {
            float lookY = _lookInput.y * _sensitivity;
            float currentXRotation = _cameraPivot.localEulerAngles.x;
        
            if (currentXRotation > 180f)
                currentXRotation -= 360f;
        
            float newXRotation = currentXRotation - lookY;
            newXRotation = Mathf.Clamp(newXRotation, _downRotation, _upRotation);
        
            _cameraPivot.localRotation = Quaternion.Euler(newXRotation, 0f, 0f);
        }
    }

    private void UpdateModules()
    {
        _leftArm?.Tick();
        _rightArm?.Tick();
        _leftLeg?.Tick();
        _rightLeg?.Tick();
        _core?.Tick();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext ctx)
    {
        _lookInput = ctx.ReadValue<Vector2>();
    }

    private void OnPrimaryActionPressed()
    {
        _rightArm?.OnPrimaryPressed();
    }

    private void OnPrimaryActionReleased()
    {
        _rightArm?.OnPrimaryReleased();
    }

    private void OnSecondaryActionPressed()
    {
        _leftArm?.OnPrimaryPressed();
    }

    private void OnSecondaryActionReleased()
    {
        _leftArm?.OnPrimaryReleased();
    }

    private void OnJumpPressed()
    {
        _leftLeg?.OnSecondaryPressed();
        _rightLeg?.OnSecondaryPressed();
    }

    private void OnReloadPressed()
    {
        _leftArm?.OnUtility();
        _rightArm?.OnUtility();
    }
}

public interface IRobotModule
{
    void OnAttach(RobotCharacter robot);
    void OnDetach();

    void Tick();

    void OnPrimaryPressed();
    void OnPrimaryReleased();

    void OnSecondaryPressed();
    void OnSecondaryReleased();

    void OnUtility();
}

public interface IDamageable
{
    void TakeDamage(float damage);
}

public enum ModuleSlotType
{
    LeftArm,
    RightArm,
    LeftLeg,
    RightLeg,
    Core
}

[System.Serializable]
public sealed class ModuleSlot
{
    [SerializeField] private ModuleSlotType _type;
    [SerializeField] private Transform _socket;

    private IRobotModule _module;

    public ModuleSlotType Type => _type;
    public IRobotModule Module => _module;

    public void SetModule(ArmModuleBase prefab, RobotCharacter robot)
    {
        _module?.OnDetach();

        if (_module is MonoBehaviour oldMb)
            Object.Destroy(oldMb.gameObject);

        if (prefab == null)
        {
            _module = null;
            return;
        }

        var instance = Object.Instantiate(prefab, _socket);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        _module = instance;
        _module.OnAttach(robot);
    }

    public void Tick()
    {
        _module?.Tick();
    }

    public void PrimaryPressed() => _module?.OnPrimaryPressed();
    public void PrimaryReleased() => _module?.OnPrimaryReleased();

    public void SecondaryPressed() => _module?.OnSecondaryPressed();
    public void SecondaryReleased() => _module?.OnSecondaryReleased();

    public void Utility() => _module?.OnUtility();
}