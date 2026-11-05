using Godot;

namespace SlackingOff;

public partial class VirusController : RigidBody2D
{
	// === Signals === (PascalCaseEventHandler())

	// === Enums === (PascalCase, members CONSTANT_CASE)

	// === Constants === (CONSTANT_CASE)
	const float GRABBED_DAMP = .75f;
	const float GRABBED_GRAV = 2f;
	readonly Vector2 UP_VEC = Vector2.Up; //used to check if we are on the floor
	const float MAX_FLOOR_ANGLE_DEGREES = 30f;

	// === Exported Vars === (PascalCase)

	// === Public Vars === (PascalCase)
	public Vector2 Bounds = new Vector2(1920, 1440);
	public bool Grabbable = true;
	public bool Grounded { get; private set; }
	public float GroundedTime { get; private set; }

	// === Private Vars === (_underscoredCamelCase)
	private KinematicCollision2D _collision;
	private bool _grabbed = false;
	private float _ungrabbedDamp;
	private float _ungrabbedGrav;
	private StaticBody2D _pinBody;
	private PinJoint2D _pinJoint;
	private Vector2 _pinBodyPos;
	
	// === Godot Methods ===
	
	public VirusController()
	{
		//runs when the object is created in memory, Parent -> Child
	}
	
	public override void _Ready()
	{
		//runs when the object and its children are ready, Child -> Parent
		_pinBody = GetNode<StaticBody2D>("StaticPin");
		_pinBodyPos = _pinBody.Position;
		_pinJoint = _pinBody.GetNode<PinJoint2D>("GrabPin");
		_ungrabbedDamp = AngularDamp;
		_ungrabbedGrav = GravityScale;
	}
	
	// Prefer using PhysicsProcess over Process
	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if(Grounded)
		{
			GroundedTime += dt;
		}
		else
		{
			GroundedTime = 0;
		}

		if(_grabbed)
		{
			_pinBody.Position = GetLocalMousePosition();
		}
	}

    public override void _Input(InputEvent @event)
    {
        //inspect_desktop_virus
        if (@event.IsAction("grab_desktop_virus"))
		{
			if(@event.IsPressed() && Grabbable)
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

    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
		bool foundGroundThisFrame = false;
		// get collisions
		for(int i = 0; i < state.GetContactCount(); i++)
		{
			//GD.Print("contact ", i , ": contacting body with normal: ", state.GetContactLocalNormal(i));
			// check grounded
			if(!foundGroundThisFrame)
			{
				float angle = Mathf.Abs(state.GetContactLocalNormal(i).AngleTo(UP_VEC));

				if(Mathf.RadToDeg(angle) <= MAX_FLOOR_ANGLE_DEGREES)
				{
					foundGroundThisFrame = true;
				}
			}
		}

		// set grounded
		Grounded = foundGroundThisFrame;
		
    }
	
	// === Further Methods === (PascalCase, local variables camelCase)
}
