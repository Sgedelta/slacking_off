using Godot;
using System;

namespace SlackingOff;

public partial class AppWindow : AspectRatioContainer
{
    // === Signals === (PascalCaseEventHandler())

    // === Enums === (PascalCase, members CONSTANT_CASE)

    // === Constants === (CONSTANT_CASE)
    private readonly Vector2 COMPUTER_WINDOW_SIZE = new Vector2(1920, 1440);

    private const int FOCUS_Z_INDEX = 3;

    private const int UNFOCUS_Z_INDEX = 0;

    // === Exported Vars === (PascalCase)

    // === Public Vars === (PascalCase)
    public string AppName = "";

    public bool SetupComplete { get; private set; } = false;

    public int AppID { get; private set; }

    // === Private Vars === (_underscoredCamelCase)
    private bool _deferredInitComplete = false;
    private bool _setupAttempted = false;
    private SubViewportContainer _subsceneContainer;
    private Viewport _subsceneVP;
    private Control _focusArea;
	private Control _resizeHandle;
	private bool _isResizing = false;
	private Control _moveHandle;
	private bool _isMoving;
	private Vector2 _last_click_start;
	private Vector2 _last_click_size;
    private Vector2 _last_click_window_pos;
	
	// === Godot Methods ===
	
	public AppWindow()
	{
        //runs when the object is created in memory, Parent -> Child
        Callable.From(GetAndHookNodeReferences).CallDeferred();
        Callable.From(() => _deferredInitComplete = true).CallDeferred();
	}
	
	public override void _Ready()
	{
        //runs when the object and its children are ready, Child -> Parent
        

    }
	
	// Prefer using PhysicsProcess over Process
	public override void _PhysicsProcess(double delta)
	{
        if (!_deferredInitComplete) return; //skips first process frame to insure no missing refs
    }

    public override void _Process(double delta)
    {
        if (!_deferredInitComplete) return; //skips first process frame to insure no missing refs
        // drag resize prefer process because it must run every frame, not connected to physics
        if (_isResizing)
        {
			Vector2 newSize = _last_click_size + (GetGlobalMousePosition() - _last_click_start);

            newSize = newSize.Clamp(CustomMinimumSize, CustomMaximumSize - Position);

            Size = newSize;
        }
        else if (_isMoving)
        {
            Vector2 newPos = _last_click_window_pos + (GetGlobalMousePosition() - _last_click_start);

            newPos = newPos.Clamp(Vector2.Zero, COMPUTER_WINDOW_SIZE - Size);

            Position = newPos;
        }
    }

    // === Further Methods === (PascalCase, local variables camelCase)

    private void GetAndHookNodeReferences()
    {
        _subsceneContainer = GetNode<SubViewportContainer>("%SubsceneVPContainer");
        _subsceneContainer.GuiInput += FocusAreaGUIInput;

        _subsceneVP = GetNode<Viewport>("%SubsceneVP");

        _focusArea = GetNode<Control>("FocusGrabClickArea");
        _focusArea.GuiInput += FocusAreaGUIInput;
        FocusEntered += OnFocusEntered;
        FocusExited += OnFocusExited;

        _resizeHandle = GetNode<Control>("%ResizeHandle");
        _resizeHandle.MouseDefaultCursorShape = CursorShape.Fdiagsize;
        _resizeHandle.GuiInput += ResizeHandleGUIInput;
        _resizeHandle.GuiInput += FocusAreaGUIInput;

        _moveHandle = GetNode<Control>("%MoveHandle");
        _moveHandle.MouseDefaultCursorShape = CursorShape.Move;
        _moveHandle.GuiInput += MoveHandleGUIInput;
        _moveHandle.GuiInput += FocusAreaGUIInput;
    }

    /// <summary>
    /// Setup is used to inject data from above, such as the packed scene to load as the game scene
    /// </summary>
    public void Setup(int id, PackedScene subscene)
    {
        // safe setup tries to call deferred, and if that fails, does it next frame. won't reset, but this is only an issue frame 1 anyway
        if(!_deferredInitComplete)
        {
            var recall = () => { Setup(id, subscene); };
            if (_setupAttempted)
            {
                // call next frame instead of end of this deferred stack
                GetTree().Connect(SceneTree.SignalName.ProcessFrame, Callable.From(recall), (uint)ConnectFlags.OneShot);
            }
            else
            {
                // call at end of current deferred stack
                _setupAttempted = true;
                Callable.From(recall).CallDeferred();
            }
            // stop this one from doing anything
            return;
        }


        AppID = id;
        var SceneInstance = subscene.Instantiate();

        _subsceneVP.AddChild(SceneInstance);

        SetupComplete = true;
    }


    public void OnFocusEntered()
    {
        ZIndex = FOCUS_Z_INDEX;
        GD.Print("FOCUS");
    }

    public void OnFocusExited()
    {
        ZIndex = UNFOCUS_Z_INDEX;
    }

    private void FocusAreaGUIInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButtonEvent)
        {
            if (mouseButtonEvent.IsPressed())
            {
                GrabFocus();
            }
        }
    }

    private void ResizeHandleGUIInput(InputEvent @event)
	{
        if (@event is InputEventMouseButton mouseButtonEvent)
        {
            if (mouseButtonEvent.IsPressed())
            {
                _isResizing = true;
                _last_click_start = mouseButtonEvent.GlobalPosition;
				_last_click_size = Size;
            }
            else if (mouseButtonEvent.IsReleased())
            {
                _isResizing = false;
            }
        }
    }

    private void MoveHandleGUIInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButtonEvent)
        {
            if (mouseButtonEvent.IsPressed())
            {
                _isMoving = true;
                _moveHandle.MouseDefaultCursorShape = CursorShape.Drag;
                _last_click_start = mouseButtonEvent.GlobalPosition;
                _last_click_window_pos = Position;
            }
            else if (mouseButtonEvent.IsReleased())
            {
                _isMoving = false;
                _moveHandle.MouseDefaultCursorShape = CursorShape.Move;
            }
        }
    }

}
