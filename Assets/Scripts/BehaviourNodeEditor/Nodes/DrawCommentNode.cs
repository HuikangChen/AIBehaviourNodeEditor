using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine.BehaviourNodeEditor
{
    [CreateAssetMenu(menuName = "Editor/Nodes/Comment Node")]
    public class DrawCommentNode : DrawNode
    {   
        [SerializeField]
        string comment = "Add Comment";

        public override void DrawWindow(Node b)
        {
            comment = GUILayout.TextArea(comment, 200);
        }

        public override void DrawCurve(Node b)
        {
            throw new System.NotImplementedException();
        }
    }
}