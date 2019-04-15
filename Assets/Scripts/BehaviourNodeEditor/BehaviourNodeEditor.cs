using UnityEngine;
using UnityEditor;

/// <summary>
/// The main class that controls the editor window.
/// This script's Responsibilites:
/// 1.Handles all the user inputs
/// 2.Add State/Transition/Comment/Portal Nodes
/// 3.Draws all the Windows
/// 4.Panning the window
/// 
/// INSTRUCTIONS FOR ADDING NEW ACTIONS AND CONDITIONS SCRIPTABLE OBJECTS:
/// Inherit from the Action class to make new actions 
/// Inherit from the condition class to make new conditions
/// </summary>
/// 
namespace StateMachine.BehaviourNodeEditor
{
    public class BehaviourNodeEditor : EditorWindow
    {
        #region Variables

        //Settings for all the nodes and GUI styles
        public static EditorSettings settings;

        //The statemanager that's on a selected gameobject, the graph/editor will be visualizing this
        public static StateManager CurrentStateManager { get; private set; }

        //used for setting dirty so window won't lose info when unity closes
        public static bool forceSetDirty;

        //self reference
        private static BehaviourNodeEditor editor; 

        // used to check if its same as current state for repainting the window
        private static State previousState;

        //The mouse position at all times
        private Vector3 mousePosition;

        //Is any node window clicked on?
        private bool clickedOnWindow;

        //The current node you clicked on
        private Node selectedNode;

        //The ID of the node you are making a transition from
        private int transitFromId; 

        //Rect to store mouse position when making/drawing transitions
        private Rect mouseRect = new Rect(0, 0, 1, 1); 

        //Rect of the editor window on initialization
        private Rect all = new Rect(-5, -5, 10000, 10000);

        //style of the main window, get from settings
        private GUIStyle style;

        //style of an active node, get from settings
        private GUIStyle activeStyle;

        //Used for panning
        //The start position when you start dragging
        private Vector2 scrollStartPos;

        //used for panning
        //the distance/drag amount between the current mouse position and scrollStartPos
        private Vector2 drag;

        //using for panning
        //the total dragged distance in that direction
        private Vector2 totalDrag;
        
        //All the user action/input allowed to make
        //To add more, go to DoUserActions() to call it's function
        public enum UserActions { addState,
                                  addTransitionNode,
                                  deleteNode,
                                  addCommentNode,
                                  makeTransition,
                                  makePortal,
                                  resetPan }
        #endregion

        #region Initialization
        [MenuItem("Behaviour Editor/Editor")]
        //Initialize editor window title and size
        static void ShowEditor()
        {
            editor = GetWindow<BehaviourNodeEditor>(); 
            editor.titleContent = new GUIContent("Node Editor");
            editor.minSize = new Vector2(800, 800);
        }

        //loads the settings and styles, cashe the styles
        private void OnEnable()
        {
            settings = Resources.Load("EditorSettings") as EditorSettings;
            Debug.Log(settings);
            style = settings.skin.GetStyle("window");
            activeStyle = settings.activeSkin.GetStyle("window");
        }
        #endregion

        private void Update()
        {
            //updates/repaints the window if the statemanager has switched states
            if (CurrentStateManager != null)
            {
                if (previousState != CurrentStateManager.currentState)
                {
                    Repaint();
                    previousState = CurrentStateManager.currentState;
                }
            }
        }

        #region GUI Funtions

        /// <summary>
        /// The main update loop that draws all the windows and handles inputs during runtime
        /// </summary>
        private void OnGUI()
        {
            //checks if player/user clicked on an gameobject in scene
            if (Selection.activeTransform != null)
            {
                //sets the currentStateManager so we can displays it's state machine in the editor
                CurrentStateManager = Selection.activeTransform.GetComponentInChildren<StateManager>();
            }

            Event e = Event.current;

            //store mouse posiiton 
            mousePosition = e.mousePosition;

            UserInput(e);

            DrawWindows();

            //Check for middle mouse button for panning and update/repaint window 
            if (e.type == EventType.MouseDrag)
            {
                //make sure we have a graph
                if (settings.currentGraph != null)
                {
                    Repaint();
                }
            }

            UpdateWindowOnChange();

            //Starts the curve line and draw to mouse position
            DrawTransitionCurveOnInput();

            //We do this to make sure we have our graph saved when unity is closed
            SetDirtyToChangedObjects();
        }

