using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    //Reference variables
    CharacterController _characterController;
    Animator _animator;
    PlayerInput _playerInput;

    public Animator Animator
    {
        get => _animator;
    }
    public CharacterController CharacterController
    {
        get => _characterController;
    }

    // variables to store optimazed setter/getter parameter IDs
    int 
    _isWalkingHash, 
    _isRunningHash;
    public int IsWalkingHash {get => _isWalkingHash;}
    public int IsRunningHash {get => _isRunningHash;}

    //variables to store player input values
    Vector2 
    _currentMovementInput;
    Vector3 
    _currentMovement, 
    _currentRunMovement, 
    _appliedMovement;
    bool 
    _ismovementPressed, 
    _isRunPressed;

    bool 
    _isWalking,
    _isRuning;
    public Vector2 CurrentMovementInput
    {get => _currentMovementInput;}
    public float CurrentMovementY
    {get => _currentMovement.y; set => _currentMovement.y = value;}
    public Vector3 CurrentRunMovement
    {get => _currentRunMovement;}
    public float AppliedMovementY
    {get => _appliedMovement.y; set => _appliedMovement.y = value;}
    public float AppliedMovementX
    {get => _appliedMovement.x; set => _appliedMovement.x = value;}
    public float AppliedMovementZ
    {get => _appliedMovement.z; set => _appliedMovement.z = value;}
    public bool IsmovementPressed
    {get => _ismovementPressed;}
    public bool IsRunPressed
    {get => _isRunPressed;}
    
    // constants
    float 
    _rotationFactorPerFrame = 13f, 
    _runMultiplier = 3;
    int _zero = 0;
    public float RunMultiplier
    {get => _runMultiplier;}

    //gravity variables
    float 
    _gravity = -9.8f,
    _groundGravity = -0.5f;
    public float Gravity
    {get => _gravity; set => _gravity = value;}
    public float GroundGravity
    {get => _groundGravity; set => _groundGravity = value;}

    //jummp variables
    bool _isJumpPressed, _requiereNewJumpPress;
    float 
    _initialJumpVelocity,
    _maxJumpHeight = 4.0f,
    _maxJumpTime = 0.75f;
    bool _isJumpping = false;
    int _isJumppingHash, 
    _jumpCountHash;
    bool _isJumpAnimating = false;
    int _jumpCount = 0;
    Dictionary<int, float> _initialJumpVelocities = new Dictionary<int, float>();
    Dictionary<int, float> _jumpGravities = new Dictionary<int, float>();
    Coroutine _currentJumpResetRoutine = null;
    public float InitialJumpVelocity {get => _initialJumpVelocity;}
    public float MaxJumpHeight {get => _maxJumpHeight;}
    public float MaxJumpTime {get => _maxJumpTime;}
    public int IsJumppingHash {get => _isJumppingHash;}
    public int JumpCountHash {get => _jumpCountHash;}
    public int JumpCount {get => _jumpCount; set => _jumpCount = value;}
    public bool IsJumpPressed {get => _isJumpPressed;}
    public bool IsJumpping {get => _isJumpping; set => _isJumpping = value;}
    public bool IsJumpAnimating {get => _isJumpAnimating; set => _isJumpAnimating = value;}
    public Dictionary<int, float> InitialJumpVelocities
    {get => _initialJumpVelocities;}
    public Dictionary<int, float> JumpGravities
    {get => _jumpGravities;}
    public Coroutine CurrentJumpResetRoutine
    {get => _currentJumpResetRoutine; set => _currentJumpResetRoutine = value;}
    public bool RequiereNewJumpPress {get => _requiereNewJumpPress; set => _requiereNewJumpPress = value;}
    //state variables 
    PlayerBaseState _currentState;
    PlayerStateFactory _states;
    public bool isGrounded;

    public PlayerBaseState CurrentState
    {
        get => _currentState;
        set => _currentState = value;
    }

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerInput = new PlayerInput();
        _animator = GetComponent<Animator>();

        _states = new PlayerStateFactory(this);
        _currentState = _states.Grounded();
        _currentState.EnterState();

        _isWalkingHash = Animator.StringToHash("Walking");
        _isRunningHash = Animator.StringToHash("Running");
        _isJumppingHash = Animator.StringToHash("Jumping");
        _jumpCountHash = Animator.StringToHash("JumpCount");

        _playerInput.CharacterControlls.Move.performed += OnMoveInput;
        _playerInput.CharacterControlls.Move.canceled += OnMoveInput;
        _playerInput.CharacterControlls.Run.started += OnRun;
        _playerInput.CharacterControlls.Run.canceled += OnRun;
        _playerInput.CharacterControlls.Jump.started += OnJump;
        _playerInput.CharacterControlls.Jump.canceled += OnJump;

        SetUpJumpVariables();
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        _isJumpPressed = ctx.ReadValueAsButton();
        _requiereNewJumpPress = false;
    }

    void OnMoveInput(InputAction.CallbackContext ctx)
    {
        _currentMovementInput = ctx.ReadValue<Vector2>();
        _currentMovement.x = _currentMovementInput.x;
        _currentMovement.z = _currentMovementInput.y;
        _currentRunMovement.x = _currentMovementInput.x * _runMultiplier;
        _currentRunMovement.z = _currentMovementInput.y * _runMultiplier;
        _ismovementPressed = _currentMovementInput.x != 0 || _currentMovementInput.y != 0;
    }

    void OnRun(InputAction.CallbackContext ctx)
    {
        _isRunPressed = ctx.ReadValueAsButton();
    }

    void OnEnable()
    {
        _playerInput.CharacterControlls.Enable();
    }

    void OnDisable()
    {
        _playerInput.CharacterControlls.Disable();
    }

    void SetUpJumpVariables()
    {
        float timeToApex = _maxJumpTime / 2;
        _gravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        _initialJumpVelocity = (2 * _maxJumpHeight) / timeToApex;
        float secondJumpGravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex * 1.25f, 2);
        float SecondJumpInitialVelocity = (2 * _maxJumpHeight) / (timeToApex * 1.25f);
        float thirdJumpGravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex * 1.5f, 2);
        float thirdJumpInitialVelocity = (2 * _maxJumpHeight) / (timeToApex * 1.5f);

        _initialJumpVelocities.Add(1, _initialJumpVelocity);
        _initialJumpVelocities.Add(2, SecondJumpInitialVelocity);
        _initialJumpVelocities.Add(3, thirdJumpInitialVelocity);
    
        _jumpGravities.Add(0, _gravity);
        _jumpGravities.Add(1, _gravity);
        _jumpGravities.Add(2, secondJumpGravity);
        _jumpGravities.Add(3, thirdJumpGravity);
    }


    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        CurrentState.UpdateStates();
        _characterController.Move(_appliedMovement * Time.deltaTime);
        isGrounded = _characterController.isGrounded;
    }

    void HandleRotation()
    {
        Vector3 positionToLoockAt;
        positionToLoockAt.x = _currentMovement.x;
        positionToLoockAt.y = 0.0f;
        positionToLoockAt.z = _currentMovement.z;
        Quaternion currentRotation = transform.rotation;

        if(_ismovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLoockAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationFactorPerFrame);
        }
        
    }
}
