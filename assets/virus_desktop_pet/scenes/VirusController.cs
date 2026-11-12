using Godot;

namespace SlackingOff;

public partial class VirusController : RigidBody2D
{
	// === Signals === (PascalCaseEventHandler())
	[Signal] public delegate void RequestGameEventHandler();

	// === Enums === (PascalCase, members CONSTANT_CASE)

	// === Constants === (CONSTANT_CASE)
	const float GRABBED_DAMP = .75f;
	const float GRABBED_GRAV = 2f;
	readonly Vector2 UP_VEC = Vector2.Up; //used to check if we are on the floor
	const float MAX_FLOOR_ANGLE_DEGREES = 30f;
	const float COYOTE_TIME = .2f;
	const float STANDING_ACCEPTABLE_TILT_RADS = .33f; // ~10 degrees
	const float MAX_IMPULSE = 1000; // max scale of desired impulse vector. desired impulse is multiplied by a factor, so actual max force is that result
	const float VIRUS_CLICK_DIST = 250;
	public const float MAX_ENERGY = 100;
	public const float MAX_COMPOSURE = 100;

	// === Exported Vars === (PascalCase)
	[Export] float TimeToTryToStand = 2.5f;
	[Export] float StandForce = 80;
	[Export] float WanderForce = 500;
	/// <summary>
	/// the chance (per second) that the virus will request starting a new game. Divide 1 by this to estimate the amount of time it will take for the virus to request a game when it is able to.
	/// </summary>
	[Export] Curve GameStartChanceByEnergy;
	[Export] float MinWanderTimeBeforeOpenRequest = 1.0f; //Min time between games

	// === Public Vars === (PascalCase)
	public Vector2 Bounds = new Vector2(1920, 1440);
	public bool Grabbable = true;
	public bool Grounded { get { return _timeSinceGround < COYOTE_TIME; } }
	public float GroundedTime { get; private set; }
	public float StandingTime { get; private set; }

	public float Energy
	{
		get { return _energy; }
		set {UpdateEnergy(value);}
	}
	public float Composure 
	{ 
		get { return _energy; } 
		set {UpdateComposure(value);}
	}



	// === Private Vars === (_underscoredCamelCase)
	private RandomNumberGenerator _rng;
	private float _energy = MAX_ENERGY / 4.0f;
	private float _composure = MAX_COMPOSURE;

	private KinematicCollision2D _collision;
	private Control _statDisplay;
    // NOT grounded! used with _timeSinceGround and COYOTE_TIME to determine if we are Grounded!
    private bool _currentlyOnGround;
	private bool _hitWall;
	private float _timeSinceGround = COYOTE_TIME; //start ungrounded
	private bool _grabbed = false;
	private float _ungrabbedDamp;
	private float _ungrabbedGrav;
	private StaticBody2D _pinBody;
	private PinJoint2D _pinJoint;
	private Vector2 _pinBodyPos;
	private Vector2 _wanderDir;
	private TextureProgressBar _energyDisplay;
	private TextureProgressBar _composureDisplay;

	// Private Fields
	private float _radsToVertical { get { return Vector2.FromAngle(Rotation).AngleTo(UP_VEC.Rotated(Mathf.Pi / 2)); } }
	private bool _isStanding { get { return Grounded && Mathf.Abs(_radsToVertical) < STANDING_ACCEPTABLE_TILT_RADS; } }
	
	// === Godot Methods ===
	
	public VirusController()
	{
		//runs when the object is created in memory, Parent -> Child
		_rng = new RandomNumberGenerator();
	}
	
	public override void _Ready()
	{
		//runs when the object and its children are ready, Child -> Parent
		VisibilityChanged += OnVirusVisChange;

		_pinBody = GetNode<StaticBody2D>("StaticPin");
		_pinBodyPos = _pinBody.Position;
		_pinJoint = _pinBody.GetNode<PinJoint2D>("GrabPin");
		_ungrabbedDamp = AngularDamp;
		_ungrabbedGrav = GravityScale;

		_statDisplay = GetNode<Control>("%VirusStatDisplay");

		_wanderDir = new Vector2(1, 0);

		_energyDisplay = GetNode<TextureProgressBar>("%EnergyBar");
		_energyDisplay.Value = _energy;
		_composureDisplay = GetNode<TextureProgressBar>("%ComposureBar");
		_composureDisplay.Value = _composure;

	}
	
