using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    [CreateAssetMenu(menuName = "Actions/AddHealth")]
    public class ChangeHealth : Action
    {
        public override void Act(StateManager states)
        {
            states.health += 10;
        }
    }
}