        void DrawTransitionCurveOnInput()
        {
            if (settings.makeTransition)
            {
                //saves the mouse positions into the rect so we can pass it in DrawNodeCurve()
                mouseRect.x = mousePosition.x;
                mouseRect.y = mousePosition.y;

                //gets the rect of the node we are transitioning from
                Rect from = settings.currentGraph.GetNodeWithIndex(transitFromId).windowRect;

                //draws curve from the from node to the mouse position
                DrawNodeCurve(from, mouseRect, true, Color.blue);
                Repaint();
            }
        }

        void SetDirtyToChangedObjects()
        {
            if (forceSetDirty)
            {
                //set back to false when it has done with setting dirty so it doesn't constantly set
                forceSetDirty = false;

                //Sets dirty to the settings, currentGraph and all of currentGraphs windows
                EditorUtility.SetDirty(settings);
                EditorUtility.SetDirty(settings.currentGraph);

                for (int i = 0; i < settings.currentGraph.windows.Count; i++)
                {
                    Node n = settings.currentGraph.windows[i];
                    if (n.stateRef.currentState != null)
                        EditorUtility.SetDirty(n.stateRef.currentState);
                }
            }
        }

        void UpdateWindowOnChange()
        {
            if (GUI.changed)
            {
                settings.currentGraph.DeleteWindowsThatNeedTo();
                Repaint();
            }
        }

        /// <summary>
        /// Responsible for drawing all the windows in the graph
        /// Takes the list of all the windows in the currentgraph and creates a window based on it's info
        /// </summary>
        void DrawWindows()
        {
            GUILayout.BeginArea(all, style);
            BeginWindows();

            //Draw our background grid lines for decoration before all our windows
            //Small grid
            DrawGrid(20, .2f, Color.black);
            //Big grid with deeper color
            DrawGrid(100, .4f, Color.black);

            //Sets the object field on the top left of the window to get regerence to the graph
            //temp variable to set a color to pass into the  label gield
            GUIStyle s = new GUIStyle(EditorStyles.textField);
            s.normal.textColor = Color.black;
            //our assign graph label
            EditorGUILayout.LabelField("Assign Graph", GUILayout.Width(100));
            //set our current graph to the graph in the object field, will be null if user has not assgined it in the window
            settings.currentGraph = (NodeGraph)EditorGUILayout.ObjectField(settings.currentGraph, typeof(NodeGraph), false, GUILayout.Width(200));


            //Checks to see if we even have a graph assigned in the window before we do any window drawing
            if (settings.currentGraph != null)
            {
                //draws the transition curves for all the windows
                foreach (Node n in settings.currentGraph.windows)
                {
                    n.DrawCurve();
                }

                //draws all the windows in the currentGraph
                for (int i = 0; i < settings.currentGraph.windows.Count; i++)
                {
                    Node b = settings.currentGraph.windows[i];

                    if (b.drawNode is DrawStateNode)
                    {
                        //checks if user clicked on any gameobject with statemanager in the scene and if the 
                        if (CurrentStateManager != null && b.stateRef.currentState == CurrentStateManager.currentState)
                        {
                            //sets the selected gameobhject's current state's window style to an active style to indicate
                            b.windowRect = GUI.Window(i, b.windowRect,
                                DrawNodeWindow, b.windowTitle, activeStyle);
                        }
                        else
                        {
                            //sets the window to a normal style
                            b.windowRect = GUI.Window(i, b.windowRect,
                                DrawNodeWindow, b.windowTitle);
                        }
                    }
                    else
                    {
                        b.windowRect = GUI.Window(i, b.windowRect,
                            DrawNodeWindow, b.windowTitle);
                    }
                }
            }
            EndWindows();
            GUILayout.EndArea();
        }

        //Draw the node window given an id 
        void DrawNodeWindow(int id)
        {
            settings.currentGraph.windows[id].DrawWindow();
            GUI.DragWindow();
        }


