using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public class StateManager : MonoBehaviour
    {
        public float health;
        public State currentState;

        [HideInInspector]
        public float delta;
        [HideInInspector]
        public Transform mTransform;

        private void Awake()
        {
            mTransform = transform;
        }

        private void Update()
        {
            if (currentState != null)
            {
                currentState.UpdateState(this);
            }
        }
    }
}
