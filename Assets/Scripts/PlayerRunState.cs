using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    public PlayerRunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory){

    }

    public override void EnterState()
    {
        _ctx.Animator.SetBool(_ctx.IsWalkingHash, true);
        _ctx.Animator.SetBool(_ctx.IsRunningHash, true);
    }
    public override void UpdateState()
    {
        CheckSwitchStates();
        _ctx.AppliedMovementX = _ctx.CurrentMovementInput.x * _ctx.RunMultiplier;
        _ctx.AppliedMovementZ = _ctx.CurrentMovementInput.y * _ctx.RunMultiplier;
    }
    public override void ExitState(){}
    public override void CheckSwitchStates()
    {
        if(!_ctx.IsmovementPressed)
        {
            SwitchState(_factory.Idle());
        } else
        if(_ctx.IsmovementPressed && !_ctx.IsRunPressed)
        {
            SwitchState(_factory.Walk());
        }
    }
    public override void InitializeSubState(){}
}
