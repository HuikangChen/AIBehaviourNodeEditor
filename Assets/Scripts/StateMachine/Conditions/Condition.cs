using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public abstract class Condition : ScriptableObject
    {
        public string description;
        public abstract bool Check(StateManager state);
    }
}
