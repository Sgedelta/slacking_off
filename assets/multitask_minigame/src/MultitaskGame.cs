using Godot;
using Godot.Collections;
using System;

namespace SlackingOff;

public partial class MultitaskGame : Node
{
	// === Signals === (PascalCaseEventHandler())

	/// <summary>
	/// Runs when a virus pullback is created, and passes the root of the created pullback scene
	/// </summary>
	/// <returns></returns>
	[Signal] public delegate void OnSpawnPullbackEventHandler(Node2D spawnedVirus);

	/// <summary>
	/// Runs when a virus pullback is closed by the player, and passes the index of the closed pullback
	/// </summary>
	/// <param name="pullbackIndex"></param>
	[Signal] public delegate void OnPullbackClosedEventHandler(int pullbackIndex);

	/// <summary>
	/// Runs when the virus successfully pulls the work tab down and closes it.
	/// </summary>
	/// <param name="byVirus"></param>
	[Signal] public delegate void OnWorkClosedEventHandler(bool byVirus);

	// === Enums === (PascalCase, members CONSTANT_CASE)

	// === Constants === (CONSTANT_CASE)
	public const int MAX_VIRUS_COUNT = 16; // should be <= the shader's PULLBACK_INPUT_SIZE

	// the sizes of the borders that the virus can spawn on
	private const float VIRUS_SPAWN_RECT_X = 1920;
	private const float VIRUS_SPAWN_RECT_Y = 1440;

	/// <summary>
	/// If a randomly chosen point on the perimeter is within this number a corner, it will become the corner instead
	/// </summary>
	private const float CORNER_SNAP_RANGE_PX = 400; 

	// === Exported Vars === (PascalCase)

	[Export] public PackedScene PullbackScene;

	[Export] public GradientTexture1D PullbackFalloffGrad;

	[Export] public Array<Vector3> PullbackWeightAndFalloffsByPosIndex = new Array<Vector3>();

	/// <summary>
	/// How long the spawn timer will be set to per 
	/// </summary>
	[Export] public Curve SpawnTimeByElapsedTimeCurve;
	/// <summary>
	/// A multiplier to the percieved time passed for spawn timer when there are NO viruses on screen
	/// </summary>
	[Export] public float TimeScaleWhenNoVirus { get; private set; }


	// === Public Vars === (PascalCase)

	public int PullbacksSpawned { get; private set; }

	// === Private Vars === (_underscoredCamelCase)

	private RandomNumberGenerator _rng;
	private float _totalTimeElapsed;
	private float _spawnTime;
	private float _timeSinceLastSpawn;

	Array<VirusPullback> _virusInstances;

	Array<float> _virusPositionAlongPerim;

	ShaderMaterial _mat;

	Node2D _mousePosIndicator;

	// === Godot Methods ===
	
	public MultitaskGame()
	{
		//runs when the object is created in memory, Parent -> Child
	}
	
	public override void _Ready()
	{
		_rng = new RandomNumberGenerator();

		//runs when the object and its children are ready, Child -> Parent
		_totalTimeElapsed = 0;
        _spawnTime = SpawnTimeByElapsedTimeCurve.Sample(0);
		_timeSinceLastSpawn = 0;

		_virusInstances = new Array<VirusPullback>();
		_virusPositionAlongPerim = new Array<float>();

        _mat = (ShaderMaterial)GetNode<MeshInstance2D>("WorkScreen").Material;

		_mousePosIndicator = GetNode<Node2D>("%DEBUG_MousePosIndicator");
	}
	
	// Prefer using PhysicsProcess over Process
	public override void _PhysicsProcess(double delta)
	{
		Vector4[] locDirs = new Vector4[MAX_VIRUS_COUNT];
		Vector3[] weightFalloffs = new Vector3[MAX_VIRUS_COUNT];
		Texture2D[] falloffTexs = new Texture2D[MAX_VIRUS_COUNT];

		foreach (VirusPullback v in _virusInstances)
		{
			v.UpdateShaderVals(ref locDirs, ref weightFalloffs, ref falloffTexs);
		}

		_mat.SetShaderParameter("pullback_count", PullbacksSpawned);
		_mat.SetShaderParameter("pullback_loc_dirs", locDirs);
		_mat.SetShaderParameter("pullback_weights", weightFalloffs);
		_mat.SetShaderParameter("pullback_texs", falloffTexs);

	}

    public override void _Process(double delta)
    {

        // update timer - can't do this w/ timer because we are scaling the time sometimes.
        // Specifically do this in Process because it has to do with time, which we want to be as accurate as possible, instead of movement
        _totalTimeElapsed += (float)delta;
        _timeSinceLastSpawn += (float)delta * (PullbacksSpawned > 0 ? TimeScaleWhenNoVirus : 1);
        if (_timeSinceLastSpawn >= _spawnTime)
        {
			SpawnVirus();
			float old = _spawnTime;
			_timeSinceLastSpawn = 0;
			_spawnTime = SpawnTimeByElapsedTimeCurve.Sample(_totalTimeElapsed);
			GD.Print(_totalTimeElapsed, " ", old, " ", _spawnTime);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventMouseMotion evMM)
		{
			_mousePosIndicator.Position = evMM.Position;
        }
    }

    public override void _Notification(int what)
    {
        switch((long)what)
		{
			case NotificationVpMouseExit:
				foreach(VirusPullback vp in _virusInstances)
				{
					//when mouse exits let go of all held viruses
					vp.TryGrabOrLetGo(false);
				}
				break;
		}
    }
	
