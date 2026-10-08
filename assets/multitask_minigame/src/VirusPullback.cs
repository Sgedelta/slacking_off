using Godot;
using System;

namespace SlackingOff;

public partial class VirusPullback : Node2D
{
    // === Signals === (PascalCaseEventHandler())

    [Signal] public delegate void OnDraggedToZeroEventHandler(int index);

    [Signal] public delegate void OnReachedMaxEventHandler();

    [Signal] public delegate void OnWaitedMaxEventHandler();

    // === Enums === (PascalCase, members CONSTANT_CASE)

    // === Constants === (CONSTANT_CASE)

    // === Exported Vars === (PascalCase)
    [Export] public Curve DragScalarByProgress;

    /// <summary>
    /// How long the virus will take to get to the max progress if not interefered with (as long as ProgressAdvanceSpeedByProgress approximates to an integral of 1)
    /// </summary>
    [Export] public float GetToMaxProgTime;

    /// <summary>
    /// Amount of progress moved forward per second at each level of progress.
    /// </summary>
    [Export] public Curve ProgressAdvanceSpeedByProgress;

    /// <summary>
    /// How long the virus needs to be at it's max progress before pulling the screen down
    /// </summary>
    [Export] public float WaitAtMaxTime;

    // === Public Vars === (PascalCase)
    public int Index;
    public Vector2 Direction;
    public Vector3 WeightAndFalloffs;
    public GradientTexture1D FalloffTex;


    // === Private Vars === (_underscoredCamelCase)
    private bool _initialized;

    private float _progress;

    private float _waitProgress;

    private Area2D _handle;

    private bool _dragable = false;
    private bool _beingDragged = false;

    private Vector2 _collectedInputDeltaSinceLastFrame;

    private Sprite2D _virusVisual;

    private bool _reachedWaitEnd = false;

    // === Godot Methods ===

    public VirusPullback()
    {
        //runs when the object is created in memory, Parent -> Child
        _initialized = false;
    }

    public override void _Ready()
    {
        //runs when the object and its children are ready, Child -> Parent
        _progress = 0;
        _waitProgress = 0;

        _handle = GetNode<Area2D>("HandleArea");
        _handle.MouseEntered += () => { _dragable = true; };
        _handle.MouseExited += () => { _dragable = false; };

        _collectedInputDeltaSinceLastFrame = new Vector2();

        _virusVisual = GetNode<Sprite2D>("VirusVisual");

    }

    // Prefer using PhysicsProcess over Process
    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        // update progress for the virus pulling it down
        float virusSpeed = ProgressAdvanceSpeedByProgress.Sample(_progress) / GetToMaxProgTime;
        if(_beingDragged)
        {
            virusSpeed /= 2;
        }

        _progress = Mathf.Clamp(_progress + virusSpeed * dt, 0, 1);

        // update the progress using collected input delta. check bool first to avoid doing unneeded work (even tho input is not collected)
        if(_beingDragged)
        {
            float distAlongDir = Mathf.Min(_collectedInputDeltaSinceLastFrame.Dot(Direction), 0);

            // technically not *perfect*, but as we are doing this every physics frame (60/sec) approximates the integral of the curve cheaply
            _progress = Mathf.Clamp(_progress + distAlongDir * DragScalarByProgress.Sample(_progress) / WeightAndFalloffs.X, 0, 1);

            _collectedInputDeltaSinceLastFrame = new Vector2();
        }

        _handle.Position = Vector2.Zero.Lerp(new Vector2(0, WeightAndFalloffs.X), _progress);

        _virusVisual.Position = (Vector2.Down * WeightAndFalloffs.X).Lerp(Vector2.Down * WeightAndFalloffs.X / 2, _progress);
        
        // emit signals for progress values

        if (_progress == 0)
        {
            EmitSignal(SignalName.OnDraggedToZero, Index);
        }
        else if (_progress == 1 && _waitProgress == 0)
        {
            EmitSignal(SignalName.OnReachedMax);
        }

        // update wait progress if needed
        if(_progress != 1)
        {
            _waitProgress = 0; //reset
        }
        else
        {
            _waitProgress = Mathf.Clamp(_waitProgress + dt / WaitAtMaxTime, 0, 1);
        }

        // emit wait progress signal if needed
        if(_waitProgress == 1 && !_reachedWaitEnd)
        {
            _reachedWaitEnd = true;
            EmitSignal(SignalName.OnWaitedMax);
        }

    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("grab_virus_pullback"))
        {
            // will never set false if true, but will set true if able to be set true
            _beingDragged = _beingDragged || _dragable; 
        }
        else if (@event.IsActionReleased("grab_virus_pullback"))
        {
            _beingDragged = false;
        }

        if (_beingDragged)
        {
            if (@event is InputEventMouseMotion mouseMotion)
            {
                _collectedInputDeltaSinceLastFrame += mouseMotion.Relative;
            }
            else if (@event is InputEventScreenDrag drag)
            {
                //note: counts all drags, does not support multidrag..
                _collectedInputDeltaSinceLastFrame += drag.Relative;
            }
        }

    }

    // === Further Methods === (PascalCase, local variables camelCase)

    /// <summary>
    /// Sets initial values. Must be called before this begins to animate properly
    /// </summary>
    public void Init(int ind, Vector2 localPos, Vector2 pullDir, Vector3 weightAndFalloffs, GradientTexture1D falloffTex)
    {
        Index = ind;
        Position = localPos;
        Direction = pullDir.Normalized();
        WeightAndFalloffs = weightAndFalloffs;
        FalloffTex = falloffTex;

        Rotation = pullDir.AngleTo(Vector2.Right);

        //mark initialized
        _initialized = true;
    }

    public void UpdateIndexIfNeeded(int removedInd)
    {
        if(removedInd < Index)
        {
            Index -= 1;
        }
    }

    public void UpdateShaderVals(ref Vector4[] locDirs, ref Vector3[] weights, ref Texture2D[] texs)
    {
        if(!_initialized)
        {
            return;
        }

        locDirs[Index] = new Vector4(Position.X, Position.Y, Direction.X, Direction.Y);
        weights[Index] = WeightAndFalloffs * new Vector3(_progress, 1, 1);
        texs[Index] = FalloffTex;
    }
}
