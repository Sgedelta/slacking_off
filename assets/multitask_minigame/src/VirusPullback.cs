using Godot;
using System;

namespace SlackingOff;

public partial class VirusPullback : Node2D
{
    // === Signals === (PascalCaseEventHandler())

    // === Enums === (PascalCase, members CONSTANT_CASE)

    // === Constants === (CONSTANT_CASE)

    // === Exported Vars === (PascalCase)
    [Export] public Curve DragScalarByProgress;

    // === Public Vars === (PascalCase)
    public int Index;
    public Vector2 Direction;
    public Vector3 WeightAndFalloffs;
    public GradientTexture1D FalloffTex;



    // === Private Vars === (_underscoredCamelCase)
    private bool _initialized;

    private float _progress;

    private Area2D _handle;

    private bool _dragable = false;
    private bool _beingDragged = false;

    private Vector2 _collectedInputDeltaSinceLastFrame;

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

        _handle = GetNode<Area2D>("HandleArea");
        _handle.MouseEntered += () => { _dragable = true; };
        _handle.MouseExited += () => { _dragable = false; };

        _collectedInputDeltaSinceLastFrame = new Vector2();

    }

    // Prefer using PhysicsProcess over Process
    public override void _PhysicsProcess(double delta)
    {

        // update the progress using collected input delta. check bool first to avoid doing unneeded work (even tho input is not collected)
        if(_beingDragged)
        {
            float distAlongDir = _collectedInputDeltaSinceLastFrame.Dot(Direction);

            // technically not *perfect*, but as we are doing this every physics frame (60/sec) approximates the integral of the curve cheaply
            _progress = Mathf.Clamp(_progress + distAlongDir * DragScalarByProgress.Sample(_progress) / WeightAndFalloffs.X, 0, 1);

            _collectedInputDeltaSinceLastFrame = new Vector2();
        }

        _handle.Position = Vector2.Zero.Lerp(new Vector2(0, WeightAndFalloffs.X), _progress);

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
