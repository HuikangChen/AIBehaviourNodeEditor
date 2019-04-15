using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine.BehaviourNodeEditor;

namespace StateMachine
{
    [CreateAssetMenu(menuName = "Graph")]
    public class NodeGraph : ScriptableObject
    {
        public List<Node> windows = new List<Node>();
        public int idCount;
        List<int> indexToDelete = new List<int>();

        #region Checkers
        public Node GetNodeWithIndex(int index)
        {
            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].id == index)
                {
                    return windows[i];
                }
            }
            return null;
        }

        public void DeleteWindowsThatNeedTo()
        {
            for (int i = 0; i < indexToDelete.Count; i++)
            {
                Node b = GetNodeWithIndex(indexToDelete[i]);
                if (b != null)
                {
                    windows.Remove(b);
                }
            }

            indexToDelete.Clear();
        }

        public void DeleteNode(int index)
        {
            if(!indexToDelete.Contains(index))
            indexToDelete.Add(index);
        }

        public bool IsStateDuplicate(Node b)
        {
            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].id == b.id)
                    continue;

                if (windows[i].stateRef.currentState == b.stateRef.currentState && !windows[i].isDuplicate)
                {
                    return true;
                }
            }
            return false;
        }

        public bool isTransitionDuplicate(Node b)
        {
            Node enter = GetNodeWithIndex(b.enterNode);
            if (enter == null)
                return false;

            for (int i = 0; i < enter.stateRef.currentState.transitions.Count; i++)
            {
                Transition t = enter.stateRef.currentState.transitions[i];
                if (t.condition == b.transRef.previousCondition && b.transRef.transitionId != t.id)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
