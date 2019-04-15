using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.IO;

namespace StateMachine.BehaviourNodeEditor
{
    [CreateAssetMenu(menuName = "Editor/Nodes/State Node")]
    public class DrawStateNode : DrawNode
    { 
        public override void DrawWindow(Node b)
        {
            if (b.stateRef.currentState == null)

            {
                EditorGUILayout.LabelField("Add State to Modify:");
            }
            else
            {
                if (!b.collapse)
                {
                    
                }
                else
                {
                    b.windowRect.height = 100;
                }

                b.collapse = EditorGUILayout.Toggle("Collapse All", b.collapse);
            }

            b.stateRef.currentState = (State) EditorGUILayout.ObjectField(b.stateRef.currentState, typeof(State), false);

            if (b.previousCollapse != b.collapse)
            {
                b.previousCollapse = b.collapse;
            }

            if (b.stateRef.previousState != b.stateRef.currentState)
            {             
                b.isDuplicate = BehaviourNodeEditor.settings.currentGraph.IsStateDuplicate(b);
                b.stateRef.previousState = b.stateRef.currentState;
                if (!b.isDuplicate)
                {
                    Vector3 pos = new Vector3(b.windowRect.x, b.windowRect.y, 0);
                    pos.x += b.windowRect.width * 2;

                    SetupReorderableLists(b);

                    for (int i = 0; i < b.stateRef.currentState.transitions.Count; i++)
                    {
                        pos.y += i * 100;
                        BehaviourNodeEditor.AddTransitionNodeFromTransition(b.stateRef.currentState.transitions[i], b, pos);
                    }

                    BehaviourNodeEditor.forceSetDirty = true;
                }

            }

            if (b.isDuplicate)
            {
                EditorGUILayout.LabelField("State is a Duplicate!");
                b.windowRect.height = 100;
                return;
            }

            if (b.stateRef.currentState != null)
            {
                b.isAssigned = true;

                if (!b.collapse)
                {
                    if (b.stateRef.serializedState == null)
                    {
                        SetupReorderableLists(b);
                    }
                    float standard = 150;
                    b.stateRef.serializedState.Update();
                    b.showActions = EditorGUILayout.Toggle("Show Actions ", b.showActions);
                    if (b.showActions)
                    {
                        EditorGUILayout.LabelField("");
                        b.stateRef.onFixedUpdateList.DoLayoutList();
                        EditorGUILayout.LabelField("");
                        b.stateRef.onUpdateList.DoLayoutList();
                        standard += 100 + 40 + (b.stateRef.onUpdateList.count + b.stateRef.onFixedUpdateList.count) * 20;
                    }
                    b.showEnterExit = EditorGUILayout.Toggle("Show Enter/Exit ", b.showEnterExit);
                    if (b.showEnterExit)
                    {
                        EditorGUILayout.LabelField("");
                        b.stateRef.onEnterList.DoLayoutList();
                        EditorGUILayout.LabelField("");
                        b.stateRef.onExitList.DoLayoutList();
                        standard += 100 + 40 + (b.stateRef.onEnterList.count + b.stateRef.onExitList.count) * 20;
                    }

                    b.stateRef.serializedState.ApplyModifiedProperties();
                    b.windowRect.height = standard;
                }
            }
            else
            {
                b.isAssigned = false;
            }
        }

        void SetupReorderableLists(Node b)
        {
            b.stateRef.serializedState = new SerializedObject(b.stateRef.currentState);
            b.stateRef.onFixedUpdateList = new ReorderableList(b.stateRef.serializedState, b.stateRef.serializedState.FindProperty("onFixedUpdate"), true, true, true, true);
            b.stateRef.onUpdateList = new ReorderableList(b.stateRef.serializedState, b.stateRef.serializedState.FindProperty("onUpdate"), true, true, true, true);
            b.stateRef.onEnterList = new ReorderableList(b.stateRef.serializedState, b.stateRef.serializedState.FindProperty("onEnter"), true, true, true, true);
            b.stateRef.onExitList = new ReorderableList(b.stateRef.serializedState, b.stateRef.serializedState.FindProperty("onExit"), true, true, true, true);

            HandleReorderableList(b.stateRef.onFixedUpdateList, "On FixedUpdate");
            HandleReorderableList(b.stateRef.onUpdateList, "On Update");
            HandleReorderableList(b.stateRef.onEnterList, "On Enter");
            HandleReorderableList(b.stateRef.onExitList, "On Exit");
        }

        void HandleReorderableList(ReorderableList list, string targetName)
        {
            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, targetName);
            };

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocus) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            };
        }

        public override void DrawCurve(Node b)
        {

        }

        public Transition AddTransition(Node b)
        {
            return b.stateRef.currentState.AddTransition();
        }

    }
}
