using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace StateMachine.BehaviourNodeEditor
{
    [System.Serializable]
    public class Node
    {
        public int id;
        public DrawNode drawNode;
        public Rect windowRect;
        public string windowTitle;
        public int enterNode;
        public int targetNode;
        public bool isDuplicate;
        public string comment;
        public bool isAssigned;
        public bool showDescription;
        public bool isOnCurrent;

        public bool collapse;
        public bool showActions = true;
        public bool showEnterExit = false;
        [HideInInspector]
        public bool previousCollapse;

        public StateNodeReferences stateRef;
        public TransitionNodeReferences transRef;

        public void DrawWindow()
        {
            if (drawNode != null)
            {
                drawNode.DrawWindow(this);
            }
        }

        public void DrawCurve()
        {
            if (drawNode != null)
            {
                drawNode.DrawCurve(this);
            }
        }
    }

    /// <summary>
    /// References to all it's connected states and actions
    /// </summary>
    [System.Serializable]
    public class StateNodeReferences
    {
        [HideInInspector]
        public State currentState;
        [HideInInspector]
        public State previousState;
        public SerializedObject serializedState;
        public ReorderableList onFixedUpdateList;
        public ReorderableList onUpdateList;
        public ReorderableList onEnterList;
        public ReorderableList onExitList;
    }

    /// <summary>
    /// References to all it's transitions
    /// </summary>
    [System.Serializable]
    public class TransitionNodeReferences
    {
        [HideInInspector]
        public Condition previousCondition;
        public int transitionId;
    }
}
