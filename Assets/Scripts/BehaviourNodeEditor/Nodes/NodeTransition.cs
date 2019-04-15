using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace StateMachine.BehaviourNodeEditor
{
    [CreateAssetMenu(menuName = "Editor/Nodes/Transition Node")]
    public class NodeTransition : DrawNode
    {
        public override void DrawWindow(Node b)
        {
            EditorGUILayout.LabelField("");
            Node enterNode = BehaviourNodeEditor.settings.currentGraph.GetNodeWithIndex(b.enterNode);
            if (enterNode == null)
            {
                return;
            }

            if (enterNode.stateRef.currentState == null)
            {
                BehaviourNodeEditor.settings.currentGraph.DeleteNode(b.id);
                return;
            }

            Transition transition = enterNode.stateRef.currentState.GetTransition(b.transRef.transitionId);

            if (transition == null)
                return;

            transition.condition = (Condition)EditorGUILayout.ObjectField(transition.condition, typeof(Condition), false);

            if (transition.condition == null)
            {
                EditorGUILayout.LabelField("No Condition!");
                b.isAssigned = false;
            }
            else
            {
                b.isAssigned = true;
                if (b.isDuplicate)
                {
                    EditorGUILayout.LabelField("Duplicate Condition");
                }
                else
                {
                    GUILayout.Label(transition.condition.description);

                    Node targetNode = BehaviourNodeEditor.settings.currentGraph.GetNodeWithIndex(b.targetNode);
                    if (targetNode != null)
                    {
                        if (!targetNode.isDuplicate)
                            transition.targetState = targetNode.stateRef.currentState;
                        else
                            transition.targetState = null;
                    }
                    else
                    {
                        transition.targetState = null;
                    }
                }
            }

            if (b.transRef.previousCondition != transition.condition)
            {
                b.transRef.previousCondition = transition.condition;
                b.isDuplicate = BehaviourNodeEditor.settings.currentGraph.isTransitionDuplicate(b);
                if (!b.isDuplicate)
                {
                    BehaviourNodeEditor.forceSetDirty = true;
                }
            }
        }

        public override void DrawCurve(Node b)
        {
            Rect rect = b.windowRect;
            rect.y += b.windowRect.height * .5f;
            rect.width = 1;
            rect.height = 1;

            Node e = BehaviourNodeEditor.settings.currentGraph.GetNodeWithIndex(b.enterNode);

            if (e == null)
            {
                BehaviourNodeEditor.settings.currentGraph.DeleteNode(b.id);
            }
            else
            {
                Color targetColor = Color.green;
                if (!b.isAssigned || b.isDuplicate)
                {
                    targetColor = Color.red;
                }

                Rect r = e.windowRect;
                BehaviourNodeEditor.DrawNodeCurve(r, rect, true, targetColor);
            }

            if (b.isDuplicate)
                return;

            if (b.targetNode > 0)
            {
                Node t = BehaviourNodeEditor.settings.currentGraph.GetNodeWithIndex(b.targetNode);

                if (t == null)
                {
                    b.targetNode = -1;
                }
                else
                {
                    rect = b.windowRect;
                    rect.x += rect.width;
                    Rect endRect = t.windowRect;
                    endRect.x -= endRect.width * .5f;

                    Color targetColor = Color.green;

                    if (t.drawNode is DrawStateNode)
                    {

                        if (!t.isAssigned || t.isDuplicate)
                        {
                            targetColor = Color.red;
                        }
                    }
                    else
                    {
                        if (!t.isAssigned)
                            targetColor = Color.red;
                        else
                            targetColor = Color.yellow;
                    }


                    BehaviourNodeEditor.DrawNodeCurve(rect, endRect, false, targetColor);
                }
                
            }
        }
    }
}