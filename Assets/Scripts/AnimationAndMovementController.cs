using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationAndMovementController : MonoBehaviour
{

    PlayerInput playerInput;
    Vector2 currentMovementInput;
    Vector3 currentMovement, currentRunMovement, appliedMovement;
    CharacterController characterController;
    Animator animator;
    public bool ismovementPressed, isRunPressed, isWalking, 
    isRuning, isJumpping, isJumpPressed, isJumpAnimating;
    float rotationFactorPerFrame = 13f, runMultiplier = 3, 
    maxJumpHeight = 2.0f, maxJumpTime = 0.75f, initialJumpVelocity;
    int isWalkingHash, isRunningHash, isJumppingHash, jumpCountHash;
    float groundGravity = -0.5f;
    float gravity = -9.8f;
    int jumpCount;
    Dictionary<int, float> initialJumpVelocities = new Dictionary<int, float>();
    Dictionary<int, float> jumpGravities = new Dictionary<int, float>();
    Coroutine currentJumpResetRoutine = null;

    // Start is called before the first frame update
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = new PlayerInput();
        animator = GetComponent<Animator>();

        isWalkingHash = Animator.StringToHash("Walking");
        isRunningHash = Animator.StringToHash("Running");
        isJumppingHash = Animator.StringToHash("Jumping");
        jumpCountHash = Animator.StringToHash("JumpCount");

        playerInput.CharacterControlls.Move.performed += OnMoveInput;
        playerInput.CharacterControlls.Move.canceled += OnMoveInput;
        playerInput.CharacterControlls.Run.started += OnRun;
        playerInput.CharacterControlls.Run.canceled += OnRun;
        playerInput.CharacterControlls.Jump.started += OnJump;
        playerInput.CharacterControlls.Jump.canceled += OnJump;

        SetUpJumpVariables();
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        isJumpPressed = ctx.ReadValueAsButton();
    }

    void OnMoveInput(InputAction.CallbackContext ctx)
    {
        currentMovementInput = ctx.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        currentRunMovement.x = currentMovementInput.x * runMultiplier;
        currentRunMovement.z = currentMovementInput.y * runMultiplier;
        ismovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    void OnRun(InputAction.CallbackContext ctx)
    {
        isRunPressed = ctx.ReadValueAsButton();
    }

    void OnEnable()
    {
        playerInput.CharacterControlls.Enable();
    }

    void SetUpJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
        float secondJumpGravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex * 1.25f, 2);
        float SecondJumpInitialVelocity = (2 * maxJumpHeight) / (timeToApex * 1.25f);
        float thirdJumpGravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex * 1.5f, 2);
        float thirdJumpInitialVelocity = (2 * maxJumpHeight) / (timeToApex * 1.5f);

        initialJumpVelocities.Add(1, initialJumpVelocity);
        initialJumpVelocities.Add(2, SecondJumpInitialVelocity);
        initialJumpVelocities.Add(3, thirdJumpInitialVelocity);
    
        jumpGravities.Add(0, gravity);
        jumpGravities.Add(1, gravity);
        jumpGravities.Add(2, secondJumpGravity);
        jumpGravities.Add(3, thirdJumpGravity);
    }

    void HandleJump()
    {
        if(!isJumpping && characterController.isGrounded && isJumpPressed)
        {
            if(jumpCount < 3 && currentJumpResetRoutine != null)
            {
                StopCoroutine(currentJumpResetRoutine);
            }
            animator.SetBool(isJumppingHash, true);
            isJumpAnimating = true;
            isJumpping = true;
            jumpCount += 1;
            animator.SetInteger(jumpCountHash, jumpCount);
            currentMovement.y = initialJumpVelocities[jumpCount];
            appliedMovement.y = initialJumpVelocities[jumpCount];
        }
        else
        if(isJumpping && characterController.isGrounded && !isJumpPressed)
        {
            isJumpping = false;
        }
    }

    void OnDisable()
    {
        playerInput.CharacterControlls.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        HandleAnimation();

        if(isRunPressed)
        {
            appliedMovement.x = currentRunMovement.x;
            appliedMovement.z = currentRunMovement.z;
        }
        else
        {
            appliedMovement.x = currentMovement.x;
            appliedMovement.z = currentMovement.z;
        }
        characterController.Move(appliedMovement * Time.deltaTime);

        HandleGravity();
        HandleJump();
    }

    void HandleAnimation()
    {
       isWalking = animator.GetBool(isWalkingHash);
       isRuning = animator.GetBool(isRunningHash);

       if(ismovementPressed && !isWalking)
       {
            animator.SetBool(isWalkingHash, true);
       }

       if(!ismovementPressed && isWalking)
       {
            animator.SetBool(isWalkingHash, false);
       }

       if((ismovementPressed && isRunPressed ) && !isRuning)
       {
            animator.SetBool(isRunningHash, true);
       }

       if((!ismovementPressed || !isRunPressed ) && isRuning)
       {
            animator.SetBool(isRunningHash, false);
       }
    }

    void HandleGravity()
    {
        bool isFalling = currentMovement.y <= 0.0f || !isJumpPressed;
        float fallMultiplier = 2.0f;

        if(characterController.isGrounded)
        {
            if(isJumpAnimating)
            {
                animator.SetBool(isJumppingHash, false);
                isJumpAnimating = false;
                currentJumpResetRoutine = StartCoroutine(jumpResetRoutine());
                if(jumpCount == 3)
                {
                    jumpCount = 0;
                    animator.SetInteger(jumpCountHash, jumpCount);
                }
            }
            
            currentMovement.y = groundGravity;
            appliedMovement.y = groundGravity;
        }
        else if(isFalling)
        {
            float previousYvelocity = currentMovement.y;
            currentMovement.y = currentMovement.y + (jumpGravities[jumpCount] * fallMultiplier * Time.deltaTime);
            appliedMovement.y = Mathf.Max((currentMovement.y + previousYvelocity) * 0.5f, -20.0f); 
        }
        else
        {
            float previousYvelocity = currentMovement.y;
            currentMovement.y = currentMovement.y + (jumpGravities[jumpCount] * Time.deltaTime);
            appliedMovement.y = (currentMovement.y + previousYvelocity) * 0.5f;
        }
    }

    void HandleRotation()
    {
        Vector3 positionToLoockAt;
        positionToLoockAt.x = currentMovement.x;
        positionToLoockAt.y = 0.0f;
        positionToLoockAt.z = currentMovement.z;
        Quaternion currentRotation = transform.rotation;

        if(ismovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLoockAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame);
        }
        
    }
    IEnumerator jumpResetRoutine()
        {
            yield return new WaitForSeconds(.5f);
            jumpCount = 0;
        }
}
