using Godot;
using System;

[GlobalClass]
public partial class ControlToCollisionShapeRelater : Resource
{
    [Export] public NodePath ControlNode;
    [Export] public NodePath CollisionNode;
    [ExportGroup("FixedValues")]
    [Export] public Vector2 FixedSize = new Vector2(-1,-1);
    [Export] public Vector2 FixedPositionOffset = new Vector2(0, 0);
    [Export] public Vector2 FixedSizeAddition = new Vector2(0, 0);
    [ExportGroup("RelativeValues")]
    [Export] public Vector2 RelativeSize = new Vector2(1, 1);
    [Export] public Vector2 RelativePositionOffset = new Vector2(0, 0);


    public ControlToCollisionShapeRelater()
    {

    }
} 
