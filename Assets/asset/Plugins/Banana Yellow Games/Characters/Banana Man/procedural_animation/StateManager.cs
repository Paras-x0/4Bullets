using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class StateManager<Estate> : MonoBehaviour where Estate : System.Enum
{
   protected Dictionary<Estate, BaseState<Estate>> States = new Dictionary<Estate, BaseState<Estate>>();
   protected BaseState<Estate> CurrentState;
    protected bool isTransitioningState = false;
   void Start()//called once and EnterState is also called one time for initialization
    {
        CurrentState.EnterState();
    }
   void Update()
    {
        Estate nextStateKey = CurrentState.GetNextState();
        if (!isTransitioningState && nextStateKey.Equals(CurrentState.StateKey))
        {
            CurrentState.UpdateState();
        }
        else if(!isTransitioningState)
        {
            TransitionToState(CurrentState.StateKey);
        }
    }

    public virtual void TransitionToState(Estate stateKey)
    {
        isTransitioningState = true;
        CurrentState.ExitState();
        CurrentState = States[stateKey];
        CurrentState.EnterState();
        isTransitioningState = false;
    }

   void OnTriggerEnter(Collider other)
    {
        CurrentState.OnTriggerEnter(other);
    }
   void OnTriggerStay(Collider other)
    {
        CurrentState.OnTriggerStay(other);
    }
   void OnTriggerExit(Collider other)
    {
        CurrentState.OnTriggerExit(other);
    }
}
