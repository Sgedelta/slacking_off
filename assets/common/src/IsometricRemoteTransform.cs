using Godot;
using System;

namespace SlackingOff;


[Tool]
public partial class IsometricRemoteTransform : Node2D
{
    // === Signals === (PascalCaseEventHandler())

    // === Enums === (PascalCase, members CONSTANT_CASE)

    // === Constants === (CONSTANT_CASE)

    // === Exported Vars === (PascalCase)
    [Export] public Node2D Target;
    [Export]
    public float AspectRatio
    {
        get { return _aspectRatio; }
        set
        {
            _aspectRatio = value;
            UpdateRemote();
        }
    }
    [Export] public bool WeAreCartesian = true;
    [Export] public bool UpdatePosition = true;
    [Export] public bool UseGlobalPosition = true;
    [Export] public bool UpdateRotation = false;
    [Export] public bool UpdateScale = false;




    // === Public Vars === (PascalCase)

    // === Private Vars === (_underscoredCamelCase)

    private float _aspectRatio = 2.0f;

    // === Godot Methods ===

    public IsometricRemoteTransform()
    {
        //runs when the object is created in memory, Parent -> Child
    }

    public override void _Ready()
    {
        //runs when the object and its children are ready, Child -> Parent
        SetNotifyTransform(true);

        if (IsInstanceValid(Target) && Target.IsNodeReady())
        {
            UpdateRemote();
        }
        else
        {
            Target.Ready += UpdateRemote;
        }
    }

    public override void _Notification(int what)
    {
        switch ((long)what)
        {
            case NotificationTransformChanged:
                UpdateRemote();
                break;


            default:
                break;
        }
    }

    // === Further Methods === (PascalCase, local variables camelCase)

    public void UpdateRemote()
    {

        if (!IsInstanceValid(Target))
        {
            GD.PushWarning("Target of Remote Invalid!");
            return;
        }

        Transform2D genTrans = Target.Transform;

        genTrans = ApplyConversion(genTrans);

        if (!genTrans.IsFinite())
        {
            return;
        }

        Target.Transform = genTrans;


    }

    public Transform2D ApplyConversion(Transform2D target)
    {
        return ConvertTransform(target, WeAreCartesian, UpdatePosition, UpdateRotation, UpdateScale);
    }

    public Transform2D RevertConversion(Transform2D target)
    {
        return ConvertTransform(target, !WeAreCartesian, UpdatePosition, UpdateRotation, UpdateScale);
    }


    public Transform2D ConvertTransform(Transform2D input, bool CartesianToIsometric, bool doPosition, bool doRotation, bool doScale)
    {
        if (CartesianToIsometric)
        {
            if (doPosition)
            {
                input.Origin = UseGlobalPosition ? CartIsoConverter.CartesianPosToIsometric(GlobalPosition, AspectRatio) :
                                                      CartIsoConverter.CartesianPosToIsometric(Position, AspectRatio);
            }

            if (doRotation)
            {
                float angle = input.Rotation - Mathf.DegToRad(45.0f);

                input.X = input.X.Rotated(GlobalRotation - angle);
                input.Y = input.Y.Rotated(GlobalRotation - angle);
            }

            if (doScale)
            {
                input.X = input.X * Transform.Scale.X / input.Scale.X * AspectRatio / Mathf.Sqrt2;
                input.Y = input.Y * Transform.Scale.Y / input.Scale.Y / Mathf.Sqrt2;
            }
        }
        else
        {
            if (doPosition)
            {
                input.Origin = UseGlobalPosition ? CartIsoConverter.IsometricPosToCartesian(GlobalPosition, AspectRatio) :
                                                      CartIsoConverter.IsometricPosToCartesian(Position, AspectRatio);
            }

            if (doRotation)
            {
                float angle = input.Rotation + Mathf.DegToRad(45.0f);

                input.X = input.X.Rotated(GlobalRotation - angle);
                input.Y = input.Y.Rotated(GlobalRotation - angle);
            }

            if (doScale)
            {
                input.X = input.X * Transform.Scale.X / input.Scale.X / AspectRatio * Mathf.Sqrt2;
                input.Y = input.Y * Transform.Scale.Y / input.Scale.Y * Mathf.Sqrt2;
            }
        }

        return input;
    }
}
