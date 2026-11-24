using Godot;
using System;

namespace SlackingOff;

public static class CartIsoConverter
{

	public static Vector2 CartesianPosToIsometric(Vector2 pos, float projAspectRatio, float projScaleFactor = 1.0f)
	{
		return new Vector2(
			(pos.X - pos.Y) * projAspectRatio / 2, // aspectRatio/2 results in this simplifying in the standard 2:1 case
			(pos.X + pos.Y) / 2 // as aspect ratio is always W / H, height is constant 1. use scale factor to modify further
			) * projScaleFactor;
	}

    public static Vector2 IsometricPosToCartesian(Vector2 pos, float projAspectRatio, float projScaleFactor = 1.0f)
    {
		// note division, as factor is then the same in CartesianToIsometric
		var unscale = pos / projScaleFactor;
		return new Vector2(
			(unscale.X / projAspectRatio) + unscale.Y,
			unscale.Y - (unscale.X / projAspectRatio)
			);
    }

}
