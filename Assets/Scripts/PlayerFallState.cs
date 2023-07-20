using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallState : PlayerBaseState
{
     public PlayerFallState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory){
        _isRootState = true;
        InitializeSubState();
    }

    public override void EnterState()
    {
        _ctx.Animator.SetBool(_ctx.IsFallingHash, true);
    }
    public override void UpdateState()
    {
        CheckSwitchStates();
        HandleGravity();
    }
    public override void ExitState()
    {
        _ctx.Animator.SetBool(_ctx.IsFallingHash, false);
    }
    public override void CheckSwitchStates()
    {
        if(_ctx.isGrounded)
        {
            SwitchState(_factory.Grounded());
        }
    }
    public override void InitializeSubState()
    {
        
    }

    public void HandleGravity()
    {
        float previousYVelocity = _ctx.CurrentMovementY;
        _ctx.CurrentMovementY = _ctx.CurrentMovementY + _ctx.Gravity * Time.deltaTime;
        _ctx.AppliedMovementY = Mathf.Max((previousYVelocity + _ctx.CurrentMovementY) * .5f, -20.0f);
    }
}
