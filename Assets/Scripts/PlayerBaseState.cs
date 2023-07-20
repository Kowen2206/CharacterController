using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseState : MonoBehaviour
{
    protected bool _isRootState = false;
    protected PlayerStateMachine _ctx;
    protected PlayerStateFactory _factory;
    protected PlayerBaseState _currentSuperState;
    protected PlayerBaseState _currentSubState;

    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    {
        _ctx = currentContext;
        _factory = playerStateFactory;
    }
    //Debería llamarse OnEnterState o SetUpState porque este método se ejecuta solo cuando
    //necesitamos ejecutar código al inicio del estado, como por ejemplo ejecutar la 
    //animación de saltar al entrar al estado de Jump. 
    public abstract void EnterState();
    
    public abstract void UpdateState();
    //Debería llamarse OnExitState o OnFinishState, ya que su función es ejecutar código cuando
    //la ejecución del estado en cuestion esta a punto de terminar.
    public abstract void ExitState();
    //Este método si esta nombrado correctamente, pues se encarga de checar las transiciones de
    //un estado a otro
    public abstract void CheckSwitchStates();
    //Debería llamarse SelectSubState pues se encarga de seleccionar el siguiente sub estado
    //del estado principal
    public abstract void InitializeSubState();
    //Debería llamarse MonoBehaviourFakeUpdate ya que su unica finalidad es servir como sustituto del
    //Método update de la clase monobehaviour
    public void UpdateStates()
    {
        UpdateState();
        if(_currentSubState != null)
        {
            _currentSubState.UpdateStates();
        }
    }

//Este método no debería existir
//-----------------------------------
  /*  public void ExitStates()
    {
        ExitState();
        if(_currentSubState != null)
        {
            Debug.Log("Que mierda hace esto?");
            _currentSubState.ExitStates();
        }
    } */

    protected void SwitchState(PlayerBaseState newState){
        ExitState();
        newState.EnterState();
        if(_isRootState)
        {
            _ctx.CurrentState = newState;
        } else if(_currentSuperState != null)
        {
            _currentSuperState.SetSubState(newState);
        }
    }

    protected void SetSuperState(PlayerBaseState newSuperState)
    {
        _currentSuperState = newSuperState;
    }
    
    protected void SetSubState(PlayerBaseState newSubState)
    {
        _currentSubState = newSubState;
        newSubState.SetSuperState(this);
    }
}
