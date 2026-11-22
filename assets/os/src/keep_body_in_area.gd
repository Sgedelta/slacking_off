extends Area2D


@export var bounded_bodies : Array[RigidBody2D];
@export var local_return_pos : Vector2;

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _on_body_exited(body: Node2D) -> void:
	if bounded_bodies.has(body):
		body.reset_physics_interpolation()
		(body as RigidBody2D).linear_velocity = Vector2.ZERO;
		(body as RigidBody2D).angular_velocity = 0.0;
		var newTransform = body.transform
		newTransform.origin = to_global(local_return_pos)
		PhysicsServer2D.body_set_state(
			body.get_rid(), 
			PhysicsServer2D.BODY_STATE_TRANSFORM,
			newTransform
		)
