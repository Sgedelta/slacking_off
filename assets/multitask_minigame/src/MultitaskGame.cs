using Godot;
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
	private float _totalTimeElapsed;
	private float _spawnTime;
	private float _timeSinceLastSpawn;
	
	// === Godot Methods ===
	
	public MultitaskGame()
	{
		//runs when the object is created in memory, Parent -> Child
	}
	
	public override void _Ready()
	{
		//runs when the object and its children are ready, Child -> Parent
		_totalTimeElapsed = 0;
        _spawnTime = SpawnTimeByElapsedTimeCurve.Sample(0);
		_timeSinceLastSpawn = 0;
	}
	
	// Prefer using PhysicsProcess over Process
	public override void _PhysicsProcess(double delta)
	{

		
	}

    public override void _Process(double delta)
    {
		// update timer - can't do this w/ timer because we are scaling the time sometimes.
		// Specifically do this in Process because it has to do with time, which we want to be as accurate as possible, instead of movement
		_totalTimeElapsed += (float)delta;
        _timeSinceLastSpawn += delta * PullbacksSpawned > 0 ? TimeScaleWhenNoVirus : 1;
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
		VirusPullback pb = PullbackScene.Instantiate<VirusPullback>();

		pb.Init(PullbacksSpawned, Vector2.Zero, Vector2.Zero, Vector3.Zero, null); //TODO
		PullbacksSpawned += 1;

		AddChild(pb);

	}
}
