using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    IEnumerator jumpResetRoutine()
    {
        yield return new WaitForSeconds(.5f);
        _ctx.JumpCount = 0;
    }
    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory){
        _isRootState = true;
        InitializeSubState();
    }
    public override void EnterState()
    {
        HandleJump();
    }
    public override void UpdateState()
    {
        CheckSwitchStates();
        HandleGravity();
    }
    public override void ExitState()
    {
        _ctx.Animator.SetBool(_ctx.IsJumppingHash, false);
        if(_ctx.IsJumpPressed)
        {
            _ctx.RequiereNewJumpPress = true;
        }
        _ctx.CurrentJumpResetRoutine = _ctx.StartCoroutine(jumpResetRoutine());
        if(_ctx.JumpCount == 3)
        {
            _ctx.JumpCount = 0;
            _ctx.Animator.SetInteger(_ctx.JumpCountHash, _ctx.JumpCount);
        }
    }
    public override void CheckSwitchStates()
    {
        if(_ctx.isGrounded)
        {
            SwitchState(_factory.Grounded());
        }
    }

    public override void InitializeSubState(){}

    public void HandleJump()
    {
        if(_ctx.JumpCount < 3 && _ctx.CurrentJumpResetRoutine != null)
            {
                _ctx.StopCoroutine(_ctx.CurrentJumpResetRoutine);
            }
        _ctx.Animator.SetBool(_ctx.IsJumppingHash, true);
        _ctx.IsJumpping = true;
        _ctx.JumpCount += 1;
        _ctx.Animator.SetInteger(_ctx.JumpCountHash, _ctx.JumpCount);
        _ctx.CurrentMovementY = _ctx.InitialJumpVelocities[_ctx.JumpCount];
        _ctx.AppliedMovementY = _ctx.InitialJumpVelocities[_ctx.JumpCount];
    }

    public void HandleGravity()
    {
        bool isFalling = _ctx.CurrentMovementY <= 0.0f || !_ctx.IsJumpPressed;
        float fallMultiplier = 2.0f;

    
        if(isFalling)
        {
            float previousYvelocity = _ctx.CurrentMovementY;
            _ctx.CurrentMovementY = _ctx.CurrentMovementY + (_ctx.JumpGravities[_ctx.JumpCount] * fallMultiplier * Time.deltaTime);
            _ctx.AppliedMovementY = Mathf.Max((_ctx.CurrentMovementY + previousYvelocity) * 0.5f, -20.0f); 
        }
        else
        {
            float previousYvelocity = _ctx.CurrentMovementY;
            _ctx.CurrentMovementY = _ctx.CurrentMovementY + (_ctx.JumpGravities[_ctx.JumpCount] * Time.deltaTime);
            _ctx.AppliedMovementY = (_ctx.CurrentMovementY + previousYvelocity) * 0.5f;
        }
    }
}
