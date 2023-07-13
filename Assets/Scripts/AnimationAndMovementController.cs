using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationAndMovementController : MonoBehaviour
{

    PlayerInput playerInput;
    Vector2 currentMovementInput;
    Vector3 currentMovement, currentRunMovement;
    CharacterController characterController;
    Animator animator;
    public bool ismovementPressed, isRunPressed, isWalking, isRuning;
    float rotationFactorPerFrame = 13f, runMultiplier = 3;
    int isWalkingHash, isRunningHash;
    

    // Start is called before the first frame update
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = new PlayerInput();
        animator = GetComponent<Animator>();

        isWalkingHash = Animator.StringToHash("Walking");
        isRunningHash = Animator.StringToHash("Running");
        playerInput.CharacterControlls.Move.performed += OnMoveInput;
        playerInput.CharacterControlls.Move.canceled += OnMoveInput;
        playerInput.CharacterControlls.Run.started += OnRun;
        playerInput.CharacterControlls.Run.canceled += OnRun;
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

    void OnDisable()
    {
        playerInput.CharacterControlls.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        HandleAnimation();
        HandleGravity();

        if(isRunPressed)
        {
            characterController.Move(currentRunMovement * Time.deltaTime);
        }
        else
        {
            characterController.Move(currentMovement * Time.deltaTime);
        }
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
        if(characterController.isGrounded)
        {
            float groundGravity = -0.5f;
            currentMovement.y = groundGravity;
            currentRunMovement.y = groundGravity;
        }
        else
        {
            float gravity = -9.8f;
            currentMovement.y += gravity;
            currentRunMovement.y += gravity;
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
}
