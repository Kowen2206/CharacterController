using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationAndMovementController : MonoBehaviour
{

    //Reference variables
    CharacterController _characterController;
    Animator _animator;
    PlayerInput _playerInput;

    // variables to store optimazed setter/getter parameter IDs
    int 
    _isWalkingHash, 
    _isRunningHash;

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
    
    // constants
    float 
    _rotationFactorPerFrame = 13f, 
    _runMultiplier = 3;
    int _zero = 0;

    //gravity variables
    float 
    _gravity = -9.8f,
    _groundGravity = -0.5f;

    //jummp variables
    bool _isJumpPressed;
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

    // Start is called before the first frame update
    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerInput = new PlayerInput();
        _animator = GetComponent<Animator>();

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

    void HandleJump()
    {
        if(!_isJumpping && _characterController.isGrounded && _isJumpPressed)
        {
            if(_jumpCount < 3 && _currentJumpResetRoutine != null)
            {
                StopCoroutine(_currentJumpResetRoutine);
            }
            _animator.SetBool(_isJumppingHash, true);
            _isJumpAnimating = true;
            _isJumpping = true;
            _jumpCount += 1;
            _animator.SetInteger(_jumpCountHash, _jumpCount);
            _currentMovement.y = _initialJumpVelocities[_jumpCount];
            _appliedMovement.y = _initialJumpVelocities[_jumpCount];
        }
        else
        if(_isJumpping && _characterController.isGrounded && !_isJumpPressed)
        {
            _isJumpping = false;
        }
    }

    void OnDisable()
    {
        _playerInput.CharacterControlls.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        HandleAnimation();

        if(_isRunPressed)
        {
            _appliedMovement.x = _currentRunMovement.x;
            _appliedMovement.z = _currentRunMovement.z;
        }
        else
        {
            _appliedMovement.x = _currentMovement.x;
            _appliedMovement.z = _currentMovement.z;
        }
        _characterController.Move(_appliedMovement * Time.deltaTime);

        HandleGravity();
        HandleJump();
    }

    void HandleAnimation()
    {
       _isWalking = _animator.GetBool(_isWalkingHash);
       _isRuning = _animator.GetBool(_isRunningHash);

       if(_ismovementPressed && !_isWalking)
       {
            _animator.SetBool(_isWalkingHash, true);
       }

       if(!_ismovementPressed && _isWalking)
       {
            _animator.SetBool(_isWalkingHash, false);
       }

       if((_ismovementPressed && _isRunPressed ) && !_isRuning)
       {
            _animator.SetBool(_isRunningHash, true);
       }

       if((!_ismovementPressed || !_isRunPressed ) && _isRuning)
       {
            _animator.SetBool(_isRunningHash, false);
       }
    }

    void HandleGravity()
    {
        bool isFalling = _currentMovement.y <= 0.0f || !_isJumpPressed;
        float fallMultiplier = 2.0f;

        if(_characterController.isGrounded)
        {
            if(_isJumpAnimating)
            {
                _animator.SetBool(_isJumppingHash, false);
                _isJumpAnimating = false;
                _currentJumpResetRoutine = StartCoroutine(jumpResetRoutine());
                if(_jumpCount == 3)
                {
                    _jumpCount = 0;
                    _animator.SetInteger(_jumpCountHash, _jumpCount);
                }
            }
            
            _currentMovement.y = _groundGravity;
            _appliedMovement.y = _groundGravity;
        }
        else if(isFalling)
        {
            float previousYvelocity = _currentMovement.y;
            _currentMovement.y = _currentMovement.y + (_jumpGravities[_jumpCount] * fallMultiplier * Time.deltaTime);
            _appliedMovement.y = Mathf.Max((_currentMovement.y + previousYvelocity) * 0.5f, -20.0f); 
        }
        else
        {
            float previousYvelocity = _currentMovement.y;
            _currentMovement.y = _currentMovement.y + (_jumpGravities[_jumpCount] * Time.deltaTime);
            _appliedMovement.y = (_currentMovement.y + previousYvelocity) * 0.5f;
        }
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
    IEnumerator jumpResetRoutine()
        {
            yield return new WaitForSeconds(.5f);
            _jumpCount = 0;
        }
}
