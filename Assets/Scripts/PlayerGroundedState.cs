using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory){
        _isRootState = true;
        InitializeSubState();
    }
    public override void EnterState()
    {
        _ctx.CurrentMovementY = _ctx.Gravity;
        _ctx.AppliedMovementY = _ctx.Gravity;
    }
    public override void UpdateState(){
        CheckSwitchStates();
    }
    public override void ExitState(){}
    public override void CheckSwitchStates()
    {
        if(_ctx.IsJumpPressed && !_ctx.RequiereNewJumpPress)
        {
            SwitchState(_factory.Jump());
        } else
        if(!_ctx.isGrounded)
        {
            SwitchState(_factory.Fall());
        }
    }
    public override void InitializeSubState()
    {
        if(!_ctx.IsmovementPressed && !_ctx.IsRunPressed)
        {
            SetSubState(_factory.Idle());
        } else
        if(_ctx.IsmovementPressed && !_ctx.IsRunPressed)
        {
            SetSubState(_factory.Walk());
        } else
        {
          SetSubState(_factory.Run());
        }

    }
}