	// Prefer using PhysicsProcess over Process
	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if(_currentlyOnGround)
		{
			_timeSinceGround = 0;
			GroundedTime += dt;
        }
		else
		{
			// start counting for Coyote Time
            _timeSinceGround += dt;
        }

		if(!Grounded)
		{
			GroundedTime = 0;
			StandingTime = 0;
		}
		else if (_isStanding)
		{
			StandingTime += dt;
		}

		if(_hitWall && Grounded)
		{
			_wanderDir *= -1;
		}

		if(_grabbed)
		{
			_pinBody.Position = GetLocalMousePosition();
		}
		else if (!Freeze)
		{
			if(GroundedTime >= TimeToTryToStand)
			{
				// get rot change
				float rotChange = _radsToVertical;

                // rescale so we can use powers to apply more force when sideways
                float force = Mathf.Lerp(4, 6, Mathf.Abs(rotChange) / Mathf.Pi);

				// apply torque to stand
				ApplyTorque(Mathf.Pow(force, 5) * StandForce * Mathf.Sign(rotChange));
			}

			if(_isStanding)
			{
				//attempt simple wander
				Vector2 wanderForce = GetImpulseToAchieveVel(_wanderDir * WanderForce);
				//GD.Print("wanderForce: ", wanderForce);

                ApplyCentralForce(wanderForce);
			}

			if(StandingTime >= MinWanderTimeBeforeOpenRequest)
			{
				// get chance and scale it by dt to make time independent (per second)
				float chance = GameStartChanceByEnergy.Sample(Energy / MAX_ENERGY) * dt;
				GD.Print(chance);

				// roll and request if we need
				if(_rng.Randf() <= chance)
				{
					EmitSignal(SignalName.RequestGame);
				}

            }
		}
	}

    public override void _Input(InputEvent @event)
    {
        //inspect_desktop_virus
        if (@event.IsAction("grab_desktop_virus"))
		{
			if(@event.IsPressed() && Grabbable && GetLocalMousePosition().LengthSquared() <= VIRUS_CLICK_DIST * VIRUS_CLICK_DIST)
			{
                _grabbed = true;
				_pinJoint.NodeA = _pinJoint.GetPathTo(this);
				AngularDamp = GRABBED_DAMP;
				GravityScale = GRABBED_GRAV;
            }
			else if (@event.IsReleased())
			{
				_grabbed = false;
				_pinJoint.NodeA = null;
				_pinBody.Position = _pinBodyPos;
				AngularDamp = _ungrabbedDamp;
				GravityScale = _ungrabbedGrav;
			}
			
		}
		if (@event.IsAction("inspect_desktop_virus"))
		{
			GD.Print(GetLocalMousePosition());
			if(@event is InputEventMouseButton mb && mb.IsPressed() && GetLocalMousePosition().LengthSquared() <= VIRUS_CLICK_DIST * VIRUS_CLICK_DIST)
			{
				_statDisplay.Visible = !_statDisplay.Visible;
			}
		}

    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
		bool hitGround = false;
		bool hitWall = false;

		// get collisions
		for(int i = 0; i < state.GetContactCount(); i++)
		{
            //GD.Print("contact ", i , ": contacting body with normal: ", state.GetContactLocalNormal(i));
            
            float adjusted_normal_angle = Mathf.Abs(state.GetContactLocalNormal(i).AngleTo(UP_VEC));

            // check grounded
            if (Mathf.RadToDeg(adjusted_normal_angle) <= MAX_FLOOR_ANGLE_DEGREES)
            {
                hitGround = true;
            }
			
			if(Mathf.Abs(Mathf.RadToDeg(adjusted_normal_angle)) > 70 && Mathf.Abs(Mathf.RadToDeg(adjusted_normal_angle)) < 110)
			{
                hitWall = true;
			}

		}

        _currentlyOnGround = hitGround;
		_hitWall = hitWall;


    }
	
	// === Further Methods === (PascalCase, local variables camelCase)

	public void OnVirusVisChange()
	{
		if(Visible)
		{
			// reset related vars for internal behavior
			_timeSinceGround = 0;
            GroundedTime = 0;
			StandingTime = 0;
		}
	}

	private Vector2 GetImpulseToAchieveVel(Vector2 targetVelocity)
	{
		Vector2 dir = targetVelocity - LinearVelocity;

		float mag = dir.Length(); 
		dir = dir.Normalized();

		mag *= Mass;
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
}
