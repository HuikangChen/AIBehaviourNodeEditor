using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    [CreateAssetMenu(menuName ="Conditions/Is Dead")]
    public class IsDead : Condition
    {
        public override bool Check(StateManager state)
        {
            return state.health <= 0;
        }
    }
}
