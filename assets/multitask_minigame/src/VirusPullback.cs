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
    /// How long the virus will take to get to the max progress if not interefered with 
    /// </summary>
    [Export] public float GetToMaxProgTime;

    /// <summary>
    /// Visual progress by linear progress when advancing
    /// </summary>
    [Export] public Curve VisualProgressByLinearProgressForward;
    /// <summary>
    /// Visual progress by linear progress when being hit back
    /// </summary>
    [Export] public Curve VisualProgressByLinearProgressReverse;
    [Export] private float _visualProgressCorrectionTime = .5f;

    /// <summary>
    /// How long the virus needs to be at it's max progress before pulling the screen down
    /// </summary>
    [Export] public float WaitAtMaxTime;

    [Export] public Vector2 HandleOffset = new Vector2();

    [Export] public float VirusAccel = 1;
    [Export] public float ClickLinearProgressVelocityEffect = .33f;

    // === Public Vars === (PascalCase)
    public int Index;
    public Vector2 Direction;
    public Vector3 WeightAndFalloffs;
    public GradientTexture1D FalloffTex;


    // === Private Vars === (_underscoredCamelCase)
    private bool _initialized;

    private float _visualProgress;
    private float _linearProgress;
    private float _progressVel;
    private float _lastInflectionVP = 0;

    private float _waitProgress;

    private Area2D _handle;

    private bool _mouseInside = false;
    private bool _beingDragged = false;

    private Vector2 _collectedInputDeltaSinceLastFrame;

    private Sprite2D _virusVisual;

    private bool _reachedWaitEnd = false;

    private int _accumulatedClicks = 0;
    private float _timeSinceLastInflection = 0;


    // === Godot Methods ===

    public VirusPullback()
    {
        //runs when the object is created in memory, Parent -> Child
        _initialized = false;
    }

    public override void _Ready()
    {
        //runs when the object and its children are ready, Child -> Parent
        _visualProgress = 0;
        _waitProgress = 0;

        _handle = GetNode<Area2D>("VirusVisual/ClickArea");
        _handle.MouseEntered += () => { _mouseInside = true; };
        _handle.MouseExited += () => { _mouseInside = false; };

        _collectedInputDeltaSinceLastFrame = new Vector2();

        _virusVisual = GetNode<Sprite2D>("VirusVisual");
        _progressVel = (1 / GetToMaxProgTime); // set to it's max at the start - will be clamped in 
        _timeSinceLastInflection = _visualProgressCorrectionTime;
    }

    // Prefer using PhysicsProcess over Process
    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;


        // update progress velocity
        float prevProgressVel = _progressVel;
        _progressVel = Mathf.Clamp(_progressVel + (VirusAccel * dt) - (ClickLinearProgressVelocityEffect * _accumulatedClicks), float.MinValue, (1 / GetToMaxProgTime));
        _accumulatedClicks = 0;

        // did we inflect? if we did, we need to store where we were for lerp
        if (Mathf.Sign(prevProgressVel) == -Mathf.Sign(_progressVel))
        {
            _lastInflectionVP = _visualProgress;
            _timeSinceLastInflection = 0;
        }
        else
        {
            _timeSinceLastInflection += dt;

        }

        float _prevLinearProgress = _linearProgress;
        _linearProgress = Mathf.Clamp(_linearProgress + _progressVel * dt, 0, 1);

        // where we want to be, if we are forward or backward.
        // this is equivalent to sampling the curve in most cases, but rarely it isn't. 
        // if we are moving forward, but the reverse position is ABOVE our current position, we need 
        float forwardVisual = VisualProgressByLinearProgressForward.Sample(_linearProgress);
        float backwardVisual = VisualProgressByLinearProgressReverse.Sample(_linearProgress);
        float desiredVisual = _progressVel > 0 ?
             forwardVisual : backwardVisual;
        float prevDesiredVisual = _progressVel > 0 ? 
            VisualProgressByLinearProgressForward.Sample(_prevLinearProgress) : 
            VisualProgressByLinearProgressReverse.Sample(_prevLinearProgress);
        int desiredDir = Mathf.Sign(desiredVisual - prevDesiredVisual);

        // solve underestimates 
        /*if(Mathf.Sign(_visualProgress - prevDesiredVisual) == -Mathf.Sign(_progressVel))
        {
            _visualProgress = 
        }*/


        if( desiredDir > 0)
        {
            _visualProgress = Mathf.Max(
                (float)Mathf.Lerp(_lastInflectionVP, desiredVisual, Mathf.Min(_timeSinceLastInflection / _visualProgressCorrectionTime, 1.0)),
                _visualProgress);
        }
        else if ( desiredDir < 0 )
        {
            _visualProgress = Mathf.Min(
                (float)Mathf.Lerp(_lastInflectionVP, desiredVisual, Mathf.Min(_timeSinceLastInflection / _visualProgressCorrectionTime, 1.0)),
                _visualProgress);
        }


        /*
        // update the progress using collected input delta. check bool first to avoid doing unneeded work (even tho input is not collected)
        if(_beingDragged)
        {
            float distAlongDir = Mathf.Min(_collectedInputDeltaSinceLastFrame.Dot(Direction), 0);

            // technically not *perfect*, but as we are doing this every physics frame (60/sec) approximates the integral of the curve cheaply
            _progress = Mathf.Clamp(_progress + distAlongDir * DragScalarByProgress.Sample(_progress) / WeightAndFalloffs.X, 0, 1);

            _collectedInputDeltaSinceLastFrame = new Vector2();
        } */

        _handle.Position = Vector2.Zero.Lerp(new Vector2(0, WeightAndFalloffs.X), _visualProgress) + HandleOffset;

        _virusVisual.Position = (Vector2.Down * WeightAndFalloffs.X).Lerp(Vector2.Down * WeightAndFalloffs.X / 2, _visualProgress);
        
        // emit signals for progress values

        if (_linearProgress == 0)
        {
            EmitSignal(SignalName.OnDraggedToZero, Index);
        }
        else if (_linearProgress == 1 && _waitProgress == 0)
        {
            EmitSignal(SignalName.OnReachedMax);
        }

        // update wait progress if needed
        if(_linearProgress != 1)
        {
            _waitProgress = 0; //reset
            Modulate = Color.Color8(255, 255, 255, 255);
        }
        else
        {
            _waitProgress = Mathf.Clamp(_waitProgress + dt / WaitAtMaxTime, 0, 1);
            byte colorVal = (byte)(255 - Mathf.Clamp(Mathf.FloorToInt(Mathf.Lerp(0, 325, _waitProgress * _waitProgress)), 0, 255));
            Modulate = Color.Color8(255, colorVal, colorVal, 255);
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
            TryGrabOrLetGo(true);
        }
        else if (@event.IsActionReleased("grab_virus_pullback"))
        {
            TryGrabOrLetGo(false);
            if(_mouseInside)
            {
                _accumulatedClicks += 1;
            }
            
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

        Rotation = -pullDir.AngleTo(Vector2.Down);

        //mark initialized
        _initialized = true;
    }

    public void Init(int index, (Vector2, Vector2, Vector3, GradientTexture1D) data) => Init(index, data.Item1, data.Item2, data.Item3, data.Item4);

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
        weights[Index] = WeightAndFalloffs * new Vector3(_visualProgress, 1, 1);
        texs[Index] = FalloffTex;
    }

    public void TryGrabOrLetGo(bool grab)
    {
        if(grab)
        {
            // will never set false if true, but will set true if able to be set true
            _beingDragged = _beingDragged || _mouseInside;
        }
        else
        {
            _beingDragged = false;
        }

    }
}
