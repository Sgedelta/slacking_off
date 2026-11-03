using Godot;
using Godot.Collections;
using System;

public partial class CollisionShapesToControlBounds : Node
{

	[Export] private Array<ControlToCollisionShapeRelater> _Relaters = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		foreach(var relater in _Relaters)
		{
			AssignRelater(relater);
		}
	}
	

	public void AssignRelater(ControlToCollisionShapeRelater relater)
	{
		Control control = GetNode<Control>(relater.ControlNode);
        CollisionShape2D collider = GetNode<CollisionShape2D>(relater.CollisionNode);

		if(!IsInstanceValid(control) || !IsInstanceValid(collider))
		{
			GD.PushWarning("not updating ", Name, "'s relater ", relater.ResourceName, " because could not find control or collider");
			return;
		}

        var rect = new RectangleShape2D();
        collider.Shape = rect;

        control.Resized += () => 
        {
            SizeColliderToControl(relater, control, collider);
        };

        SizeColliderToControl(relater, control, collider);
    }

	private void SizeColliderToControl(ControlToCollisionShapeRelater relater, Control control, CollisionShape2D collider)
	{
        var bounds = control.Size;
        var rectSize = bounds;

        if (relater.FixedSize.X != -1)
        {
            rectSize.X = relater.FixedSize.X;
        }
        if (relater.FixedSize.Y != -1)
        {
            rectSize.Y = relater.FixedSize.Y;
        }

        (collider.Shape as RectangleShape2D).Size = rectSize * relater.RelativeSize + relater.FixedSizeAddition;

        collider.Position = relater.FixedPositionOffset + bounds * relater.RelativePositionOffset;
    }


}


