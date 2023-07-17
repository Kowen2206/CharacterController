using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    public PlayerWalkState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory){

    }

    public override void EnterState()
    {
        _ctx.Animator.SetBool(_ctx.IsWalkingHash, true);
        _ctx.Animator.SetBool(_ctx.IsRunningHash, false);
    }
    public override void UpdateState()
    {
        CheckSwitchStates();
        _ctx.AppliedMovementX = _ctx.CurrentMovementInput.x;
        _ctx.AppliedMovementZ = _ctx.CurrentMovementInput.y;
    }
    public override void ExitState(){}
    public override void CheckSwitchStates()
    {
        if(!_ctx.IsmovementPressed)
        {
            SwitchState(_factory.Idle());
        } else
        if(_ctx.IsmovementPressed && _ctx.IsRunPressed)
        {
            SwitchState(_factory.Run());
        }
    }
    public override void InitializeSubState(){}
}