        void UserInput(Event e)
        {
            //check if we have an assigned graph
            if (settings.currentGraph == null)
                return;

            //check for right click and if not making transition
            if (e.button == 1 && !settings.makeTransition)
            {
                if (e.type == EventType.MouseDown)
                {
                    RightClick(e);
                }
            }

            //if left clicked and you are currently making a transition
            if (e.button == 0 && settings.makeTransition)
            {
                if (e.type == EventType.MouseDown)
                {
                    MakeTransition();
                }
            }

            //for middle mouse click
            if (e.button == 2)
            {
                if (e.type == EventType.MouseDown)
                {
                    scrollStartPos = e.mousePosition;
                }
                else if (e.type == EventType.MouseDrag)
                {
                    HandlePanning(e);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void HandlePanning(Event e)
        {
            drag = e.mousePosition - scrollStartPos;
            drag *= .6f;
            scrollStartPos = e.mousePosition;
            totalDrag += drag;

            for (int i = 0; i < settings.currentGraph.windows.Count; i++)
            {
                Node b = settings.currentGraph.windows[i];

                    b.windowRect.x += drag.x;
                    b.windowRect.y += drag.y;             
            }
        }

        /// <summary>
        /// draws a gride based on the spacing between each line
        /// </summary>
        void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            //calculate width and heigh between the lines by dividing the windows width and height by the grid spacing
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            Vector3 newOffset = new Vector3(totalDrag.x % gridSpacing, totalDrag.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        //Resets the window view from wherever the user started panning
        void ResetScroll()
        {
            for (int i = 0; i < settings.currentGraph.windows.Count; i++)
            {
                Node b = settings.currentGraph.windows[i];
                b.windowRect.x -= totalDrag.x;
                b.windowRect.y -= totalDrag.y;
            }

            totalDrag = Vector2.zero;
        }


        void RightClick(Event e)
        {
            selectedNode = null;
            clickedOnWindow = false;

            //Check to see which window the mouse pointer is currently on when clicked
            for (int i = 0; i < settings.currentGraph.windows.Count; i++)
            {
                if (settings.currentGraph.windows[i].windowRect.Contains(e.mousePosition))
                {
                    clickedOnWindow = true;
                    selectedNode = settings.currentGraph.windows[i]; //makes the selected node the window that the mouse pointer is on
                    break;
                }
            }

            if (!clickedOnWindow)
            {
                ShowAddNodeMenu(e);
            }
            else
            {
                ModifyNode(e);
            }
        }

        /// <summary>
        /// Connect the curve line to a node we have clicked on to make a transition
        /// </summary>
        void MakeTransition()
        {
            settings.makeTransition = false;
            clickedOnWindow = false;
            for (int i = 0; i < settings.currentGraph.windows.Count; i++)
            {
                if (settings.currentGraph.windows[i].windowRect.Contains(mousePosition))
                {
                    clickedOnWindow = true;
                    selectedNode = settings.currentGraph.windows[i]; //makes the selected node the window that the mouse pointer is on
                    break;
                }
            }

            if (clickedOnWindow)
            {
                if (selectedNode.drawNode is DrawStateNode || selectedNode.drawNode is DrawPortalNode)
                {
                    if (selectedNode.id != transitFromId)
                    {
                        Node transNode = settings.currentGraph.GetNodeWithIndex(transitFromId);
                        transNode.targetNode = selectedNode.id;

                        Node enterNode = settings.currentGraph.GetNodeWithIndex(transNode.enterNode);
                        Transition transition = enterNode.stateRef.currentState.GetTransition(transNode.transRef.transitionId);
                        transition.targetState = selectedNode.stateRef.currentState;
                    }
                }

            }
        }
        #endregion

        #region Context Menus
        

        /// <summary>
        /// When user right clicks on empty space, we will show the menu for adding nodes
        /// </summary>
        void ShowAddNodeMenu(Event e)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddSeparator("");

            if (settings.currentGraph != null)
            {
                menu.AddItem(new GUIContent("Add State"), false, DoUserAction, UserActions.addState);
                menu.AddItem(new GUIContent("Add Portal"), false, DoUserAction, UserActions.makePortal);
                menu.AddItem(new GUIContent("Add Comment"), false, DoUserAction, UserActions.addCommentNode);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Reset View"), false, DoUserAction, UserActions.resetPan);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Add State"));
                menu.AddDisabledItem(new GUIContent("Add Comment"));
            }

            menu.ShowAsContext();
            e.Use();
        }

        /// <summary>
        /// when user right clicks on node, we show a menu to modify it
        /// </summary>
        void ModifyNode(Event e)
        {
            GenericMenu menu = new GenericMenu();

            if (selectedNode.drawNode is DrawStateNode)
            {
                if (selectedNode.stateRef.currentState != null)
                {
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Add Condition"), false, DoUserAction, UserActions.addTransitionNode);
                }
                else
                {
                    menu.AddSeparator("");
                    menu.AddDisabledItem(new GUIContent("Add Condition"));
                }

                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, DoUserAction, UserActions.deleteNode);
            }


            if (selectedNode.drawNode is DrawPortalNode)
            {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, DoUserAction, UserActions.deleteNode);
            }

