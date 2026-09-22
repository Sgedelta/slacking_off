using Godot;
using System;
using Godot.Collections;
using System.Linq;

namespace SlackingOff;

public partial class TESTING_PeelbackBoneControl : Node
{
	// === Signals === (PascalCaseEventHandler())

	// === Enums === (PascalCase, members CONSTANT_CASE)

	// === Constants === (CONSTANT_CASE)

	// === Exported Vars === (PascalCase)
	[Export] private Skeleton2D Skele;
	[Export] private Polygon2D ControlPoly;
	
	// === Public Vars === (PascalCase)
	
	// === Private Vars === (_underscoredCamelCase)

	/// <summary>
	/// key points are places where the shape changes, such as corners
	/// </summary>
	private Array<Vector2> _keyPoints = new Array<Vector2>();
	
	// === Godot Methods ===
	
	public TESTING_PeelbackBoneControl()
	{
		//runs when the object is created in memory, Parent -> Child
	}
	
	public override void _Ready()
	{
		//runs when the object and its children are ready, Child -> Parent
		SubdivideFromPoints(new Array<Vector2>(ControlPoly.Polygon), 100);
		FixPolygonUVs();

    }
	
	// Prefer using PhysicsProcess over Process
	public override void _PhysicsProcess(double delta)
	{
		
	}

	// === Further Methods === (PascalCase, local variables camelCase)

	/// <summary>
	/// takes the point array provided and creates a polygon that hits all of those points, subdividing all lengths into sections of at most maxDistPerPoint evenly
	/// </summary>
	/// <param name="points"></param>
	/// <param name="maxDistPerPoint"></param>
	public void SubdivideFromPoints(Array<Vector2> points, float maxDistPerPoint, bool overwriteKey = true)
	{
		Array<Vector2> newPoints = new Array<Vector2>();
		if(overwriteKey)
		{
			_keyPoints.Clear();
		}

		for(int pointIndex = 0; pointIndex < points.Count; pointIndex++)
		{
			int nextPointIndex = (pointIndex + 1) % points.Count;
			Vector2 diffVec = points[nextPointIndex] - points[pointIndex];
			int subdivideCount = Mathf.CeilToInt(diffVec.Length() / maxDistPerPoint);

            for (float i = 0; i < subdivideCount; i++)
			{
				// calculate new point using a percent of diff vec
				Vector2 newPoint = points[pointIndex] + (diffVec / subdivideCount) * i;
				
				// add to array
				newPoints.Add(newPoint);
				if(overwriteKey && i == 0)
				{
					_keyPoints.Add(newPoint);
				}
			}
		}

		// set polygon as packed array of constructed
		ControlPoly.Polygon  = newPoints.ToArray();

	}

	/// <summary>
	/// uses the current Polygon array to construct the uvs. Assumes polygon starts from -,- and goes toward +,+. Uses max value (x&y independent) of array to scale.
	/// </summary>
	public void FixPolygonUVs()
	{
		Vector2[] points = ControlPoly.Polygon;

		//get min and max
		Vector2 mins  = Vector2.Inf;
        Vector2 maxes = -Vector2.Inf;

        foreach (Vector2 p in points)
		{
			if(p.X < mins.X)
			{
				mins.X = p.X;
			}
			if (p.X > maxes.X)
			{
				maxes.X = p.X;
			}

            if (p.Y < mins.Y)
            {
                mins.Y = p.Y;
            }
            if (p.Y > maxes.Y)
            {
                maxes.Y = p.Y;
            }
        }

		//get relevant tex scale and minMax difference
		Vector2 minMaxDiff = maxes - mins;
		Vector2 texSize = ControlPoly.Texture.GetSize();

		Array<Vector2> newUVs = new Array<Vector2>();

		//calculate new UVs
		for (int i = 0; i < points.Length; i++)
		{
			Vector2 uvLoc = new Vector2(
				Mathf.Lerp(0, texSize.X, (points[i].X - mins.X)/(minMaxDiff.X)),
                Mathf.Lerp(0, texSize.Y, (points[i].Y - mins.Y) / (minMaxDiff.Y))
                );
			newUVs.Add(uvLoc);
		}

		ControlPoly.UV = newUVs.ToArray();

	}
}
