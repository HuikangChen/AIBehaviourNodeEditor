using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    [CreateAssetMenu(menuName = "Conditions/FOV")]
    public class FOV : Condition
    {
        public override bool Check(StateManager state)
        {
            Debug.Log("fov");
            return false;
        }
    }
}
