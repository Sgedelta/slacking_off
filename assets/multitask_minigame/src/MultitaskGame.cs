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
	public const int MAX_VIRUS_COUNT = 1; // should be <= the shader's PULLBACK_INPUT_SIZE

	private static readonly Vector2 VIRUS_SPAWN_RECT = new Vector2(1920, 1440); //the border of the rectangle the virus can spawn on

	// === Exported Vars === (PascalCase)

	[Export] public PackedScene PullbackScene;

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

	ShaderMaterial _mat;

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
			float old = _spawnTime;
			_timeSinceLastSpawn = 0;
			_spawnTime = SpawnTimeByElapsedTimeCurve.Sample(_totalTimeElapsed);
			GD.Print(_totalTimeElapsed, " ", old, " ", _spawnTime);
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

		//pb.Init(PullbacksSpawned, new Vector2(960, 0), Vector2.Down, new Vector3(250, 0, 250), null); //TODO
		pb.Init(PullbacksSpawned, Vector2.Zero, Vector2.One.Normalized(), new Vector3(250, 0, 250), null); //TODO
		PullbacksSpawned += 1;

		_virusInstances.Add(pb);
		pb.OnDraggedToZero += KillVirus;

		AddChild(pb);

        EmitSignal(SignalName.OnSpawnPullback, pb);

    }

	private void KillVirus(int index)
	{
		var removed = _virusInstances[index];
		_virusInstances.RemoveAt(index);

		foreach (var pb in _virusInstances)
		{
			pb.UpdateIndexIfNeeded(index);
		}

		PullbacksSpawned -= 1;

        removed.QueueFree();

		EmitSignal(SignalName.OnPullbackClosed, index);
	}

	private (Vector2, Vector2, Vector3, GradientTexture1D) GetNewRandomVirusData()
	{
        (Vector2, Vector2, Vector3, GradientTexture1D) ret = (new Vector2(), new Vector2(), new Vector3(), null);

		// get linear random point along perimeter

		// snap a certain range from corners to corners?

		// grab direction vector

		// use constant for pullback and gradient? Injected from above for difficulty modifications?

		return ret;
	}
}
