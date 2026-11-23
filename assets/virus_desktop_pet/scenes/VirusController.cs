using Godot;

namespace SlackingOff;

public partial class VirusController : Node2D
{
	// === Signals === (PascalCaseEventHandler())
	[Signal] public delegate void RequestGameEventHandler();

	// === Enums === (PascalCase, members CONSTANT_CASE)

	public enum VirusState
	{
		WANDER,
		SEEK_GAME,
		ASLEEP
	}


	// === Constants === (CONSTANT_CASE)
	const float GRABBED_DAMP = .75f;
	const float MAX_IMPULSE = 1000; // max scale of desired impulse vector. desired impulse is multiplied by a factor, so actual max force is that result
	public const float MAX_ENERGY = 100;
	public const float MAX_COMPOSURE = 100;
	public const float MAX_2D_SPEED = 750;

	public readonly Vector4 WANDER_BOUNDS = new Vector4(100, 100, 1820, 1340);
	public const float WANDER_ARRIVED_DIST = 75;

	// === Exported Vars === (PascalCase)
	[Export] float TimeToTryToStand = 2.5f;
	[Export] float StandForce = 80;
	[Export] float WanderForce = 500;
	/// <summary>
	/// the chance (per second) that the virus will request starting a new game. Divide 1 by this to estimate the amount of time it will take for the virus to request a game when it is able to.
	/// </summary>
	[Export] Curve GameStartChanceByEnergy;
	[Export] float MinWanderTimeBeforeOpenRequest = 1.0f; //Min time between games
	[Export] CollisionShape2D WanderRegion;
	[Export(PropertyHint.Flags, "GRAB_TARGET:4, DEBUG_RIGIDBODY:8")] int InteractionType = 4;

	// === Public Vars === (PascalCase)
	public RigidBody2D RB;
	public Area2D GrabTarget;
	public Vector2 Bounds = new Vector2(1920, 1440);
	public bool Grabbable = true;
	public bool MouseInGrabTarget = false;
	public bool MouseInRB = false;

	public bool MouseOver { get { return MouseInGrabTarget || MouseInRB; } }

	public float Energy
	{
		get { return _energy; }
		set {UpdateEnergy(value);}
	}
	public float Composure 
	{ 
		get { return _composure; } 
		set {UpdateComposure(value);}
	}

	public VirusState State
	{
		get;
		private set;
	}

	// === Private Vars === (_underscoredCamelCase)
	private Node2D _display;
	private RandomNumberGenerator _rng;
	private float _energy = MAX_ENERGY / 4.0f;
	private float _composure = MAX_COMPOSURE;

	private KinematicCollision2D _collision;
	private Control _statDisplay;
	private IsometricRemoteTransform _isoRemote;
	private bool _grabbed = false;
	private float _ungrabbedDamp;
	private float _ungrabbedGrav;
	private StaticBody2D _pinBody;
	private PinJoint2D _pinJoint;
	private Vector2 _pinBodyPos;
	private TextureProgressBar _energyDisplay;
	private TextureProgressBar _composureDisplay;
	private bool _clickedOnRB = false;
	private Vector2 _wanderLoc = Vector2.Zero;
	private float _wanderSpeed = 250.0f;
	
	// === Godot Methods ===
	
	public VirusController()
	{
		//runs when the object is created in memory, Parent -> Child
		_rng = new RandomNumberGenerator();
	}
	
	public override void _Ready()
	{
        //runs when the object and its children are ready, Child -> Parent
        RB = GetNode<RigidBody2D>("VirusPhysics");
		GrabTarget = GetNode<Area2D>("%GrabTarget");
		_isoRemote = GetNode<IsometricRemoteTransform>("%VisualsIsoConverter");
		_display = GetNode<Node2D>("%VirusDisplay");

        RB.MouseEntered += () =>
        {
            MouseInRB = (true && ((InteractionType & 8) != 0));
        };

        RB.MouseExited += () =>
        {
			MouseInRB = false;
        };

        GrabTarget.MouseEntered += () =>
		{
			MouseInGrabTarget = (true && ((InteractionType & 4) != 0));
        };

        GrabTarget.MouseExited += () =>
        {
            MouseInGrabTarget = false;
        };


        VisibilityChanged += OnVirusVisChange;

		_pinBody = GetNode<StaticBody2D>("%StaticPin");
		_pinBodyPos = _pinBody.Position;
		_pinJoint = _pinBody.GetNode<PinJoint2D>("%GrabPin");
		_ungrabbedDamp = RB.AngularDamp;

		_statDisplay = GetNode<Control>("%VirusStatDisplay");

        GD.Print(_wanderLoc);

        _energyDisplay = GetNode<TextureProgressBar>("%EnergyBar");
		_energyDisplay.Value = _energy;
		_composureDisplay = GetNode<TextureProgressBar>("%ComposureBar");
		_composureDisplay.Value = _composure;

	}
	
