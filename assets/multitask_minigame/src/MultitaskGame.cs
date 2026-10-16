using Godot;
using Godot.Collections;
using System;
using System.Linq;

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
	private const float SPAWN_PERIM = (VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y) * 2;

    /// <summary>
    /// If a randomly chosen point on the perimeter is within this number a corner, it will become the corner instead
    /// </summary>
    private const float CORNER_SNAP_RANGE_PX = 300;

	/// <summary>
	/// no viruses can spawn within this radius of each other. if viruses are within this radius of each other, will reattempt spawning
	/// Warning: the max virus count should account for this! for instance, if the spawn rect is 1920x1440, the perimiter is 6720. 
	/// with 16 viruses, this number can only be <= 420, because otherwise there will be a spot that the virus wants to spawn, but it cannot.
	/// corner snap does mess with this. suggested to subtract that before calculating! because with that in mind the max for 16 is 220.
	/// going over max won't break anything, but may impact performance with more expensive calculations..
	/// </summary>
	private const float MIN_DIST_BETWEEN_VIRUSES = 300;

	/// <summary>
	/// if the virus spawns within this distance of another virus, will try to move in the other direction to correct. Balances relative to other viruses
	/// </summary>
	private const float DESIRED_DIST_BETWEEN_VIRUSES = 750;

	// === Exported Vars === (PascalCase)

	[Export] public PackedScene PullbackScene;

	[Export] public GradientTexture1D PullbackFalloffGrad;

	[Export] public Array<Vector3> PullbackWeightAndFalloffsByPosIndex = new Array<Vector3>();


	/// <summary>
	/// A multiplier to the percieved time passed for spawn timer when there are NO viruses on screen
	/// </summary>
	[Export] public float TimeScaleWhenNoVirus { get; private set; }

	[Export] private Curve _spawnTimeByDifficulty;

    // === Public Vars === (PascalCase)

    public int PullbacksSpawned { get; private set; }

    /// <summary>
    /// how long the spawn timer will be set to each 
    /// </summary>
    public float SpawnTime
    {
        get
        {
            return _spawnTime;
        }
    }

    // === Private Vars === (_underscoredCamelCase)

    private RandomNumberGenerator _rng;
	private float _totalTimeElapsed;
	private float _timeSinceLastSpawn;

	Array<VirusPullback> _virusInstances;

	Array<float> _virusPositionAlongPerim;

	ShaderMaterial _mat;

	private float _spawnTime;

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
		_spawnTime = _spawnTimeByDifficulty.Sample(0);
        _timeSinceLastSpawn = _spawnTime / 2;

		_virusInstances = new Array<VirusPullback>();
		_virusPositionAlongPerim = new Array<float>();

        _mat = (ShaderMaterial)GetNode<MeshInstance2D>("WorkScreen").Material;
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
			_timeSinceLastSpawn = 0;
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

		(Vector2, Vector2, Vector3, GradientTexture1D) virus_data;

		try
		{
			virus_data = GetNewRandomVirusData();
		}
		catch (Exception ex)
		{
			GD.PushWarning("could not find valid virus spawn position");
			return;
		}

		pb.Init(PullbacksSpawned, virus_data); 
		PullbacksSpawned += 1;

		_virusInstances.Add(pb);
		pb.OnDraggedToZero += KillVirus;

		pb.OnWaitedMax += () => {
			EmitSignal(SignalName.OnWorkClosed, true);
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

		var chancePerRegion = GetSpawnChancePerRegion();

		while(chancePerRegion.Count > 0)
		{
			// get one region and remove it
			int region = (int)_rng.RandWeighted(chancePerRegion.ToArray());
			chancePerRegion.RemoveAt(region);

			// now, get a position
			float random_pos = GetRandomPositionInRegion(region);
			GD.Print("trying position: ", random_pos);

            // id the area and snap the position
            int pbState = random_pos switch
            {
                <= CORNER_SNAP_RANGE_PX or >= 2 * VIRUS_SPAWN_RECT_X + 2 * VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX => 0,
                > CORNER_SNAP_RANGE_PX and < VIRUS_SPAWN_RECT_X - CORNER_SNAP_RANGE_PX => 1,
                >= VIRUS_SPAWN_RECT_X - CORNER_SNAP_RANGE_PX and <= VIRUS_SPAWN_RECT_X + CORNER_SNAP_RANGE_PX => 2,
                > VIRUS_SPAWN_RECT_X + CORNER_SNAP_RANGE_PX and < VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX => 3,
                >= VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX and <= VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y + CORNER_SNAP_RANGE_PX => 4,
                > VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y + CORNER_SNAP_RANGE_PX and < 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX => 5,
                >= 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX and <= 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y + CORNER_SNAP_RANGE_PX => 6,
                > 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y + CORNER_SNAP_RANGE_PX => 7,
                _ => throw new Exception($"distance along perimiter {random_pos} was invalid!")
            };

            random_pos = pbState switch
            {
                0 => 0,
                2 => VIRUS_SPAWN_RECT_X,
                4 => VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y,
                6 => 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y,
                _ => random_pos
            };

			Vector2 nearest = ValidateSpawnPosition(random_pos).Item2;

            // try to center self
            Vector2 desiredFromLeftAndRight = new Vector2(
                Mathf.Max(0, DESIRED_DIST_BETWEEN_VIRUSES - nearest.X),
                Mathf.Max(0, DESIRED_DIST_BETWEEN_VIRUSES - nearest.Y)
                );

            float change = 0;

            if (desiredFromLeftAndRight.X == 0 || desiredFromLeftAndRight.Y == 0)
            {
                // one of these is 0, the other is how we need to change, or this does nothing
                change = desiredFromLeftAndRight.X - desiredFromLeftAndRight.Y;
            }
            else
            {
                // we need to balance the change -> add (with right as negative), div 2, change sign
                change = -(desiredFromLeftAndRight.X - desiredFromLeftAndRight.Y) / 2;
            }

            random_pos = (random_pos - change + SPAWN_PERIM) % SPAWN_PERIM;

            // id the area AGAIN because we changed our position
            pbState = random_pos switch
            {
                <= CORNER_SNAP_RANGE_PX or >= 2 * VIRUS_SPAWN_RECT_X + 2 * VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX => 0,
                > CORNER_SNAP_RANGE_PX and < VIRUS_SPAWN_RECT_X - CORNER_SNAP_RANGE_PX => 1,
                >= VIRUS_SPAWN_RECT_X - CORNER_SNAP_RANGE_PX and <= VIRUS_SPAWN_RECT_X + CORNER_SNAP_RANGE_PX => 2,
                > VIRUS_SPAWN_RECT_X + CORNER_SNAP_RANGE_PX and < VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX => 3,
                >= VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX and <= VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y + CORNER_SNAP_RANGE_PX => 4,
                > VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y + CORNER_SNAP_RANGE_PX and < 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX => 5,
                >= 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX and <= 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y + CORNER_SNAP_RANGE_PX => 6,
                > 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y + CORNER_SNAP_RANGE_PX => 7,
                _ => throw new Exception($"distance along perimiter {random_pos} was invalid!")
            };

            random_pos = pbState switch
            {
                0 => 0,
                2 => VIRUS_SPAWN_RECT_X,
                4 => VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y,
                6 => 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y,
                _ => random_pos
            };

            // validate again
            var validationData = ValidateSpawnPosition(random_pos);
			bool isValid = validationData.Item1;

			// if it's still not valid, just. try again
			if (!isValid)
			{
				GD.Print(random_pos, " was not valid");
				continue;
			}

            // we have a valid one! return.
            // keep track of it
            if (trackPermiter)
            {
                _virusPositionAlongPerim.Add(random_pos);

            }

            // put into v2
            ret.Item1 = new Vector2(
                Mathf.Clamp(random_pos, 0, VIRUS_SPAWN_RECT_X) - Mathf.Clamp(random_pos - VIRUS_SPAWN_RECT_X - VIRUS_SPAWN_RECT_Y, 0, VIRUS_SPAWN_RECT_X),
                Mathf.Clamp(random_pos - VIRUS_SPAWN_RECT_X, 0, VIRUS_SPAWN_RECT_Y) - Mathf.Clamp(random_pos - (2 * VIRUS_SPAWN_RECT_X) - VIRUS_SPAWN_RECT_Y, 0, VIRUS_SPAWN_RECT_Y)
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

		// if we left loop w/o returning, throw errors
		throw new Exception();


        // get linear random point along perimeter
        /*
		float perim = (VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y) * 2;
        Vector2 distToClosest = new Vector2(float.MaxValue, float.MaxValue);
		float distAlongPerim = 0;

        for(int i = 0; i <= _virusPositionAlongPerim.Count; i++) 
		{
			float minPerim = i >= 1 ? _virusPositionAlongPerim[i] : 0;
			float maxPerim = i < _virusPositionAlongPerim.Count ? _virusPositionAlongPerim[i] : perim;

			distAlongPerim = _rng.RandfRange(minPerim, maxPerim);

			// push away from others
			distToClosest = new Vector2(float.MaxValue, float.MaxValue);
			foreach (float other in _virusPositionAlongPerim)
			{
				float right = (perim - distAlongPerim + other) % perim;
				float left = perim - right;

				if (left < distToClosest.X)
				{
					distToClosest.X = left;
				}
				if (right < distToClosest.Y)
				{
					distToClosest.Y = right;
				}
			}

			Vector2 desiredFromLeftAndRight = new Vector2(
				Mathf.Max(0, DESIRED_DIST_BETWEEN_VIRUSES - distToClosest.X),
				Mathf.Max(0, DESIRED_DIST_BETWEEN_VIRUSES - distToClosest.Y)
				);

			float change = 0;

			if (desiredFromLeftAndRight.X == 0 || desiredFromLeftAndRight.Y == 0)
			{
				// one of these is 0, the other is how we need to change, or this does nothing
				change = desiredFromLeftAndRight.X - desiredFromLeftAndRight.Y;
			}
			else
			{
				// we need to balance the change -> add (with right as negative), div 2, change sign
				change = -(desiredFromLeftAndRight.X - desiredFromLeftAndRight.Y) / 2;
			}

			distAlongPerim = (distAlongPerim - change + perim) % perim;

			GD.Print(distAlongPerim, " after change ", change);

			//GD.Print("distance to closest: ", distToClosest);

            if (distToClosest.X > MIN_DIST_BETWEEN_VIRUSES && distToClosest.Y > MIN_DIST_BETWEEN_VIRUSES)
			{
				// we found one
				break;
			}

			// if we did not find one by the end, that's bad!
			if(i == _virusPositionAlongPerim.Count)
			{
				GD.PrintErr("Safe spot for virus spawn not found when spacing was ", MIN_DIST_BETWEEN_VIRUSES);
				throw new Exception("Safe spot for virus spawn not found when spacing was " + MIN_DIST_BETWEEN_VIRUSES);
			}
		}




        int pbState = distAlongPerim switch
        {
            <= CORNER_SNAP_RANGE_PX or >= 2 * VIRUS_SPAWN_RECT_X + 2 * VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX => 0,
            > CORNER_SNAP_RANGE_PX and < VIRUS_SPAWN_RECT_X - CORNER_SNAP_RANGE_PX => 1,
            >= VIRUS_SPAWN_RECT_X - CORNER_SNAP_RANGE_PX and <= VIRUS_SPAWN_RECT_X + CORNER_SNAP_RANGE_PX => 2,
            > VIRUS_SPAWN_RECT_X + CORNER_SNAP_RANGE_PX and < VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX => 3,
            >= VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX and <= VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y + CORNER_SNAP_RANGE_PX => 4,
            > VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y + CORNER_SNAP_RANGE_PX and < 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX => 5,
            >= 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y - CORNER_SNAP_RANGE_PX and <= 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y + CORNER_SNAP_RANGE_PX => 6,
            > 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y + CORNER_SNAP_RANGE_PX => 7,
            _ => throw new Exception($"distance along perimiter {distAlongPerim} was invalid!")
        };

        distAlongPerim = pbState switch
        {
            0 => 0,
            2 => VIRUS_SPAWN_RECT_X,
            4 => VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y,
            6 => 2 * VIRUS_SPAWN_RECT_X + VIRUS_SPAWN_RECT_Y,
            _ => distAlongPerim
        };

        //keep track of it
        if (trackPermiter)
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
		*/
	}


	private (bool, Vector2) ValidateSpawnPosition(float dist)
	{
        Vector2 distToClosest = new Vector2(float.MaxValue, float.MaxValue);
        foreach (float other in _virusPositionAlongPerim)
        {
            float right = (other - dist + SPAWN_PERIM) % SPAWN_PERIM;
            float left = SPAWN_PERIM - right;

            if (left < distToClosest.X)
            {
                distToClosest.X = left;
            }
            if (right < distToClosest.Y)
            {
                distToClosest.Y = right;
            }
        }

		// returns flase if EITHER are invalid.
		return (distToClosest.X > MIN_DIST_BETWEEN_VIRUSES && distToClosest.Y > MIN_DIST_BETWEEN_VIRUSES, distToClosest);
    }

	private Array<float> GetSpawnChancePerRegion(int regionOffset = 0)
	{
		// default, if 1 or fewer virus spawns there's only one region to spawn in. use real values because we are putting meaning in them
		if (_virusPositionAlongPerim.Count <= 1) return new Array<float> { SPAWN_PERIM - (_virusPositionAlongPerim.Count * MIN_DIST_BETWEEN_VIRUSES * 2) };

		// otherwise, time to do some math for each region 
		var ret = new Array<float>();

		for(int i = regionOffset; i < _virusPositionAlongPerim.Count + regionOffset; i++)
		{
		

			int fixedI = i % _virusPositionAlongPerim.Count;
			int fixedNext = (i + 1) % _virusPositionAlongPerim.Count;

			GD.Print(i, "=>", fixedI, " ", fixedNext);
			
			float from = _virusPositionAlongPerim[fixedI];
			float to = _virusPositionAlongPerim[fixedNext];

			float size = ((to - from + SPAWN_PERIM) % SPAWN_PERIM) - (MIN_DIST_BETWEEN_VIRUSES * 2);

			GD.Print(i, ": ", size);

            if (size <=0)
			{
				continue;
			}

			

            ret.Add(size);

		}
		return ret;
	}

	private (float, float) GetSpawnRegionBorders(int regionIndex)
	{
		if (_virusPositionAlongPerim.Count == 0) return (0, SPAWN_PERIM);

		if (regionIndex > _virusPositionAlongPerim.Count)
		{
			throw new ArgumentException($"region index {regionIndex} invalid");
		}

		float start = (_virusPositionAlongPerim[regionIndex] + MIN_DIST_BETWEEN_VIRUSES) % SPAWN_PERIM;
		float end = ((_virusPositionAlongPerim[(regionIndex + 1) % _virusPositionAlongPerim.Count]) - MIN_DIST_BETWEEN_VIRUSES + SPAWN_PERIM) % SPAWN_PERIM;

		return (start, end);
    }

	private float GetRandomPositionInRegion(int regionIndex)
	{
		var borders = GetSpawnRegionBorders(regionIndex);

		GD.Print(regionIndex, " borders: ", borders);

		// un-wrap a wrapped border (will wrap later)
		if (borders.Item2 < borders.Item1) borders.Item2 += SPAWN_PERIM;

        // get a random position
        float pos = _rng.RandfRange(borders.Item1, borders.Item2);

		// re-wrap
		pos = pos % SPAWN_PERIM;

		return pos;
    }

	public void SetSpawnTimeByNormalizedDifficulty(float difficulty)
	{
		_spawnTime = _spawnTimeByDifficulty.Sample(difficulty);
	}
}