	// === Further Methods === (PascalCase, local variables camelCase)
	private void SpawnVirus()
	{
		if(_virusInstances.Count >= MAX_VIRUS_COUNT)
		{
			return;
		}

		VirusPullback pb = PullbackScene.Instantiate<VirusPullback>();

		pb.Init(PullbacksSpawned, GetNewRandomVirusData()); 
		PullbacksSpawned += 1;

		_virusInstances.Add(pb);
		pb.OnDraggedToZero += KillVirus;

		//PLAYTEST:
		pb.OnWaitedMax += () => {
            GD.Print(GetNode("%EndScreenLoader").Call("load_scene")); 
		};

		AddChild(pb);

        EmitSignal(SignalName.OnSpawnPullback, pb);

    }

	private void KillVirus(int index)
	{
		var removed = _virusInstances[index];
		_virusInstances.RemoveAt(index);
		_virusPositionAlongPerim.RemoveAt(index);

        foreach (var pb in _virusInstances)
		{
			pb.UpdateIndexIfNeeded(index);
		}

		PullbacksSpawned -= 1;

        removed.QueueFree();

		EmitSignal(SignalName.OnPullbackClosed, index);
	}

	private (Vector2, Vector2, Vector3, GradientTexture1D) GetNewRandomVirusData(bool trackPermiter = true)
	{
        (Vector2, Vector2, Vector3, GradientTexture1D) ret = (Vector2.Zero, Vector2.Zero, Vector3.Zero, null);

		// get linear random point along perimeter
		float perim = (VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y) * 2;
		float distAlongPerim = _rng.RandfRange(0, perim);
		
		// push away from others
        Vector2 distToClosest = new Vector2(float.MaxValue, float.MaxValue);
		foreach (float other in _virusPositionAlongPerim)
		{
			float right = (perim - distAlongPerim + other) % perim;
			float left = perim - right;
			if(left < distToClosest.X)
			{
				distToClosest.X = left;
			}
			if (right < distToClosest.Y)
			{
				distToClosest.Y = right;
			}
		}

		Vector2 desiredFromLeftAndRight = new Vector2(
			Mathf.Max(0, CORNER_SNAP_RANGE_PX - distToClosest.X),
			Mathf.Max(0, CORNER_SNAP_RANGE_PX - distToClosest.Y)
			);

		float change = 0;

		if(desiredFromLeftAndRight.X == 0 || desiredFromLeftAndRight.Y == 0)
		{
            // one of these is 0, the other is how we need to change, or this does nothing
            change = desiredFromLeftAndRight.X - desiredFromLeftAndRight.Y; 
		}
		else
		{
            // we need to balance the change -> add (with right as negative), div 2, change sign
            change = -(desiredFromLeftAndRight.X - desiredFromLeftAndRight.Y) / 2;
		}

		distAlongPerim += change;


        int pbState = distAlongPerim switch
        {
            <= CORNER_SNAP_RANGE_PX or  >= 2 * VIRUS_SPAWN_RECT_X + 2 * VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX => 0,
			> CORNER_SNAP_RANGE_PX and < VIRUS_SPAWN_RECT_X - CORNER_SNAP_RANGE_PX => 1,
            >= VIRUS_SPAWN_RECT_X - CORNER_SNAP_RANGE_PX and <= VIRUS_SPAWN_RECT_X + CORNER_SNAP_RANGE_PX => 2,
			> VIRUS_SPAWN_RECT_X + CORNER_SNAP_RANGE_PX and < VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX => 3,
            >= VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX and <= VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y + CORNER_SNAP_RANGE_PX => 4,
			> VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y + CORNER_SNAP_RANGE_PX and < 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX => 5,
            >= 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX and <= 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y + CORNER_SNAP_RANGE_PX => 6,
			> 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y + CORNER_SNAP_RANGE_PX => 7,
            _ => throw new Exception($"distance along perimiter {distAlongPerim} was invalid!")
        };

        // snap a certain range from corners to corners?
        distAlongPerim = pbState switch
        {
            0 => 0,
            2 => VIRUS_SPAWN_RECT_X,
            4 => VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y,
            6 => 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y,
            _ => distAlongPerim
        };

		//keep track of it
		if(trackPermiter)
		{
			_virusPositionAlongPerim.Add(distAlongPerim);

        }

        // put into v2
        ret.Item1 = new Vector2(
			Mathf.Clamp(distAlongPerim, 0, VIRUS_SPAWN_RECT_X) - Mathf.Clamp(distAlongPerim - VIRUS_SPAWN_RECT_X - VIRUS_SPAWN_RECT_Y, 0, VIRUS_SPAWN_RECT_X),
            Mathf.Clamp(distAlongPerim - VIRUS_SPAWN_RECT_X, 0, VIRUS_SPAWN_RECT_Y) - Mathf.Clamp(distAlongPerim - (2 * VIRUS_SPAWN_RECT_X) - VIRUS_SPAWN_RECT_Y, 0, VIRUS_SPAWN_RECT_Y)
        );

		// grab direction vector

		ret.Item2 = pbState switch
		{
			0 => new Vector2(1, 1).Normalized(),
			1 => new Vector2(0, 1),
			2 => new Vector2(-1, 1).Normalized(),
			3 => new Vector2(-1, 0),
			4 => new Vector2(-1, -1).Normalized(),
			5 => new Vector2(0, -1),
			6 => new Vector2(1, -1).Normalized(),
			7 => new Vector2(1, 0),
            _ => throw new Exception($"internal state {pbState} not recognized!")
        };

		ret.Item3 = PullbackWeightAndFalloffsByPosIndex[pbState];

        // use constant for pullback and gradient? Injected from above for difficulty modifications?

        ret.Item4 = PullbackFalloffGrad;

        return ret;
	}
}
