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

    // === Private Vars === (_underscoredCamelCase)
    private SubViewportContainer _subsceneContainer;
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
	}
	
	public override void _Ready()
	{
        //runs when the object and its children are ready, Child -> Parent
        _subsceneContainer = GetNode<SubViewportContainer>("SubsceneVPContainer");
        _subsceneContainer.GuiInput += FocusAreaGUIInput;

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
	
	// Prefer using PhysicsProcess over Process
	public override void _PhysicsProcess(double delta)
	{
		
	}

    public override void _Process(double delta)
    {
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

    /// <summary>
    /// Setup is used to inject data from above, such as the packed scene to load as the game scene
    /// </summary>
    public void Setup(PackedScene subscene)
    {


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