            if (selectedNode.drawNode is NodeTransition)
            {
                if (selectedNode.isDuplicate || !selectedNode.isAssigned)
                {
                    menu.AddSeparator("");
                    menu.AddDisabledItem(new GUIContent("Make Transition"));
                }
                else
                {
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Make Transition"), false, DoUserAction, UserActions.makeTransition);
                }
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, DoUserAction, UserActions.deleteNode);
            }

            if (selectedNode.drawNode is DrawCommentNode)
            {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, DoUserAction, UserActions.deleteNode);
            }
            
            menu.ShowAsContext();
            e.Use();
        }


        //The user actions thats being executed
        void DoUserAction(object o)
        {
            UserActions a = (UserActions)o;

            switch (a)
            {
                case UserActions.addState:
                    editor.AddNodeOnGraph(settings.stateNode, 200, 100, "State", mousePosition);
                    break;

                case UserActions.makePortal:
                    editor.AddNodeOnGraph(settings.portalNode, 200, 80, "Portal", mousePosition);
                    break;

                case UserActions.addTransitionNode:
                    AddTransitionNode(selectedNode, mousePosition);
                    break;

                case UserActions.addCommentNode:
                    Node commentNode = editor.AddNodeOnGraph(settings.commentNode, 200, 100, "Comment", mousePosition);
                    commentNode.comment = "Add Comment Here";
                    break;

                case UserActions.deleteNode:
                    if (selectedNode.drawNode is NodeTransition)
                    {
                        Node enterNode = settings.currentGraph.GetNodeWithIndex(selectedNode.enterNode);
                        enterNode.stateRef.currentState.RemoveTransition(selectedNode.transRef.transitionId);
                    }
                    settings.currentGraph.DeleteNode(selectedNode.id);

                    if (settings.currentGraph != null)
                    {
                        settings.currentGraph.DeleteWindowsThatNeedTo();
                        Repaint();
                    }
                    break;

                case UserActions.makeTransition:
                    transitFromId = selectedNode.id;
                    settings.makeTransition = true;
                    break;

                case UserActions.resetPan:
                    ResetScroll();
                    break;

                default:
                    break;
            }
            forceSetDirty = true;
        }
        #endregion

        #region Add Node Methods
        
        /// <summary>
        /// Going from state node to a transition node
        /// </summary>
        public static Node AddTransitionNode(Node enterNode, Vector3 pos)
        {
            Node transNode = editor.AddNodeOnGraph(settings.transitionNode, 200, 100, "Condition", pos);
            transNode.enterNode = enterNode.id;
            Transition t = settings.stateNode.AddTransition(enterNode);
            transNode.transRef.transitionId = t.id;
            return transNode;
        }

        /// <summary>
        /// Going from transition node to a state node
        /// </summary>
        public static Node AddTransitionNodeFromTransition(Transition transition, Node enterNode, Vector3 pos)
        {
            Node transNode = editor.AddNodeOnGraph(settings.transitionNode, 200, 100, "Condition", pos);
            transNode.enterNode = enterNode.id;
            transNode.transRef.transitionId = transition.id;
            return transNode;

        }

        /// <summary>
        /// Draws the curve from a rect to another rect
        /// </summary>
        public static void DrawNodeCurve(Rect start, Rect end, bool left, Color curveColor)
        {
            Vector3 startPos = new Vector3((left) ? start.x + start.width : start.x, start.y + (start.height * .5f), 0);

            Vector3 endPos = new Vector3(end.x + (end.width * .5f), end.y + (end.height * .5f), 0);
            Vector3 startTan = startPos + Vector3.right * 50;
            Vector3 endTan = endPos + Vector3.left * 50;

            Color shadow = new Color(0, 0, 0, 0.06f);

            for (int i = 0; i < 3; i++)
            {
                Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);
            }

            Handles.DrawBezier(startPos, endPos, startTan, endTan, curveColor, null, 3);
        }

        //returns the node to add
        public Node AddNodeOnGraph(DrawNode type, float width, float height, string title, Vector3 pos)
        {
            Node baseNode = new Node();
            baseNode.drawNode = type;
            baseNode.windowRect.width = width;
            baseNode.windowRect.height = height;
            baseNode.windowTitle = title;
            baseNode.windowRect.x = pos.x;
            baseNode.windowRect.y = pos.y;
            settings.currentGraph.windows.Add(baseNode);
            baseNode.transRef = new TransitionNodeReferences();
            baseNode.stateRef = new StateNodeReferences();
            baseNode.id = settings.currentGraph.idCount;
            settings.currentGraph.idCount++;
            return baseNode;
        }

        #endregion
    }
}
