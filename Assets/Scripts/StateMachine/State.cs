using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    [CreateAssetMenu]
    public class State : ScriptableObject
    {
        public Action[] onFixedUpdate;
        public Action[] onUpdate;
        public Action[] onEnter;
        public Action[] onExit;

        public int idCount;
        public List<Transition> transitions = new List<Transition>();

        public void OnEnter(StateManager states)
        {
            DoActions(states, onEnter);
        }

        public void UpdateState(StateManager states)
        {
            DoActions(states, onUpdate);
            CheckTransitions(states);
        }

        public void FixedUpdateState(StateManager states)
        {
            DoActions(states, onFixedUpdate);
        }

        public void OnExit(StateManager states)
        {
            DoActions(states, onExit);
        }

        public void CheckTransitions(StateManager states)
        {
            for (int i = 0; i < transitions.Count; i++)
            {
                if (transitions[i].disable)
                    continue;

                if (transitions[i].condition.Check(states))
                {
                    if (transitions[i].targetState != null)
                    {
                        states.currentState = transitions[i].targetState;
                        OnExit(states);
                        states.currentState.OnEnter(states);
                    }
                    return;
                }
            }
        }

        public void DoActions(StateManager states, Action[] actions)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] != null)
                    actions[i].Act(states);
            }
        }

        public Transition AddTransition()
        {
            Transition retVal = new Transition();
            transitions.Add(retVal);
            retVal.id = idCount;
            idCount++;
            return retVal;
        }

        public Transition GetTransition(int id)
        {
            for (int i = 0; i < transitions.Count; i++)
            {
                if (transitions[i].id == id)
                    return transitions[i];
            }
            return null;
        }

        public void RemoveTransition(int id)
        {
            Transition t = GetTransition(id);
            if (t != null)
                transitions.Remove(t);
        }
    }
}