	// Prefer using PhysicsProcess over Process
	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if(_grabbed)
		{
			_pinBody.Position = _clickedOnRB ?
				RB.GetLocalMousePosition() :
				GrabTarget.GetLocalMousePosition().Rotated(Mathf.DegToRad(-45.0f) - _pinBody.GlobalRotation);
		}
		else if (!RB.Freeze)
		{
			
			switch (State)
			{
				case VirusState.WANDER:
					if(RB.GlobalPosition.DistanceSquaredTo(_wanderLoc) <= WANDER_ARRIVED_DIST * WANDER_ARRIVED_DIST)
					{
						_wanderLoc = GetGlobalWanderLoc();
                    }
					else
					{
						RB.ApplyCentralImpulse(GetImpulseToAchieveVel((_wanderLoc - RB.GlobalPosition).Normalized() * _wanderSpeed));

                    }


					break;
			}


		}

		IntegrateRBForces(PhysicsServer2D.BodyGetDirectState(RB.GetRid()));
	}

    public override void _Input(InputEvent @event)
    {
        //inspect_desktop_virus
        if (@event.IsAction("grab_desktop_virus"))
		{
			if(@event.IsPressed() && Grabbable && MouseOver)
			{
                _grabbed = true;
				_pinJoint.NodeA = _pinJoint.GetPathTo(RB);
                RB.AngularDamp = GRABBED_DAMP;
				_clickedOnRB = MouseInRB;
            }
			else if (@event.IsReleased())
			{
				Drop();
            }
			
		}
		if (@event.IsAction("inspect_desktop_virus"))
		{
			GD.Print(GetLocalMousePosition());
			if(@event is InputEventMouseButton mb && mb.IsPressed() && MouseOver)
			{
				_statDisplay.Visible = !_statDisplay.Visible;
			}
		}

    }

    // === Further Methods === (PascalCase, local variables camelCase)

    public void IntegrateRBForces(PhysicsDirectBodyState2D state)
    {

		state.LinearVelocity = state.LinearVelocity.LimitLength(MAX_2D_SPEED);

    }

	public void OnVirusVisChange()
	{
		if(Visible)
		{
			// reset related vars for internal behavior
			
		}
	}

	public void Drop()
	{
        _grabbed = false;
        _pinJoint.NodeA = null;
        _pinBody.Position = _pinBodyPos;
        RB.AngularDamp = _ungrabbedDamp;
    }

	private Vector2 GetImpulseToAchieveVel(Vector2 targetVelocity)
	{
		Vector2 dir = targetVelocity - RB.LinearVelocity;

		float mag = dir.Length(); 
		dir = dir.Normalized();

		mag *= RB.Mass;
		mag = Mathf.Clamp(mag, 0, MAX_IMPULSE);

		return dir * mag;
	}

	private void UpdateEnergy(float value)
	{
		value = Mathf.Clamp(value, 0, MAX_ENERGY);

		_energyDisplay.Value = value;

		_energy = value;
	}

	private void UpdateComposure(float value)
	{
        value = Mathf.Clamp(value, 0, MAX_COMPOSURE);

		_composureDisplay.Value = value;

        _composure = value;
	}

	private Vector2 GetGlobalWanderLoc()
	{
        var shape = WanderRegion.Shape as RectangleShape2D;

        var localPoint = new Vector2(
            _rng.RandfRange(-shape.Size.X / 2.0f, shape.Size.X / 2.0f),
            _rng.RandfRange(-shape.Size.Y / 2.0f, shape.Size.Y / 2.0f)
            );

        return WanderRegion.ToGlobal(localPoint);
    }

}
