using UnityEngine;

/// <summary>
/// Contains the settings of the nodes and GUIskin/styles
/// The nodes are scriptable objects, inject them in the inspector
/// </summary>

namespace StateMachine.BehaviourNodeEditor
{
    [CreateAssetMenu(menuName = "Editor/Settings")]
    public class EditorSettings : ScriptableObject
    {
        public NodeGraph currentGraph;
        public DrawStateNode stateNode;
        public DrawPortalNode portalNode;
        public NodeTransition transitionNode;
        public DrawCommentNode commentNode;
        public bool makeTransition;
        public GUISkin skin;
        public GUISkin activeSkin;
    }
}